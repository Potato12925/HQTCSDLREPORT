<template>
  <div class="ml-4">
    <JoinConditionItem
      v-for="(cond, i) in join.on.conditions"
      :key="i"
      :cond="cond"
      :tables="tables"
      @remove="remove(i)"
    />

    <div class="flex gap-2 mt-2">
      <button @click="add('AND')" class="text-xs text-blue-500">+ AND</button>
      <button @click="add('OR')" class="text-xs text-purple-500">+ OR</button>
      <button @click="addRaw" class="text-xs text-green-500">+ RAW</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import JoinConditionItem from "./JoinConditionItem.vue";
import type { Join, Operator, QueryTable } from "@/types/queryState";

const props = defineProps<{
  join: Join;
  tables: QueryTable[];
}>();

const add = (type: "AND" | "OR") => {
  props.join.on.conditions.push({
    column: { tableId: 0, columnId: 0, columnName: "" },
    operator: "=" as Operator,
    value: { tableId: 0, columnId: 0, columnName: "" },
  });

  props.join.on.type = type;
};

const addRaw = () => {
  props.join.on.conditions.push({
    type: "raw",
    sql: "",
  });
};

const remove = (i: number) => {
  props.join.on.conditions.splice(i, 1);
};
</script>
