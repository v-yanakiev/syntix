import { defineStore } from 'pinia'
import { ref } from 'vue'
import { type ExecutionTemplate, useChatStore } from './chat'
import { apiCall, apiCallJsonBody } from '@/api/utils'
import { useCustomEnvironmentsStore } from '@/stores/customEnvironmentsStore'

export const useAuthStore = defineStore('auth', () => {
  const chatStore = useChatStore()
  const customEnvironmentsStore = useCustomEnvironmentsStore()
  
  const userId = ref(getCookie('user_id'))
  function getCookie(name: string): string | undefined {
    const toReturn = extractCookie(name,document.cookie)
    return toReturn;

    function extractCookie(name: string, cookieString: string): string | undefined {
      // Adjusted pattern to handle optional spaces more thoroughly
      const cookiePattern = new RegExp(`(?:^|\\s*;\\s*)${name}\\s*=\\s*([^;]*)`);
      const match = cookieString.match(cookiePattern);
      
      return match ? decodeURIComponent(match[1]) : undefined;
    }
  }
  async function signOut() {
    const response = await apiCall('logout', { method: 'POST' })
    if (!response.ok) return

    userId.value = undefined
    
    chatStore.resetToInitialState()
    customEnvironmentsStore.resetToInitialState()
  }

  async function deleteAccount() {
    const response = await apiCall('deleteAccount', { method: 'POST' })

    if (!response.ok) return
    userId.value = undefined
    chatStore.resetToInitialState()
    customEnvironmentsStore.resetToInitialState()
  }
  
  async function unsubscribeFromStripe(){
    const response=await apiCall("unsubscribe");
  }
  
  async function getUserState(){
    const response=await apiCall("getUserState");
    const json=await response.json();
    const state=json.state as string;
    return state;
  }
  return {
    userId,
    signOut,
    deleteAccount,
    unsubscribeFromStripe,
    getUserState
  }
})
