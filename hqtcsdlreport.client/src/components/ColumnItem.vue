<template>
  <li class="hover:bg-primary/10 px-2 py-1 rounded cursor-pointer">
    {{ column.columnName }} ({{ column.dataType }})
  </li>
</template>

<script setup>
defineProps({
  column: Object
})
</script>