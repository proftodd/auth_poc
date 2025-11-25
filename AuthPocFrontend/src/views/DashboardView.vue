<template>
  <div v-if="user">
    <h1>Welcome to MyOrg {{ user.Name ?? user.Login }}!</h1>
    <button @click="querySubstances">Query Substances</button>
    <button @click="logout">Logout</button>
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
const { user, logout } = useAuth()

const searchResult = ref<SearchResult | null>(null)

onMounted(async () => {
  try {
    if (user.value) {
      router.replace({ path: route.path, query: {} })
    } else {
      router.push('/')
      return
    }
  } catch (err) {
    console.error(err)
  }
})

const querySubstances = async () => {
    if (!user.value) {
        return
    }
    // const ok = await refreshTokenIfNeeded()
    // if (!ok) {
    //     return
    // }
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
