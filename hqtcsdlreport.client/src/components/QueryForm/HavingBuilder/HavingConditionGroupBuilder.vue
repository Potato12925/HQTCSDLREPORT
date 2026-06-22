<template>
  <div class="bg-white rounded-2xl shadow p-4 mb-4">
    <div class="flex items-center justify-between mb-2">
      <div class="flex items-center gap-2">
        <span class="font-semibold text-primary">GROUP</span>

        <select v-model="model.type" class="input">
          <option value="AND">AND</option>
          <option value="OR">OR</option>
        </select>
      </div>

      <button v-if="removable" @click="$emit('remove')" class="text-red-500">x</button>
    </div>

    <div class="space-y-2">
      <div
        v-for="(cond, index) in model.conditions"
        :key="index"
        class="pl-2 border-l border-primary/20"
      >
        <HavingConditionItem
          v-if="isCondition(cond)"
          v-model="model.conditions[index] as HavingCondition"
          :columns="columns"
          :aliases="aliases"
          @remove="removeCondition(index)"
        />

        <RawConditionItem
          v-else-if="isRaw(cond)"
          v-model="model.conditions[index] as RawCondition"
          @remove="removeCondition(index)"
        />

        <HavingConditionGroupBuilder
          v-else
          v-model="model.conditions[index] as HavingConditionGroup"
          :columns="columns"
          :aliases="aliases"
          removable
          @remove="removeCondition(index)"
        />
      </div>
    </div>

    <div class="flex gap-2 mt-3">
      <select v-model="newType" class="input">
        <option value="condition">Condition</option>
        <option value="group" :hidden="!canAddGroup">Group</option>
        <option value="raw">Raw</option>
      </select>

      <button @click="addCondition" class="btn">+ Add</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue";
import type {
  ColumnRef,
  HavingCondition,
  HavingConditionGroup,
  RawCondition,
} from "@/types/queryState";
import type { SelectableAlias, SelectableColumn } from "./HavingBuilder.vue";
import HavingConditionItem from "./HavingConditionItem.vue";
import RawConditionItem from "../WhereBuilder/RawConditionItem.vue";

const props = defineProps<{
  removable?: boolean;
  columns: SelectableColumn[];
  aliases: SelectableAlias[];
}>();

defineEmits(["remove"]);

const model = defineModel<HavingConditionGroup>({
  required: true,
});

const newType = ref<"condition" | "group" | "raw">("condition");
const canAddGroup = computed(() =>
  model.value.conditions.some((cond) => {
    if (isRaw(cond)) {
      return cond.sql.trim().length > 0;
    }

    if (!isCondition(cond)) {
      return false;
    }

    if (cond.type === "aggregate") {
      return !!cond.fn && hasColumnRef(cond.column);
    }

    if (cond.type === "group_column") {
      return hasColumnRef(cond.column);
    }

    return !!cond.alias?.trim() || hasColumnRef(cond.aliasColumn);
  }),
);

function hasColumnRef(column?: ColumnRef) {
  return typeof column === "object" && column?.columnId !== undefined;
}

function isRaw(cond: unknown): cond is RawCondition {
  return !!cond && typeof cond === "object" && "type" in cond && cond.type === "raw";
}

function isGroup(cond: unknown): cond is HavingConditionGroup {
  return !!cond && typeof cond === "object" && "conditions" in cond;
}

function isCondition(cond: unknown): cond is HavingCondition {
  return !isGroup(cond) && !isRaw(cond);
}

function createCondition(): HavingCondition {
  const firstColumn = props.columns[0];

  return {
    type: "group_column",
    column: firstColumn
      ? {
          tableId: firstColumn.tableId,
          columnId: firstColumn.columnId,
          columnName: firstColumn.columnName,
          dataType: firstColumn.dataType,
        }
      : ({} as ColumnRef),
    operator: "=",
    value: "",
  };
}

function createGroup(): HavingConditionGroup {
  return {
    type: "AND",
    conditions: [],
  };
}

function createRaw(): RawCondition {
  return {
    type: "raw",
    sql: "",
  };
}

function addCondition() {
  if (newType.value === "group") {
    if (!canAddGroup.value) {
      return;
    }

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

watch(canAddGroup, (allowed) => {
  if (!allowed && newType.value === "group") {
    newType.value = "condition";
  }
});
</script>
