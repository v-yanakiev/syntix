<template>
  <div class="layout">
    <header class="header">
      <nav class="nav">
        <template v-if="authStore.userId">
          <RouterLink to="/">Home</RouterLink>
          <RouterLink to="/chat">Chats</RouterLink>
          <RouterLink to="/billing">Billing</RouterLink>
          <RouterLink to="/customEnvironments">Custom Environments</RouterLink>
          <a href="https://discord.gg/EsJPJdBaR3">Discord</a>
          <a href="https://github.com/v-yanakiev/syntix/issues">Feedback</a>
          <Button :loading="signingOutInProgress" severity="warn" @click="handleSignOut"
            >Sign out</Button
          >
          <Button
            :loading="accountDeletionInProgress"
            severity="danger"
            @click="handleDeleteAccount"
            >Delete account
          </Button>
        </template>
      </nav>
    </header>

    <main class="main">
      <!-- This will render the child route component -->
      <router-view></router-view>

      <!-- Show welcome content when no child route is active -->
      <div v-if="$route.path === '/'" class="welcome">
        <h1>Welcome to Syntix</h1>
        <template v-if="authStore.userId">
          <p>Good to see you!</p>
          <p>
            When you signed up, you got a 1 day free trial. After that, the price is 4.99 USD per
            month.
          </p>
          <p>
            In those 4.99 USD you also get 5 USD non-accumulating LLM credits - priced equally with
            the LLM provider (no overcharge).
          </p>
          <RouterLink to="/chat" class="cta-button"> Continue to Chat </RouterLink>
        </template>
        <template v-else>
          <br />
          <GoogleLogin />
          <br />
          <br />
          <p>Syntix is an AI code interpreter supporting Dockerfile-defined custom environments.</p>
          <br />
          <p>
            When you sign up, you get a 1 day free trial. After that, the price is 4.99 USD per
            month.
          </p>
          <p>
            In those 4.99 USD you also get 5 USD non-accumulating LLM credits - priced equally with
            the LLM provider (no overcharge).
          </p>
        </template>
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import { RouterLink } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import Button from 'primevue/button'
import router from '@/router'
import { ref } from 'vue'
import GoogleLogin from '@/components/User/GoogleLogin.vue'
defineOptions({
  name: 'HomeView'
})

const authStore = useAuthStore()

const signingOutInProgress = ref(false)

async function handleSignOut() {
  signingOutInProgress.value = true
  await authStore.signOut()
  await router.push('/auth')
  signingOutInProgress.value = false
}

const accountDeletionInProgress = ref(false)

async function handleDeleteAccount() {
  accountDeletionInProgress.value = true
  await authStore.deleteAccount()
  await router.push('/auth')
  accountDeletionInProgress.value = false
}
</script>
<style lang="css" scoped>
.nav {
  display: flex;
  flex-direction: row;
  gap: 10px;
  margin-bottom: 10px;
}
</style>
