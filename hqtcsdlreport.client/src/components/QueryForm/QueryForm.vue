<template>
  <div class="flex-1 p-4 bg-light overflow-y-auto">
    <div v-if="hasTables">
      <SelectBuilder :state="props.queryState" />
<!-- 
      <FromBuilder :state="props.queryState"></FromBuilder> -->

      <WhereBuilder :state="props.queryState" :columns="allColumns" />

      <GroupByBuilder :state="props.queryState" :columns="allColumns" />

      <OrderByBuilder :state="props.queryState" :columns="allColumns" />

      <PaginationControl :state="props.queryState" />
    </div>
    <!-- EMPTY STATE -->
    <div v-else class="text-gray-400 text-center mt-10">Please select columns from tables</div>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import type { QueryState, ColumnRef } from "@/types/queryState";
import SelectBuilder from "./SelectBuilder.vue";
import WhereBuilder from "./WhereBuilder.vue";
import GroupByBuilder from "./GroupByBuilder.vue";
import OrderByBuilder from "./OrderByBuilder.vue";
import PaginationControl from "./PaginationControl.vue";
import FromBuilder from "./FromBuilder.vue";

const props = defineProps<{
  queryState: QueryState;
}>();

const hasTables = computed(() => {
  const tables = props.queryState.tables;
  return !!tables && Object.keys(tables).length > 0;
});
const allColumns = computed<ColumnRef[]>(() => {
  if (!props.queryState.tables) return [];

  return Object.values(props.queryState.tables).flatMap((t) => t.columns.map((c) => c.column));
});
</script>
