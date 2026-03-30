<template>
  <div class="flex gap-2 items-center">
    <!-- COLUMN -->
    <select v-model="selectedColumn" class="input">
      <option disabled value="">Select column</option>

      <option v-for="col in columns" :key="col.columnId + '-' + col.tableId" :value="col">
        {{ col.label }}
      </option>
    </select>

    <!-- OPERATOR -->
    <select v-model="model.operator" class="input">
      <option v-for="op in operators" :key="op" :value="op">
        {{ op }}
      </option>
    </select>

    <!-- VALUE -->
    <template v-if="model.operator === 'BETWEEN'">
      <input v-model="betweenValue[0]" placeholder="min" class="input w-24" />
      <span class="text-xs text-dark">AND</span>
      <input v-model="betweenValue[1]" placeholder="max" class="input w-24" />
    </template>

    <input
      v-else-if="model.operator === 'IN'"
      v-model="inValue"
      placeholder="a, b, c"
      class="input min-w-[140px]"
    />

    <input
      v-else-if="!noValueOperators.includes(model.operator)"
      v-model="model.value"
      placeholder="value"
      class="input"
    />

    <button @click="$emit('remove')" class="text-red-500">x</button>
  </div>
</template>

<script setup lang="ts">
import { computed, watch } from "vue";
import type { Condition, ColumnRef, Operator } from "@/types/queryState";

/* ================= TYPES ================= */

type SelectableColumn = ColumnRef & {
  label: string;
};

/* ================= PROPS ================= */

const props = defineProps<{
  columns: SelectableColumn[];
}>();

defineEmits(["remove"]);

/* ================= MODEL ================= */

const model = defineModel<Condition>({
  required: true,
});

/* ================= OPERATORS ================= */

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

const noValueOperators: Operator[] = ["IS NULL", "IS NOT NULL"];

/* ================= COMPUTED ================= */

const selectedColumn = computed<SelectableColumn | null>({
  get() {
    if (typeof model.value.column === "string") return null;

    return (
      props.columns.find(
        (c) =>
          typeof model.value.column !== "string" &&
          c.columnId === model.value.column.columnId &&
          c.tableId === model.value.column.tableId,
      ) || null
    );
  },
  set(val) {
    if (val) {
      model.value.column = {
        tableId: val.tableId,
        columnId: val.columnId,
        columnName: val.columnName,
        dataType: val.dataType,
      };
    }
  },
});

const betweenValue = computed<[string, string]>({
  get() {
    if (!Array.isArray(model.value.value) || model.value.value.length < 2) {
      model.value.value = ["", ""];
    }
    return model.value.value as [string, string];
  },
  set(val) {
    model.value.value = val;
  },
});

const inValue = computed<string>({
  get() {
    if (!Array.isArray(model.value.value)) return "";
    return model.value.value.join(", ");
  },
  set(val: string) {
    model.value.value = val
      .split(",")
      .map((v) => v.trim())
      .filter(Boolean);
  },
});

/* ================= WATCH ================= */

watch(
  () => model.value.operator,
  (op) => {
    if (op === "BETWEEN") {
      model.value.value = ["", ""];
    } else if (op === "IN") {
      model.value.value = [];
    } else if (op === "IS NULL" || op === "IS NOT NULL") {
      model.value.value = undefined;
    } else if (Array.isArray(model.value.value) || model.value.value == null) {
      model.value.value = "";
    }
  },
);
</script>
