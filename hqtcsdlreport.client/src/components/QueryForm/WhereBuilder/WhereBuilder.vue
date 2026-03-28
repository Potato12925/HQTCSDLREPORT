<template>
  <div class="mb-6">
    <h3 class="font-semibold text-primary mb-3 text-lg">WHERE</h3>

    <ConditionGroupBuilder v-if="where" v-model="where.group" :column-names="columnNames" />
  </div>
</template>

<script setup lang="ts">
import type { QueryState, WhereClause, ConditionGroup } from "@/types/queryState";
import ConditionGroupBuilder from "./ConditionGroupBuilder.vue";
import { computed } from "vue";

const props = defineProps<{
  state: QueryState;
}>();

const columnNames = computed<string[]>(() => {
  const tables = props.state.tables ?? [];
  const names = new Set<string>();

  for (const table of tables) {
    const tableOrAlias = table.alias?.trim() || table.tableName;

    for (const queryColumn of table.columns) {
      names.add(`${tableOrAlias}.${queryColumn.column.columnName}`);
    }
  }

  return Array.from(names);
});

const where = computed<WhereClause>({
  get() {
    if (!props.state.where) {
      props.state.where = {
        mode: "builder",
        group: {
          type: "AND",
          conditions: [],
        } as ConditionGroup,
      };
    }
    return props.state.where;
  },
  set(val) {
    props.state.where = val;
  },
});
</script>
