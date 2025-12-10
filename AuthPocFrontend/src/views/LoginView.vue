<template>
  <div class="login">
    <button @click="doLogin">Login to Github</button>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuth, type User } from '@/composables/useAuth'

const router = useRouter()
const auth = useAuth()
const frontendOrigin = window.location.origin
const url = new URL(`https://localhost:7055/auth/login?origin=${encodeURIComponent(frontendOrigin)}`)
const urlString  = url.toString()

const doLogin = () => {
  const width = 600, height = 700
  const left = (screen.width - width) / 2
  const top = (screen.height - height) / 2

  const handleMessage = (event: MessageEvent) => {
    if (event.origin !== 'https://localhost:7055') {
      return
    }

    if (!event.data || typeof event.data !== 'object') {
      return
    }

    const user = event.data as User

    if (!user) {
      return
    }

    try {
      auth.init(JSON.stringify({ ...user }))
    } catch (err) {
      console.error('[auth] init failed', err)
    } finally {
      clearTimeout(to)
      window.removeEventListener('message', handleMessage)
      try { popup?.close() } catch {}
      router.push('/dashboard')
    }
  }

  window.addEventListener('message', handleMessage)

  const popup = window.open(
    urlString,
    '_blank',
    `width=${width},height=${height},top=${top},left=${left}`
  )

  if (!popup) {
    window.removeEventListener('message', handleMessage)
    console.warn('[auth] popup blocked')
    return
  }

  const timeoutMs = 2 * 60 * 1000
  const to = setTimeout(() => {
    console.warn('[auth] auth message timeout; removing listener')
    window.removeEventListener('message', handleMessage)
    try { popup.close() } catch {}
  }, timeoutMs)
}
</script>

<style>
@media (min-width: 1024px) {
  .login {
    min-height: 100vh;
    display: flex;
    align-items: center;
  }
}
</style>
