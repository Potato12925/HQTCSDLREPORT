<template>
  <div class="flex gap-2 mb-1 items-center text-sm">
    <!-- ================= RAW ================= -->
    <template v-if="isRaw(cond)">
      <input v-model="cond.sql" class="border px-2 py-1 flex-1" />
    </template>

    <!-- ================= NORMAL ================= -->
    <template v-else-if="isCondition(cond)">
      <!-- COLUMN -->
      <input v-model="columnDisplay" class="border px-2 py-1 w-40" />

      <!-- OPERATOR -->
      <select v-model="cond.operator" class="border px-2 py-1">
        <option v-for="op in operators" :key="op" :value="op">
          {{ op }}
        </option>
      </select>

      <!-- ================= VALUE ================= -->

      <!-- NO VALUE -->
      <template v-if="noValueOperators.includes(cond.operator)">
      </template>

      <!-- BETWEEN -->
      <template v-else-if="cond.operator === 'BETWEEN'">
        <input v-model="betweenValue[0]" class="border px-2 py-1 w-20" />
        <span>AND</span>
        <input v-model="betweenValue[1]" class="border px-2 py-1 w-20" />
      </template>

      <!-- IN -->
      <template v-else-if="cond.operator === 'IN'">
        <input v-model="inValue" class="border px-2 py-1 w-40" />
      </template>

      <!-- DEFAULT -->
      <template v-else>
        <input v-model="valueDisplay" class="border px-2 py-1 w-40" />
      </template>
    </template>

    <!-- ================= GROUP ================= -->
    <template v-else>
      <span class="text-gray-400 text-xs">[Group]</span>
    </template>

    <!-- REMOVE -->
    <button @click="$emit('remove')" class="text-red-500 text-xs">
      ✕
    </button>
  </div>
</template>

<script setup lang="ts">
import { computed, watch } from "vue";
import type {
  Condition,
  RawCondition,
  ConditionGroup,
  Operator,
  QueryTable,
} from "@/types/queryState";

const props = defineProps<{
  cond: Condition | RawCondition | ConditionGroup;
  tables: QueryTable[];
}>();

defineEmits(["remove"]);

// ================= TYPE GUARD =================

function isRaw(cond: any): cond is RawCondition {
  return cond?.type === "raw";
}

function isGroup(cond: any): cond is ConditionGroup {
  return cond?.type === "AND" || cond?.type === "OR";
}

function isCondition(cond: any): cond is Condition {
  return !isRaw(cond) && !isGroup(cond);
}

// ================= OPERATORS =================

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

// ================= HELPERS =================

const getTable = (tableId: number) => {
  return props.tables.find((t) => t.id === tableId);
};

const formatColumn = (col: any) => {
  if (!col) return "";

  const table = getTable(col.tableId);

  if (!table) return col.columnName;

  const name = table.alias || table.tableName;

  return `${name}.${col.columnName}`;
};

// ================= COLUMN DISPLAY =================

const columnDisplay = computed({
  get() {
    if (!isCondition(props.cond)) return "";

    const col = props.cond.column;

    if (typeof col === "string") return col;

    return formatColumn(col);
  },
  set(val: string) {
    if (!isCondition(props.cond)) return;

    if (typeof props.cond.column === "object") {
      const parts = val.split(".");
      props.cond.column.columnName = parts.pop() || "";
    } else {
      props.cond.column = val;
    }
  },
});

// ================= VALUE DISPLAY =================

const valueDisplay = computed({
  get() {
    if (!isCondition(props.cond)) return "";

    const val = props.cond.value;

    if (typeof val === "object" && val !== null) {
      return formatColumn(val);
    }

    return val ?? "";
  },
  set(val: string) {
    if (!isCondition(props.cond)) return;

    if (typeof props.cond.value === "object" && props.cond.value !== null) {
      const parts = val.split(".");
      props.cond.value.columnName = parts.pop() || "";
    } else {
      props.cond.value = val;
    }
  },
});

// ================= BETWEEN =================

const betweenValue = computed({
  get() {
    if (!isCondition(props.cond)) return ["", ""];

    if (!Array.isArray(props.cond.value)) {
      props.cond.value = ["", ""];
    }

    return props.cond.value;
  },
  set(val) {
    if (isCondition(props.cond)) {
      props.cond.value = val;
    }
  },
});

// ================= IN =================

const inValue = computed({
  get() {
    if (!isCondition(props.cond)) return "";

    if (Array.isArray(props.cond.value)) {
      return props.cond.value.join(",");
    }
    return "";
  },
  set(val: string) {
    if (isCondition(props.cond)) {
      props.cond.value = val.split(",").map((v) => v.trim());
    }
  },
});

// ================= WATCH =================

watch(
  () => (isCondition(props.cond) ? props.cond.operator : undefined),
  (op) => {
    if (!isCondition(props.cond)) return;
    if (!op) return;

    if (props.cond.value == null) {
      if (["IS NULL", "IS NOT NULL"].includes(op)) {
        props.cond.value = null;
      } else if (op === "BETWEEN") {
        props.cond.value = ["", ""];
      } else if (op === "IN") {
        props.cond.value = [];
      } else {
        props.cond.value = "";
      }
    }
  }
);
</script>