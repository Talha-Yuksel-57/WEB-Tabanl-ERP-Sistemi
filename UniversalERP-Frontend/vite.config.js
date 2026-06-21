import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    // .NET HTTPS çoğu zaman self-signed; tarayıcıda sertifikayı kabul etmen gerekebilir.
    port: 5173
  }
})
