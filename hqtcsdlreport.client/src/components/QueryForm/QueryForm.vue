<template>
  <div class="p-4 bg-light">
    <div v-if="hasTables">
      <SelectBuilder :state="props.queryState" />

      <FromBuilder :state="props.queryState"></FromBuilder>

      <WhereBuilder :state="props.queryState" />

      <GroupByBuilder v-show="shouldShowGroupBy" :state="props.queryState" />

      <HavingBuilder v-show="shouldShowGroupBy" :state="props.queryState" />

      <OrderByBuilder :state="props.queryState" :columns="allColumns" />
    </div>

    <div v-else class="text-gray-400 text-center mt-10">Please select columns from tables</div>
  </div>
</template>

<script setup lang="ts">
import { computed, watch } from "vue";
import type { QueryState, ColumnRef } from "@/types/queryState";
import SelectBuilder from "@/components/QueryForm/SelectBuilder/SelectBuilder.vue";
import WhereBuilder from "@/components/QueryForm/WhereBuilder/WhereBuilder.vue";
import GroupByBuilder from "./GroupByBuilder.vue";
import OrderByBuilder from "./OrderByBuilder.vue";
import FromBuilder from "./FromBuilder/FromBuilder.vue";
import HavingBuilder from "./HavingBuilder/HavingBuilder.vue";
const props = defineProps<{
  queryState: QueryState;
}>();

const resetStateWhenNoTables = () => {
  props.queryState.from = undefined;
  props.queryState.joins = undefined;
  props.queryState.where = undefined;
  props.queryState.groupBy = undefined;
  props.queryState.having = undefined;
  props.queryState.orderBy = undefined;
};

watch(
  () => props.queryState.tables?.length ?? 0,
  (tableCount) => {
    if (tableCount === 0) {
      resetStateWhenNoTables();
    }
  },
  { immediate: true },
);

const hasTables = computed(() => {
  const tables = props.queryState.tables;
  return !!tables && tables.length > 0;
});
const shouldShowGroupBy = computed(() => {
  const tables = props.queryState.tables ?? [];
  const selectedColumns = tables.flatMap((table) => table.columns).filter((column) => column.show);

  const hasAggregateColumn = selectedColumns.some((column) => column.aggregate);

  return hasAggregateColumn;
});
const allColumns = computed<ColumnRef[]>(() => {
  if (!props.queryState.tables) return [];

  const result: ColumnRef[] = [];

  for (const table of props.queryState.tables) {
    for (const col of table.columns) {
      result.push(col.column);
    }
  }

  return result;
});
</script>
