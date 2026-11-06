<script setup>
import { ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { fetchPublicVerification } from '../api/applications.js'

const route = useRoute()
const state = ref('loading')
let lastRequest = 0

async function check(code) {
  const requestId = ++lastRequest
  state.value = 'loading'

  if (!code) {
    state.value = 'not_verified'
    return
  }

  try {
    const result = await fetchPublicVerification(code)
    if (requestId === lastRequest) {
      state.value = result.verified ? 'verified' : 'not_verified'
    }
  } catch {
    if (requestId === lastRequest) {
      state.value = 'error'
    }
  }
}

watch(
  () => route.params.code,
  (code) => check(String(code ?? '')),
  { immediate: true },
)
</script>

<template>
  <section class="verify-page" aria-live="polite">
    <p v-if="state === 'loading'" class="loading">Проверяем…</p>
    <p v-else-if="state === 'verified'" class="ok">Номер телефона подтверждён</p>
    <p v-else-if="state === 'not_verified'" class="no">Номер телефона не подтверждён</p>
    <p v-else class="failed">Не удалось проверить. Попробуйте позже.</p>
  </section>
</template>

<style scoped>
.verify-page {
  min-height: 100vh;
  display: grid;
  place-items: center;
  padding: 24px;
  text-align: center;
}

.verify-page p {
  margin: 0;
  font-size: clamp(1.25rem, 4vw, 1.6rem);
  font-weight: 600;
}

.ok {
  color: #15803d;
}

.no {
  color: #b91c1c;
}

.loading,
.failed {
  color: #6b7280;
  font-weight: 500;
}
</style>
