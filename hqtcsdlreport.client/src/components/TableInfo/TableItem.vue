<template>
  <li>
    <!-- TABLE -->
    <div class="flex items-center gap-2">
      <input type="checkbox" v-model="table.checked" />

      <div
        @click="toggle"
        class="cursor-pointer font-semibold text-dark hover:text-primary w-full flex items-center gap-2"
      >
        <svg
          v-if="!expanded"
          xmlns="http://www.w3.org/2000/svg"
          viewBox="0 0 20 20"
          fill="currentColor"
          class="size-4"
        >
          <path
            d="M10.75 4.75a.75.75 0 0 0-1.5 0v4.5h-4.5a.75.75 0 0 0 0 1.5h4.5v4.5a.75.75 0 0 0 1.5 0v-4.5h4.5a.75.75 0 0 0 0-1.5h-4.5v-4.5Z"
          />
        </svg>

        <svg
          v-else
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
          stroke-width="1.5"
          stroke="currentColor"
          class="size-4"
        >
          <path stroke-linecap="round" stroke-linejoin="round" d="M5 12h14" />
        </svg>

        {{ table.tableName }}
      </div>
    </div>

    <!-- EXPAND -->
    <div v-if="expanded" class="ml-4 mt-2 border-l border-gray-200 pl-2">
      <ul class="space-y-1 text-sm text-gray-600">
        <ColumnItem
          v-for="col in table.columns"
          :key="col.columnId"
          :column="col"
          @toggle-column="handleToggleColumn"
        />
      </ul>
    </div>
  </li>
</template>

<script setup lang="ts">
import { ref, watch } from "vue";
import ColumnItem from "./ColumnItem.vue";
import type { TableMetadata } from "@/types/database";
import type { ColumnMetadata } from "@/types/database";

const props = defineProps<{
  table: TableMetadata;
}>();

const expanded = ref<boolean>(false);

const toggle = (): void => {
  expanded.value = !expanded.value;
};

const emit = defineEmits<{
  (e: "toggle-column", column: ColumnMetadata, table: TableMetadata): void;
}>();

const handleToggleColumn = (column: ColumnMetadata) => {
  emit("toggle-column", column, props.table);
};

watch(
  () => props.table.checked,
  (newVal: boolean) => {
    if (newVal) {
      props.table.columns.forEach((col) => {
        col.checked = true;
        expanded.value = true;
        emit("toggle-column", col, props.table);
      });
    } else {
      const allChecked = props.table.columns.every((col) => col.checked);

      if (allChecked) {
        props.table.columns.forEach((col) => {
          col.checked = false;
          emit("toggle-column", col, props.table);
        });
      }
    }
  },
);

watch(
  () => props.table.columns.map((col) => col.checked),
  (newValues: boolean[]) => {
    const allChecked = newValues.every((v) => v === true);
    props.table.checked = allChecked;
  },
);
</script>
