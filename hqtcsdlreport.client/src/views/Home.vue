<template>
  <div class="flex h-screen">
<!-- SIDEBAR -->
<div class="w-64 bg-white border-r border-primary/30 p-4 overflow-y-auto">
  <h2 class="text-lg font-bold text-primary mb-4">Tables</h2>

  <!-- LOADING -->
  <div v-if="loading">⏳ Loading...</div>

  <!-- TABLE LIST -->
  <ul v-else class="space-y-2">
    <li
      v-for="table in tables"
      :key="table.objectId"
      @click="selectTable(table)"
      class="cursor-pointer px-3 py-2 rounded hover:bg-primary/10"
      :class="{
        'bg-primary text-white': selectedTable?.objectId === table.objectId
      }"
    >
      📦 {{ table.tableName }}
    </li>
  </ul>
</div>

<!-- MAIN CONTENT -->
<div class="flex-1 p-6 overflow-y-auto bg-light">

  <!-- HEADER -->
  <div class="flex items-center justify-between mb-4">
    <h1 class="text-2xl font-bold text-primary">
      {{ selectedTable?.tableName || 'Chọn bảng' }}
    </h1>

    <button
      @click="loadDb"
      class="bg-primary text-white px-4 py-2 rounded hover:bg-secondary"
    >
      Reload
    </button>
  </div>

  <!-- EMPTY -->
  <div v-if="!selectedTable" class="text-gray-500">
    👉 Chọn một bảng bên trái để xem chi tiết
  </div>

  <!-- TABLE DETAIL -->
  <div v-else class="space-y-6">

    <!-- COLUMNS -->
    <div class="bg-white p-4 rounded shadow">
      <h2 class="font-bold text-lg text-primary mb-3">Columns</h2>

      <table class="w-full text-sm border">
        <thead class="bg-primary text-white">
          <tr>
            <th class="p-2 text-left">Column</th>
            <th class="p-2 text-left">Type</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="col in selectedTable.columns"
            :key="col.columnId"
            class="border-t hover:bg-gray-50"
          >
            <td class="p-2">{{ col.columnName }}</td>
            <td class="p-2 text-gray-600">{{ col.dataType }}</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- FOREIGN KEYS -->
    <div
      v-if="selectedTable.foreignKeys?.length"
      class="bg-white p-4 rounded shadow"
    >
      <h2 class="font-bold text-lg text-primary mb-3">Relationships</h2>

      <table class="w-full text-sm border">
        <thead class="bg-secondary text-white">
          <tr>
            <th class="p-2 text-left">Column</th>
            <th class="p-2 text-left">Reference</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="fk in selectedTable.foreignKeys"
            :key="fk.foreignKeyName"
            class="border-t hover:bg-gray-50"
          >
            <td class="p-2">{{ fk.parentColumn }}</td>
            <td class="p-2">
              {{ fk.referencedTable }}.{{ fk.referencedColumn }}
            </td>
          </tr>
        </tbody>
      </table>
    </div>

  </div>

</div>

  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { connectDbApi } from "@/api/dataApi"

const tables = ref([])
const selectedTable = ref(null)
const loading = ref(false)

const server = ref('')
const database = ref('')

// load database
const loadDb = async () => {
  if (!server.value || !database.value) return

  loading.value = true

  try {
    const res = await connectDbApi({
      server: server.value,
      database: database.value
    })

    tables.value = res.data.tables || []

    // auto chọn table đầu tiên
    if (tables.value.length) {
      selectedTable.value = tables.value[0]
    }

  } catch (err) {
    console.error(err)
    alert('Lỗi load database!')
  } finally {
    loading.value = false
  }
}

// chọn table
const selectTable = (table) => {
  selectedTable.value = table
}

// init
onMounted(() => {
  server.value = localStorage.getItem('server') || ''
  database.value = localStorage.getItem('database') || ''

  if (server.value && database.value) {
    loadDb()
  }
})
</script>
