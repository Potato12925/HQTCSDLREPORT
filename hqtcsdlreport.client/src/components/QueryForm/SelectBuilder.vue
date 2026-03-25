<template>
  <div class="mb-6">
    <h3 class="font-semibold text-primary mb-2">SELECT</h3>
    <DistinctToggle :state="state" />
    <div v-for="table in tables" :key="table.id" class="mb-4">
      <!-- TABLE NAME -->
      <div class="font-medium text-dark mb-1">
        {{ table.tableName }}
      </div>

      <!-- COLUMNS -->
      <div
        v-for="col in table.columns"
        :key="col.column.columnId"
        class="flex gap-2 items-center mb-1 flex-wrap"
      >
        <!-- COLUMN NAME -->
        <span class="w-40 text-sm text-dark">
          {{ col.column.columnName }}
        </span>

        <!-- AGGREGATE -->
        <select v-model="col.aggregate" class="border px-2 py-1 rounded bg-white text-sm">
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
          class="border px-2 py-1 rounded bg-white text-sm"
        />

        <!-- CRITERIA (simple input) -->
        <input
          v-model="criteriaMap[col.column.columnId]"
          placeholder="criteria (e.g > 10)"
          class="border px-2 py-1 rounded bg-white text-sm"
          @change="updateCriteria(col)"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, reactive } from "vue";
import type { QueryState, QueryTable, QueryColumn, ConditionGroup } from "@/types/queryState";
import DistinctToggle from "./DistinctToggle.vue";
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
  if (!props.state.tables) return [];
  return Object.values(props.state.tables);
});

/* ========================
   CRITERIA MAP
======================== */
const criteriaMap = reactive<Record<number, string>>({});

/* ========================
   UPDATE CRITERIA
======================== */
const updateCriteria = (col: QueryColumn) => {
  const value = criteriaMap[col.column.columnId];

  if (!value) {
    col.criteria = null;
    return;
  }

  // 🔥 simple parse (basic)
  const condition: ConditionGroup = {
    type: "AND",
    conditions: [
      {
        column: col.column,
        operator: ">",
        value: value,
      },
    ],
  };

  col.criteria = condition;
};
</script>
