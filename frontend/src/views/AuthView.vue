<template>
  <div class="auth-forms">
    <GoogleLogin />
  </div>
</template>

<script setup lang="ts">
import GoogleLogin from '@/components/User/GoogleLogin.vue'
</script>

<style scoped>
.auth-forms {
  display: flex;
  flex-direction: row;
  gap: 30px;
}
</style>
