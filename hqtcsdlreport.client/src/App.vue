<script setup>
</script>

<template>
  <header class="bg-white shadow-md">
    <nav class="max-w-7xl mx-auto px-6 py-4 flex items-center justify-between">

      <!-- Logo -->
      <div class="text-xl font-bold text-blue-600">
        HQTCSDL
      </div>

      <!-- Menu -->
      <div class="flex gap-6 text-gray-700 font-medium">

        <router-link
          to="/"
          class="hover:text-blue-600 transition-colors duration-200"
          active-class="text-blue-600 border-b-2 border-blue-600 pb-1"
        >
          Trang chủ
        </router-link>

        <router-link
          to="/about"
          class="hover:text-blue-600 transition-colors duration-200"
          active-class="text-blue-600 border-b-2 border-blue-600 pb-1"
        >
          Giới thiệu
        </router-link>

      </div>

    </nav>
  </header>
  
  <main>
    <router-view />
  </main>
</template>

<style scoped>
header {
  line-height: 1.5;
}

.logo {
  display: block;
  margin: 0 auto 2rem;
}

@media (min-width: 1024px) {
  header {
    display: flex;
    place-items: center;
    padding-right: calc(var(--section-gap) / 2);
  }

  .logo {
    margin: 0 2rem 0 0;
  }

  header .wrapper {
    display: flex;
    place-items: flex-start;
    flex-wrap: wrap;
  }
}
</style>
