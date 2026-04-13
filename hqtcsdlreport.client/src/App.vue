<template>
  <header class="bg-light shadow-md relative z-10">
    <nav class="mx-auto px-6 py-4 flex items-center justify-between">
      <div class="text-xl font-bold text-primary">HQTCSDL</div>
      <div class="flex items-end gap-4">
        <!-- SERVER -->
        <div class="flex flex-col">
          <label class="text-xs text-dark mb-1">SERVER</label>
          <input
            v-model="server"
            type="text"
            placeholder="Nhập server"
            :disabled="isConnected"
            class="border border-primary bg-white rounded px-3 py-1 focus:outline-none focus:ring-2 focus:ring-primary disabled:opacity-50"
          />
        </div>

        <!-- BUTTON LOAD DB -->
        <button
          v-if="!isConnected"
          @click="getdatabases"
          :disabled="loadingDB"
          class="bg-primary text-white px-4 py-1 rounded hover:bg-secondary transition disabled:opacity-50 h-[34px]"
        >
          {{ loadingDB ? "Đang tải..." : "Tải danh sách" }}
        </button>

        <!-- DATABASE -->
        <div class="flex flex-col">
          <label class="text-xs text-dark mb-1">DATABASE</label>

          <!-- SELECT -->
          <select
            v-if="listDatabase.length"
            v-model="database"
            :disabled="isConnected"
            class="border border-primary bg-white rounded px-3 py-1 focus:outline-none focus:ring-2 focus:ring-primary disabled:opacity-50"
          >
            <option disabled value="">-- Chọn database --</option>
            <option v-for="db in listDatabase" :key="db" :value="db">
              {{ db }}
            </option>
          </select>

          <!-- INPUT fallback -->
          <input
            v-else
            v-model="database"
            type="text"
            placeholder="Nhập database"
            :disabled="isConnected"
            class="border border-primary bg-white rounded px-3 py-1 focus:outline-none focus:ring-2 focus:ring-primary disabled:opacity-50"
          />
        </div>

        <!-- CONNECT -->
        <button
          v-if="!isConnected"
          @click="connect"
          :disabled="loading"
          class="bg-primary text-white px-4 py-1 rounded hover:bg-secondary transition disabled:opacity-50 h-[34px]"
        >
          {{ loading ? "Đang kết nối..." : "Kết nối" }}
        </button>

        <!-- DISCONNECT -->
        <button
          v-else
          @click="disconnect"
          class="bg-secondary text-white px-4 py-1 rounded hover:opacity-90 h-[34px]"
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
import { ref, onMounted } from "vue";
import { testApi, getDatabasesApi } from "@/api/dataApi";

const server = ref("");
const database = ref("");
const listDatabase = ref([]);
const isConnected = ref(false);
const loading = ref(false);
const loadingDB = ref(false);

const getdatabases = async () => {
  if (!server.value) {
    alert("Vui lòng nhập server!");
    return;
  }
  loadingDB.value = true;
  try {
    const res = await getDatabasesApi(server.value);
    listDatabase.value = res.data || [];
  } catch (err) {
    console.error(err);
    alert("Không thể lấy danh sách database!");
  } finally {
    loadingDB.value = false;
  }
};

const connect = async () => {
  if (!server.value || !database.value) {
    alert("Vui lòng nhập đầy đủ thông tin!");
    return;
  }

  loading.value = true;

  try {
    const res = await testApi({
      server: server.value,
      database: database.value,
    });
    console.log(res.data);

    if (res?.data?.message) {
      isConnected.value = true;

      localStorage.setItem("server", server.value);
      localStorage.setItem("database", database.value);
    } else {
      alert("Kết nối thất bại!");
    }
  } catch (err) {
    console.error(err);
    alert("Không thể kết nối server!");
  } finally {
    loading.value = false;
    window.location.reload();
  }
};

const disconnect = () => {
  isConnected.value = false;
  localStorage.removeItem("server");
  localStorage.removeItem("database");

  window.location.reload();
};

onMounted(async () => {
  server.value = localStorage.getItem("server") || "(localdb)\\MSSQLLocalDB";
  database.value = localStorage.getItem("database") || "QLVT_DATHANG";

  const savedServer = localStorage.getItem("server");
  const savedDatabase = localStorage.getItem("database");

  if (savedServer && savedDatabase) {
    try {
      const res = await testApi({
        server: savedServer,
        database: savedDatabase,
      });

      isConnected.value = !!res?.data?.message;
    } catch {
      isConnected.value = false;
    }
  }
});
</script>
