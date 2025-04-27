<template>
  <div class="chat-view" v-if="chatsSetUp">
    <div class="chat-setup" :key="chatStore.activeChatId" v-if="chatStore.activeChatId">
      <Select
        v-model="chatStore.activeExecutionEnvironmentTemplateId"
        filter
        option-label="name"
        option-value="id"
        :options="chatStore.allTemplates"
        :disabled="configuredChat"
        placeholder="Select an execution environment"
        class="w-full md:w-14rem"
      />

      <Select
        v-model="selectedLLM"
        :options="LLMs"
        :disabled="configuredChat"
        placeholder="Select a language model"
        class="w-full md:w-14rem"
      />

      <chat-container v-if="configuredChat" :selected-l-l-m="selectedLLM!"></chat-container>
    </div>
    <div v-if="chatStore.allUserChats" class="chats">
      <div class="chat-option" v-for="chat in chatStore.allUserChats" :key="chat.id">
        <Button
          :loading="chatLoadingInProgress"
          severity="info"
          @click="setActiveChat(chat.id)"
          clickable
        >
          {{ chat.name }}
        </Button>
        <Button
          :loading="chatDeletionInProgress"
          severity="danger"
          @click="deleteChat(chat.id)"
          clickable
          >Delete</Button
        >
      </div>
    </div>
    <div class="action-buttons">
      <Button
        :loading="chatCreationInProgress"
        color="primary"
        label="Create new chat"
        @click="createChat()"
      />
    </div>
  </div>
  <Spinner v-else />
</template>

<script setup lang="ts">
import ChatContainer from '@/components/Chat/ChatContainer.vue'
import { useChatStore } from '@/stores/chat'
import Button from 'primevue/button'
import { computed, ref } from 'vue'
import Spinner from '@/components/Common/Spinner.vue'
import Select from 'primevue/select'
const chatsSetUp = ref(false)

const configuredChat = computed(
  () => !!(chatStore.activeExecutionEnvironmentTemplateId && selectedLLM.value?.length)
)
const selectedLLM = ref<string>()
const LLMs = ref(['gpt-4o-mini', 'gpt-4o'])

const chatStore = useChatStore()
chatStore.setUpChats().then(() => {
  chatsSetUp.value = true
})

const chatCreationInProgress = ref(false)
async function createChat() {
  try {
    chatStore.activeExecutionEnvironmentTemplateId = undefined
    selectedLLM.value = undefined
    chatCreationInProgress.value = true
    await chatStore.createChat()
  } finally {
    chatCreationInProgress.value = false
  }
}

const chatLoadingInProgress = ref(false)
async function setActiveChat(chatId: string) {
  chatStore.activeExecutionEnvironmentTemplateId = undefined
  selectedLLM.value = undefined
  chatLoadingInProgress.value = true
  try {
    await chatStore.setActiveChat(chatId)
  } finally {
    chatLoadingInProgress.value = false
  }
}

const chatDeletionInProgress = ref(false)
async function deleteChat(chatId: string) {
  chatDeletionInProgress.value = true
  try {
    await chatStore.deleteChat(chatId)
  } finally {
    chatDeletionInProgress.value = false
  }
}
</script>

<style scoped>
.chats {
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.chat-option {
  display: flex;
  flex-direction: row;
  gap: 10px;
}

.chat-view {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.chat-setup {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.action-buttons {
  display: flex;
  flex-direction: row;
  gap: 10px;
  align-items: center;
}
</style>
