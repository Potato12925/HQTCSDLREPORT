<template>
  <div class="p-3 mb-3">
    <!-- SQL HEADER -->
    <div class="flex flex-wrap items-center gap-2 text-sm mb-2">
      <select v-model="join.type" class="border border-primary/20 px-2 py-1 rounded bg-light text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
        <option value="INNER">INNER</option>
        <option value="LEFT">LEFT</option>
        <option value="RIGHT">RIGHT</option>
        <option value="FULL">FULL</option>
        <option value="CROSS">CROSS</option>
      </select>

      <span class="font-medium">JOIN</span>

      <select
        :value="displayTableId"
        class="border border-primary/20 px-2 py-1 rounded bg-light text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 opacity-80 pointer-events-none"
      >
        <option v-for="t in tables" :key="t.id" :value="t.id">
          {{ t.tableName }}
        </option>
      </select>

      <input
        v-if="table"
        v-model="table.alias"
        placeholder="alias"
        class="border border-primary/20 px-2 py-1 rounded bg-light text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 w-20"
      />

      <span v-if="join.type !== 'CROSS'" class="font-medium">ON</span>
      <!-- CONDITIONS -->
      <JoinConditionList
        v-if="join.type !== 'CROSS'"
        :join="join"
        :tables="tables"
        :allow-first-edit="isCrossSeedJoin"
      />
    </div>

  </div>
</template>

<script setup lang="ts">
import { computed, watch } from "vue";
import JoinConditionList from "./JoinConditionList.vue";
import type { Join, QueryTable } from "@/types/queryState";

const props = defineProps<{
  join: Join;
  tables: QueryTable[];
  rootTableId?: number;
}>();

const displayTableId = computed(() => {
  if (typeof props.rootTableId !== "number") {
    return props.join.toTableId ?? props.join.fromTableId;
  }

  if (props.join.toTableId === props.rootTableId) {
    return props.join.fromTableId;
  }

  if (props.join.fromTableId === props.rootTableId) {
    return props.join.toTableId;
  }

  return props.join.toTableId ?? props.join.fromTableId;
});

const table = computed(() => props.tables.find((t) => t.id === displayTableId.value));
const isCrossSeedJoin = computed(() => props.join._meta?.key?.startsWith("CROSS_") ?? false);

watch(
  () => props.join.type,
  (nextType, prevType) => {
    if (prevType !== "CROSS" || nextType === "CROSS") return;
    if (props.join.on.conditions.length > 0) return;

    props.join.on.conditions.push({
      column: "",
      operator: "=",
      value: "",
    });
  },
);
</script>
