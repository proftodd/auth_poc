<template>
  <div class="callback">
    <p>Logging you in...</p>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import axios from 'axios'
import { useRoute, useRouter } from 'vue-router'

const router = useRouter()
const route = useRoute()

onMounted(async () => {
  const code = route.query.code as string
  const state = route.query.state as string

  if (!code || !state) {
    console.error('Authorization code or state missing')
    router.push('/')
    return
  }

  try {
    const response = await axios.get('http://localhost:5164/Auth/Login', {
      params: { code, state }
    })
    const token = response.data.access_token
    if (!token) {
      console.error('No token received!')
      router.push('/')
      return
    }

    localStorage.setItem('authToken', response.data.access_token)

    router.push('/dashboard')
  } catch (error) {
    console.error('Login failed', error)
    router.push('/')
  }
})
</script>

<style>
.callback {
  text-align: center;
  margin-top: 50px;
}
</style>
