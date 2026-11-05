<script setup>
import { RouterLink, RouterView } from 'vue-router'
</script>

<template>
  <header class="site-header">
    <RouterLink to="/news">Новости</RouterLink>&nbsp;&nbsp;
    <RouterLink to="/demo">Демо-заявки</RouterLink>
  </header>
  <main class="container">
    <RouterView />
  </main>
</template>