<template>
  <div class="login">
    <a :href="urlString">Login to Github</a>
  </div>
</template>

<script setup lang="ts">
const url = new URL('http://localhost:5164/Auth/Authorize')
const urlString  = url.toString()
console.log(urlString)
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
