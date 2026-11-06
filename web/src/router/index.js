import { createRouter, createWebHistory } from 'vue-router'
import NewsListView from '../views/NewsListView.vue'
import NewsDetailView from '../views/NewsDetailView.vue'
import DemoView from '../views/DemoView.vue'
import PublicVerifyView from '../views/PublicVerifyView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', redirect: '/news' },
    { path: '/news', name: 'news', component: NewsListView },
    { path: '/news/:id', name: 'news-item', component: NewsDetailView },
    { path: '/verify/:code', name: 'public-verify', component: PublicVerifyView, meta: { public: true } },
    { path: '/demo', name: 'demo', component: DemoView },
  ],
})

export default router