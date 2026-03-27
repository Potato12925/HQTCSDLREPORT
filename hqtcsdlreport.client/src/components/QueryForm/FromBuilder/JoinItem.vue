<template>
  <div class="border rounded-xl p-3 mb-3">
    <!-- SQL HEADER -->
    <div class="flex flex-wrap items-center gap-2 text-sm mb-2">
      <select v-model="join.type" class="border px-2 py-1 text-sm">
        <option value="INNER">INNER</option>
        <option value="LEFT">LEFT</option>
        <option value="RIGHT">RIGHT</option>
        <option value="FULL">FULL</option>
        <option value="CROSS">CROSS</option>
      </select>

      <span class="font-medium">JOIN</span>

      <select v-model="join.toTableId" class="border px-2 py-1 text-sm">
        <option v-for="t in tables" :key="t.id" :value="t.id">
          {{ t.tableName }}
        </option>
      </select>

      <input
        v-if="table"
        v-model="table.alias"
        placeholder="alias"
        class="border px-2 py-1 text-sm w-20"
      />

      <span v-if="join.type !== 'CROSS'" class="font-medium">ON</span>
    </div>

    <!-- CONDITIONS -->
    <JoinConditionList v-if="join.type !== 'CROSS'" :join="join" :tables="tables" />
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import JoinConditionList from "./JoinConditionList.vue";
import type { Join, QueryTable } from "@/types/queryState";

const props = defineProps<{
  join: Join;
  tables: QueryTable[];
}>();

const table = computed(() => props.tables.find((t) => t.id === props.join.toTableId));
</script>
