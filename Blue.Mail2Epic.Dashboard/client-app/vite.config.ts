import {defineConfig} from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

function resolveBackendUrl() {
  if (process.env.ASPNETCORE_URLS) {
    const url = process.env.ASPNETCORE_URLS.split(';')[0]
    console.log('Vite proxy target:', url)
    return url
  }

  throw new Error('No API host is found')
}

export default defineConfig({
  plugins: [
      react(),
      tailwindcss()
  ],
  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      '/api': {
        target: resolveBackendUrl(),
        changeOrigin: true,
        secure: false,
      }
    }
  }
})