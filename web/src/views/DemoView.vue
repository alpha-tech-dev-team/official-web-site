<script setup>
import { ref, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import {
  login as apiLogin,
  setToken,
  clearToken,
  getToken,
  fetchApplications,
  createApplication,
  submitApplication,
  startVerification,
  fetchVerificationStatus,
  previewApplication,
  downloadPdf,
  cancelApplication,
  changeApplicationPhone,
} from '../api/applications.js'

const loginField = ref('')
const passwordField = ref('')
const authorized = ref(Boolean(getToken()))

const nameField = ref('')
const phoneField = ref('')
const courseField = ref('')
const commentField = ref('')

const applications = ref([])
const sessions = ref([])
const previewText = ref('здесь появится текст заявления...')
const message = ref('')

async function loadApplications() {
  if (!authorized.value) return
  try {
    applications.value = (await fetchApplications()).reverse()
    message.value = ''
  } catch (e) {
    message.value = 'Не удалось загрузить заявки'
  }
}

async function doLogin() {
  try {
    const { token } = await apiLogin(loginField.value, passwordField.value)
    setToken(token)
    authorized.value = true
    await loadApplications()
  } catch (e) {
    authorized.value = false
    message.value = e instanceof TypeError
      ? 'API не отвечает — запущен ли dotnet run в папке src?'
      : 'Неверный логин или пароль'
  }
}

function doLogout() {
  clearToken()
  authorized.value = false
  applications.value = []
}

async function onCreate() {
  message.value = ''
  try {
    const created = await createApplication(nameField.value, phoneField.value, {
      course: courseField.value,
      comment: commentField.value,
    })
    const saved = JSON.parse(localStorage.getItem('phones') ?? '{}')
    saved[created.id] = phoneField.value
    localStorage.setItem('phones', JSON.stringify(saved))
    await loadApplications()
  } catch (e) {
    message.value = typeof e === 'object' ? JSON.stringify(e.errors ?? e, null, 2) : String(e)
  }
}

async function onSubmit(id) {
  message.value = ''
  try {
    await submitApplication(id)
    await loadApplications()
  } catch (e) {
    message.value = e.error ?? 'Ошибка подачи'
  }
}

async function onChangePhone(app) {
  const phone = prompt('Новый телефон заявки:', '')
  if (!phone) return

  message.value = ''
  try {
    await changeApplicationPhone(app.id, phone)
    const saved = JSON.parse(localStorage.getItem('phones') ?? '{}')
    saved[app.id] = phone
    localStorage.setItem('phones', JSON.stringify(saved))
    await loadApplications()
  } catch (e) {
    message.value = typeof e === 'object' ? JSON.stringify(e.errors ?? e, null, 2) : String(e)
  }
}

async function onCancel(app) {
  if (!confirm(`Отменить заявку ${app.number ?? app.publicCode}?`)) return

  message.value = ''
  try {
    await cancelApplication(app.id)
    await loadApplications()
  } catch (e) {
    message.value = e.error ?? 'Ошибка отмены'
  }
}

async function onStartVerification(app) {
  message.value = ''
  const saved = JSON.parse(localStorage.getItem('phones') ?? '{}')
  const phone = saved[app.id] ?? prompt('Телефон заявки:', '')
  if (!phone) return
  try {
    const r = await startVerification(app.id, phone)
    const session = { sessionId: r.sessionId, link: r.link, status: 'pending', reason: '' }
    sessions.value.unshift(session)
    pollSession(session)
  } catch (e) {
    message.value = typeof e === 'object' ? JSON.stringify(e.errors ?? e, null, 2) : String(e)
  }
}

function pollSession(session) {
  setTimeout(async () => {
    try {
      const r = await fetchVerificationStatus(session.sessionId)
      session.status = r.status
      session.reason = r.reason
      if (r.status === 'pending') pollSession(session)
    } catch {
      session.status = 'lost'
    }
  }, 2000)
}

async function onPreview(id) {
  message.value = ''
  try {
    previewText.value = await previewApplication(id)
  } catch {
    message.value = 'Не удалось получить текст'
  }
}

async function onDownloadPdf(id) {
  message.value = ''
  try {
    await downloadPdf(id)
  } catch {
    message.value = 'Заявку нужно подать перед PDF'
  }
}

onMounted(loadApplications)
</script>

<template>
  <section class="demo-block">
    <h2>Вход админа</h2>
    <form class="row" @submit.prevent="doLogin">
      <input v-model="loginField" placeholder="логин" />
      <input v-model="passwordField" type="password" placeholder="пароль" />
      <button type="submit">Войти</button>
      <button type="button" @click="doLogout">Выйти</button>
      <span>{{ authorized ? 'авторизован' : 'не авторизован' }}</span>
    </form>
  </section>

  <section class="demo-block">
    <h2>Новая заявка</h2>
    <form class="row" @submit.prevent="onCreate">
      <input v-model="nameField" placeholder="ФИО" style="width: 280px" />
      <input v-model="phoneField" placeholder="+375 29 123-45-67" style="width: 200px" />
      <input v-model="courseField" placeholder="Курс" />
      <input v-model="commentField" placeholder="Комментарий" />
      <button type="submit">Создать</button>
    </form>
  </section>

  <section class="demo-block">
    <h2>Заявки</h2>
    <table class="apps">
      <thead>
        <tr>
          <th>#</th><th>ФИО</th><th>Телефон</th><th>Код</th><th>Курс</th><th>Статус</th><th>Действия</th>
        </tr>
      </thead>
      <tbody>
        <tr v-if="!authorized">
          <td colspan="7">войдите как админ, чтобы видеть заявки</td>
        </tr>
        <tr v-else-if="applications.length === 0">
          <td colspan="7">заявок нет — создайте выше</td>
        </tr>
        <tr v-for="(a, i) in applications" v-else :key="a.id">
          <td>{{ i + 1 }}</td>
          <td>{{ a.applicantName }}</td>
          <td>{{ a.phone }}</td>
          <td><code class="public-code">{{ a.publicCode }}</code></td>
          <td>{{ a.data?.course ?? '—' }}</td>
          <td>
            <span :class="['badge', a.status]">
              {{ a.status }} {{ a.number ?? '' }}
            </span>
          </td>
          <td>
            <button v-if="a.status === 'draft'" @click="onSubmit(a.id)">Подать</button>
            <button
              v-if="a.status !== 'cancelled' && a.status !== 'rejected'"
              @click="onStartVerification(a)"
            >Верификация</button>
            <button @click="onPreview(a.id)">Текст</button>
            <button v-if="a.status !== 'draft'" @click="onDownloadPdf(a.id)">PDF</button>
            <button
              v-if="a.status !== 'cancelled' && a.status !== 'rejected'"
              @click="onChangePhone(a)"
            >Сменить телефон</button>
            <button v-if="a.status !== 'cancelled'" @click="onCancel(a)">Отменить</button>
            <RouterLink
              class="verify-link"
              :to="{ name: 'public-verify', params: { code: a.publicCode } }"
              target="_blank"
              rel="noopener"
            >Проверить</RouterLink>
          </td>
        </tr>
      </tbody>
    </table>
  </section>

  <section class="demo-block">
    <h2>Верификации по Telegram</h2>
    <p v-if="sessions.length === 0">пока пусто — нажмите «Верификация» у заявки</p>
    <div v-for="s in sessions" :key="s.sessionId" class="session">
      <span :class="['badge', s.status]">{{ s.status }}</span>
      <span v-if="s.reason" class="reason">{{ s.reason }}</span>
      <a :href="s.link" target="_blank">{{ s.link }}</a>
      <button @click="navigator.clipboard.writeText(s.link)">Копировать</button>
    </div>
  </section>

  <p v-if="message" class="error-message">{{ message }}</p>

  <section class="demo-block">
    <h2>Предпросмотр документа</h2>
    <pre>{{ previewText }}</pre>
  </section>
</template>

<style scoped>
.demo-block {
  border: 1px solid #ddd;
  border-radius: 10px;
  padding: 14px 18px;
  margin-bottom: 16px;
}
.demo-block h2 {
  margin: 0 0 10px;
  font-size: 17px;
}
.row {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  align-items: center;
}
.row input {
  padding: 6px 8px;
}
button {
  padding: 6px 12px;
  cursor: pointer;
}
.apps {
  border-collapse: collapse;
  width: 100%;
}
.apps th,
.apps td {
  border: 1px solid #ddd;
  padding: 6px 8px;
  font-size: 14px;
  text-align: left;
}
.badge {
  display: inline-block;
  padding: 2px 10px;
  border-radius: 10px;
  font-size: 12px;
  color: #fff;
  background: gray;
}
.badge.submitted { background: #2196f3; }
.badge.pending { background: orange; }
.badge.verified { background: green; }
.badge.cancelled { background: #6b7280; }
.badge.rejected,
.badge.failed,
.badge.expired,
.badge.lost { background: red; }
.public-code {
  font-size: 12px;
  white-space: nowrap;
}
.verify-link {
  display: inline-block;
  padding: 6px 12px;
  margin-left: 4px;
  color: var(--accent);
}
.session {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 6px;
  flex-wrap: wrap;
}
.session a { color: #2196f3; word-break: break-all; }
.reason { color: #b00; font-size: 13px; }
.error-message {
  color: darkred;
  white-space: pre-wrap;
  font-family: monospace;
}
pre {
  background: #f6f6f6;
  padding: 12px;
  white-space: pre-wrap;
}
</style>
