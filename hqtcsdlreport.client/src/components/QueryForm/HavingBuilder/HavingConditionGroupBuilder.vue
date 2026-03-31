<template>
  <div class="border border-primary/20 rounded-xl p-3 mb-3 bg-light">
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
        <option value="group">Group</option>
      </select>

      <button @click="addCondition" class="btn">+ Add</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import type { ColumnRef, HavingCondition, HavingConditionGroup } from "@/types/queryState";
import type { SelectableAlias, SelectableColumn } from "./HavingBuilder.vue";
import HavingConditionItem from "./HavingConditionItem.vue";

const props = defineProps<{
  removable?: boolean;
  columns: SelectableColumn[];
  aliases: SelectableAlias[];
}>();

defineEmits(["remove"]);

const model = defineModel<HavingConditionGroup>({
  required: true,
});

const newType = ref<"condition" | "group">("condition");

function isGroup(cond: unknown): cond is HavingConditionGroup {
  return !!cond && typeof cond === "object" && "conditions" in cond;
}

function isCondition(cond: unknown): cond is HavingCondition {
  return !isGroup(cond);
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

function addCondition() {
  if (newType.value === "group") {
    model.value.conditions.push(createGroup());
    return;
  }

  model.value.conditions.push(createCondition());
}

function removeCondition(index: number) {
  model.value.conditions.splice(index, 1);
}
</script>
