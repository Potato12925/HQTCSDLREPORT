<template>
  <header class="bg-white shadow-md">
    <nav class=" mx-auto px-6 py-4 flex items-center justify-between">

      <!-- Logo -->
      <div class="text-xl font-bold text-blue-600">
        HQTCSDL
      </div>

      <!-- Inputs -->
      <div class="flex items-center gap-3">
        <input
          v-model="server"
          type="text"
          placeholder="Server"
          class="border rounded px-3 py-1 focus:outline-none focus:ring-2 focus:ring-blue-400"
        />

        <input
          v-model="database"
          type="text"
          placeholder="Database"
          class="border rounded px-3 py-1 focus:outline-none focus:ring-2 focus:ring-blue-400"
        />
        <button
          @click="connect"
          class="bg-blue-600 text-white px-4 py-1 rounded hover:bg-blue-700 transition"
        >
          Kết nối
        </button>
      </div>
      
    </nav>
  </header>
    
  <main>
    <router-view />
  </main>
</template>

<script setup>
import { ref } from 'vue'

const server = ref('')
const database = ref('')
</script>