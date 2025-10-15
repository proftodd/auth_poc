<template>
  <div v-if="user">
    <h1>Welcome to MyOrg {{ user.Name ?? user.Login }}!</h1>
    <p v-if="user.Jwt">Your access token: {{ user.Jwt }}</p>
    <p v-else>Fetching token...</p>
  </div>
  <div v-else>
    <p>Loading user...</p>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import type { User } from '../models/user.ts'

const user = ref<User | null>(null)
const route = useRoute()
const router = useRouter()

onMounted(() => {
    const userJson = Array.isArray(route.query.token)
        ? route.query.token[0]
        : route.query.token
    if (userJson) {
        const userObj = JSON.parse(userJson) as User
        user.value = userObj
        localStorage.setItem("authToken", userObj.Jwt)
        router.replace("/dashboard")
    } else {
        fetch("http://localhost:5164/auth/jwt")
            .then((res) => res.json())
            .then((data) => {
                if (data.access_token) {
                    localStorage.setItem("authToken", data.access_token)
                } else {
                    router.push('/')
                }
            })
            .catch(() => router.push('/'))
    }
})
</script>
