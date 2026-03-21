<template>
  <div class="flex h-screen">
  <TableTree :tables="tables" :loading="loading" />

  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { connectDbApi } from "@/api/dataApi"
import TableTree from '@/components/TableTree.vue'
const tables = ref([])
const loading = ref(false)

// state expand
const expandedTables = ref([])
const expandedColumns = ref([])
const expandedKeys = ref([])

const server = ref('')
const database = ref('')

// toggle functions
const toggleTable = (id) => {
  if (expandedTables.value.includes(id)) {
    expandedTables.value = expandedTables.value.filter(i => i !== id)
  } else {
    expandedTables.value.push(id)
  }
}

const toggleColumns = (id) => {
  if (expandedColumns.value.includes(id)) {
    expandedColumns.value = expandedColumns.value.filter(i => i !== id)
  } else {
    expandedColumns.value.push(id)
  }
}

const toggleKeys = (id) => {
  if (expandedKeys.value.includes(id)) {
    expandedKeys.value = expandedKeys.value.filter(i => i !== id)
  } else {
    expandedKeys.value.push(id)
  }
}

// load DB
const loadDb = async () => {
  loading.value = true

  try {
    const res = await connectDbApi({
      server: server.value,
      database: database.value
    })

    tables.value = res.data.tables || []

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
</script>
