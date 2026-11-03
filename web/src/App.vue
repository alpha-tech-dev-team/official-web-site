<script setup>
import { RouterLink, RouterView } from 'vue-router'
</script>

<template>
  <header class="site-header">
    <RouterLink to="/news">Новости</RouterLink>
  </header>
  <main class="container">
    <RouterView />
  </main>
</template>