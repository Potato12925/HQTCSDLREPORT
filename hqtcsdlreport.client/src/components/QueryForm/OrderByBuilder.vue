<template>
  <div class="mb-6">
    <!-- HEADER -->
    <h3 class="font-semibold text-primary mb-3 text-lg tracking-wide">ORDER BY</h3>

    <!-- ADD -->
    <div class="flex items-center gap-2">
      <!-- COLUMN -->
      <select
        v-model="col"
        class="border border-primary/20 px-2 py-1 rounded bg-white text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 min-w-[180px]"
      >
        <option disabled value="">Select column</option>

        <option v-for="c in columns" :key="key(c)" :value="c" :disabled="isAdded(c)">
          {{ c.columnName }}
        </option>
      </select>

      <!-- DIRECTION -->
      <select
        v-model="dir"
        class="border border-primary/20 px-2 py-1 rounded bg-light text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
      >
        <option value="ASC">ASC</option>
        <option value="DESC">DESC</option>
      </select>

      <!-- ADD BUTTON -->
      <button @click="add" class="px-3 py-1 bg-primary text-white rounded text-sm hover:opacity-90">
        + Add
      </button>
    </div>

    <!-- LIST -->
    <div class="mt-3 flex flex-wrap gap-2">
      <div
        v-for="(o, i) in state.orderBy"
        :key="i"
        class="flex items-center gap-2 px-3 py-1 rounded-full bg-white border border-primary/20 text-sm"
      >
        <span class="text-dark font-medium">
          {{ getColumnName(o.column) }}
        </span>

        <span class="text-xs text-dark/60"> ({{ o.direction }}) </span>

        <button @click="remove(i)" class="text-red-500 hover:text-red-600 text-xs">✕</button>
      </div>

      <!-- EMPTY -->
      <span v-if="!state.orderBy || state.orderBy.length === 0" class="text-xs text-dark/60 italic">
        No order by
      </span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import type { QueryState, ColumnRef } from "@/types/queryState";

/* ========================
   PROPS
======================== */
const props = defineProps<{
  state: QueryState;
  columns: ColumnRef[];
}>();

/* ========================
   STATE
======================== */
const col = ref<ColumnRef | "">("");
const dir = ref<"ASC" | "DESC">("ASC");

/* ========================
   HELPERS
======================== */
const key = (c: ColumnRef) => `${c.tableId}-${c.columnId}`;

const getColumnName = (col: ColumnRef | string) => (typeof col === "string" ? col : col.columnName);

const isAdded = (c: ColumnRef) =>
  props.state.orderBy?.some((o) => key(o.column as ColumnRef) === key(c)) ?? false;

/* ========================
   ACTIONS
======================== */
const add = () => {
  if (!col.value) return;

  if (!props.state.orderBy) props.state.orderBy = [];

  if (!isAdded(col.value)) {
    props.state.orderBy.push({
      column: col.value,
      direction: dir.value,
    });
  }

  col.value = "";
};

const remove = (index: number) => {
  props.state.orderBy?.splice(index, 1);
};
</script>
