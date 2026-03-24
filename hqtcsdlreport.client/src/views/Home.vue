<template>
  <div class="flex h-screen">
    <TableTree
      :tables="tables"
      :loading="loading"
      @toggle-column="handleToggleColumn"
    />

    <QueryForm :queryState="queryState" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { connectDbApi } from "@/api/dataApi";
import TableTree from "@/components/TableInfo/TableTree.vue";
import QueryForm from "@/components/QueryForm/QueryForm.vue";

import type {
  QueryState,
  QueryTable,
  QueryColumn,
  Join,
} from "@/types/queryState";

import type {
  TableMetadata,
  DatabaseMetadata,
  ColumnMetadata,
  ForeignKeyMetadata,
} from "@/types/database";

/* ================================
   STATE
================================ */
const tables = ref<TableMetadata[]>([]);
const loading = ref<boolean>(false);

const server = ref<string>("");
const database = ref<string>("");

const queryState = ref<QueryState>({});

/* ================================
   HELPERS
================================ */
const mapToQueryColumn = (
  table: TableMetadata,
  column: ColumnMetadata
): QueryColumn => {
  return {
    show: true,
    column: {
      tableId: table.objectId,
      columnId: column.columnId,
      columnName: column.columnName,
    },
    alias: null,
    aggregate: null,
    criteria: null,
  };
};

const mapForeignKeyToJoin = (foreignKey: ForeignKeyMetadata): Join => {
  return {
    type: "INNER",
    tableId: foreignKey.referencedObjectId,
    on: {
      type: "AND",
      conditions: [
        {
          column: {
            tableId: foreignKey.parentObjectId,
            columnId: foreignKey.parentColumnId,
            columnName: foreignKey.parentColumn,
          },
          operator: "=",
          value: {
            tableId: foreignKey.referencedObjectId,
            columnId: foreignKey.referencedColumnId,
            columnName: foreignKey.referencedColumn,
          },
        },
      ],
    },
  };
};

/* ================================
   CORE: UPDATE STATE (EVENT-DRIVEN)
================================ */
const handleToggleColumn = (
  column: ColumnMetadata,
  table: TableMetadata
) => {
  if (!queryState.value.tables) {
    queryState.value.tables = {};
  }

  const tableId = table.objectId;

  // tạo table nếu chưa có
  if (!queryState.value.tables[tableId]) {
    queryState.value.tables[tableId] = {
      id: tableId,
      tableName: table.tableName,
      alias: null,
      columns: [],
    };
  }

  const queryTable = queryState.value.tables[tableId];

  const existingIndex = queryTable.columns.findIndex(
    (c) => c.column.columnId === column.columnId
  );

  if (column.checked) {
    if (existingIndex === -1) {
      queryTable.columns.push(mapToQueryColumn(table, column));
    }
  }

  else {
    if (existingIndex !== -1) {
      queryTable.columns.splice(existingIndex, 1);
    }
  }

  if (queryTable.columns.length === 0) {
    delete queryState.value.tables[tableId];
  }

  rebuildJoins();

  console.clear();
  console.log("queryState:\n", JSON.stringify(queryState.value, null, 2));
};

/* ================================
   JOIN LOGIC
================================ */
const rebuildJoins = () => {
  if (!queryState.value.tables) {
    queryState.value.joins = undefined;
    return;
  }

  const selectedTableIds = new Set<number>(
    Object.keys(queryState.value.tables).map(Number)
  );

  const joinsMap = new Map<string, Join>();

  tables.value.forEach((table) => {
    if (!selectedTableIds.has(table.objectId)) return;

    table.foreignKeys.forEach((foreignKey) => {
      const canJoin =
        selectedTableIds.has(foreignKey.parentObjectId) &&
        selectedTableIds.has(foreignKey.referencedObjectId);

      if (!canJoin) return;

      const key = [
        foreignKey.foreignKeyName,
        foreignKey.parentObjectId,
        foreignKey.parentColumnId,
        foreignKey.referencedObjectId,
        foreignKey.referencedColumnId,
      ].join("|");

      if (!joinsMap.has(key)) {
        joinsMap.set(key, mapForeignKeyToJoin(foreignKey));
      }
    });
  });

  queryState.value.joins = joinsMap.size
    ? Array.from(joinsMap.values())
    : undefined;
};

/* ================================
   LOAD DB
================================ */
const loadDb = async (): Promise<void> => {
  loading.value = true;

  try {
    const res = await connectDbApi({
      server: server.value,
      database: database.value,
    });

    const data = res.data as DatabaseMetadata;

    tables.value = data.tables || [];

    console.log("data:\n", JSON.stringify(data, null, 2));
  } catch (err) {
    console.error(err);
  } finally {
    loading.value = false;
  }
};

/* ================================
   INIT
================================ */
onMounted(() => {
  server.value = localStorage.getItem("server") || "";
  database.value = localStorage.getItem("database") || "";

  if (server.value && database.value) {
    loadDb();
  }
});
</script>