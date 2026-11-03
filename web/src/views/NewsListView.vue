<script setup>
import { ref, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import { fetchNewsList, imageAttrs } from '../api/strapi.js'
import { formatDate } from '../utils/format.js'

const items = ref([])
const loading = ref(true)
const error = ref('')

onMounted(async () => {
  try {
    items.value = await fetchNewsList()
  } catch (e) {
    items.value = []
    if (e.status === 404) error.value = 'Новости не найдены'
    else if (e instanceof TypeError) error.value = 'Сервер недоступен'
    else error.value = e.message
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <section>
    <h1>Новости</h1>
    <p v-if="loading">Загрузка...</p>
    <p v-else-if="error" class="error">{{ error }}</p>
    <p v-else-if="items.length === 0">Новостей пока нет.</p>

    <ul v-else class="news-list">
      <li v-for="item in items" :key="item.documentId" class="news-card">
        <RouterLink :to="`/news/${item.documentId}`">
          <img v-if="item.cover" v-bind="imageAttrs(item.cover, 'medium', item.title)" />
          <div class="body">
            <h2>{{ item.title }}</h2>
            <span class="date">{{ formatDate(item.publicationDate) }}</span>
          </div>
        </RouterLink>
      </li>
    </ul>
  </section>
</template>
