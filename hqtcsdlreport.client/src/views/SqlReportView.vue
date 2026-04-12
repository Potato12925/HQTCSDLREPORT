<template>
  <main class="min-h-screen bg-light text-dark p-6">
    <div class="max-w-7xl mx-auto space-y-6">
      <!-- Header -->
      <div class="bg-white border border-primary/20 rounded-2xl p-5 shadow-sm">
        <h1 class="text-2xl font-semibold text-primary">SQL Report</h1>
        <p class="text-sm text-dark/70 mt-1">Server: {{ server }} | Database: {{ database }}</p>
      </div>

      <!-- SQL Preview -->
      <div class="bg-white border border-primary/20 rounded-2xl p-5 shadow-sm">
        <h2 class="text-sm font-semibold text-dark/80">SQL</h2>
        <pre class="mt-3 bg-light p-4 rounded-xl text-sm overflow-auto"
          >{{ sql }}
        </pre>
      </div>

      <!-- Loading -->
      <div
        v-if="preparing"
        class="bg-white border border-primary/20 rounded-2xl p-5 shadow-sm text-sm"
      >
        Preparing report...
      </div>

      <!-- Error -->
      <div
        v-else-if="errorMessage"
        class="bg-red-50 border border-red-200 text-red-600 rounded-2xl p-5 shadow-sm text-sm"
      >
        {{ errorMessage }}
      </div>

      <!-- Viewer -->
      <div v-else class="bg-white border border-primary/20 rounded-2xl p-5 shadow-sm">
        <div
          ref="viewerHost"
          class="w-full overflow-hidden"
          :style="{ height: `${viewerHeight}px`, minHeight: `${MIN_VIEWER_HEIGHT}px` }"
        ></div>
      </div>
    </div>
  </main>
</template>

<script setup lang="ts">
import "devexpress-reporting/dist/css/dx-reporting-skeleton-screen.css";
import "@devexpress/analytics-core/dist/css/dx-analytics.common.css";
import "devexpress-reporting/dist/css/dx-webdocumentviewer.css";
import "devextreme-dist/css/dx.light.css";

