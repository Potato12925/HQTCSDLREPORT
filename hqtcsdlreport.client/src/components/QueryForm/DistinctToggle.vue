<template>
  <div class="mb-4 flex items-center gap-2">
    <input type="checkbox" v-model="state.distinct" />
    <span class="font-medium text-dark">DISTINCT</span>
  </div>
</template>

<script setup lang="ts">
import type { QueryState } from "@/types/queryState";

defineProps<{
  state: QueryState;
}>();
</script>
