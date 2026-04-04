<template>
  <div class="mb-6">
    <h3 class="font-semibold text-primary mb-3 text-lg tracking-wide">GROUP BY</h3>

    <!-- TAG LIST -->
    <div class="flex flex-wrap gap-2">
      <div
        v-for="(g, i) in autoGroupBy"
        :key="i"
        class="flex items-center gap-2 px-3 py-1 rounded-full bg-white border border-primary/20 text-sm"
      >
        <span class="text-dark font-medium">
          {{ getColumnName(g.column) }}
        </span>
      </div>

      <span v-if="autoGroupBy.length === 0" class="text-xs text-dark/60 italic"> No group by </span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, watchEffect } from "vue";
import type { QueryState, QueryTable, ColumnRef, GroupBy } from "@/types/queryState";

/* ========================
   PROPS
======================== */
const props = defineProps<{
  state: QueryState;
}>();

/* ========================
   FLATTEN COLUMNS
======================== */
const tables = computed<QueryTable[]>(() => props.state.tables ?? []);

const allColumns = computed(() => tables.value.flatMap((t) => t.columns));

/* ========================
   AUTO GROUP BY LOGIC
======================== */
const autoGroupBy = computed<GroupBy[]>(() => {
  const hasAggregate = allColumns.value.some((c) => c.show && c.aggregate);

  if (!hasAggregate) return [];

  return allColumns.value
    .filter((c) => c.show && !c.aggregate)
    .map((c) => ({
      column: c.column,
    }));
});

/* ========================
   SYNC TO STATE
======================== */
watchEffect(() => {
  if (autoGroupBy.value.length === 0) {
    props.state.groupBy = undefined;
  } else {
    props.state.groupBy = autoGroupBy.value;
  }
});

/* ========================
   HELPERS
======================== */
const getColumnName = (col: ColumnRef | string) => {
  if (typeof col === "string") return col;

  const table = tables.value.find((t) => t.id === col.tableId);

  if (!table) return col.columnName;

  const tableLabel = table.alias || table.tableName;

  return `${tableLabel}.${col.columnName}`;
};
</script>
