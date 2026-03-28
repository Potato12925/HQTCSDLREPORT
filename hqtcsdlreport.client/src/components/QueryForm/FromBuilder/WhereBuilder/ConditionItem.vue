<template>
  <div class="flex gap-2 items-center">
    <input v-model="model.column" placeholder="column" class="input" />

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

    <button @click="$emit('remove')" class="text-red-500">
      ✕
    </button>
  </div>
</template>

<script setup lang="ts">
import type { Condition } from "@/types/queryState";

defineEmits(["remove"]);

const model = defineModel<Condition>();
</script>