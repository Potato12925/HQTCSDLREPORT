<template>
  <div class="mb-6">
    <h3 class="font-semibold text-primary mb-3 text-lg">HAVING</h3>

    <HavingConditionGroupBuilder
      v-if="having"
      v-model="having.group"
      :columns="columns"
      :aliases="aliases"
    />
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import type {
  ColumnDataType,
  ColumnRef,
  HavingClause,
  HavingConditionGroup,
  QueryState,
} from "@/types/queryState";
import HavingConditionGroupBuilder from "./HavingConditionGroupBuilder.vue";

export type SelectableColumn = ColumnRef & {
  label: string;
};

export type SelectableAlias = {
  alias: string;
  label: string;
  dataType: ColumnDataType;
  column: ColumnRef;
};

const props = defineProps<{
  state: QueryState;
}>();

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

const aliases = computed<SelectableAlias[]>(() => {
  const tables = props.state.tables ?? [];

  return tables.flatMap((table) => {
    const tableName = table.alias?.trim() || table.tableName;

    return table.columns
      .filter((c) => !!c.alias?.trim())
      .map((c) => {
        const alias = c.alias!.trim();

        return {
          alias,
          label: `${alias} (${tableName}.${c.column.columnName})`,
          dataType: inferAliasType(c.column.dataType, c.aggregate),
          column: {
            tableId: c.column.tableId,
            columnId: c.column.columnId,
            columnName: c.column.columnName,
            dataType: c.column.dataType,
          },
        };
      });
  });
});

const having = computed<HavingClause>({
  get() {
    if (!props.state.having) {
      props.state.having = {
        mode: "builder",
        group: {
          type: "AND",
          conditions: [],
        } as HavingConditionGroup,
      };
    }

    return props.state.having;
  },
  set(val) {
    props.state.having = val;
  },
});
function inferAliasType(
  dataType: ColumnDataType | undefined,
  aggregate: "COUNT" | "SUM" | "AVG" | "MIN" | "MAX" | null | undefined,
): ColumnDataType {
  if (aggregate === "COUNT" || aggregate === "SUM" || aggregate === "AVG") {
    return "number";
  }

  return dataType ?? "string";
}
</script>
