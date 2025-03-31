<template>
  <div class="login">
    <button @click="doLogin">Login to Github</button>
  </div>
</template>

<script setup lang="ts">
const url = new URL('http://localhost:5164/auth/login')
const urlString  = url.toString()

const doLogin = () => {
  window.location.href = urlString
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
