<template>
  <div class="login">
    <a :href="urlString">Login to Github</a>
  </div>
</template>

<script setup lang="ts">
const clientId = 'Iv1.799eb98fe903a65c'
const redirectUri = 'https://simply-included-dassie.ngrok-free.app/Auth/Login'
const state = 'abc123'
const allowSignup = 'false'
const url = new URL('https://github.com/login/oauth/authorize')
url.searchParams.append('client_id', clientId)
url.searchParams.append('redirect_uri', redirectUri)
url.searchParams.append('state', state)
url.searchParams.append('allow_signup', allowSignup)
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
