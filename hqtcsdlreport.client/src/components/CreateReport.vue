<template>
  <div class="w-full max-w-3xl rounded-2xl bg-white p-5 shadow-xl">
    <div class="mb-4 flex items-center justify-between border-b border-primary/15 pb-3">
      <h3 class="text-lg font-semibold text-primary">Create Report</h3>
      <button
        type="button"
        class="rounded px-2 py-1 text-sm text-dark hover:bg-light"
        @click="emitClose"
      >
        Close
      </button>
    </div>
    <section class="mb-4">
      <h4 class="mb-1 text-sm font-semibold text-dark/80">Report Title</h4>

      <input
        v-model="title"
        class="w-full border border-primary/20 px-2 py-1 rounded bg-light text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
      />
    </section>

    <!-- PARAMETER -->
    <section v-if="parameterColumns.length > 0">
      <h4 class="mb-2 text-sm font-semibold text-dark/80">Parameter Values</h4>
      <div class="space-y-2">
        <div
          v-for="col in parameterColumns"
          :key="col.key"
          class="grid grid-cols-[220px_1fr] items-center gap-3 rounded-lg border border-primary/15 px-3 py-2"
        >
          <label :for="`param-${col.key}`" class="text-sm font-medium text-dark">
            {{ col.label }}
          </label>
          <input
            :id="`param-${col.key}`"
            v-model="parameterValues[col.key]"
            type="text"
            class="w-full rounded border border-primary/20 bg-light px-2 py-1 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
            placeholder="Enter value"
          />
        </div>
      </div>
    </section>

    <!-- GROUP -->
    <section v-if="groupOrder.length > 0">
      <h4 class="mb-2 text-sm font-semibold text-dark/80">Group Order</h4>
      <div class="space-y-2">
        <div
          v-for="(col, index) in groupOrder"
          :key="col.key"
          class="grid grid-cols-[40px_1fr_auto] items-center gap-2 rounded-lg border border-primary/15 px-3 py-2"
        >
          <span class="text-center text-sm font-semibold text-primary">{{ index + 1 }}</span>
          <span class="text-sm text-dark">{{ col.label }}</span>
          <div class="flex items-center gap-1">
            <button
              type="button"
              class="rounded border border-primary/20 px-2 py-1 text-xs hover:bg-light disabled:cursor-not-allowed disabled:opacity-40"
              :disabled="index === 0"
              @click="moveGroup(index, -1)"
            >
              Up
            </button>
            <button
              type="button"
              class="rounded border border-primary/20 px-2 py-1 text-xs hover:bg-light disabled:cursor-not-allowed disabled:opacity-40"
              :disabled="index === groupOrder.length - 1"
              @click="moveGroup(index, 1)"
            >
              Down
            </button>
          </div>
        </div>
      </div>
    </section>

    <div class="mt-5 flex justify-end gap-2 border-t border-primary/15 pt-4">
      <button
        type="button"
        class="rounded border border-primary/20 px-4 py-2 text-sm text-dark hover:bg-light"
        @click="emitClose"
      >
        Cancel
      </button>
      <button
        type="button"
        class="rounded bg-primary px-4 py-2 text-sm text-white hover:bg-secondary disabled:cursor-not-allowed disabled:opacity-50"
        :disabled="!canSubmit"
        @click="openReportTab"
      >
        Execute Report
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue";
import { useRouter } from "vue-router";
import type { QueryState, QueryColumn, QueryTable } from "@/types/queryState";
import type { SqlReportPayload } from "@/types/sqlReport";
import { executeSqlApi } from "@/api/dataApi";

type ReportColumnItem = {
  key: string;
  label: string;
  tableId: number;
  columnId: number;
  columnName: string;
  alias?: string | null;
};

const props = defineProps<{
  state: QueryState;
  server?: string;
  database?: string;
  sql: string;
  title?: string;
}>();

const emit = defineEmits<{
  (e: "close"): void;
}>();

const router = useRouter();

const parameterValues = ref<Record<string, string>>({});
const groupOrder = ref<ReportColumnItem[]>([]);

const title = ref(props.title ?? "");
const canSubmit = computed(() => {
  return !!props.sql?.trim() && !!props.server?.trim() && !!props.database?.trim();
});

function toItem(table: QueryTable, col: QueryColumn): ReportColumnItem {
  return {
    key: `${table.id}_${col.column.columnId}`,
    label: `${table.alias || table.tableName}.${col.alias || col.column.columnName}`,
    tableId: table.id,
    columnId: col.column.columnId,
    columnName: col.column.columnName,
    alias: col.alias,
  };
}

