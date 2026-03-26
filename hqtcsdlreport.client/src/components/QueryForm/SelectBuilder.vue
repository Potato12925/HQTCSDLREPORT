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

        <!-- CRITERIA -->
        <select
          :value="criteriaOperatorMap[getCriteriaKey(col)] ?? '='"
          class="border px-2 py-1 rounded bg-white text-sm"
          @change="onOperatorChange(col, $event)"
        >
          <option v-for="op in operatorOptions" :key="op" :value="op">
            {{ op }}
          </option>
        </select>

        <input
          v-model="criteriaValueMap[getCriteriaKey(col)]"
          placeholder="value"
          class="border px-2 py-1 rounded bg-white text-sm"
          @input="updateCriteria"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, reactive } from "vue";
import type {
  QueryState,
  QueryTable,
  QueryColumn,
  Condition,
  Operator,
} from "@/types/queryState";
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
   CRITERIA MAP (UI STATE)
======================== */
const operatorOptions: Operator[] = ["=", "!=", ">", "<", ">=", "<=", "LIKE"];

const criteriaOperatorMap = reactive<Record<string, Operator>>({});
const criteriaValueMap = reactive<Record<string, string>>({});

const getCriteriaKey = (col: QueryColumn) =>
  `${col.column.tableId}-${col.column.columnId}`;

/* ========================
   HANDLERS
======================== */
const onOperatorChange = (col: QueryColumn, event: Event) => {
  criteriaOperatorMap[getCriteriaKey(col)] =
    (event.target as HTMLSelectElement).value as Operator;

  updateCriteria();
};

/* ========================
   UPDATE CRITERIA
======================== */
const updateCriteria = () => {

  for (const table of tables.value) {
    for (const col of table.columns) {
      const key = getCriteriaKey(col);
      const operator = criteriaOperatorMap[key] ?? "=";
      const rawValue = (criteriaValueMap[key] ?? "").trim();

      // ❌ Không có value → clear
      if (!rawValue) {
        col.criteria = null;
        continue;
      }

      let condition: Condition;

      // ✅ Handle special operators
      if (operator === "IS NULL" || operator === "IS NOT NULL") {
        condition = {
          column: col.column,
          operator,
        };
      } else if (operator === "IN") {
        condition = {
          column: col.column,
          operator,
          value: rawValue.split(",").map((v) => v.trim()),
        };
      } else if (operator === "BETWEEN") {
        const [min, max] = rawValue.split(",");
        condition = {
          column: col.column,
          operator,
          value: [min?.trim(), max?.trim()],
        };
      } else {
        condition = {
          column: col.column,
          operator,
          value: rawValue,
        };
      }

      col.criteria = condition;
    }
  }
};
</script>