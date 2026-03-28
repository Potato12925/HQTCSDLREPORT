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

      <!-- COLUMNS -->
      <div
        v-for="col in table.columns"
        :key="col.column.columnId"
        class="flex gap-2 items-center mb-2 flex-wrap p-2 rounded-lg transition hover:bg-primary/5"
      >
        <!-- SHOW -->
        <label class="flex items-center gap-1 text-xs text-dark">
          <input v-model="col.show" type="checkbox" />
        </label>

        <!-- COLUMN NAME -->
        <span class="w-40 text-sm text-dark font-medium">
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
          placeholder="alias"
          class="border border-primary/20 px-2 py-1 rounded bg-light text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
        />

        <!-- ✅ CONDITION ITEM -->
        <div class="flex items-center gap-2">
          <!-- nếu chưa có criteria -->
          <button
            v-if="!col.criteria"
            @click="initCriteria(col)"
            class="text-sm px-2 py-1 bg-primary text-white rounded"
          >
            + Filter
          </button>

          <!-- nếu có criteria -->
          <Criteria
            v-else
            v-model="col.criteria"
            :tables="tables"
            :fixedColumn="col.column"
            :hideColumn="true"
            @remove="clearCriteria(col)"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import type { QueryState, QueryTable, QueryColumn } from "@/types/queryState";

import DistinctToggle from "@/components/QueryForm/DistinctToggle.vue";
import Criteria from "@/components/QueryForm/SelectBuilder/Criteria.vue";

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

/* ========================
   HANDLERS
======================== */
function initCriteria(col: QueryColumn) {
  col.criteria = {
    column: col.column,
    operator: "=",
    value: "",
  };
}

function clearCriteria(col: QueryColumn) {
  col.criteria = null;
}
</script>

<style scoped>
select,
input {
  transition: all 0.15s ease;
}
</style>
