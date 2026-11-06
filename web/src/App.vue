<script setup>
import { computed } from 'vue'
import { RouterLink, RouterView, useRoute } from 'vue-router'

const route = useRoute()
const isPublic = computed(() => route.meta.public === true)
</script>

<template>
  <header v-if="!isPublic" class="site-header">
    <RouterLink to="/news">Новости</RouterLink>&nbsp;&nbsp;
    <RouterLink to="/demo">Демо-заявки</RouterLink>
  </header>
  <main :class="isPublic ? 'public-main' : 'container'">
    <RouterView />
  </main>
</template>
