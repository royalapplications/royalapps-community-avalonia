import { defineConfig } from 'vitepress'
import apiSidebar from '../api/sidebar.mjs'

const repository = 'https://github.com/royalapplications/royalapps-community-avalonia'

export default defineConfig({
  title: 'RoyalApps Avalonia',
  description: 'Reusable Avalonia controls, behaviors, and Windows Forms hosting with explicit lifetime management.',
  base: '/royalapps-community-avalonia/',
  cleanUrls: true,
  themeConfig: {
    logo: '/assets/RoyalApps_1024.png',
    nav: [
      { text: 'Guide', link: '/articles/getting-started' },
      { text: 'API', link: '/api/' },
      { text: 'GitHub', link: repository }
    ],
    sidebar: [
      { text: 'Guide', items: [
        { text: 'Getting started', link: '/articles/getting-started' },
        { text: 'Support matrix', link: '/articles/support-matrix' },
        { text: 'Ambient glow', link: '/articles/ambient-glow' },
        { text: 'Ring spinner', link: '/articles/ring-spinner' },
        { text: 'GridSplitter behavior', link: '/articles/grid-splitter' },
        { text: 'WinForms hosting', link: '/articles/winforms-hosting' },
        { text: 'Contributing', link: '/articles/contributing' }
      ] },
      { text: 'API reference', items: [{ text: 'Overview', link: '/api/' }, ...apiSidebar] }
    ],
    socialLinks: [{ icon: 'github', link: repository }],
    editLink: { pattern: `${repository}/edit/main/docs/:path`, text: 'Edit this page on GitHub' },
    search: { provider: 'local' },
    outline: [2, 3],
    footer: { message: 'MIT Licensed', copyright: 'Copyright Royal Apps GmbH' }
  }
})
