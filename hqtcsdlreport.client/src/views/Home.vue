<template>
  <div class="p-6 space-y-6">

    <!-- HEADER -->
    <div class="flex items-center justify-between">
      <h1 class="text-2xl font-bold text-primary">Database Explorer</h1>

      <button
        @click="loadDb"
        class="bg-primary text-white px-4 py-2 rounded hover:bg-secondary transition"
      >
        Tải dữ liệu
      </button>
    </div>

    <!-- INFO -->
    <div class="text-sm text-dark opacity-70">
      Server: <b>{{ server }}</b> | Database: <b>{{ database }}</b>
    </div>

    <!-- LOADING -->
    <div v-if="loading" class="text-center py-10">
      ⏳ Đang tải dữ liệu...
    </div>

    <!-- EMPTY -->
    <div v-else-if="!tables.length" class="text-center py-10 text-gray-500">
      Chưa có dữ liệu
    </div>

    <!-- TABLE LIST -->
    <div
      v-else
      class="grid md:grid-cols-2 lg:grid-cols-3 gap-4"
    >
      <div
        v-for="table in tables"
        :key="table.objectId"
        class="bg-white border border-primary/30 rounded-xl p-4 shadow hover:shadow-lg transition"
      >
        <!-- TABLE NAME -->
        <h2 class="text-lg font-bold text-primary mb-3">
          {{ table.tableName }}
        </h2>

        <!-- COLUMNS -->
        <div class="mb-3">
          <h3 class="font-semibold text-dark mb-1">Columns:</h3>
          <ul class="ml-4 list-disc text-sm">
            <li v-for="col in table.columns" :key="col.columnId">
              {{ col.columnName }} 
              <span class="text-gray-500">({{ col.dataType }})</span>
            </li>
          </ul>
        </div>

        <!-- FOREIGN KEYS -->
        <div v-if="table.foreignKeys?.length">
          <h3 class="font-semibold text-dark mb-1">Relations:</h3>
          <ul class="ml-4 list-disc text-sm">
            <li v-for="fk in table.foreignKeys" :key="fk.foreignKeyName">
              {{ fk.parentColumn }}
              →
              <span class="text-secondary">
                {{ fk.referencedTable }}.{{ fk.referencedColumn }}
              </span>
            </li>
          </ul>
        </div>

      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { connectDbApi } from "@/api/dataApi"

const tables = ref([])
const loading = ref(false)

const server = ref('')
const database = ref('')

// 🔥 load database
const loadDb = async () => {
  if (!server.value || !database.value) {
    alert('Thiếu thông tin kết nối!')
    return
  }

  loading.value = true

  try {
    const res = await connectDbApi({
      server: server.value,
      database: database.value
    })

    console.log(res.data)

    tables.value = res.data.tables || []

  } catch (err) {
    console.error(err)
    alert('Không load được database!')
  } finally {
    loading.value = false
  }
}

// 🔥 auto load khi vào trang
onMounted(() => {
  server.value = localStorage.getItem('server') || ''
  database.value = localStorage.getItem('database') || ''

  if (server.value && database.value) {
    loadDb()
  }
})
</script>

