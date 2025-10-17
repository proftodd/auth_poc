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
import { useAuth } from '../composables/useAuth'

const route = useRoute()
const router = useRouter()
const { user, init, refreshTokenIfNeeded } = useAuth()
const searchResult = ref<SearchResult | null>(null)

onMounted(async () => {
    const rawToken = route.query.token
    let tokenQuery: string | undefined
    if (Array.isArray(rawToken)) {
        tokenQuery = typeof rawToken[0] === 'string' ? rawToken[0] : undefined
    } else if (typeof rawToken === 'string') {
        tokenQuery = rawToken
    } else {
        tokenQuery = undefined
    }
    init(tokenQuery)

    if (user.value) {
        router.replace({ path: route.path, query: {} })
    } else {
        router.push('/')
        return
    }

    await refreshTokenIfNeeded()
})

const querySubstances = async () => {
    if (!user.value) {
        return
    }
    const ok = await refreshTokenIfNeeded()
    if (!ok) {
        return
    }
    try {
        fetch("https://localhost:7063/search?st=h2o&st=water", {
                method: "GET",
                headers: {
                    "Authorization": `Bearer ${user.value!.Jwt}`
                }
            })
            .then((res) => res.json())
            .then((data) => searchResult.value = data)
    } catch (err) {
        console.error('Search failed', err)
    }
}
</script>
