<template>
  <div class="bg-white rounded-2xl shadow p-4 mb-4">
    <FromRoot :table="rootTable" />

    <JoinList :joins="filteredJoins" :tables="tables" :root-table-id="rootTable?.id" />

    <div v-if="!rootTable" class="text-gray-400 text-sm">No tables selected</div>
  </div>
</template>

<script setup lang="ts">
import { computed, watch } from "vue";
import FromRoot from "./FromRoot.vue";
import JoinList from "./JoinList.vue";
import type { QueryState } from "@/types/queryState";

const props = defineProps<{ state: QueryState }>();

const tables = computed(() => props.state.tables ?? []);
const joins = computed(() => props.state.joins ?? []);
const rootTable = computed(() => props.state.from ?? null);

const filteredJoins = computed(() => {
  if (!rootTable.value) return joins.value;
  return joins.value.filter(
    (j) => !(j.toTableId === rootTable.value?.id && j.fromTableId === rootTable.value?.id),
  );
});

watch(
  filteredJoins,
  (value) => {
    console.log("filteredJoins:", JSON.stringify(value, null, 2));
  },
  { immediate: true },
);
</script>
