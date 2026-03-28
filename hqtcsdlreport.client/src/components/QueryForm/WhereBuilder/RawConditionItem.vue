<template>
  <div class="flex gap-2 items-center">
    <input v-model="model.sql" placeholder="RAW SQL..." class="input w-full" />

    <button @click="$emit('remove')" class="text-red-500">✕</button>
  </div>
</template>

<script setup lang="ts">
import type { RawCondition } from "@/types/queryState";

defineEmits(["remove"]);

const model = defineModel<RawCondition>();
</script>
