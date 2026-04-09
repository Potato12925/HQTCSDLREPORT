<template>
  <div class="p-4 bg-light shadow-md">
    <div class="mb-2 flex items-center justify-between">
      <h3 class="text-lg font-semibold text-primary">SQL Preview</h3>
      <div class="flex items-center gap-2">
        <button
          type="button"
          class="inline-flex items-center gap-2 rounded bg-primary px-3 py-1 text-sm text-white hover:bg-secondary disabled:cursor-not-allowed disabled:opacity-50"
          :disabled="!canExecute"
          @click="openExecuteTab"
        >
          <span>Execute</span>
        </button>
        <button
          type="button"
          class="inline-flex items-center gap-2 rounded bg-white px-2 py-1 text-sm text-dark hover:bg-gray-100"
          @click="copySql"
        >
          <svg
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
            stroke-width="1.5"
            stroke="currentColor"
            class="size-5"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              d="M8.25 7.5V6.108c0-1.135.845-2.098 1.976-2.192.373-.03.748-.057 1.123-.08M15.75 18H18a2.25 2.25 0 0 0 2.25-2.25V6.108c0-1.135-.845-2.098-1.976-2.192a48.424 48.424 0 0 0-1.123-.08M15.75 18.75v-1.875a3.375 3.375 0 0 0-3.375-3.375h-1.5a1.125 1.125 0 0 1-1.125-1.125v-1.5A3.375 3.375 0 0 0 6.375 7.5H5.25m11.9-3.664A2.251 2.251 0 0 0 15 2.25h-1.5a2.251 2.251 0 0 0-2.15 1.586m5.8 0c.065.21.1.433.1.664v.75h-6V4.5c0-.231.035-.454.1-.664M6.75 7.5H4.875c-.621 0-1.125.504-1.125 1.125v12c0 .621.504 1.125 1.125 1.125h9.75c.621 0 1.125-.504 1.125-1.125V16.5a9 9 0 0 0-9-9Z"
            />
          </svg>
          <span>{{ copyLabel }}</span>
        </button>
      </div>
    </div>

    <pre
      class="bg-white text-dark p-3 overflow-auto text-sm rounded-xl shadow-sm border border-primary/10"
      >{{ sql }}</pre
    >
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { useRouter } from "vue-router";
import type {
  QueryState,
  QueryTable,
  QueryColumn,
  Condition,
  ConditionGroup,
  HavingCondition,
  HavingConditionGroup,
} from "@/types/queryState";

const props = defineProps<{
  state: QueryState;
  server?: string;
  database?: string;
}>();
const router = useRouter();
const copyLabel = ref("Copy");
let copyResetTimer: ReturnType<typeof setTimeout> | null = null;

// ===================== HELPERS =====================

// table alias or name
function getTableName(table: QueryTable) {
  return table.alias || table.tableName;
}

// column name
function getColumn(col: any): string {
  if (typeof col === "string") return col;

  // tìm table theo tableId
  const table = props.state.tables?.find((t) => t.id === col.tableId);

  if (!table) return col.columnName;

  const tableName = table.alias || table.tableName;

  return `${tableName}.${col.columnName}`;
}

// ===================== SELECT =====================
function buildSelect(state: QueryState) {
  if (!state.tables) return "";

  const cols: string[] = [];

  state.tables.forEach((t) => {
    const tableName = getTableName(t);

    t.columns.forEach((c: QueryColumn) => {
      if (!c.show) return;

      let col = `${tableName}.${c.column.columnName}`;

      if (c.aggregate) {
        col = `${c.aggregate}(${col})`;
      }

      if (c.alias) {
        col += ` AS ${c.alias}`;
      }

      cols.push(col);
    });
  });

  return cols.length ? cols.join(", ") : "*";
}

// ===================== FROM + JOIN =====================
function buildFrom(state: QueryState) {
  if (!state.from) return "";

  let sql = `FROM ${state.from.tableName}`;

  if (state.from.alias) {
    sql += ` ${state.from.alias}`;
  }

  if (state.joins) {
    state.joins.forEach((j) => {
      const to = state.tables?.find((t) => t.id === j.toTableId);
      if (!to) return;

      sql += ` \n${j.type} JOIN ${to.tableName}`;

      if (to.alias) sql += ` ${to.alias}`;

      sql += ` ON ${buildConditionGroup(j.on)}`;
    });
  }

  return sql;
}

// ===================== WHERE =====================
function formatValue(value: any): string {
  if (value === null || value === undefined) return "NULL";

  if (typeof value === "number") return value.toString();

  if (typeof value === "boolean") return value ? "1" : "0";

  return `'${value}'`; // string
}
function buildCondition(cond: Condition): string {
  const col = getColumn(cond.column);

  // NULL operators
  if (cond.operator === "IS NULL" || cond.operator === "IS NOT NULL") {
    return `${col} ${cond.operator}`;
  }

  // 👉 CASE 1: value là column (ColumnRef)
  if (cond.value && typeof cond.value === "object" && "columnName" in cond.value) {
    const valueCol = getColumn(cond.value);
    return `${col} ${cond.operator} ${valueCol}`;
  }

  // 👉 CASE 2: IN (array)
  if (cond.operator === "IN" && Array.isArray(cond.value)) {
    const values = cond.value.map((v) => `'${v}'`).join(", ");
    return `${col} IN (${values})`;
  }

  // 👉 CASE 3: BETWEEN
  if (cond.operator === "BETWEEN" && Array.isArray(cond.value)) {
    return `${col} BETWEEN '${cond.value[0]}' AND '${cond.value[1]}'`;
  }

  // 👉 DEFAULT: value thường
  // DEFAULT
  return `${col} ${cond.operator} ${formatValue(cond.value)}`;
}

