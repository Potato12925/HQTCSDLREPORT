<template>
  <div class="mb-6">
    <!-- HEADER -->
    <h3 class="font-semibold text-primary mb-3 text-lg tracking-wide">SELECT</h3>

    <DistinctToggle :state="state" />

    <!-- TABLE LIST -->
    <div
      v-for="table in tables"
      :key="table.id"
      class="mb-4 p-4 rounded-xl bg-white shadow-sm border border-primary/10"
    >
      <!-- TABLE NAME -->
      <div class="font-semibold text-dark mb-3 text-base">
        {{ table.tableName }}
      </div>

      <!-- COLUMNS HEADER -->
      <div
        class="grid grid-cols-[80px_1fr_160px_180px_170px_150px] gap-2 px-2 py-1 mb-2 text-sm font-semibold uppercase tracking-wide text-dark"
      >
        <span class="text-center">Show</span>
        <span>Column</span>
        <span>Aggregattion</span>
        <span>Alias</span>
        <span class="text-center">Parameter Report</span>
        <span class="text-center">Group Report</span>
      </div>

      <!-- COLUMNS -->
      <div
        v-for="col in table.columns"
        :key="col.column.columnId"
        class="grid grid-cols-[80px_1fr_160px_180px_170px_150px] gap-2 items-center mb-2 p-2 rounded-lg transition hover:bg-primary/5"
      >
        <!-- SHOW -->
        <label class="flex items-center justify-center text-xs text-dark">
          <input
            v-model="col.show"
            type="checkbox"
            class="w-5 h-5 cursor-pointer accent-primary"
            @change="onChangeShow(col)"
          />
        </label>

        <!-- COLUMN NAME -->
        <span class="text-sm text-dark font-medium truncate">
          {{ col.column.columnName }}
        </span>

        <!-- AGGREGATE -->
        <select
          v-model="col.aggregate"
          class="border border-primary/20 px-2 py-1 rounded bg-light text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
          @change="onChangeAggregate(col)"
          @focus="normalizeAggregate(col)"
        >
          <option :value="null">None</option>
          <option v-for="agg in getAvailableAggregates(col)" :key="agg" :value="agg">
            {{ agg }}
          </option>
        </select>

        <!-- ALIAS -->
        <input
          v-model="col.alias"
          @blur="normalizeAlias(col)"
          placeholder="alias"
          class="w-full border border-primary/20 px-2 py-1 rounded bg-light text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
        />

        <!-- PARAMETER REPORT -->
        <label class="flex items-center justify-center">
          <input
            v-model="col.parameterReport"
            type="checkbox"
            class="w-5 h-5 cursor-pointer accent-primary"
            @change="onChangParameterReport(col)"
          />
        </label>

        <!-- GROUP REPORT -->
        <label class="flex items-center justify-center">
          <input
            v-model="col.groupReport"
            type="checkbox"
            class="w-5 h-5 cursor-pointer accent-primary"
            @change="onchangeGroupReport(col)"
          />
        </label>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import type { QueryState, QueryTable, QueryColumn, AggregateFunction  } from "@/types/queryState";

import DistinctToggle from "@/components/QueryForm/DistinctToggle.vue";
/* ========================
   PROPS
======================== */
const props = defineProps<{
  state: QueryState;
}>();

/* ========================
   TABLES
======================== */
const tables = computed<QueryTable[]>(() => {
  return props.state.tables ?? [];
});

const getColumnName = (column: QueryColumn) => {
  const raw = (column.column as { name?: string; columnName?: string } | undefined) ?? {};
  return (raw.name ?? raw.columnName ?? "").trim().toLowerCase();
};

const onChangParameterReport = (col: QueryColumn) => {
  if (col.parameterReport) {
    col.show = false;
    col.groupReport = false;

    const selectedColumnName = getColumnName(col);
    if (!selectedColumnName) return;

    for (const table of tables.value) {
      for (const tableCol of table.columns) {
        if (tableCol === col) continue;
        if (getColumnName(tableCol) !== selectedColumnName) continue;

        // Tắt parameterReport của cột khác cùng tên đang bật
        if (tableCol.parameterReport) {
          tableCol.parameterReport = false;
          onChangParameterReport(tableCol);
        }

        // Tắt show nếu đang bật
        if (tableCol.show) {
          tableCol.show = false;
          onChangeShow(tableCol);
        }
      }
    }
  }
};
const onChangeShow = (col: QueryColumn) => {
  if (col.show) {
    col.parameterReport = false;

    const selectedColumnName = getColumnName(col);
    if (selectedColumnName) {
      for (const table of tables.value) {
        for (const tableCol of table.columns) {
          if (tableCol === col) continue;
          if (getColumnName(tableCol) !== selectedColumnName) continue;
          if (!tableCol.parameterReport) continue;

          tableCol.parameterReport = false;
        }
      }
    }
  } else {
    if (col.groupReport) {
      col.groupReport = false;
    }
  }
};
const onchangeGroupReport = (col: QueryColumn) => {
  if (col.groupReport) {
    col.show = true;
    col.parameterReport = false;

    const selectedColumnName = getColumnName(col);
    if (!selectedColumnName) return;

    for (const table of tables.value) {
      for (const tableCol of table.columns) {
        if (tableCol === col) continue;
        if (getColumnName(tableCol) !== selectedColumnName) continue;

        if (tableCol.groupReport) {
          tableCol.groupReport = false;
        }
      }
    }
  }
};

const normalizeAlias = (col: QueryColumn) => {
  const alias = (col.alias || "").trim();

  if (!alias) {
    col.alias = null;
    return;
  }

  // Cho phép: chữ, số, tiếng Việt, dấu cách, _
  // Chặn: ; -- [ ] ' " . , ...
  const isValid = /^[\p{L}\p{N}_ ]+$/u.test(alias);

  if (!isValid || alias.length > 50) {
    alert("Alias chỉ được chứa chữ, số, dấu cách, dấu _ và tối đa 50 ký tự.");
    col.alias = null;
    return;
  }

  // Gom nhiều dấu cách thành 1
  col.alias = alias.replace(/\s+/g, " ");
};


const getAvailableAggregates = (col: QueryColumn): AggregateFunction[] => {
  const dataType = col.column.dataType ?? "other";

  switch (dataType) {
    case "number":
      return ["COUNT", "SUM", "AVG", "MIN", "MAX"];

    case "string":
    case "date":
    case "boolean":
    case "guid":
      return ["COUNT", "MIN", "MAX"];

    case "binary":
    case "other":
    default:
      return ["COUNT"];
  }
};

const onChangeAggregate = (col: QueryColumn) => {
  if (!col.aggregate) return;

  const allowed = getAvailableAggregates(col);

  if (!allowed.includes(col.aggregate)) {
    col.aggregate = null;
  }

  if (col.aggregate) {
    col.show = true;
    col.parameterReport = false;
    col.groupReport = false;
  }
};
const normalizeAggregate = (col: QueryColumn) => {
  if (!col.aggregate) return;

  const allowed = getAvailableAggregates(col);

  if (!allowed.includes(col.aggregate)) {
    col.aggregate = null;
  }
};
</script>

<style scoped>
select,
input {
  transition: all 0.15s ease;
}
</style>
