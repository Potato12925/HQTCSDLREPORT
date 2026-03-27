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
/* ================================
   JOIN UPDATE (INCREMENTAL)
================================ */
const removeCrossForTable = (tableId: number) => {
  if (!queryState.value.joins) return;

  queryState.value.joins = queryState.value.joins.filter((j: any) => {
    return !(j.type === "CROSS" && j.tableId === tableId);
  });
};
// ADD joins khi table được thêm
const addJoinsForTable = (tableId: number) => {
  //không có table thì bỏ qua
  if (!queryState.value.tables) return;
  //không có joins thì tạo mảng join
  if (!queryState.value.joins) {
    queryState.value.joins = [];
  }
  //lấy ra tableID có checked là true
  const selectedTableIds = getSelectedTableIds(queryState.value.tables);

  let hasRelation = false;
  //loop mỗi table
  tables.value.forEach((table) => {
    table.foreignKeys.forEach((fk) => {
      //kiểm tra xem table có relationship ở parentobj hay refobj trong tables chung hay không
      const isRelated = fk.parentObjectId === tableId || fk.referencedObjectId === tableId;

      if (!isRelated) return;
      //kiểm tra xem trong các table được chọn có cặp table relationship hay không
      const canJoin =
        selectedTableIds.has(fk.parentObjectId) && selectedTableIds.has(fk.referencedObjectId);

      if (!canJoin) return;

      hasRelation = true;
      //tạo key unique cho cặp khóa ngoại của 2
      const key = buildJoinKey(fk);
      // Kiểm tra xem trong _meta chứa key có tồn tại hay chưa
      const exists = queryState.value.joins!.some((j: any) => j._meta?.key === key);

      if (!exists) {
        //tạo format ForeignKeyMetadata cho join
        const join = mapForeignKeyToJoin(fk);
        // thêm _meta chứa key cho join đó
        (join as any)._meta = {
          key,
          //xác định join này do hệ thống tự tạo
          auto: true,
        };
        // remove cross join của 2 table nếu có
        removeCrossForTable(fk.parentObjectId);
        removeCrossForTable(fk.referencedObjectId);
        queryState.value.joins!.push(join);
      }
    });
  });

  //nếu table không có FK với các table nào khác được chọn => CROSS JOIN
  if (!hasRelation && selectedTableIds.size > 1) {
    //xác định có tồn tại cross table bắt đầu với CROSS_ hay không
    const exists = queryState.value.joins!.some((j: any) => j._meta?.key === `CROSS_${tableId}`);
    //Nếu không tồn tại thì thêm join mới
    if (!exists) {
      queryState.value.joins!.push({
        type: "CROSS",
        tableId,
        on: {
          type: "AND",
          conditions: [],
        },
        _meta: {
          key: `CROSS_${tableId}`,
          auto: true,
        },
      } as any);
    }
  }
};

const removeJoinsForTable = (tableId: number) => {
  if (!queryState.value.joins) return;

  queryState.value.joins = queryState.value.joins.filter((join: any) => {
    const key = join._meta?.key || "";

    // loại bỏ ngay CROSS JOIN
    if (join.type === "CROSS") return false;

    // loại bỏ join có liên quan tới table bị xoá
    if (key.includes(`|${tableId}|`)) return false;

    // giữ lại các join còn lại
    return true;
  });
};

/* ================================
   CORE: UPDATE STATE
================================ */

const handleToggleColumn = (column: ColumnMetadata, table: TableMetadata) => {
  //lấy table trước khi thay đổi
  const before = getSelectedTableIds(queryState.value.tables);
  // không có tables thì tạo mới
  if (!queryState.value.tables) {
    queryState.value.tables = [];
  }

  const queryTables = queryState.value.tables;
  const tableId = table.objectId;
  // nếu tableid chưa tồn tại thì thêm vào mảng
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

  //xác định xem column đã tồn tại trong queryTable.columns chưa
  const existingIndex = queryTable.columns.findIndex((c) => c.column.columnId === column.columnId);

  if (column.checked) {
    //nếu được checked mà chưa tồn tại thì tạo mới
    if (existingIndex === -1) {
      queryTable.columns.push(mapToQueryColumn(table, column));
    }
  } else {
    //nếu checked = false mà tồn tại thì bỏ
    if (existingIndex !== -1) {
      queryTable.columns.splice(existingIndex, 1);
    }
  }

  // remove table nếu không còn column
  if (queryTable.columns.length === 0) {
    queryState.value.tables = queryTables.filter((t) => t.id !== tableId);
  }
  //xác định table id sau khi thay đổi
  const after = getSelectedTableIds(queryState.value.tables);

  const addedTables = Array.from(after).filter((id) => !before.has(id));
  const removedTables = Array.from(before).filter((id) => !after.has(id));

  // chỉ update phần liên quan
  addedTables.forEach(addJoinsForTable);
  removedTables.forEach(removeJoinsForTable);
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
