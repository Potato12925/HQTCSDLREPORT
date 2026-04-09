<template>
  <div
    class="flex gap-2 items-center text-sm"
    :class="props.lockFirst ? 'opacity-80 pointer-events-none' : ''"
  >
    <!-- ================= RAW ================= -->  
    <template v-if="isRaw(cond)">
      <input
        v-model="cond.sql"
        class="border border-primary/20 px-2 py-1 rounded bg-light text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 flex-1"
      />
    </template>

    <!-- ================= NORMAL ================= -->
    <template v-else-if="isCondition(cond)">
      <!-- COLUMN -->
      <select
        v-if="useColumnPicker && joinColumnOptions.length"
        v-model="leftColumnKey"
        class="border border-primary/20 px-2 py-1 rounded bg-light text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 w-48"
      >
        <option value="" disabled>Select column</option>
        <option v-for="opt in joinColumnOptions" :key="`left-${opt.key}`" :value="opt.key">
          {{ opt.label }}
        </option>
      </select>
      <input
        v-else
        v-model="columnDisplay"
        class="border border-primary/20 px-2 py-1 rounded bg-light text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 w-40"
      />

      <!-- OPERATOR -->
      <select
        v-model="cond.operator"
        class="border border-primary/20 px-2 py-1 rounded bg-light text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
      >
        <option v-for="op in operators" :key="op" :value="op">
          {{ op }}
        </option>
      </select>

      <!-- ================= VALUE ================= -->

      <!-- NO VALUE -->
      <template v-if="noValueOperators.includes(cond.operator)"> </template>

      <!-- BETWEEN -->
      <template v-else-if="cond.operator === 'BETWEEN'">
        <input
          v-model="betweenValue[0]"
          class="border border-primary/20 px-2 py-1 rounded bg-light text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 w-20"
        />
        <span>AND</span>
        <input
          v-model="betweenValue[1]"
          class="border border-primary/20 px-2 py-1 rounded bg-light text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 w-20"
        />
      </template>

      <!-- IN -->
      <template v-else-if="cond.operator === 'IN'">
        <input
          v-model="inValue"
          class="border border-primary/20 px-2 py-1 rounded bg-light text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 w-40"
        />
      </template>

      <!-- DEFAULT -->
      <template v-else>
        <select
          v-if="useColumnPicker && joinColumnOptions.length"
          v-model="rightColumnKey"
          class="border border-primary/20 px-2 py-1 rounded bg-light text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 w-48"
        >
          <option value="" disabled>Select column</option>
          <option v-for="opt in joinColumnOptions" :key="`right-${opt.key}`" :value="opt.key">
            {{ opt.label }}
          </option>
        </select>
        <input
          v-else
          v-model="valueDisplay"
          class="border border-primary/20 px-2 py-1 rounded bg-light text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 w-40"
        />
      </template>
    </template>

    <!-- ================= GROUP ================= -->
    <template v-else>
      <span class="text-gray-400 text-xs">[Group]</span>
    </template>

    <!-- REMOVE -->
    <button v-if="!props.isFirst" @click="$emit('remove')" class="text-red-500 text-xs">✕</button>
  </div>
</template>

<script setup lang="ts">
import { computed, watch } from "vue";
import type {
  Condition,
  RawCondition,
  ConditionGroup,
  Operator,
  ColumnRef,
  Join,
  QueryTable,
} from "@/types/queryState";

const props = defineProps<{
  cond: Condition | RawCondition | ConditionGroup;
  join?: Join;
  tables: QueryTable[];
  isFirst: boolean;
  lockFirst?: boolean;
  useJoinColumnPicker?: boolean;
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

type JoinColumnOption = {
  key: string;
  label: string;
  ref: ColumnRef;
};

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

const useColumnPicker = computed(() => props.useJoinColumnPicker === true && props.isFirst);

const joinColumnOptions = computed<JoinColumnOption[]>(() => {
  if (!props.join) return [];

  const ids = [props.join.fromTableId, props.join.toTableId];
  const dedup = [...new Set(ids)];

  return dedup.flatMap((tableId) => {
    const table = getTable(tableId);
    if (!table) return [];

    const tableName = table.alias || table.tableName;

    return table.columns.map((c) => {
      const col = c.column;
      const key = `${col.tableId}:${col.columnId}`;

      return {
        key,
        label: `${tableName}.${col.columnName}`,
        ref: {
          tableId: col.tableId,
          columnId: col.columnId,
          columnName: col.columnName,
          dataType: col.dataType,
        },
      };
    });
  });
});

const optionByKey = (key: string) => {
  return joinColumnOptions.value.find((o) => o.key === key);
};

const keyFromRef = (col: unknown) => {
  if (!col || typeof col !== "object") return "";
  const maybeRef = col as Partial<ColumnRef>;
  if (typeof maybeRef.tableId !== "number" || typeof maybeRef.columnId !== "number") return "";
  return `${maybeRef.tableId}:${maybeRef.columnId}`;
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

const leftColumnKey = computed({
  get() {
    if (!isCondition(props.cond)) return "";
    return keyFromRef(props.cond.column);
  },
  set(val: string) {
    if (!isCondition(props.cond)) return;
    const picked = optionByKey(val);
    if (!picked) return;
    props.cond.column = { ...picked.ref };
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

const rightColumnKey = computed({
  get() {
    if (!isCondition(props.cond)) return "";
    return keyFromRef(props.cond.value);
  },
  set(val: string) {
    if (!isCondition(props.cond)) return;
    const picked = optionByKey(val);
    if (!picked) return;
    props.cond.value = { ...picked.ref };
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
  },
);

watch(
  [() => useColumnPicker.value, () => joinColumnOptions.value],
  ([enabled, options]) => {
    if (!enabled || !isCondition(props.cond) || options.length === 0) return;
    if (typeof props.cond.column === "string" || !props.cond.column) {
      props.cond.column = { ...options[0].ref };
    }
    if (!props.cond.value || typeof props.cond.value === "string") {
      props.cond.value = { ...options[0].ref };
    }
  },
  { immediate: true },
);
</script>
