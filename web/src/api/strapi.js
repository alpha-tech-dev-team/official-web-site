const BASE_URL = import.meta.env.VITE_STRAPI_URL ?? 'http://localhost:1337'
const NEWS_PATH = '/api/news-items'

export class ApiError extends Error {
  constructor(status) {
    super(`Strapi вернул ${status}`)
    this.status = status
  }
}

async function request(path) {
  const res = await fetch(`${BASE_URL}${path}`)
  if (!res.ok) throw new ApiError(res.status)
  return res.json()
}

export function mediaUrl(url) {
  if (!url) return ''
  return url.startsWith('http') ? url : `${BASE_URL}${url}`
}



export function imageAttrs(media, size = 'medium', fallbackAlt = '') {
  if (!media) return null
  const file = media.formats?.[size] ?? media
  return {
    src: mediaUrl(file.url),
    width: file.width,
    height: file.height,
    alt: media.alternativeText || fallbackAlt,
    loading: 'lazy',
  }
}

export async function fetchNewsList() {
  const json = await request(`${NEWS_PATH}?populate=cover&sort=publicationDate:desc&pagination[pageSize]=12`)
  return json.data
}

export async function fetchNewsItem(documentId) {
  const json = await request(`${NEWS_PATH}/${encodeURIComponent(documentId)}?populate=cover`)
  return json.data
}