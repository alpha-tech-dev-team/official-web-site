import { defineStore } from 'pinia'
import { ref } from 'vue'
import { fetchNewsList } from '../api/strapi.js'

export const useNewsStore = defineStore('news', () => {
  const items = ref([])
  const loading = ref(false)
  const error = ref('')

  async function load() {
    loading.value = true
    error.value = ''
    try {
      items.value = await fetchNewsList()
    } catch (e) {
      items.value = []
      error.value = e instanceof TypeError ? 'сервер недоступен' : e.message
    } finally {
      loading.value = false
    }
  }

  return { items, loading, error, load }
})