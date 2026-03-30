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

    <!-- BOOLEAN -->
    <select
      v-if="columnType === 'boolean' && !isNullOperator"
      v-model="condition.value"
      class="border border-primary/20 px-2 py-1 rounded bg-light text-sm"
    >
      <option :value="true">True</option>
      <option :value="false">False</option>
    </select>

    <!-- BETWEEN -->
    <template v-else-if="condition.operator === 'BETWEEN'">
      <input
        v-model="betweenValue[0]"
        :type="inputType"
        placeholder="min"
        class="border border-primary/20 px-2 py-1 rounded bg-light text-sm w-24"
      />
      <span class="text-xs text-dark">AND</span>
      <input
        v-model="betweenValue[1]"
        :type="inputType"
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
      v-else-if="!isNullOperator"
      v-model="condition.value"
      :type="inputType"
      placeholder="value"
      class="border border-primary/20 px-2 py-1 rounded bg-light text-sm min-w-[120px]"
    />

    <!-- REMOVE -->
    <button @click="$emit('remove')" class="text-red-500 hover:text-red-600 text-sm px-1">✕</button>
  </div>
</template>

<script setup lang="ts">
import { computed, watch } from "vue";
import type {
  QueryTable,
  Condition,
  Operator,
  ColumnRef,
  ColumnDataType,
} from "@/types/queryState";

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
   COLUMN TYPE
======================== */
const columnType = computed<ColumnDataType>(() => {
  const col = condition.value.column as ColumnRef | undefined;
  return col?.dataType || "string";
});

/* ========================
   OPERATORS (DYNAMIC)
======================== */
const operators = computed<Operator[]>(() => {
  switch (columnType.value) {
    case "number":
    case "date":
      return ["=", "!=", ">", "<", ">=", "<=", "BETWEEN", "IN", "IS NULL", "IS NOT NULL"];

    case "string":
      return ["=", "!=", "LIKE", "IN", "IS NULL", "IS NOT NULL"];

    case "boolean":
      return ["=", "!=", "IS NULL", "IS NOT NULL"];

    default:
      return ["=", "IS NULL", "IS NOT NULL"];
  }
});

/* ========================
   INPUT TYPE
======================== */
const inputType = computed(() => {
  switch (columnType.value) {
    case "number":
      return "number";
    case "date":
      return "date";
    default:
      return "text";
  }
});

/* ========================
   NULL CHECK
======================== */
const isNullOperator = computed(() =>
  ["IS NULL", "IS NOT NULL"].includes(condition.value.operator),
);

/* ========================
   BETWEEN
======================== */
const betweenValue = computed({
  get: () => (Array.isArray(condition.value.value) ? condition.value.value : ["", ""]),
  set: (val: [any, any]) => {
    condition.value.value = columnType.value === "number" ? val.map((v) => Number(v)) : val;
  },
});

/* ========================
   IN
======================== */
const inValue = computed({
  get: () => (Array.isArray(condition.value.value) ? condition.value.value.join(", ") : ""),
  set: (val: string) => {
    const arr = val
      .split(",")
      .map((v) => v.trim())
      .filter(Boolean);

    condition.value.value = columnType.value === "number" ? arr.map((v) => Number(v)) : arr;
  },
});

/* ========================
   WATCH: COLUMN CHANGE
======================== */
watch(
  () => condition.value.column,
  () => {
    const ops = operators.value;
    if (!ops.includes(condition.value.operator)) {
      condition.value.operator = ops[0];
    }
  },
);

/* ========================
   WATCH: OPERATOR CHANGE
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

/* ========================
   WATCH: PARSE VALUE
======================== */
watch(
  () => condition.value.value,
  (val) => {
    if (columnType.value === "number") {
      if (Array.isArray(val)) {
        const parsed = val.map((v) => Number(v));

        if (JSON.stringify(parsed) !== JSON.stringify(val)) {
          condition.value.value = parsed;
        }
      } else {
        const parsed = val !== "" ? Number(val) : "";

        if (parsed !== val) {
          condition.value.value = parsed;
        }
      }
    }
  },
);
</script>
