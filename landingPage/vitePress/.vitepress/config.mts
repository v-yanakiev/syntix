import { defineConfig } from 'vitepress'

// https://vitepress.dev/reference/site-config
export default defineConfig({
  title: "Syntix",
  description: "AI Code Interpreter",
  themeConfig: {
    // https://vitepress.dev/reference/default-theme-config
    nav: [
      { text: 'App', link: 'https://app.syntix.pro' },
    ]
  }
})
