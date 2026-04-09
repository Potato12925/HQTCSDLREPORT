<template>
  <main class="min-h-screen bg-light p-6 text-dark">
    <div class="mx-auto max-w-6xl space-y-4">
      <div class="rounded-xl border border-primary/20 bg-white p-4 shadow-sm">
        <h1 class="text-xl font-semibold text-primary">Execute SQL</h1>
        <p class="mt-1 text-sm text-dark/70">Server: {{ server }} | Database: {{ database }}</p>
      </div>

      <div class="rounded-xl border border-primary/20 bg-white p-4 shadow-sm">
        <h2 class="text-sm font-semibold text-dark/80">SQL</h2>
        <pre class="mt-2 overflow-auto rounded-lg bg-light p-3 text-sm">{{ sql }}</pre>
      </div>

      <div v-if="loading" class="rounded-xl border border-primary/20 bg-white p-4 shadow-sm">
        Executing...
      </div>

      <div v-else-if="errorMessage" class="rounded-xl border border-red-200 bg-red-50 p-4 text-red-700 shadow-sm">
        {{ errorMessage }}
      </div>

      <div v-else class="rounded-xl border border-primary/20 bg-white p-4 shadow-sm">
        <div class="mb-2 text-sm text-dark/70">Rows: {{ rows.length }}</div>
        <div class="overflow-auto">
          <table class="min-w-full border-collapse text-sm">
            <thead>
              <tr>
                <th
                  v-for="col in columns"
                  :key="col"
                  class="border-b border-primary/20 px-3 py-2 text-left font-semibold"
                >
                  {{ col }}
                </th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(row, index) in rows" :key="index">
                <td
                  v-for="col in columns"
                  :key="`${index}-${col}`"
                  class="border-b border-primary/10 px-3 py-2 align-top"
                >
                  {{ formatCell(row[col]) }}
                </td>
              </tr>
              <tr v-if="rows.length === 0">
                <td class="px-3 py-3 text-dark/60" :colspan="Math.max(columns.length, 1)">No rows returned.</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </main>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import { executeSqlApi } from "@/api/dataApi";

type SqlExecutePayload = {
  sql: string;
  server?: string;
  database?: string;
};

type SqlExecuteResponse = {
  data?: {
    columns?: string[];
    rows?: Record<string, unknown>[];
  };
};

type ApiErrorShape = {
  response?: {
    data?: {
      message?: string;
    };
  };
};

const route = useRoute();

const server = ref("");
const database = ref("");
const sql = ref("");

const loading = ref(false);
const errorMessage = ref("");
const columns = ref<string[]>([]);
const rows = ref<Record<string, unknown>[]>([]);

function formatCell(value: unknown): string {
  if (value === null || value === undefined) return "NULL";
  if (typeof value === "object") return JSON.stringify(value);
  return String(value);
}

function getSingleQueryValue(value: unknown): string {
  if (Array.isArray(value)) return String(value[0] || "");
  return String(value || "");
}

function loadPayloadFromSession() {
  const id = getSingleQueryValue(route.query.id);
  if (!id) {
    errorMessage.value = "Missing execute id in URL.";
    alert("Missing execute id in URL.");
    console.error("SqlExecuteView: missing query id.");
    return false;
  }

  const storageKey = `sql_${id}`;
  const rawPayload = sessionStorage.getItem(storageKey);

  if (!rawPayload) {
    errorMessage.value = "Execute payload not found in sessionStorage.";
    alert("Execute payload not found. Please open Execute tab again.");
    console.error(`SqlExecuteView: session payload not found for key "${storageKey}".`);
    return false;
  }

  let payload: SqlExecutePayload;
  try {
    payload = JSON.parse(rawPayload) as SqlExecutePayload;
  } catch (error) {
    errorMessage.value = "Invalid execute payload format.";
    alert("Invalid execute payload. Please open Execute tab again.");
    console.error("SqlExecuteView: failed to parse payload.", error);
    return false;
  }

  server.value = String(payload.server || "");
  database.value = String(payload.database || "");

  const encodedSql = String(payload.sql || "");
  try {
    sql.value = decodeURIComponent(encodedSql);
  } catch {
    sql.value = encodedSql;
  }

  if (!server.value || !database.value || !sql.value) {
    errorMessage.value = "Missing server, database or sql in execute payload.";
    alert("Missing server/database/sql data in payload.");
    console.error("SqlExecuteView: payload missing required fields.", payload);
    return false;
  }

  return true;
}

async function executeSql() {
  const hasPayload = loadPayloadFromSession();
  if (!hasPayload) return;

  loading.value = true;
  errorMessage.value = "";

  try {
    const normalizedSql = sql.value.replace(/\r?\n/g, " ").trim();

    const res = (await executeSqlApi({
      server: server.value,
      database: database.value,
      sql: normalizedSql,
    })) as SqlExecuteResponse;

    columns.value = res?.data?.columns || [];
    rows.value = res?.data?.rows || [];
  } catch (err: unknown) {
    const apiError = err as ApiErrorShape;
    errorMessage.value = apiError?.response?.data?.message || "Execute failed.";
  } finally {
    loading.value = false;
  }
}

onMounted(() => {
  executeSql();
});
</script>
