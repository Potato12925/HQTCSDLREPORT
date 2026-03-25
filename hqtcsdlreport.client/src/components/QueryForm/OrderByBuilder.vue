<template>
  <div class="mb-6">
    <h3 class="font-semibold text-primary mb-2">ORDER BY</h3>

    <div class="flex gap-2">
      <select v-model="col" class="input">
        <option disabled value="">Column</option>
        <option v-for="c in columns" :key="key(c)" :value="c">
          {{ c.columnName }}
        </option>
      </select>

      <select v-model="dir" class="input">
        <option>ASC</option>
        <option>DESC</option>
      </select>

      <button @click="add" class="btn">Add</button>
    </div>

    <div class="mt-2 text-sm">
      <div v-for="(o, i) in state.orderBy" :key="i">
        {{ getColumnName(o.column) }} ({{ o.direction }})
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
const dir = ref<"ASC" | "DESC">("ASC");
const getColumnName = (col: ColumnRef | string) => {
  return typeof col === "string" ? col : col.columnName
}
const add = () => {
  if (!col.value) return;

  if (!props.state.orderBy) props.state.orderBy = [];

  props.state.orderBy.push({
    column: col.value,
    direction: dir.value,
  });
};

const key = (c: ColumnRef) => `${c.tableId}-${c.columnId}`;
</script> 