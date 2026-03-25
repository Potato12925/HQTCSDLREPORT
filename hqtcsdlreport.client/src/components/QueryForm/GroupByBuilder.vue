<template>
  <div class="mb-6">
    <h3 class="font-semibold text-primary mb-2">GROUP BY</h3>

    <select v-model="col" class="input">
      <option disabled value="">Select column</option>
      <option v-for="c in columns" :key="key(c)" :value="c">
        {{ c.columnName }}
      </option>
    </select>

    <button @click="add" class="btn ml-2">Add</button>

    <div class="mt-2 text-sm">
      <div v-for="(g, i) in state.groupBy" :key="i">
        {{ getColumnName(g.column) }}
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import type { QueryState, ColumnRef } from "@/types/queryState";

const props = defineProps<{
  state: QueryState;
  columns: ColumnRef[];
}>();

const col = ref<ColumnRef | "">("");
const getColumnName = (col: ColumnRef | string) => {
  return typeof col === "string" ? col : col.columnName
}
const add = () => {
  if (!col.value) return;

  if (!props.state.groupBy) props.state.groupBy = [];

  props.state.groupBy.push({ column: col.value });
};

const key = (c: ColumnRef) => `${c.tableId}-${c.columnId}`;
</script>
