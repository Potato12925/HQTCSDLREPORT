<template>
  <div class="flex h-screen">
  <TableTree :tables="tables" :loading="loading" />

  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import { connectDbApi } from "@/api/dataApi"
import TableTree from '@/components/TableTree.vue'
const tables = ref([])
const loading = ref(false)

const server = ref('')
const database = ref('')


const selectedData = computed(() => {
  const selectedTables = []
  const selectedColumns = []

  tables.value.forEach(table => {
    if (table.checked) {
      selectedTables.push(table)
    }

    table.columns.forEach(col => {
      if (col.checked) {
        selectedColumns.push(col)
      }
    })
  })

  return {
    tables: selectedTables,
    columns: selectedColumns
  }
})

const addCheckedField = (tables) => {
  return tables.map(table => ({
    ...table,
    checked: false, // table
    columns: table.columns.map(col => ({
      ...col,
      checked: false // column
    }))
  }))
}

const loadDb = async () => {
  loading.value = true

  try {
    const res = await connectDbApi({
      server: server.value,
      database: database.value
    })

    tables.value = addCheckedField(res.data.tables || [])

  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  server.value = localStorage.getItem('server') || ''
  database.value = localStorage.getItem('database') || ''

  if (server.value && database.value) {
    loadDb()
  }
})

watch(selectedData, (val) => {
  console.log("SELECTED:", val)
}, { deep: true })
</script>
