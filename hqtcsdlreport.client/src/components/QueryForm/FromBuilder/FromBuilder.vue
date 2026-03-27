<template>
  <div class="bg-white rounded-2xl shadow p-4 mb-4">
    <h2 class="text-lg font-semibold mb-3 text-primary">FROM</h2>

    <!-- ================= ROOT TABLE ================= -->
    <div v-if="rootTable" class="border rounded-xl p-3 mb-3 bg-blue-50">
      <div class="flex items-center gap-2 text-sm">
        <span class="text-gray-500">FROM</span>

        <span class="font-semibold">
          {{ rootTable.tableName }}
        </span>

        <input
          v-model="rootTable.alias"
          placeholder="alias"
          class="border rounded px-2 py-1 text-sm w-24"
        />
      </div>
    </div>

    <!-- ================= JOINS ================= -->
    <div v-if="joins.length" class="mt-4">
      <div v-for="(join, jIndex) in filteredJoins" :key="jIndex" class="border rounded-xl p-3 mb-3">
        <!-- SQL STYLE HEADER -->
        <div class="flex flex-wrap items-center gap-2 text-sm mb-2">
          <!-- JOIN TYPE -->
          <select v-model="join.type" class="border px-2 py-1 text-sm">
            <option value="INNER">INNER</option>
            <option value="LEFT">LEFT</option>
            <option value="RIGHT">RIGHT</option>
            <option value="FULL">FULL</option>
            <option value="CROSS">CROSS</option>
          </select>

          <span class="font-medium">JOIN</span>

          <!-- TABLE -->
          <select v-model="join.toTableId" class="border px-2 py-1 text-sm">
            <option v-for="t in tables" :key="t.id" :value="t.id">
              {{ t.tableName }}
            </option>
          </select>

          <!-- ALIAS -->
          <input
            v-if="getTable(join.toTableId)"
            v-model="getTable(join.toTableId).alias"
            placeholder="alias"
            class="border px-2 py-1 text-sm w-20"
          />

          <!-- ON -->
          <span v-if="join.type !== 'CROSS'" class="font-medium">ON</span>
        </div>

        <!-- ================= CONDITIONS ================= -->
        <div v-if="join.type !== 'CROSS'" class="ml-4">
          <div
            v-for="(cond, cIndex) in join.on.conditions"
            :key="cIndex"
            class="flex gap-2 mb-1 items-center text-sm"
          >
            <!-- RAW -->
            <template v-if="'type' in cond && cond.type === 'raw'">
              <input v-model="cond.sql" placeholder="RAW SQL" class="border px-2 py-1 flex-1" />
            </template>

            <!-- NORMAL -->
            <template v-else-if="'column' in cond && typeof cond.column === 'object'">
              <input v-model="cond.column.columnName" class="border px-2 py-1 w-32" />

              <select v-model="cond.operator" class="border px-2 py-1">
                <option value="=">=</option>
                <option value="!=">!=</option>
                <option value=">">&gt;</option>
                <option value="<">&lt;</option>
              </select>

              <input v-model="cond.value.columnName" class="border px-2 py-1 w-32" />
            </template>

            <!-- REMOVE -->
            <button @click="removeCondition(join, cIndex)" class="text-red-500 text-xs">✕</button>
          </div>

          <!-- ACTIONS -->
          <div class="flex gap-2 mt-2">
            <button @click="addCondition(join, 'AND')" class="text-xs text-blue-500">+ AND</button>

            <button @click="addCondition(join, 'OR')" class="text-xs text-purple-500">+ OR</button>

            <button @click="addRaw(join)" class="text-xs text-green-500">+ RAW</button>
          </div>
        </div>
      </div>
    </div>

    <!-- EMPTY -->
    <div v-if="!rootTable" class="text-gray-400 text-sm">No tables selected</div>
  </div>
</template>

<script setup lang="ts">
import { computed, watch } from "vue";
import type { QueryState, Join, Operator } from "@/types/queryState";

const props = defineProps<{
  state: QueryState;
}>();

// ================= DATA =================

const tables = computed(() => props.state.tables ?? []);
const joins = computed(() => props.state.joins ?? []);
const rootTable = computed(() => props.state.from ?? null);
const filteredJoins = computed(() => {
  if (!rootTable.value) return joins.value;

  return joins.value.filter((j) => j.toTableId !== rootTable.value.id);
});
// ================= HELPERS =================

const getTable = (id: number) => {
  return tables.value.find((t) => t.id === id);
};

// ================= AUTO DEFAULT ON =================

const ensureDefaultCondition = (join: Join) => {
  if (join.type === "CROSS") return;

  if (!join.on.conditions.length) {
    join.on.conditions.push({
      column: {
        tableId: join.fromTableId,
        columnId: 0,
        columnName: "id",
      },
      operator: "=" as Operator,
      value: {
        tableId: join.toTableId,
        columnId: 0,
        columnName: "id",
      },
    });
  }
};

watch(
  () => joins.value,
  (list) => {
    list.forEach((j) => ensureDefaultCondition(j));
  },
  { deep: true, immediate: true },
);

// ================= ACTIONS =================

const addCondition = (join: Join, type: "AND" | "OR") => {
  join.on.conditions.push({
    column: {
      tableId: join.fromTableId,
      columnId: 0,
      columnName: "",
    },
    operator: "=" as Operator,
    value: {
      tableId: join.toTableId,
      columnId: 0,
      columnName: "",
    },
  });

  join.on.type = type;
};

const addRaw = (join: Join) => {
  join.on.conditions.push({
    type: "raw",
    sql: "",
  });
};

const removeCondition = (join: Join, index: number) => {
  join.on.conditions.splice(index, 1);
};
</script>
