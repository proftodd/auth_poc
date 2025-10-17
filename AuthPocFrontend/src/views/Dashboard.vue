<template>
  <div v-if="user">
    <h1>Welcome to MyOrg {{ user.Name ?? user.Login }}!</h1>
    <button @click="querySubstances">Query Substances</button>
    <div v-if="searchResult && searchResult.entities">
        <p>{{ searchResult.searchTerms.join(',') }}</p>
        <p :key="e.id" v-for="e in searchResult.entities">{{ e.descriptors.map(d => d.desc).join(',') }}</p>
    </div>
  </div>
  <div v-else>
    <p>Loading user...</p>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import type { SearchResult } from '../models/searchResults.ts'
import type { User } from '../models/user.ts'

const user = ref<User | null>(null)
const searchResult = ref<SearchResult | null>(null)
const route = useRoute()
const router = useRouter()

onMounted(() => {
    const userJson = Array.isArray(route.query.token)
        ? route.query.token[0]
        : route.query.token
    if (userJson) {
        user.value = JSON.parse(userJson) as User
        localStorage.setItem("authedUser", userJson)
        router.replace("/dashboard")
    } else {
        const savedUser = localStorage.getItem("authedUser")
        if (savedUser) {
            user.value = JSON.parse(savedUser) as User
        } else {
            router.push('/')
        }
    }
})

const querySubstances = () => {
    fetch("https://localhost:7063/search?st=h2o&st=water", {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${user.value!.Jwt}`
            }
        })
        .then((res) => res.json())
        .then((data) => searchResult.value = data)
}
</script>
