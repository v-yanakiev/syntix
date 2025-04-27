import { useChatStore } from '@/stores/chat'

export function getUploadUrlIncludingChatAndTemplate() {
  const chatStore = useChatStore()

  const searchParams = new URLSearchParams()
  searchParams.set('chatId', chatStore.activeChatId!.toString())
  searchParams.set(
    'executionEnvironmentTemplateId',
    chatStore.activeExecutionEnvironmentTemplateId!.toString()
  )
  return `upload?${searchParams.toString()}`
}
