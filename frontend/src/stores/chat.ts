import { defineStore } from 'pinia'
import { apiCall } from '@/api/utils'
import { computed, type Ref, ref } from 'vue'
import { useCustomEnvironmentsStore } from './customEnvironmentsStore'

export type ExecutionTemplate = {
  name: string
  id: string
}
export const useChatStore = defineStore('chat', () => {
  const initialChatMessage = {
    role: 'ai',
    text: 'Hello. I will execute code based on your requests. I have full internet access and can install dependencies.'
  }
  const customEnvironmentsStore = useCustomEnvironmentsStore()
  const activeChatId = ref() as Ref<string | undefined>
  const activeExecutionEnvironmentTemplateId = ref() as Ref<number | undefined>
  const allUserChats = ref([]) as Ref<{ id: string; name: string }[]>

  const activeChatInitialMessages = ref() as Ref<{ role: string; text: string }[] | undefined>

  const onNewMessage = ref() as Ref<(newMessage: { role: string; text: string }) => void>
  const builtInTemplates = ref<ExecutionTemplate[] | undefined>(undefined)

  const allTemplates = computed(() => [
    ...customEnvironmentsStore.customEnvironments!,
    ...builtInTemplates.value!
  ])

  async function setActiveChat(chatId: string) {
    const response = await apiCall('message/getAllChatMessages/' + chatId)
    if (!response.ok) return

    activeChatId.value = chatId

    const data = await response.json()
    if (!data?.length) {
      activeChatInitialMessages.value = [initialChatMessage]
    } else {
      const messages = data.map((a: { role: string; content: string }) => ({
        role: a.role,
        text: a.content
      }))
      activeChatInitialMessages.value = messages
    }
    activeChatId.value = chatId
  }

  async function setUpChats() {
    await Promise.all([
      (async () => {
        const chats = await apiCall('chat/get')
        if (!chats.ok) return

        const chatsJson = await chats.json()

        allUserChats.value = chatsJson
      })(),
      customEnvironmentsStore.loadCustomEnvironments(),
      (async () => {
        const templateResponse = await apiCall('templates')
        if (!templateResponse.ok) return

        const templateJson = (await templateResponse.json()) as ExecutionTemplate[]
        builtInTemplates.value = templateJson
      })()
    ])
  }

  async function deleteChat(chatId: string) {
    const response = await apiCall('chat/delete/' + chatId, {
      method: 'DELETE'
    })
    if (!response.ok) return

    allUserChats.value = allUserChats.value.filter((a) => a.id != chatId)
    if (activeChatId.value == chatId) activeChatId.value = allUserChats.value[0]?.id
  }

  async function createChat() {
    const chatName = 'Chat ' + findNextChatNumber()
    const response = await apiCall('chat/create/' + chatName, { method: 'POST' })
    if (!response.ok) return

    const chatId = (await response.json()) as string
    activeChatInitialMessages.value = [initialChatMessage]
    activeChatId.value = chatId
    allUserChats.value.push({ id: chatId, name: chatName })

    function findNextChatNumber() {
      let max = 0
      for (const chat of allUserChats.value) {
        const number = +chat.name.split(' ')[1]
        if (number > max) {
          max = number
        }
      }
      return max + 1
    }
  }

  function resetToInitialState() {
    activeChatId.value = undefined
    allUserChats.value = []
    activeChatInitialMessages.value = undefined
    builtInTemplates.value = undefined
    activeExecutionEnvironmentTemplateId.value = undefined
  }

  return {
    activeChatId,
    allUserChats,
    activeChatInitialMessages,
    setUpChats,
    setActiveChat,
    deleteChat,
    createChat,
    resetToInitialState,
    onNewMessage,
    builtInTemplates,
    activeExecutionEnvironmentTemplateId,
    allTemplates
  }
})
