<template>
  <div class="flex h-screen">
    <TableTree :tables="tables" :loading="loading" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue'
import { connectDbApi } from "@/api/dataApi"
import TableTree from '@/components/TableInfo/TableTree.vue'

import type {
  TableMetadata,
  ForeignKeyMetadata,
  DatabaseMetadata
} from '@/types/database'

/* ========================
   STATE
======================== */
const tables = ref<TableMetadata[]>([])
const loading = ref<boolean>(false)

const server = ref<string>('')
const database = ref<string>('')

/* ========================
   TYPES cho output
======================== */
interface SelectedColumn {
  columnId: number
  columnName: string
  dataType: string
  conditions: any[]
  selected: boolean
}

interface SelectedTable {
  tableName: string
  objectId: number
  columns: SelectedColumn[]
  foreignKeys: ForeignKeyMetadata[]
}

/* ========================
   COMPUTED
======================== */
const selectedTablesDict = computed<Record<number, SelectedTable>>(() => {
  const result: Record<number, SelectedTable> = {}

  tables.value.forEach((table) => {
    const selectedCols = table.checked
      ? table.columns
      : table.columns.filter(col => col.checked)

    if (selectedCols.length > 0) {
      const columns: SelectedColumn[] = selectedCols.map(col => ({
        columnId: col.columnId,
        columnName: col.columnName,
        dataType: col.dataType,
        conditions: [],
        selected: true
      }))

      result[table.objectId] = {
        tableName: table.tableName,
        objectId: table.objectId,
        columns,
        foreignKeys: table.foreignKeys || []
      }
    }
  })

  return result
})

/* ========================
   API
======================== */
const loadDb = async (): Promise<void> => {
  loading.value = true

  try {
    const res = await connectDbApi({
      server: server.value,
      database: database.value
    })

    const data = res.data as DatabaseMetadata

    // ❌ KHÔNG cần addCheckedField nữa
    tables.value = data.tables || []

    console.log("TABLES:\n", JSON.stringify(tables.value, null, 2))
  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false
  }
}

/* ========================
   LIFECYCLE
======================== */
onMounted(() => {
  server.value = localStorage.getItem('server') || ''
  database.value = localStorage.getItem('database') || ''

  if (server.value && database.value) {
    loadDb()
  }
})

/* ========================
   WATCH
======================== */
watch(
  selectedTablesDict,
  (val: Record<number, SelectedTable>) => {
    console.log("SELECTED:\n", JSON.stringify(val, null, 2))
  },
  { deep: true }
)
</script>