<script setup lang="ts">
import { computed, watch } from "vue";
import type { QueryTable, Condition, Operator, ColumnRef } from "@/types/queryState";

/* ========================
   PROPS
======================== */
const props = defineProps<{
  tables: QueryTable[];
  modelValue: Condition;
  fixedColumn?: ColumnRef;
  hideColumn?: boolean;
}>();

const emit = defineEmits(["update:modelValue", "remove"]);

/* ========================
   MODEL
======================== */
const condition = computed({
  get: () => props.modelValue,
  set: (val) => emit("update:modelValue", val),
});

/* ========================
   OPERATORS
======================== */
const operators: Operator[] = [
  "=",
  "!=",
  ">",
  "<",
  ">=",
  "<=",
  "LIKE",
  "IN",
  "BETWEEN",
  "IS NULL",
  "IS NOT NULL",
];

/* ========================
   LOCAL STATE
======================== */
const betweenValue = computed({
  get: () => (Array.isArray(condition.value.value) ? condition.value.value : ["", ""]),
  set: (val: [string, string]) => {
    condition.value.value = val;
  },
});

const inValue = computed({
  get: () =>
    Array.isArray(condition.value.value)
      ? condition.value.value.join(", ")
      : (condition.value.value ?? ""),
  set: (val: string) => {
    condition.value.value = val
      .split(",")
      .map((v) => v.trim())
      .filter(Boolean);
  },
});

/* ========================
   WATCH OPERATOR CHANGE
======================== */
watch(
  () => condition.value.operator,
  (op) => {
    if (op === "BETWEEN") {
      condition.value.value = ["", ""];
    } else if (op === "IN") {
      condition.value.value = [];
    } else if (op === "IS NULL" || op === "IS NOT NULL") {
      condition.value.value = undefined;
    } else {
      condition.value.value = "";
    }
  },
);
</script>

<template>
  <div class="flex items-center gap-2 flex-wrap p-2">
    <!-- COLUMN -->
    <template v-if="!hideColumn">
      <template v-if="!fixedColumn">
        <select
          v-model="condition.column"
          class="border border-primary/20 px-2 py-1 rounded bg-light text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
        >
          <option value="">--column--</option>

          <optgroup v-for="table in tables" :key="table.id" :label="table.alias || table.tableName">
            <option v-for="col in table.columns" :key="col.column.columnId" :value="col.column">
              {{ col.column.columnName }}
            </option>
          </optgroup>
        </select>
      </template>

      <span
        v-else
        class="text-sm font-medium text-dark px-2 py-1 bg-white border border-primary/10 rounded"
      >
        {{ fixedColumn.columnName }}
      </span>
    </template>

    <!-- OPERATOR -->
    <select
      v-model="condition.operator"
      class="border border-primary/20 px-2 py-1 rounded bg-light text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
    >
      <option v-for="op in operators" :key="op" :value="op">
        {{ op }}
      </option>
    </select>

    <!-- ===== VALUE ===== -->

    <!-- BETWEEN -->
    <template v-if="condition.operator === 'BETWEEN'">
      <input
        v-model="betweenValue[0]"
        placeholder="min"
        class="border border-primary/20 px-2 py-1 rounded bg-light text-sm w-24"
      />
      <span class="text-xs text-dark">AND</span>
      <input
        v-model="betweenValue[1]"
        placeholder="max"
        class="border border-primary/20 px-2 py-1 rounded bg-light text-sm w-24"
      />
    </template>

    <!-- IN -->
    <input
      v-else-if="condition.operator === 'IN'"
      v-model="inValue"
      placeholder="a, b, c"
      class="border border-primary/20 px-2 py-1 rounded bg-light text-sm min-w-[140px]"
    />

    <!-- NORMAL -->
    <input
      v-else-if="!['IS NULL', 'IS NOT NULL'].includes(condition.operator)"
      v-model="condition.value"
      placeholder="value"
      class="border border-primary/20 px-2 py-1 rounded bg-light text-sm min-w-[120px]"
    />

    <!-- REMOVE -->
    <button @click="$emit('remove')" class="text-red-500 hover:text-red-600 text-sm px-1">✕</button>
  </div>
</template>
