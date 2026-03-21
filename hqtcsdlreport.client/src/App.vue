<template>

  <header class="bg-light shadow-md relative z-10">
    <nav class="mx-auto px-6 py-4 flex items-center justify-between">

      <div class="text-xl font-bold text-primary">
        HQTCSDL
      </div>

      <div class="flex items-center gap-3">
        <input
          v-model="server"
          type="text"
          placeholder="Server"
          :disabled="isConnected"
          class="border border-primary bg-white rounded px-3 py-1
                 focus:outline-none focus:ring-2 focus:ring-primary
                 disabled:opacity-50"
        />

        <input
          v-model="database"
          type="text"
          placeholder="Database"
          :disabled="isConnected"
          class="border border-primary bg-white rounded px-3 py-1
                 focus:outline-none focus:ring-2 focus:ring-primary
                 disabled:opacity-50"
        />

        <button
          v-if="!isConnected"
          @click="connect"
          :disabled="loading"
          class="bg-primary text-white px-4 py-1 rounded
                 hover:bg-secondary transition disabled:opacity-50"
        >
          {{ loading ? 'Đang kết nối...' : 'Kết nối' }}
        </button>

        <!-- DISCONNECT BUTTON -->
        <button
          v-else
          @click="disconnect"
          class="bg-secondary text-white px-4 py-1 rounded hover:opacity-90"
        >
          Ngắt kết nối
        </button>
      </div>

    </nav>
  </header>

  <!-- MAIN -->
  <main
    class="bg-light min-h-screen text-dark transition"
    :class="{ 'blur-sm pointer-events-none select-none': !isConnected }"
  >
    <router-view />
  </main>

  <!-- OVERLAY -->
  <div
    v-if="!isConnected"
    class="fixed inset-0 bg-black/20 backdrop-blur-[2px] flex items-center justify-center"
  >
    <div class="bg-white px-6 py-4 rounded shadow text-dark">
      Vui lòng nhập Server & Database để tiếp tục
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { testApi } from "@/api/dataApi"

const server = ref('')
const database = ref('')
const isConnected = ref(false)
const loading = ref(false)

const connect = async () => {
  if (!server.value || !database.value) {
    alert('Vui lòng nhập đầy đủ thông tin!')
    return
  }

  loading.value = true

  try {
    const res = await testApi({
      server: server.value,
      database: database.value
    })
    console.log(res.data)

    if (res?.data?.message) {
      isConnected.value = true

      localStorage.setItem('server', server.value)
      localStorage.setItem('database', database.value)
    } else {
      alert('Kết nối thất bại!')
    }

  } catch (err) {
    console.error(err)
    alert('Không thể kết nối server!')
  } finally {
    loading.value = false
    window.location.reload()
  }
}

const disconnect = () => {
  isConnected.value = false
  localStorage.removeItem('server')
  localStorage.removeItem('database')

  window.location.reload()
}

onMounted(async () => {
  server.value = localStorage.getItem('server') || '(localdb)\\MSSQLLocalDB'
  database.value = localStorage.getItem('database') || 'QLVT_DATHANG'

  const savedServer = localStorage.getItem('server')
  const savedDatabase = localStorage.getItem('database')

  if (savedServer && savedDatabase) {
    try {
      const res = await testApi({
        server: savedServer,
        database: savedDatabase
      })

      isConnected.value = !!res?.data?.message

    } catch {
      isConnected.value = false
    }
  }
})
</script>

