<template>
  <div class="bg-light rounded-2xl shadow p-4 mb-4">
    <!-- HEADER -->
    <div class="flex items-center justify-between mb-2">
      <div class="flex items-center gap-2">
        <span class="font-semibold text-primary">GROUP</span>

        <select v-model="model.type" class="input">
          <option value="AND">AND</option>
          <option value="OR">OR</option>
        </select>
      </div>

      <button v-if="removable" @click="$emit('remove')" class="text-red-500 text-sm">x</button>
    </div>

    <!-- CONDITIONS -->
    <div class="space-y-2">
      <div
        v-for="(cond, index) in model.conditions"
        :key="index"
        class="pl-2 border-l border-primary/20"
      >
        <!-- CONDITION -->
        <ConditionItem
          v-if="isCondition(cond)"
          v-model="model.conditions[index] as Condition"
          :columns="columns"
          @remove="removeCondition(index)"
        />

        <!-- RAW -->
        <RawConditionItem
          v-else-if="isRaw(cond)"
          v-model="model.conditions[index] as RawCondition"
          @remove="removeCondition(index)"
        />

        <!-- GROUP -->
        <ConditionGroupBuilder
          v-else
          v-model="model.conditions[index] as ConditionGroup"
          :columns="columns"
          removable
          @remove="removeCondition(index)"
        />
      </div>
    </div>

    <!-- ADD -->
    <div class="flex gap-2 mt-3">
      <select v-model="newType" class="input">
        <option value="condition">Condition</option>
        <option value="group">Group</option>
        <option value="raw">Raw</option>
      </select>

      <button @click="addCondition" class="btn">+ Add</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import type { Condition, ConditionGroup, RawCondition, ColumnRef } from "@/types/queryState";

import ConditionItem from "./ConditionItem.vue";
import RawConditionItem from "./RawConditionItem.vue";

/* ================= TYPES ================= */

type SelectableColumn = ColumnRef & {
  label: string;
};

/* ================= PROPS ================= */

const props = defineProps<{
  removable?: boolean;
  columns: SelectableColumn[];
}>();

defineEmits(["remove"]);

/* ================= MODEL ================= */

const model = defineModel<ConditionGroup>({
  required: true,
});

/* ================= STATE ================= */

const newType = ref<"condition" | "group" | "raw">("condition");

/* ================= TYPE GUARDS ================= */

function isRaw(cond: any): cond is RawCondition {
  return cond?.type === "raw";
}

function isGroup(cond: any): cond is ConditionGroup {
  return Array.isArray(cond?.conditions);
}

function isCondition(cond: any): cond is Condition {
  return !isRaw(cond) && !isGroup(cond);
}

/* ================= CREATE ================= */

function createCondition(): Condition {
  return {
    column: {} as ColumnRef,
    operator: "=",
    value: "",
  };
}

function createRaw(): RawCondition {
  return {
    type: "raw",
    sql: "",
  };
}

function createGroup(): ConditionGroup {
  return {
    type: "AND",
    conditions: [],
  };
}

/* ================= ACTIONS ================= */

function addCondition() {
  if (newType.value === "group") {
    model.value.conditions.push(createGroup());
    return;
  }

  if (newType.value === "raw") {
    model.value.conditions.push(createRaw());
    return;
  }

  model.value.conditions.push(createCondition());
}

function removeCondition(index: number) {
  model.value.conditions.splice(index, 1);
}
</script>

