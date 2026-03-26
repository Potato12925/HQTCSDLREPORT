<template>
  <div class="mb-6">
    <h3 class="font-semibold text-primary mb-2">WHERE</h3>

    <!-- RAW SQL INPUT -->
    <textarea
      v-model="rawSql"
      placeholder="e.g. age > 18 AND name LIKE 'A%'"
      class="w-full border px-3 py-2 rounded bg-white text-dark text-sm font-mono"
      rows="4"
    />

    <!-- ACTION -->
    <div class="flex gap-2 mt-2">
      <button
        @click="clear"
        class="bg-gray-200 px-3 py-1 rounded hover:bg-gray-300 transition"
      >
        Clear
      </button>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from "vue";
import type { QueryState, RawCondition, ConditionGroup } from "@/types/queryState";

/* ========================
   PROPS
======================== */
const props = defineProps<{
  state: QueryState;
}>();

/* ========================
   STATE
======================== */
const rawSql = ref("");

/* ========================
   PREVIEW
======================== */
const preview = computed(() => rawSql.value.trim());

/* ========================
   AUTO UPDATE (🔥 MAIN FIX)
======================== */
watch(
  rawSql,
  (value) => {
    const sql = value.trim();

    if (!sql) {
      props.state.where = null;
      return;
    }

    const rawCondition: RawCondition = {
      type: "raw",
      sql,
    };

    const group: ConditionGroup = {
      type: "AND",
      conditions: [rawCondition],
    };

    props.state.where = {
      mode: "builder",
      group,
    };
  },
  { immediate: true }
);

/* ========================
   CLEAR
======================== */
const clear = () => {
  rawSql.value = "";
  props.state.where = null;
};

/* ========================
   SYNC FROM STATE
======================== */
watch(
  () => props.state.where,
  (where) => {
    if (!where || where.mode !== "builder") {
      rawSql.value = "";
      return;
    }

    const first = where.group.conditions[0];

    if (typeof first === "object" && "type" in first && first.type === "raw") {
      rawSql.value = first.sql;
    }
  },
  { immediate: true }
);
</script>