<template>
  <div v-if="table" class="border rounded-xl p-3 mb-3">
    <div class="flex items-center gap-2 text-sm">
      <h2 class="text-lg font-semibold text-primary">FROM</h2>
      <span class="font-semibold">{{ table.tableName }}</span>

      <input
        v-model="table.alias"
        placeholder="alias"
        class="border rounded px-2 py-1 text-sm w-24"
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
