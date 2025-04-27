import { defineConfig } from 'vitepress'

export default defineConfig({
  title: 'Syntix',
  description: 'AI Code Interpreter',
  head: [
    ['meta', { name: 'theme-color', content: '#1e1e1e' }],
    ['meta', { name: 'darkreader-lock' }]
  ],
  themeConfig: {
    nav: [
      { text: 'Home', link: '/' },
    ],
    socialLinks: [
      { icon: 'github', link: 'https://github.com/syntix' }
    ]
  }
})