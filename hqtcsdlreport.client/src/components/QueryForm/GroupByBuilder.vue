<template>
  <div class="mb-6 ">
    <!-- HEADER -->
    <h3 class="font-semibold text-primary mb-3 text-lg tracking-wide">GROUP BY</h3>

    <!-- ADD -->
    <div class="flex items-center gap-2 ">
      <select
        v-model="selected"
        class="border border-primary/20 px-2 py-1 rounded bg-white text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 min-w-[200px]"
      >
        <option disabled value="">Select column</option>

        <option v-for="c in flatColumns" :key="key(c)" :value="c" :disabled="isGrouped(c)">
          {{ c.columnName }}
        </option>
      </select>

      <button @click="add" class="px-3 py-1 bg-primary text-white rounded text-sm hover:opacity-90">
        + Add
      </button>
    </div>

    <!-- TAG LIST -->
    <div class="mt-3 flex flex-wrap gap-2">
      <div
        v-for="(g, i) in state.groupBy"
        :key="i"
        class="flex items-center gap-2 px-3 py-1 rounded-full bg-white border border-primary/20  text-sm"
      >
        <span class="text-dark font-medium">
          {{ getColumnName(g.column) }}
        </span>

        <button @click="remove(i)" class="text-red-500 hover:text-red-600 text-xs">✕</button>
      </div>

      <!-- EMPTY -->
      <span v-if="!state.groupBy || state.groupBy.length === 0" class="text-xs text-dark/60 italic">
        No group by
      </span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import type { QueryState, QueryTable, ColumnRef } from "@/types/queryState";

/* ========================
   PROPS
======================== */
const props = defineProps<{
  state: QueryState;
}>();

/* ========================
   STATE
======================== */
const selected = ref<ColumnRef | "">("");

/* ========================
   TABLES → FLAT
======================== */
const tables = computed<QueryTable[]>(() => props.state.tables ?? []);

const flatColumns = computed<ColumnRef[]>(() =>
  tables.value.flatMap((t) => t.columns.map((c) => c.column)),
);

/* ========================
   HELPERS
======================== */
const key = (c: ColumnRef) => `${c.tableId}-${c.columnId}`;

const getColumnName = (col: ColumnRef | string) => (typeof col === "string" ? col : col.columnName);

const isGrouped = (col: ColumnRef) =>
  props.state.groupBy?.some((g) => key(g.column as ColumnRef) === key(col)) ?? false;

/* ========================
   ACTIONS
======================== */
const add = () => {
  if (!selected.value) return;

  if (!props.state.groupBy) props.state.groupBy = [];

  if (!isGrouped(selected.value)) {
    props.state.groupBy.push({ column: selected.value });
  }

  selected.value = "";
};

const remove = (index: number) => {
  props.state.groupBy?.splice(index, 1);
};
</script>
