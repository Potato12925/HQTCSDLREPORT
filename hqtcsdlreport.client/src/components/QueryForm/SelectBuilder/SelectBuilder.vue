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
          <input v-model="col.show" type="checkbox" class="w-5 h-5 cursor-pointer accent-primary" />
        </label>

        <!-- COLUMN NAME -->
        <span class="text-sm text-dark font-medium truncate">
          {{ col.column.columnName }}
        </span>

        <!-- AGGREGATE -->
        <select
          v-model="col.aggregate"
          class="border border-primary/20 px-2 py-1 rounded bg-light text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
        >
          <option :value="null">None</option>
          <option>COUNT</option>
          <option>SUM</option>
          <option>AVG</option>
          <option>MIN</option>
          <option>MAX</option>
        </select>

        <!-- ALIAS -->
        <input
          v-model="col.alias"
          @input="col.alias = (col.alias || '').replace(/\s/g, '')"
          placeholder="alias"
          class="w-full border border-primary/20 px-2 py-1 rounded bg-light text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
        />

        <!-- PARAMETER REPORT -->
        <label class="flex items-center justify-center">
          <input
            v-model="col.parameterReport"
            type="checkbox"
            class="w-5 h-5 cursor-pointer accent-primary"
          />
        </label>

        <!-- GROUP REPORT -->
        <label class="flex items-center justify-center">
          <input
            v-model="col.groupReport"
            type="checkbox"
            class="w-5 h-5 cursor-pointer accent-primary"
          />
        </label>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import type { QueryState, QueryTable, QueryColumn } from "@/types/queryState";

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
</script>

<style scoped>
select,
input {
  transition: all 0.15s ease;
}
</style>
