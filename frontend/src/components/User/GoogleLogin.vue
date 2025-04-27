<template>
  <Button @click="redirectToGoogle">Google Login/Signup</Button>
</template>

<script setup lang="ts">
import Button from 'primevue/button'
import { apiConfig } from '@/api/apiConfig'

function redirectToGoogle(){
  window.location.href = apiConfig.apiUrl+'googleLogin';
}
</script>

<style scoped>

</style>