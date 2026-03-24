<template>
  <div class="flex h-screen">
    <TableTree :tables="tables" :loading="loading" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue'
import { connectDbApi } from "@/api/dataApi"
import TableTree from '@/components/TableInfo/TableTree.vue'
import type { QueryState,  } from '@/types/queryState'
import type {
  TableMetadata,
  DatabaseMetadata
} from '@/types/database'

const tables = ref<TableMetadata[]>([])
const loading = ref<boolean>(false)

const server = ref<string>('')
const database = ref<string>('')

const queryState = ref<QueryState>({})

const loadDb = async (): Promise<void> => {
  loading.value = true

  try {
    const res = await connectDbApi({
      server: server.value,
      database: database.value
    })

    const data = res.data as DatabaseMetadata

    tables.value = data.tables || []

    console.log("data:\n", JSON.stringify(data, null, 2))
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

watch(
  tables,
  (newTables: TableMetadata[]) => {
    console.log("Tables updated:\n", JSON.stringify(newTables, null, 2))
  },
  { deep: true }
)
</script>