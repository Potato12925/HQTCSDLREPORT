<template>
  <div class="bg-white rounded-2xl shadow p-4 mb-4">
    <h2 class="text-lg font-semibold mb-3 text-primary">FROM</h2>

    <!-- TABLE LIST -->
    <div v-if="sortedTables.length">
      <div
        v-for="table in sortedTables"
        :key="table.id"
        class="border rounded-xl p-3 mb-3 bg-light"
      >
        <div class="flex items-center gap-2">
          <!-- Table name -->
          <span class="font-medium text-dark">
            {{ table.tableName }}
            <span
              v-if="table.id === rootTable?.id"
              class="text-xs text-primary ml-1"
            >
              (root)
            </span>
          </span>

          <!-- Alias -->
          <input
            v-model="table.alias"
            placeholder="alias"
            class="border rounded px-2 py-1 text-sm"
          />

          <!-- Remove -->
          <button
            @click="removeTable(table.id)"
            class="ml-auto text-red-500 text-sm"
          >
            Remove
          </button>
        </div>
      </div>
    </div>

    <!-- EMPTY -->
    <div v-else class="text-gray-400 text-sm mb-3">
      No tables selected
    </div>

    <!-- ADD TABLE -->
    <div class="flex gap-2 mt-3">
      <input
        v-model="newTableName"
        placeholder="Enter table name"
        class="border rounded px-2 py-1 text-sm flex-1"
      />
      <button
        @click="addTable"
        class="bg-primary text-white px-3 py-1 rounded"
      >
        Add
      </button>
    </div>

    <!-- ================= JOINS ================= -->
    <div v-if="showJoins && joins.length" class="mt-5">
      <h3 class="text-md font-semibold mb-2">Joins</h3>

      <div
        v-for="(join, index) in joins"
        :key="index"
        class="border rounded-xl p-3 mb-2"
      >
        <div class="flex items-center gap-2">
          <!-- Join type -->
          <select
            v-model="join.type"
            class="border rounded px-2 py-1 text-sm"
          >
            <option value="INNER">INNER</option>
            <option value="LEFT">LEFT</option>
            <option value="RIGHT">RIGHT</option>
            <option value="FULL">FULL</option>
            <option value="CROSS">CROSS</option>
          </select>

          <!-- Table -->
          <select
            v-model="join.tableId"
            class="border rounded px-2 py-1 text-sm"
          >
            <option
              v-for="t in tables"
              :key="t.id"
              :value="t.id"
            >
              {{ t.tableName }}
            </option>
          </select>

          <!-- Remove -->
          <button
            @click="removeJoin(index)"
            class="ml-auto text-red-500 text-sm"
          >
            Remove
          </button>
        </div>

        <!-- ON condition preview -->
        <div class="text-xs text-gray-500 mt-2">
          ON ({{ formatCondition(join.on) }})
        </div>
      </div>
    </div>

    <!-- ADD JOIN -->
    <button
      v-if="showJoins"
      @click="addJoin"
      class="mt-3 text-sm text-primary"
    >
      + Add Join
    </button>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import type {
  QueryState,
  QueryTable,
  Join,
  ConditionGroup,
} from "@/types/queryState";

const props = defineProps<{
  state: QueryState;
}>();

// ================= TABLES =================

const tables = computed(() => props.state.tables ?? []);

// ================= ROOT TABLE =================

const joins = computed(() => props.state.joins ?? []);

const rootTable = computed(() => {
  if (!tables.value.length) return null;

  // ưu tiên join != CROSS
  const priorityJoin = joins.value.find((j) => j.type !== "CROSS");

  if (priorityJoin) {
    return tables.value.find((t) => t.id === priorityJoin.tableId);
  }

  // fallback: table đầu tiên
  return tables.value[0];
});

// ================= SORT TABLE =================

const sortedTables = computed(() => {
  if (!rootTable.value) return tables.value;

  return [
    rootTable.value,
    ...tables.value.filter((t) => t.id !== rootTable.value?.id),
  ];
});

// ================= SHOW JOIN =================

const showJoins = computed(() => tables.value.length > 1);

// ================= ADD / REMOVE TABLE =================

const newTableName = ref("");

const addTable = () => {
  if (!newTableName.value.trim()) return;

  const id = Date.now();

  const newTable: QueryTable = {
    id,
    tableName: newTableName.value,
    alias: null,
    columns: [],
  };

  if (!props.state.tables) {
    props.state.tables = [];
  }

  props.state.tables.push(newTable);
  newTableName.value = "";
};

const removeTable = (id: number) => {
  if (!props.state.tables) return;
  props.state.tables = props.state.tables.filter((t) => t.id !== id);

  // remove joins liên quan
  if (props.state.joins) {
    props.state.joins = props.state.joins.filter(
      (j) => j.tableId !== id
    );
  }
};

// ================= JOINS =================

const addJoin = () => {
  if (!props.state.joins) props.state.joins = [];

  const defaultGroup: ConditionGroup = {
    type: "AND",
    conditions: [],
  };

  const newJoin: Join = {
    type: "INNER",
    tableId: tables.value[0]?.id,
    on: defaultGroup,
  };

  props.state.joins.push(newJoin);
};

const removeJoin = (index: number) => {
  props.state.joins?.splice(index, 1);
};

// ================= HELPERS =================

const formatCondition = (group: ConditionGroup): string => {
  if (!group.conditions.length) return "empty";

  return group.conditions
    .map((c: any) => {
      if ("column" in c) {
        return `${(c.column as any).columnName} ${c.operator} ${
          c.value ?? ""
        }`;
      }
      return "...";
    })
    .join(` ${group.type} `);
};
</script>
