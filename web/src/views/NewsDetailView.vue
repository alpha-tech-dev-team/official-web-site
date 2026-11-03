<script setup>
import { ref, watch } from 'vue'
import { useRoute, RouterLink } from 'vue-router'
import { fetchNewsItem, imageAttrs } from '../api/strapi.js'
import { formatDate } from '../utils/format.js'
import BlocksRenderer from '../components/BlocksRenderer.js'

const route = useRoute()
const item = ref(null)
const loading = ref(true)
const error = ref('')

async function load(id) {
  loading.value = true
  error.value = ''
  try {
    item.value = await fetchNewsItem(id)
  } catch (e) {
    item.value = null
    if (e.status === 404) error.value = 'Новость не найдена'
    else if (e instanceof TypeError) error.value = 'Сервер недоступен'
    else error.value = e.message
  } finally {
    loading.value = false
  }
}

// Загружаем при открытии страницы и при смене адреса
watch(() => route.params.id, load, { immediate: true })
</script>

<template>
  <article class="article">
    <RouterLink to="/news">← Все новости</RouterLink>

    <p v-if="loading">Загрузка...</p>
    <p v-else-if="error" class="error">{{ error }}</p>

    <template v-else-if="item">
      <h1>{{ item.title }}</h1>
      <span class="date">{{ formatDate(item.publicationDate) }}</span>
      <img v-if="item.cover" v-bind="imageAttrs(item.cover, 'large', item.title)" />
      <BlocksRenderer :blocks="item.text ?? []" />
    </template>
  </article>
</template>