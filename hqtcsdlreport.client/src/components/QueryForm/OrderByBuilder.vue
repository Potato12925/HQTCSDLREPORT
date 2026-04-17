<template>
  <div class="mb-6">
    <!-- HEADER -->
    <h3 class="font-semibold text-primary mb-3 text-lg tracking-wide">ORDER BY</h3>

    <!-- ADD -->
    <div class="flex items-center gap-2">
      <select
        v-model="col"
        class="border border-primary/20 px-2 py-1 rounded bg-white text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 min-w-[180px]"
      >
        <option disabled value="">Select column</option>

        <option v-for="c in columns" :key="key(c)" :value="c" :disabled="isAdded(c)">
          {{ c.columnName }}
        </option>
      </select>

      <button
        @click="add"
        :disabled="!col"
        class="px-3 py-1 bg-primary text-white rounded text-sm hover:opacity-90 disabled:opacity-50"
      >
        + Add
      </button>
    </div>

    <!-- LIST -->
    <div class="mt-3 flex flex-col gap-2">
      <div
        v-for="(o, i) in (state.orderBy?.length ?? 0) ? state.orderBy! : []"
        :key="i"
        class="inline-flex items-center gap-3 px-3 py-1.5 rounded-full bg-white border border-primary/20 text-sm w-fit"
      >
        <!-- MOVE -->
        <button @click="moveUp(i)" :disabled="i === 0" class="text-xs">UP</button>
        <button
          @click="moveDown(i)"
          :disabled="i === (state.orderBy?.length ?? 0) - 1"
          class="text-xs"
        >
          DOWN
        </button>

        <!-- COLUMN -->
        <span class="font-medium whitespace-nowrap">
          {{ getColumnName(o.column) }}
        </span>

        <!-- DIRECTION -->
        <select
          v-model="o.direction"
          class="text-xs border border-primary/20 rounded px-1 py-[2px] bg-light"
        >
          <option value="ASC">ASC</option>
          <option value="DESC">DESC</option>
        </select>

        <!-- REMOVE -->
        <button @click="remove(i)" class="text-red-500 text-xs">✕</button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import type { QueryState, ColumnRef } from "@/types/queryState";

/* ======================== */
const props = defineProps<{
  state: QueryState;
  columns: ColumnRef[];
}>();

/* ======================== */
const col = ref<ColumnRef | "">("");

/* ======================== */
const key = (c: ColumnRef) => `${c.tableId}-${c.columnId}`;

const getColumnName = (col: ColumnRef | string) => (typeof col === "string" ? col : col.columnName);

const isAdded = (c: ColumnRef) =>
  props.state.orderBy?.some((o) => key(o.column as ColumnRef) === key(c)) ?? false;

/* ======================== */
const add = () => {
  if (!col.value) return;

  if (!props.state.orderBy) {
    props.state.orderBy = [];
  }

  if (isAdded(col.value)) return;

  props.state.orderBy.push({
    column: col.value,
    direction: "ASC",
  });

  col.value = "";
};

const remove = (index: number) => {
  props.state.orderBy?.splice(index, 1);
};

/* ========================
   REORDER
======================== */
const moveUp = (index: number) => {
  if (index === 0) return;

  const arr = props.state.orderBy!;
  [arr[index - 1], arr[index]] = [arr[index], arr[index - 1]];
};

const moveDown = (index: number) => {
  const arr = props.state.orderBy!;
  if (index === arr.length - 1) return;

  [arr[index + 1], arr[index]] = [arr[index], arr[index + 1]];
};
</script>
