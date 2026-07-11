import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import fs from 'fs'
import path from 'path'

const useHttps = process.env.VITE_MAIN_ENV !== 'production'

export default defineConfig({
  plugins: [react()],
  server: useHttps ? {
    host: true,
    port: 3000,
    strictPort: true,
    https: {
      key: fs.readFileSync(path.resolve(__dirname, '../certs_local/cert.key')),
      cert: fs.readFileSync(path.resolve(__dirname, '../certs_local/cert.crt'))
    },
    watch: {
      usePolling: true
    }
  } : undefined,
})
