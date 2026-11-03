import { h } from 'vue'
import { mediaUrl } from '../api/strapi.js'

function isSafeUrl(url) {
  try {
    const parsed = new URL(url, 'https://example.invalid')
    return ['http:', 'https:', 'mailto:', 'tel:'].includes(parsed.protocol)
  } catch {
    return false
  }
}

function renderChildren(node) {
  return (node.children ?? []).map(renderNode)
}

function renderText(node) {
  let el = node.text ?? ''
  if (node.code) el = h('code', [el])
  if (node.strikethrough) el = h('s', [el])
  if (node.underline) el = h('u', [el])
  if (node.italic) el = h('em', [el])
  if (node.bold) el = h('strong', [el])
  return el
}

function renderNode(node) {
  switch (node.type) {
    case 'text':
      return renderText(node)
    case 'paragraph':
      return h('p', renderChildren(node))
    case 'heading':
      return h(`h${Math.min(Math.max(node.level ?? 2, 1), 6)}`, renderChildren(node))
    case 'quote':
      return h('blockquote', renderChildren(node))
    case 'code':
      return h('pre', [h('code', (node.children ?? []).map((c) => c.text ?? '').join(''))])
    case 'list':
      return h(node.format === 'ordered' ? 'ol' : 'ul', renderChildren(node))
    case 'list-item':
      return h('li', renderChildren(node))
    case 'link':
      return isSafeUrl(node.url)
        ? h('a', { href: node.url, target: '_blank', rel: 'noopener noreferrer' }, renderChildren(node))
        : h('span', renderChildren(node))
    case 'image': {
      const img = node.image
      if (!img?.url) return null
      return h('img', {
        src: mediaUrl(img.url),
        alt: img.alternativeText ?? '',
        width: img.width,
        height: img.height,
        loading: 'lazy',
      })
    }
    default:
      return null
  }
}

export default {
  name: 'BlocksRenderer',
  props: {
    blocks: { type: Array, default: () => [] },
  },
  setup(props) {
    return () => props.blocks.map(renderNode)
  },
}