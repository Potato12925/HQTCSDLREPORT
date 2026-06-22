<template>
  <label
    class="mb-4 flex items-center justify-center w-[150px] gap-2 cursor-pointer p-4 rounded-xl bg-white shadow-sm border border-primary/10"
  >
    <input type="checkbox" v-model="state.distinct" />
    <span class="font-medium text-dark">DISTINCT</span>
  </label>
</template>

<script setup lang="ts">
import type { QueryState } from "@/types/queryState";

defineProps<{
  state: QueryState;
}>();
</script>
