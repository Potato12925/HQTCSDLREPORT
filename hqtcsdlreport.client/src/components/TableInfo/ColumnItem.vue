<template>
  <li class="flex items-center gap-2 hover:bg-primary/10 px-2 py-1 rounded">

    <input
      type="checkbox"
      v-model="column.checked"
    />

    <span>
      {{ column.columnName }} ({{ column.dataType }})
    </span>

  </li>
</template>

<script setup lang="ts">
import type { ColumnMetadata } from '@/types/database'


defineProps<{
  column: ColumnMetadata
}>()
</script>