<template>
  <div>
    <deep-chat
      ref="deepChatComponent"
      :history="chatStore.activeChatInitialMessages"
      :connect="connectData"
      :requestBodyLimits="{ maxMessages: -1 }"
      class="deep-chat"
    ></deep-chat>

    <FileFunctionality
      :shouldInitiateConnection="true"
      :machine-starting-data="sseMachineStartingData"
      :sseStreamConnectionUrl="sseStreamConnectionUrl"
      :enableDownload="true"
      :upload-url="getUploadUrlIncludingChatAndTemplate()"
    />
  </div>
</template>

<script setup lang="ts">
import { apiConfig } from '@/api/apiConfig'
import FileFunctionality from '@/components/FileFunctionality/FileFunctionality.vue'
import { useChatStore } from '@/stores/chat'
import 'deep-chat'
import { computed, ref } from 'vue'
import { getUploadUrlIncludingChatAndTemplate } from '../Uploaders/common'

const props = defineProps<{ selectedLLM: string }>()
const chatStore = useChatStore()
const deepChatComponent = ref()

const connectData = {
  url: apiConfig.apiUrl + 'message/getCompletion',
  additionalBodyProps: {
    chatId: chatStore.activeChatId,
    selectedTemplateId: chatStore.activeExecutionEnvironmentTemplateId!,
    languageModel: props.selectedLLM
  },
  stream: true,
  credentials: 'include'
}

const sseMachineStartingData = computed(() =>
  chatStore.activeChatId
    ? {
        chatId: chatStore.activeChatId,
        executionEnvironmentTemplateId: chatStore.activeExecutionEnvironmentTemplateId!.toString()
      }
    : undefined
)
const sseStreamConnectionUrl = computed(() => {
  if (!sseMachineStartingData.value) {
    return undefined
  }

  const queryParams = new URLSearchParams(sseMachineStartingData.value).toString()
  const toReturn = `scanDirectory?${queryParams}`

  return toReturn
})
</script>

<style scoped>
.deep-chat {
  width: 1700px !important;
  height: 500px !important;
  background-color: #333030 !important;
}
</style>
