<template>
  <div >
    <JoinConditionItem
      v-for="(cond, i) in join.on.conditions"
      :key="i"
      :cond="cond"
      :join="join"
      :tables="tables"
      :is-first="i === 0"
      :lock-first="i === 0 && !allowFirstEdit"
      :use-join-column-picker="i === 0 && allowFirstEdit"
      @remove="remove(i)"
    />

    <!-- <div class="flex gap-2 mt-3 flex-wrap">
      <button
        @click="add('AND')"
        class="px-3 py-1.5 text-xs font-medium rounded-full bg-primary/10 text-primary border border-primary/20 hover:bg-primary hover:text-white transition shadow-sm"
      >
        + AND
      </button>
      <button
        @click="add('OR')"
        class="px-3 py-1.5 text-xs font-medium rounded-full bg-primary/10 text-primary border border-primary/20 hover:bg-primary hover:text-white transition shadow-sm"
      >
        + OR
      </button>
      <button
        @click="addRaw"
        class="px-3 py-1.5 text-xs font-medium rounded-full bg-primary/10 text-primary border border-primary/20 hover:bg-primary hover:text-white transition shadow-sm"
      >
        + RAW
      </button>
    </div> -->
  </div>
</template>

<script setup lang="ts">
import JoinConditionItem from "./JoinConditionItem.vue";
import type { Join, QueryTable } from "@/types/queryState";

const props = withDefaults(defineProps<{
  join: Join;
  tables: QueryTable[];
  allowFirstEdit?: boolean;
}>(), {
  allowFirstEdit: false,
});

// const add = (type: "AND" | "OR") => {
//   props.join.on.conditions.push({
//     column: { tableId: 0, columnId: 0, columnName: "" },
//     operator: "=" as Operator,
//     value: { tableId: 0, columnId: 0, columnName: "" },
//   });

//   props.join.on.type = type;
// };

// const addRaw = () => {
//   props.join.on.conditions.push({
//     type: "raw",
//     sql: "",
//   });
// };

const remove = (i: number) => {
  if (i === 0 ) return;
  props.join.on.conditions.splice(i, 1);
};
</script>
