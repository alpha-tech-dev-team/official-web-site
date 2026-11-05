const BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5081'

export function getToken() {
  return localStorage.getItem('token') ?? ''
}

export function setToken(token) {
  localStorage.setItem('token', token)
}

export function clearToken() {
  localStorage.removeItem('token')
}

async function request(path, { method = 'GET', body } = {}) {
  const headers = { 'Content-Type': 'application/json' }
  const token = getToken()
  if (token) headers.Authorization = `Bearer ${token}`

  const res = await fetch(`${BASE_URL}${path}`, {
    method,
    headers,
    body: body === undefined ? undefined : JSON.stringify(body),
  })

  if (res.headers.get('content-type')?.includes('application/json')) {
    const json = await res.json()
    if (!res.ok) throw json
    return json
  }

  const text = await res.text()
  if (!res.ok) throw new Error(text || res.status)
  return text
}

export function login(login, password) {
  return request('/api/auth/login', { method: 'POST', body: { login, password } })
}

export function fetchApplications() {
  return request('/api/admin/applications')
}

export function createApplication(applicantName, phone, data) {
  return request('/api/applications', { method: 'POST', body: { applicantName, phone, data } })
}

export function submitApplication(id) {
  return request(`/api/applications/${id}/submit`, { method: 'POST' })
}

export function startVerification(applicationId, phone) {
  return request('/api/verification/start', { method: 'POST', body: { applicationId, phone } })
}

export function fetchVerificationStatus(sessionId) {
  return request(`/api/verification/${sessionId}`)
}

export function previewApplication(id) {
  return request(`/api/admin/applications/${id}/preview`)
}

export async function downloadPdf(id, fileName) {
  const res = await fetch(`${BASE_URL}/api/admin/applications/${id}/pdf`, {
    headers: { Authorization: `Bearer ${getToken()}` },
  })
  if (!res.ok) throw new Error(`PDF: ${res.status}`)
  const blob = await res.blob()
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `application-${id}.pdf`
  a.click()
  URL.revokeObjectURL(url)
}
