<template>
  <div class="w-72 bg-white border-r border-primary/30 p-4 overflow-y-auto">
    <h2 class="text-lg font-bold text-primary mb-4">Tables</h2>

    <div v-if="loading">⏳ Loading...</div>

    <ul v-else class="space-y-2">
      <TableItem
        v-for="table in tables"
        :key="table.objectId"
        :table="table"
      />
    </ul>
  </div>
</template>

<script setup>
import TableItem from './TableItem.vue'

defineProps({
  tables: Array,
  loading: Boolean
})
</script>