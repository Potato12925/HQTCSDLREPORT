<template>
  <div class="mb-6">
    <h3 class="font-semibold text-primary mb-2">WHERE</h3>

    <!-- INPUT ROW -->
    <div class="flex gap-2 mb-2 flex-wrap">

      <!-- COLUMN -->
      <select
        v-model="col"
        class="border px-2 py-1 rounded bg-white text-dark"
      >
        <option disabled value="">Column</option>
        <option
          v-for="c in columns"
          :key="key(c)"
          :value="c"
        >
          {{ c.columnName }} 
        </option>
      </select>

      <!-- OPERATOR -->
      <select
        v-model="op"
        class="border px-2 py-1 rounded bg-white text-dark"
      >
        <option>=</option>
        <option>!=</option>
        <option>></option>
        <option><</option>
        <option>>=</option>
        <option><=</option>
        <option>LIKE</option>
      </select>

      <!-- VALUE -->
      <input
        v-model="val"
        placeholder="value"
        class="border px-2 py-1 rounded bg-white text-dark"
      />

      <!-- ADD BUTTON -->
      <button
        @click="add"
        class="bg-primary text-white px-3 py-1 rounded hover:bg-secondary transition"
      >
        Add
      </button>
    </div>

    <!-- CONDITIONS LIST -->
    <div class="space-y-2">
      <div
        v-for="(c, i) in conditions"
        :key="i"
        class="flex justify-between items-center bg-white p-2 rounded border"
      >
        <span class="text-sm text-dark">
          {{ display(c) }}
        </span>

        <button
          @click="remove(i)"
          class="text-red-500 hover:text-red-700 transition"
        >
          ✕
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import type { QueryState, ColumnRef, Condition, ConditionGroup } from "@/types/queryState";

const props = defineProps<{
  state: QueryState;
  columns: ColumnRef[];
}>();

const col = ref<ColumnRef | "">("");
const op = ref<any>("=");
const val = ref("");

const conditions = ref<Condition[]>([]);

const add = () => {
  if (!col.value) return;

  conditions.value.push({
    column: col.value,
    operator: op.value,
    value: val.value,
  });

  update();
};

const remove = (i: number) => {
  conditions.value.splice(i, 1);
  update();
};

const update = () => {
  const group: ConditionGroup = {
    type: "AND",
    conditions: conditions.value,
  };

  props.state.where = {
    mode: "builder",
    group,
  };
};

const display = (c: Condition) => {
  if (typeof c.column === "string") return c.column;
  return `${c.column.columnName} ${c.operator} ${c.value}`;
};

const key = (c: ColumnRef) => `${c.tableId}-${c.columnId}`;
</script>