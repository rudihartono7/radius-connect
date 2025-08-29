// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  devtools: { enabled: true },
  compatibilityDate: '2025-08-23',
  ssr: false,
  modules: [
    '@pinia/nuxt',
    '@nuxtjs/tailwindcss',
    '@vueuse/nuxt'
  ],
  runtimeConfig: {
    public: {
      apiBase: process.env.NUXT_PUBLIC_API_BASE || 'http://localhost:5069/api'
    }
  },
  // Disable HTTPS in development
  devServer: {
    https: false
  },
  typescript: {
    strict: true
  },
  // Disable automatic HTTPS redirects
  nitro: {
    devProxy: {
      '/api': {
        target: process.env.NUXT_PUBLIC_API_BASE || '',
        secure: false,
        changeOrigin: true
      }
    },
    // Exclude native modules that cause build issues in Docker
    experimental: {
      wasm: false
    },
    rollupConfig: {
      external: ['better-sqlite3']
    }
  },
  app: {
    head: {
      title: 'RadiusConnect',
      meta: [
        { charset: 'utf-8' },
        { name: 'viewport', content: 'width=device-width, initial-scale=1' },
        { name: 'description', content: 'RADIUS Management System' }
      ]
    }
  }
})
