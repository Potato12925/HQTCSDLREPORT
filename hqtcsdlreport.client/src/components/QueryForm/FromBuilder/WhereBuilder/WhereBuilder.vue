<template>
  <div class="mb-6">
    <h3 class="font-semibold text-primary mb-3 text-lg">WHERE</h3>

    <ConditionGroupBuilder
      v-if="where"
      v-model="where.group"
    />
  </div>
</template>

<script setup lang="ts">
import type { QueryState, WhereClause, ConditionGroup } from "@/types/queryState";
import ConditionGroupBuilder from "./ConditionGroupBuilder.vue";
import { computed } from "vue";

const props = defineProps<{
  state: QueryState;
}>();

// computed để bind trực tiếp vào queryState.where
const where = computed<WhereClause>({
  get() {
    // nếu chưa có where thì khởi tạo
    if (!props.state.where) {
      props.state.where = {
        mode: "builder",
        group: {
          type: "AND",
          conditions: [],
        } as ConditionGroup,
      };
    }
    return props.state.where;
  },
  set(val) {
    props.state.where = val;
  },
});
</script>