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


const selectedTablesDict = computed(() => {
  const result = {}

  tables.value.forEach(table => {
    const selectedCols = table.checked
      ? table.columns
      : table.columns.filter(col => col.checked)

    if (selectedCols.length > 0) {
      const columnsDict = {}

      selectedCols.forEach(col => {
        columnsDict[Number(col.columnId)] = {
          columnName: col.columnName,
          dataType: col.dataType,
          columnId: col.columnId
        }
      })

      result[Number(table.objectId)] = {
        tableName: table.tableName,
        objectId: table.objectId,

        columns: columnsDict,

        foreignKeys: table.foreignKeys || []
      }
    }
  })

  return result
})

const addCheckedField = (tables) => {
  return tables.map(table => ({
    ...table,
    checked: false, 
    columns: table.columns.map(col => ({
      ...col,
      checked: false 
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

watch(selectedTablesDict, (val) => {
  console.log("SELECTED:\n", JSON.stringify(val, null, 2))
}, { deep: true })
</script>
