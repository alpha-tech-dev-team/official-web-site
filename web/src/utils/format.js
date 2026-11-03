export function formatDate(value) {
  if (!value) return ''
  return new Date(value).toLocaleDateString('ru-RU', { timeZone: 'UTC' })
}