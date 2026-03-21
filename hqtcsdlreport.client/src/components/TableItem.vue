<template>
  <li>

    <!-- TABLE -->
    <div
      @click="toggle"
      class="cursor-pointer font-semibold text-dark hover:text-primary"
    >
      {{ table.tableName }}
    </div>

    <!-- EXPAND -->
    <div
      v-if="expanded"
      class="ml-4 mt-2 border-l border-gray-200 pl-2"
    >
      <ul class="space-y-1 text-sm text-gray-600">
        <ColumnItem
          v-for="col in table.columns"
          :key="col.columnId"
          :column="col"
        />
      </ul>
    </div>

  </li>
</template>

<script setup>
import { ref } from 'vue'
import ColumnItem from './ColumnItem.vue'

const props = defineProps({
  table: Object
})

const expanded = ref(false)

const toggle = () => {
  expanded.value = !expanded.value
}
</script>