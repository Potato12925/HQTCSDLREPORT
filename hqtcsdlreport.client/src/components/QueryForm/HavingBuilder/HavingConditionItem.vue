<template>
  <div class="flex gap-2 items-center flex-wrap">
    <select v-model="conditionType" class="input">
      <option value="group_column">Group Column</option>
      <option value="aggregate">Aggregate</option>
      <option value="alias">Alias</option>
    </select>

    <select v-if="model.type === 'aggregate'" v-model="model.fn" class="input">
      <option v-for="fn in aggregateFunctions" :key="fn" :value="fn">{{ fn }}</option>
    </select>

    <select
      v-if="model.type === 'aggregate' || model.type === 'group_column'"
      v-model="selectedColumn"
      class="input"
    >
      <option disabled value="">Select column</option>

      <option v-for="col in columns" :key="`${col.tableId}-${col.columnId}`" :value="col">
        {{ col.label }}
      </option>
    </select>

    <select v-if="model.type === 'alias'" v-model="model.alias" class="input">
      <option disabled value="">Select alias</option>
      <option v-for="alias in aliases" :key="alias.alias" :value="alias.alias">
        {{ alias.label }}
      </option>
    </select>

    <select v-model="model.operator" class="input">
      <option v-for="op in operators" :key="op" :value="op">
        {{ op }}
      </option>
    </select>

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
import type { ColumnDataType, ColumnRef, HavingCondition, Operator } from "@/types/queryState";
import type { SelectableAlias, SelectableColumn } from "./HavingBuilder.vue";

const props = defineProps<{
  columns: SelectableColumn[];
  aliases: SelectableAlias[];
}>();

defineEmits(["remove"]);

const model = defineModel<HavingCondition>({
  required: true,
});

const aggregateFunctions: Array<"COUNT" | "SUM" | "AVG" | "MIN" | "MAX"> = [
  "COUNT",
  "SUM",
  "AVG",
  "MIN",
  "MAX",
];

const conditionType = computed<HavingCondition["type"]>({
  get() {
    return model.value.type;
  },
  set(nextType) {
    if (nextType === model.value.type) return;

    if (nextType === "aggregate") {
      model.value = {
        type: "aggregate",
        fn: "COUNT",
        column: getDefaultColumn(),
        operator: "=",
        value: "",
      };
      return;
    }

    if (nextType === "group_column") {
      model.value = {
        type: "group_column",
        column: getDefaultColumn(),
        operator: "=",
        value: "",
      };
      return;
    }

    model.value = {
      type: "alias",
      alias: getDefaultAlias(),
      operator: "=",
      value: "",
    };
  },
});

const selectedColumn = computed<SelectableColumn | null>({
  get() {
    if (model.value.type === "alias") return null;

    return (
      props.columns.find(
        (c) =>
          c.columnId === (model.value as any).column.columnId &&
          c.tableId === (model.value as any).column.tableId,
      ) ?? null
    );
  },
  set(val) {
    if (!val || model.value.type === "alias") return;

    model.value.column = {
      tableId: val.tableId,
      columnId: val.columnId,
      columnName: val.columnName,
      dataType: val.dataType,
    };
  },
});

const columnType = computed<ColumnDataType>(() => {
  if (model.value.type === "alias") {
    return props.aliases.find((a) => a.alias === (model.value as any).alias)?.dataType ?? "string";
  }

  if (model.value.type === "aggregate") {
    if (model.value.fn === "COUNT" || model.value.fn === "SUM" || model.value.fn === "AVG") {
      return "number";
    }
  }

  return selectedColumn.value?.dataType ?? "string";
});

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

watch(
  () => model.value,
  (current) => {
    if (current.type === "alias") {
      const alias = current.alias?.trim() ?? "";
      if (!alias) current.alias = getDefaultAlias();
      return;
    }

    if (!current.column || typeof current.column !== "object") {
      current.column = getDefaultColumn();
    }
  },
  { deep: true, immediate: true },
);

function getDefaultColumn(): ColumnRef {
  const firstColumn = props.columns[0];

  if (!firstColumn) {
    return {
      tableId: 0,
      columnId: 0,
      columnName: "",
      dataType: "string",
    };
  }

  return {
    tableId: firstColumn.tableId,
    columnId: firstColumn.columnId,
    columnName: firstColumn.columnName,
    dataType: firstColumn.dataType,
  };
}

function getDefaultAlias() {
  return props.aliases[0]?.alias ?? "";
}

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