function buildConditionGroup(group: ConditionGroup): string {
  return group.conditions
    .map((c) => {
      if ("conditions" in c) {
        return `(${buildConditionGroup(c)})`;
      }
      if ("sql" in c) {
        return c.sql;
      }
      return buildCondition(c);
    })
    .join(` \n${group.type} `);
}

function buildWhere(state: QueryState) {
  const whereGroup = state.where?.group;
  if (!whereGroup?.conditions?.length) return;
  return `WHERE ${buildConditionGroup(whereGroup)}`;
}

// ===================== GROUP BY =====================
function buildGroupBy(state: QueryState) {
  if (!state.groupBy?.length) return "";

  const cols = state.groupBy.map((g) => getColumn(g.column));
  return `GROUP BY ${cols.join(", ")}`;
}

// ===================== HAVING =====================
function buildHavingCondition(cond: HavingCondition): string {
  let left = "";

  // 👉 XÁC ĐỊNH LEFT SIDE
  if (cond.type === "aggregate") {
    const col = getColumn(cond.column);
    left = `${cond.fn}(${col})`;
  }

  if (cond.type === "group_column") {
    left = getColumn(cond.column);
  }

  if (cond.type === "alias") {
    left = cond.alias;
  }

  // 👉 NULL operators
  if (cond.operator === "IS NULL" || cond.operator === "IS NOT NULL") {
    return `${left} ${cond.operator}`;
  }

  // 👉 CASE: value là column
  if (cond.value && typeof cond.value === "object" && "columnName" in cond.value) {
    const valueCol = getColumn(cond.value);
    return `${left} ${cond.operator} ${valueCol}`;
  }

  // 👉 CASE: IN
  if (cond.operator === "IN" && Array.isArray(cond.value)) {
    const values = cond.value.map((v) => formatValue(v)).join(", ");
    return `${left} IN (${values})`;
  }

  // 👉 CASE: BETWEEN
  if (cond.operator === "BETWEEN" && Array.isArray(cond.value)) {
    return `${left} BETWEEN ${formatValue(cond.value[0])} AND ${formatValue(cond.value[1])}`;
  }

  // 👉 DEFAULT
  return `${left} ${cond.operator} ${formatValue(cond.value)}`;
}

function buildHavingGroup(group: HavingConditionGroup): string {
  return group.conditions
    .map((c) => {
      if ("conditions" in c) {
        return `(${buildHavingGroup(c)})`;
      }
      return buildHavingCondition(c as HavingCondition);
    })
    .join(` \n${group.type} `);
}

function buildHaving(state: QueryState) {
  const havingGroup = state.having?.group;
  if (!havingGroup?.conditions?.length) return;
  return `HAVING ${buildHavingGroup(havingGroup)}`;
}

// ===================== ORDER =====================
function buildOrderBy(state: QueryState) {
  if (!state.orderBy?.length) return "";

  return `ORDER BY ${state.orderBy.map((o) => `${getColumn(o.column)} ${o.direction}`).join(", ")}`;
}

// ===================== FINAL SQL =====================
const sql = computed(() => {
  const parts = [
    `SELECT${stateDistinct()} ${buildSelect(props.state)}`,
    buildFrom(props.state),
    buildWhere(props.state),
    buildGroupBy(props.state),
    buildHaving(props.state),
    buildOrderBy(props.state),
  ];
  const query = parts.filter(Boolean).join("\n").trimEnd();
  return query;
});
const canExecute = computed(() => {
  return !!sql.value.trim() && !!props.server?.trim() && !!props.database?.trim();
});

function stateDistinct() {
  return props.state.distinct ? " DISTINCT" : "";
}

function openExecuteTab() {
  if (!canExecute.value) return;

  const encodedSql = encodeURIComponent(sql.value);

  const route = router.resolve({
    name: "SqlExecute",
    query: {
      sql: encodedSql,
      server: props.server,
      database: props.database,
    },
  });

  window.open(route.href, "_blank");
}

async function copySql() {
  const text = sql.value;

  try {
    if (navigator?.clipboard?.writeText) {
      await navigator.clipboard.writeText(text);
    } else {
      const textarea = document.createElement("textarea");
      textarea.value = text;
      textarea.style.position = "fixed";
      textarea.style.opacity = "0";
      document.body.appendChild(textarea);
      textarea.focus();
      textarea.select();
      document.execCommand("copy");
      document.body.removeChild(textarea);
    }

    copyLabel.value = "Copied!";
  } catch {
    copyLabel.value = "Copy failed";
  }

  if (copyResetTimer) clearTimeout(copyResetTimer);
  copyResetTimer = setTimeout(() => {
    copyLabel.value = "Copy";
  }, 1200);
}
</script>
