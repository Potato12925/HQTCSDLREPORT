<template>
  <div class="flex-1 p-4 bg-light overflow-y-auto">
    <div v-if="hasTables">
      <SelectBuilder :state="props.queryState" />
      
      <FromBuilder :state="props.queryState"></FromBuilder>

      <WhereBuilder :state="props.queryState" :columns="allColumns" />

      <GroupByBuilder :state="props.queryState" :columns="allColumns" />

      <OrderByBuilder :state="props.queryState" :columns="allColumns" />

      <PaginationControl :state="props.queryState" />
    </div>
    
    <div v-else class="text-gray-400 text-center mt-10">Please select columns from tables</div>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import type { QueryState, ColumnRef } from "@/types/queryState";
import SelectBuilder from "@/components/QueryForm/SelectBuilder/SelectBuilder.vue";
import WhereBuilder from "./FromBuilder/WhereBuilder/WhereBuilder.vue";
import GroupByBuilder from "./GroupByBuilder.vue";
import OrderByBuilder from "./OrderByBuilder.vue";
import PaginationControl from "./PaginationControl.vue";
import FromBuilder from "./FromBuilder/FromBuilder.vue";

const props = defineProps<{
  queryState: QueryState;
}>();

const hasTables = computed(() => {
  const tables = props.queryState.tables;
  return !!tables && tables.length > 0;
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
