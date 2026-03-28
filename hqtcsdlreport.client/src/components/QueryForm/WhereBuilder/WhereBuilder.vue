<template>
  <div class="mb-6">
    <h3 class="font-semibold text-primary mb-3 text-lg">WHERE</h3>

    <ConditionGroupBuilder
      v-if="where"
      v-model="where.group"
      :columns="columns"
    />
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import type { QueryState, WhereClause, ConditionGroup, ColumnRef } from "@/types/queryState";
import ConditionGroupBuilder from "./ConditionGroupBuilder.vue";

const props = defineProps<{
  state: QueryState;
}>();

/* ================= COLUMN LIST ================= */

export type SelectableColumn = ColumnRef & {
  label: string;
};

const columns = computed<SelectableColumn[]>(() => {
  const tables = props.state.tables ?? [];

  return tables.flatMap((table) => {
    const tableName = table.alias?.trim() || table.tableName;

    return table.columns.map((c) => ({
      ...c.column,
      label: `${tableName}.${c.column.columnName}`,
    }));
  });
});

/* ================= WHERE ================= */

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