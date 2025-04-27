<template>
  <Button :loading="unsubscriptionInProgress" v-if="componentUserState=='paying'" severity="warn" @click="unsubscribeFromStripe" clickable>End monthly subscription</Button>
  <p v-else-if="componentUserState=='unsubscribed'">You've unsubscribed. You'll be able to subscribe again when your subscription ends.</p>
  <a v-else-if="componentUserState" :href="`https://buy.stripe.com/dR6aG5dWE2uW6iI000?client_reference_id=${authStore.userId}`">
    Subscribe via Stripe.
  </a>
  <Spinner v-else/>
</template>
<script setup lang="ts">

import Button from 'primevue/button'
import { onBeforeMount, ref } from 'vue'
import { useAuthStore } from '@/stores/auth'
import Spinner from '@/components/Common/Spinner.vue'

const componentUserState=ref();
const authStore=useAuthStore();
onBeforeMount(async ()=>{
  const userState=await authStore.getUserState();
  componentUserState.value=userState;
})

const unsubscriptionInProgress=ref(false)
async function unsubscribeFromStripe(){
  try{
    unsubscriptionInProgress.value=true
    await authStore.unsubscribeFromStripe();
    componentUserState.value="unsubscribed";
  }finally{
    unsubscriptionInProgress.value=false
  }
}

</script>


<style scoped>

</style>