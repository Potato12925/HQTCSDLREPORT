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
      <option value="=">=</option>
      <option value="!=">!=</option>
      <option value=">">></option>
      <option value="<"><</option>
      <option value=">=">>=</option>
      <option value="<="><=</option>
      <option value="LIKE">LIKE</option>
    </select>

    <!-- VALUE -->
    <input v-model="model.value" placeholder="value" class="input" />

    <button @click="$emit('remove')" class="text-red-500">✕</button>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import type { Condition, ColumnRef } from "@/types/queryState";

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
      };
    }
  },
});
</script>