import { nextTick, onBeforeUnmount, onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import $ from "jquery";
import ko from "knockout";
import { prepareReportApi } from "@/api/dataApi";
import type { SqlReportPayload } from "@/types/sqlReport";

type PrepareReportResponse = {
  data?: {
    reportUrl?: string;
    RowCount?: string;
    Columns?: string[];
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
const reportUrl = ref("");
const reportTitle = ref("");
const reportParameters = ref<Array<{ columnName: string; value: string }>>([]);
const reportGroupOrder = ref<string[]>([]);
const preparing = ref(false);
const errorMessage = ref("");
const viewerHost = ref<HTMLElement | null>(null);
const MIN_VIEWER_HEIGHT = 560;
const viewerHeight = ref(MIN_VIEWER_HEIGHT);

let viewerInstance: any = null;
let assetsLoaded = false;

function updateViewerHeight() {
  if (!viewerHost.value) return;

  const viewportBottomPadding = 24;
  const hostTop = viewerHost.value.getBoundingClientRect().top;
  const availableHeight = Math.floor(window.innerHeight - hostTop - viewportBottomPadding);
  viewerHeight.value = Math.max(MIN_VIEWER_HEIGHT, availableHeight);
}

type DevExpressWindow = Window & {
  DevExpress?: {
    Reporting?: {
      Viewer?: {
        DxReportViewer?: new (element: HTMLElement, options: any) => any;
      };
    };
  };
};

function getSingleQueryValue(value: unknown): string {
  if (Array.isArray(value)) return String(value[0] || "");
  return String(value || "");
}

function loadPayloadFromSession() {
  const id = getSingleQueryValue(route.query.id);
  if (!id) {
    errorMessage.value = "Missing report id in URL.";
    return false;
  }

  const storageKey = `report_${id}`;
  const rawPayload = sessionStorage.getItem(storageKey);

  if (!rawPayload) {
    errorMessage.value = "Report payload not found in sessionStorage.";
    return false;
  }

  let payload: SqlReportPayload;
  try {
    payload = JSON.parse(rawPayload) as SqlReportPayload;
  } catch {
    errorMessage.value = "Invalid report payload format.";
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

  reportTitle.value = String(payload.title || "").trim();
  reportParameters.value = Array.isArray(payload.parameters)
    ? payload.parameters
        .map((item) => ({
          columnName: String(item?.columnName || "").trim(),
          value: String(item?.value || ""),
        }))
        .filter((item) => item.columnName.length > 0)
    : [];
  reportGroupOrder.value = Array.isArray(payload.groupOrder)
    ? payload.groupOrder
        .map((item) => String(item?.columnName || "").trim())
        .filter((name) => name.length > 0)
    : [];

  if (!server.value || !database.value || !sql.value) {
    errorMessage.value = "Missing server, database or sql in report payload.";
    return false;
  }

  return true;
}

async function ensureViewerAssets() {
  if (assetsLoaded) return;

  (window as any).$ = $;
  (window as any).jQuery = $;
  (window as any).ko = ko;

  await import("devextreme-dist/js/dx.all");
  await import("@devexpress/analytics-core/dist/js/dx-analytics-core.min");
  await import("devexpress-reporting/dist/js/dx-webdocumentviewer.min");

  const dxWindow = window as DevExpressWindow;
  if (!dxWindow.DevExpress?.Reporting?.Viewer?.DxReportViewer) {
    throw new Error("DevExpress viewer assets loaded but DxReportViewer is unavailable.");
  }

  assetsLoaded = true;
}

async function initViewer() {
  if (!viewerHost.value || !reportUrl.value) return;

  await ensureViewerAssets();
  disposeViewer();

  const host = `${window.location.origin}/`;
  const dxWindow = window as DevExpressWindow;
  const DxReportViewer = dxWindow.DevExpress?.Reporting?.Viewer?.DxReportViewer;
  if (!DxReportViewer) {
    throw new Error("DxReportViewer is not available.");
  }
  viewerInstance = new DxReportViewer(viewerHost.value, {
    reportUrl: reportUrl.value,
    requestOptions: {
      host,
      invokeAction: "DXXRDV",
    },
  });

  if (!viewerInstance) {
    throw new Error("Viewer instance was not created.");
  }

  viewerInstance.render();
}

function disposeViewer() {
  if (viewerInstance?.dispose) {
    viewerInstance.dispose();
  }
  viewerInstance = null;

  if (viewerHost.value) {
    try {
      ko.cleanNode(viewerHost.value);
    } catch {
      // ignore knockout cleanup failures
    }
    viewerHost.value.innerHTML = "";
  }
}

async function prepareAndLoadReport() {
  const hasPayload = loadPayloadFromSession();
  if (!hasPayload) return;

  preparing.value = true;
  errorMessage.value = "";

  try {
    const normalizedSql = sql.value.replace(/\r?\n/g, " ").trim();
    const response = (await prepareReportApi({
      server: server.value,
      database: database.value,
      sql: normalizedSql,
      title: reportTitle.value,
      parameters: reportParameters.value,
      groupOrder: reportGroupOrder.value,
    })) as PrepareReportResponse;

    reportUrl.value = response?.data?.reportUrl || "";
    if (!reportUrl.value) {
      throw new Error("Report URL is missing in API response.");
    }

    // Render the viewer host first; otherwise viewerHost ref is null while preparing is true.
    preparing.value = false;
    await nextTick();
    updateViewerHeight();
    await initViewer();
  } catch (err: unknown) {
    const apiError = err as ApiErrorShape;
    const message =
      apiError?.response?.data?.message ||
      (err instanceof Error ? err.message : "Prepare report failed.");
    errorMessage.value = message;
  } finally {
    preparing.value = false;
  }
}

onMounted(() => {
  window.addEventListener("resize", updateViewerHeight);
  prepareAndLoadReport();
});

onBeforeUnmount(() => {
  window.removeEventListener("resize", updateViewerHeight);
  disposeViewer();
});
</script>
