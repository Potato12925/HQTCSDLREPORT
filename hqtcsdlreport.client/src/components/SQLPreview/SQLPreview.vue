<template>
  <div class="border rounded-xl p-4 bg-light">
    <h3 class="text-lg font-semibold text-primary mb-2">SQL Preview</h3>

    <pre class="bg-dark text-white p-3 rounded overflow-auto text-sm"
      >{{ sql }}
    </pre>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
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
}>();

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
    `SELECT ${stateDistinct()} ${buildSelect(props.state)}`,
    buildFrom(props.state),
    buildWhere(props.state),
    buildGroupBy(props.state),
    buildHaving(props.state),
    buildOrderBy(props.state),
  ];

  return parts.filter(Boolean).join("\n");
});

function stateDistinct() {
  return props.state.distinct ? "DISTINCT" : "";
}
</script>
