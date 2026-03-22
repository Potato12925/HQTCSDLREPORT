<template>
  <li>

    <!-- TABLE -->
    <div class="flex items-center gap-2">
      <input
        type="checkbox"
        v-model="table.checked"
      />

      <div
        @click="toggle"
        class="cursor-pointer font-semibold text-dark hover:text-primary"
      >
        {{ table.tableName }}
      </div>
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
import { ref, watch } from 'vue'
import ColumnItem from './ColumnItem.vue'

const props = defineProps({
  table: Object
})

const expanded = ref(false)

const toggle = () => {
  expanded.value = !expanded.value
}

watch(
  () => props.table.checked,
  (newVal) => {
    if (newVal === true) {
      props.table.columns.forEach(col => {
        col.checked = true
      })
    }
    else {
      const allChecked = props.table.columns.every(col => col.checked)

      if (allChecked) {
        props.table.columns.forEach(col => {
          col.checked = false
        })
      }
    }
  }
)

watch(
  () => props.table.columns.map(col => col.checked),
  (newValues) => {
    const allChecked = newValues.every(v => v === true)
    props.table.checked = allChecked
  },
  { deep: true }
)
</script>