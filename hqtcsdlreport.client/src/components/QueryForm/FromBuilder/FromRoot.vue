<template>
  <div v-if="table" class=" p-3">
    <div class="flex items-center gap-2 text-sm">
      <h2 class="text-lg font-semibold">FROM</h2>
      <span class="font-semibold">{{ table.tableName }}</span>

      <input
        v-model="table.alias"
        placeholder="alias"
        class="border border-primary/20 px-2 py-1 rounded bg-light text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 w-24"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import type { QueryTable } from "@/types/queryState";

defineProps<{
  table: QueryTable | null;
}>();
</script>
