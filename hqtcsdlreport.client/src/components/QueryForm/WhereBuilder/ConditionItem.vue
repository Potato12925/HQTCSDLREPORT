<template>
  <div class="flex gap-2 items-center">
    <select v-model="columnValue" class="input">
      <option disabled value="">Select column</option>
      <option v-for="name in props.columnNames" :key="name" :value="name">{{ name }}</option>
    </select>

    <select v-model="model.operator" class="input">
      <option value="=">=</option>
      <option value="!=">!=</option>
      <option value=">">></option>
      <option value="<"><</option>
      <option value=">=">>=</option>
      <option value="<="><=</option>
      <option value="LIKE">LIKE</option>
    </select>

    <input v-model="model.value" placeholder="value" class="input" />

    <button @click="$emit('remove')" class="text-red-500">✕</button>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import type { Condition } from "@/types/queryState";

defineEmits(["remove"]);

const props = withDefaults(
  defineProps<{
    columnNames?: string[];
  }>(),
  {
    columnNames: () => [],
  },
);

const model = defineModel<Condition>({
  required: true,
});

const columnValue = computed<string>({
  get() {
    return typeof model.value.column === "string"
      ? model.value.column
      : model.value.column.columnName;
  },
  set(val) {
    model.value.column = val;
  },
});
</script>
