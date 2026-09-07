import type { Theme } from 'vitepress'
import DefaultTheme from 'vitepress/theme'
import DemoMedia from './DemoMedia.vue'
import './style.css'

export default {
  extends: DefaultTheme,
  enhanceApp({ app }) {
    app.component('DemoMedia', DemoMedia)
  }
} satisfies Theme
