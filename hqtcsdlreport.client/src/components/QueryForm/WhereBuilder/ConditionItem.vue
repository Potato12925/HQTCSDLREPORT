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
    <select
      v-if="columnType === 'boolean' && !noValueOperators.includes(model.operator)"
      v-model="model.value"
      class="input"
    >
      <option :value="true">True</option>
      <option :value="false">False</option>
    </select>

    <template v-else-if="model.operator === 'BETWEEN'">
      <input v-model="betweenValue[0]" :type="inputType" placeholder="min" class="input w-24" />
      <span class="text-xs text-dark">AND</span>
      <input v-model="betweenValue[1]" :type="inputType" placeholder="max" class="input w-24" />
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
      :type="inputType"
      placeholder="value"
      class="input"
    />

    <button @click="$emit('remove')" class="text-red-500">x</button>
  </div>
</template>

<script setup lang="ts">
import { computed, watch } from "vue";
import type { Condition, ColumnRef, ColumnDataType, Operator } from "@/types/queryState";

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

/* ================= OPERATORS ================= */

const columnType = computed<ColumnDataType>(() => selectedColumn.value?.dataType ?? "string");

const operators = computed<Operator[]>(() => {
  switch (columnType.value) {
    case "number":
    case "date":
      return ["=", "!=", ">", "<", ">=", "<=", "BETWEEN", "IN", "IS NULL", "IS NOT NULL"];
    case "boolean":
      return ["=", "!=", "IS NULL", "IS NOT NULL"];
    case "string":
    default:
      return ["=", "!=", "LIKE", "IN", "IS NULL", "IS NOT NULL"];
  }
});

const noValueOperators: Operator[] = ["IS NULL", "IS NOT NULL"];

const betweenValue = computed<[string, string]>({
  get() {
    if (!Array.isArray(model.value.value) || model.value.value.length < 2) {
      model.value.value = ["", ""];
    }
    return model.value.value as [string, string];
  },
  set(val) {
    model.value.value = normalizeArrayValue(val);
  },
});

const inValue = computed<string>({
  get() {
    if (!Array.isArray(model.value.value)) return "";
    return model.value.value.join(", ");
  },
  set(val: string) {
    const arr = val
      .split(",")
      .map((v) => v.trim())
      .filter(Boolean);
    model.value.value = normalizeArrayValue(arr);
  },
});

const inputType = computed(() => {
  if (columnType.value === "number") return "number";
  if (columnType.value === "date") return "date";
  return "text";
});

/* ================= WATCH ================= */

watch(
  () => selectedColumn.value?.columnId,
  () => {
    if (!operators.value.includes(model.value.operator)) {
      model.value.operator = operators.value[0];
    }
  },
);

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
      model.value.value = columnType.value === "boolean" ? false : "";
    }
  },
);

watch(
  () => model.value.value,
  (val) => {
    if (noValueOperators.includes(model.value.operator)) return;
    if (model.value.operator === "BETWEEN" || model.value.operator === "IN") return;

    const normalized = normalizeSingleValue(val);
    if (normalized !== val) {
      model.value.value = normalized;
    }
  },
);

function normalizeSingleValue(value: unknown) {
  if (columnType.value === "number") {
    if (value === "" || value == null) return "";
    const parsed = Number(value);
    return Number.isNaN(parsed) ? "" : parsed;
  }

  if (columnType.value === "boolean") {
    if (value === true || value === false) return value;
    if (value === "true") return true;
    if (value === "false") return false;
    return false;
  }

  return value ?? "";
}

function normalizeArrayValue(values: unknown[]) {
  if (columnType.value !== "number") return values;
  return values.map((v) => {
    if (v === "" || v == null) return "";
    const parsed = Number(v);
    return Number.isNaN(parsed) ? "" : parsed;
  });
}
</script>
