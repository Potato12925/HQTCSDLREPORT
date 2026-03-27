<template>
  <div v-if="joins.length" class="mt-4">
    <JoinItem v-for="(join, index) in joins" :key="index" :join="join" :tables="tables" />
  </div>
</template>

<script setup lang="ts">
import JoinItem from "./JoinItem.vue";
import type { Join, QueryTable } from "@/types/queryState";

defineProps<{
  joins: Join[];
  tables: QueryTable[];
}>();
</script>