const parameterColumns = computed<ReportColumnItem[]>(() => {
  const result: ReportColumnItem[] = [];

  props.state.tables?.forEach((table: QueryTable) => {
    table.columns.forEach((col: QueryColumn) => {
      if (col.parameterReport) {
        result.push({
          key: `${table.id}_${col.column.columnId}`,
          label: `@${col.alias || col.column.columnName}`,
          tableId: table.id,
          columnId: col.column.columnId,
          columnName: col.column.columnName,
          alias: col.alias,
        });
      }
    });
  });

  return result;
});

const buildParameterSql = () => {
  if (!props.sql) return "";

  const paramCols = parameterColumns.value;

  if (paramCols.length === 0) return "";

  const selectParams = paramCols.map((c) => `[${c.columnName}]`).join(", ");

  // replace SELECT ... FROM
  const sql = props.sql.replace(/select\s+[\s\S]*?\s+from/i, `SELECT ${selectParams} FROM`);

  return sql;
};
const loadParameterValues = async () => {
  try {
    const sql = buildParameterSql();
    if (!sql) return;

    const res = await executeSqlApi({
      server: props.server,
      database: props.database,
      sql,
    });

    console.log("API RESULT:", JSON.stringify(res, null, 2));

    const rows = res?.data?.rows ?? [];

    if (!Array.isArray(rows) || rows.length === 0) {
      console.warn("No parameter data returned");
      return;
    }
    parameterColumns.value.forEach((col) => {
      const values = rows
        .map((row) => row?.[col.columnName])
        .filter((v) => v !== undefined && v !== null && v !== "")
        .map((v) => v.toString().trim());

      parameterValues.value[col.key] = values.join(", ");
    });
  } catch (err) {
    console.error("loadParameterValues error:", err);
  }
};
const groupColumns = computed<ReportColumnItem[]>(() => {
  const items: ReportColumnItem[] = [];
  for (const table of props.state.tables ?? []) {
    for (const col of table.columns) {
      if (!col.groupReport) continue;
      items.push(toItem(table, col));
    }
  }
  return items;
});

watch(
  () => parameterColumns.value,
  async (nextColumns, prevColumns) => {
    const nextValues: Record<string, string> = {};

    for (const col of nextColumns) {
      nextValues[col.key] = parameterValues.value[col.key] ?? "";
    }

    parameterValues.value = nextValues;

    // ✅ check có column mới không
    const hasNewColumn =
      !prevColumns || nextColumns.some((c) => !prevColumns.find((p) => p.key === c.key));

    // ✅ chỉ call API khi cần
    if (hasNewColumn && nextColumns.length > 0) {
      await loadParameterValues();
    }
  },
  { immediate: true },
);

watch(
  groupColumns,
  (nextColumns) => {
    const existingIndex = new Map(groupOrder.value.map((col, index) => [col.key, index]));
    const ordered = [...nextColumns].sort((a, b) => {
      const ia = existingIndex.get(a.key) ?? Number.MAX_SAFE_INTEGER;
      const ib = existingIndex.get(b.key) ?? Number.MAX_SAFE_INTEGER;
      if (ia !== ib) return ia - ib;
      return a.label.localeCompare(b.label);
    });
    groupOrder.value = ordered;
  },
  { immediate: true },
);
function moveGroup(index: number, offset: -1 | 1) {
  const toIndex = index + offset;
  if (toIndex < 0 || toIndex >= groupOrder.value.length) return;
  const next = [...groupOrder.value];
  const [item] = next.splice(index, 1);
  next.splice(toIndex, 0, item);
  groupOrder.value = next;
}

function createReportId(): string {
  if (typeof crypto !== "undefined" && typeof crypto.randomUUID === "function") {
    return crypto.randomUUID();
  }
  return `${Date.now()}_${Math.random().toString(36).slice(2, 10)}`;
}

function emitClose() {
  emit("close");
}

function openReportTab() {
  if (!canSubmit.value) return;

  const encodedSql = encodeURIComponent(props.sql);
  const id = createReportId();
  const storageKey = `report_${id}`;

  const parameters = Object.fromEntries(
    parameterColumns.value.map((col) => [
      `@${(col.alias || col.columnName).toUpperCase()}`,
      parameterValues.value[col.key] ?? "",
    ]),
  );

  const payload: SqlReportPayload = {
    sql: encodedSql,
    server: props.server,
    database: props.database,
    title: title.value.trim() ?? "",
    parameters,

    groupOrder: groupOrder.value.map((col) => col.alias || col.columnName),
  };

  console.log("Report Payload:", JSON.stringify(payload, null, 2));

  try {
    sessionStorage.setItem(storageKey, JSON.stringify(payload));
  } catch (error) {
    console.error("Failed to store report payload in sessionStorage.", error);
    alert("Cannot open report tab because payload could not be stored.");
    return;
  }

  const route = router.resolve({
    name: "SqlReport",
    query: { id },
  });

  window.open(route.href, "_blank");
  emitClose();
}
</script>
