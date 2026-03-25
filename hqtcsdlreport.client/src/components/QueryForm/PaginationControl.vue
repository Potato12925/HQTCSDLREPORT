<template>
  <div class="mb-6">
    <h3 class="font-semibold text-primary mb-2">Pagination</h3>

    <div class="flex gap-2">
      <input type="number" v-model.number="state.limit" placeholder="Limit" class="input w-24" />

      <input type="number" v-model.number="state.offset" placeholder="Offset" class="input w-24" />
    </div>
  </div>
</template>

<script setup lang="ts">
import type { QueryState } from "@/types/queryState";

defineProps<{
  state: QueryState;
}>();
</script>
