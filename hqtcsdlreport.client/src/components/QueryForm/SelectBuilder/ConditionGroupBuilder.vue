<script setup lang="ts">
import { computed } from "vue";
import ConditionItem from "./ConditionItem.vue";
import type { QueryTable, ConditionGroup, Condition, RawCondition } from "@/types/queryState";

// ===== PROPS =====
const props = defineProps<{
  tables: QueryTable[];
  modelValue: ConditionGroup | null;
}>();

const emit = defineEmits(["update:modelValue"]);

// ===== INIT GROUP =====
const group = computed({
  get: () =>
    props.modelValue ?? {
      type: "AND",
      conditions: [],
    },
  set: (val) => emit("update:modelValue", val),
});

// ===== TYPE GUARD =====
function isGroup(c: Condition | ConditionGroup | RawCondition): c is ConditionGroup {
  return (c as ConditionGroup).conditions !== undefined;
}

// ===== ACTIONS =====
function addCondition() {
  group.value.conditions.push({
    column: "",
    operator: "=",
    value: "",
  });
}

function addGroup() {
  group.value.conditions.push({
    type: "AND",
    conditions: [],
  });
}

function remove(index: number) {
  group.value.conditions.splice(index, 1);
}

// ===== COMPUTED PROXY (fix TS error) =====
function useGroupModel(index: number) {
  return computed({
    get: () => group.value.conditions[index] as ConditionGroup,
    set: (val: ConditionGroup) => {
      group.value.conditions[index] = val;
    },
  });
}
</script>

<template>
  <div class="border border-primary/20 p-3 rounded bg-light space-y-2">
    <!-- HEADER -->
    <div class="flex items-center gap-2">
      <select v-model="group.type" class="border px-2 py-1 rounded">
        <option value="AND">AND</option>
        <option value="OR">OR</option>
      </select>

      <button @click="addCondition" class="px-2 py-1 bg-primary text-white rounded text-sm">
        + Condition
      </button>

      <button @click="addGroup" class="px-2 py-1 bg-secondary text-white rounded text-sm">
        + Group
      </button>
    </div>

    <!-- BODY -->
    <div class="ml-4 space-y-2">
      <template v-for="(cond, index) in group.conditions" :key="index">
        <!-- CONDITION -->
        <ConditionItem
          v-if="!isGroup(cond)"
          v-model="(group.conditions[index] as Condition)"
          :tables="tables"
          @remove="remove(index)"
        />

        <!-- GROUP -->
        <ConditionGroupBuilder v-else v-model="useGroupModel(index).value" :tables="tables" />
      </template>
    </div>
  </div>
</template>
