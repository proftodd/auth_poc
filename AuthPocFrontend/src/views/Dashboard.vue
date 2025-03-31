<template>
  <div>
    <h1>Welcome to MyOrg!</h1>
    <p v-if="token">Your access token: {{ token }}</p>
    <p v-else>Fetching token...</p>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'

const token = ref<string | null>(null)
const route = useRoute()
const router = useRouter()

onMounted(() => {
    const urlToken = Array.isArray(route.query.token)
        ? route.query.token[0]
        : route.query.token
    if (urlToken) {
        localStorage.setItem("authToken", urlToken)
        token.value = urlToken
        router.replace("/dashboard")
    } else {
        fetch("http://localhost:5164/auth/token")
            .then((res) => res.json())
            .then((data) => {
                if (data.access_token) {
                    token.value = data.access_token
                    localStorage.setItem("authToken", data.access_token)
                } else {
                    router.push('/')
                }
            })
            .catch(() => router.push('/'))
    }
})
</script>
