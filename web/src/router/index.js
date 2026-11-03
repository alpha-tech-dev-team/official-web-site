import { createRouter, createWebHistory } from 'vue-router'
import NewsListView from '../views/NewsListView.vue'
import NewsDetailView from '../views/NewsDetailView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', redirect: '/news' },
    { path: '/news', name: 'news', component: NewsListView },
    { path: '/news/:id', name: 'news-item', component: NewsDetailView },
  ],
})

export default router