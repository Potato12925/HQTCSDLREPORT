<template>
  <div class="flex h-screen">
    <TableTree :tables="tables" :loading="loading" @toggle-column="handleToggleColumn" />

    <QueryForm :queryState="queryState" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from "vue";
import { connectDbApi } from "@/api/dataApi";
import TableTree from "@/components/TableInfo/TableTree.vue";
import QueryForm from "@/components/QueryForm/QueryForm.vue";

import type { QueryState, QueryTable, QueryColumn, Join } from "@/types/queryState";

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
const mapToQueryColumn = (table: TableMetadata, column: ColumnMetadata): QueryColumn => {
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
    fromTableId: foreignKey.parentObjectId,
    toTableId: foreignKey.referencedObjectId,
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

const getSelectedTableIds = (tables?: QueryTable[]) => {
  //nếu không có table thì tạo set
  if (!tables) return new Set<number>();
  //tạo set gồm các ID của tables
  return new Set(tables.map((table) => table.id));
};

const buildJoinKey = (fk: ForeignKeyMetadata) => {
  return [
    fk.foreignKeyName,
    fk.parentObjectId,
    fk.parentColumnId,
    fk.referencedObjectId,
    fk.referencedColumnId,
  ].join("|");
};

function recomputeJoins(selectedTables: QueryTable[], allTables: TableMetadata[]): Join[] {
  const joins: Join[] = [];

  const selectedIds = new Set(selectedTables.map((t) => t.id));

  // map FK theo table
  const fkMap: ForeignKeyMetadata[] = [];

  allTables.forEach((table) => {
    table.foreignKeys.forEach((fk) => {
      if (selectedIds.has(fk.parentObjectId) && selectedIds.has(fk.referencedObjectId)) {
        fkMap.push(fk);
      }
    });
  });

  const usedTable = new Set<number>();

  // ================= INNER JOIN =================
  fkMap.forEach((fk) => {
    const join = mapForeignKeyToJoin(fk);

    joins.push({
      ...join,
      _meta: {
        key: buildJoinKey(fk),
        auto: true,
      },
    });

    usedTable.add(fk.parentObjectId);
    usedTable.add(fk.referencedObjectId);
  });

  // ================= CROSS JOIN =================
  selectedTables.forEach((table) => {
    if (!usedTable.has(table.id)) {
      joins.push({
        type: "CROSS",
        fromTableId: table.id,
        toTableId: table.id,
        on: {
          type: "AND",
          conditions: [],
        },
        _meta: {
          key: `CROSS_${table.id}`,
          auto: true,
        },
      });
    }
  });

  return joins;
}
function buildGraph(joins: Join[]) {
  const graph = new Map<number, number[]>();

  joins.forEach((j) => {
    if (j.type === "CROSS") return;

    if (!graph.has(j.fromTableId)) graph.set(j.fromTableId, []);
    if (!graph.has(j.toTableId)) graph.set(j.toTableId, []);

    graph.get(j.fromTableId)!.push(j.toTableId);
    graph.get(j.toTableId)!.push(j.fromTableId);
  });

  return graph;
}
function findRoot(tables: QueryTable[], joins: Join[]): QueryTable {
  const joinedTables = joins.filter((join) => join.type !== "CROSS").map((join) => join.toTableId);

  const toSet = new Set(joinedTables);

  for (const table of tables) {
    if (!toSet.has(table.id)) {
      return table;
    }
  }

  return tables[0];
}
function traverseAll(graph: Map<number, number[]>, tables: QueryTable[]) {
  const visited = new Set<number>();
  const order: number[] = [];

  function dfs(id: number) {
    if (visited.has(id)) return;

    visited.add(id);
    order.push(id);

    const children = graph.get(id) ?? [];
    for (const next of children) {
      dfs(next);
    }
  }

  for (const table of tables) {
    dfs(table.id);
  }

  return order;
}
function orderJoins(joins: Join[], tables: QueryTable[]) {
  const innerJoins = joins.filter((j) => j.type !== "CROSS");
  const crossJoins = joins.filter((j) => j.type === "CROSS");

  const graph = buildGraph(innerJoins);

  const order = traverseAll(graph, tables);

  const ordered: Join[] = [];
  const used = new Set<string>();

  for (const tableId of order) {
    const matches = innerJoins.filter((j) => j.fromTableId === tableId || j.toTableId === tableId);

    for (const j of matches) {
      const key = j._meta?.key ?? `${j.fromTableId}-${j.toTableId}`;

      if (!used.has(key)) {
        ordered.push(j);
        used.add(key);
      }
    }
  }

  return [...ordered, ...crossJoins];
}
/* ================================
   CORE: UPDATE STATE
================================ */
const handleToggleColumn = (column: ColumnMetadata, table: TableMetadata) => {
  if (!queryState.value.tables) {
    queryState.value.tables = [];
  }

  const queryTables = queryState.value.tables;
  const tableId = table.objectId;

  let queryTable = queryTables.find((t) => t.id === tableId);

  if (!queryTable) {
    queryTable = {
      id: tableId,
      tableName: table.tableName,
      alias: null,
      columns: [],
    };
    queryTables.push(queryTable);
  }

  const existingIndex = queryTable.columns.findIndex((c) => c.column.columnId === column.columnId);

  if (column.checked) {
    if (existingIndex === -1) {
      queryTable.columns.push(mapToQueryColumn(table, column));
    }
  } else {
    if (existingIndex !== -1) {
      queryTable.columns.splice(existingIndex, 1);
    }
  }

  // remove table nếu không còn column
  if (queryTable.columns.length === 0) {
    queryState.value.tables = queryTables.filter((t) => t.id !== tableId);
  }

  const selectedTables = queryState.value.tables ?? [];

  const rawJoins = recomputeJoins(selectedTables, tables.value);

  const orderedJoins = orderJoins(rawJoins, selectedTables);

  const root = findRoot(selectedTables, rawJoins);
  queryState.value.from = root || undefined;
  
  queryState.value.joins = orderedJoins;
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

watch(
  queryState,
  () => {
    console.clear();
    console.log("Updated Query State:\n", JSON.stringify(queryState.value, null, 2));
  },
  { deep: true },
);
</script>
