<template>
  <details :open="open">
    <summary>{{ overridingDirectoryName || directory.name }}</summary>
    <div class="element" v-for="child of directory.children" :key="child.name">
      <div class="file" v-if="child.children === undefined">
        {{ child.name }}
        <Button
          v-if="enableDownload"
          :loading="fileDownloadingInProgress"
          @click="downloadFile(child)"
          class="file-download-button"
          >Download</Button
        >
      </div>
      <Directory
        v-else
        :directory="child"
        :current-path="getChildPath(child)"
        :enable-download="enableDownload"
      />
    </div>
  </details>
</template>

<script setup lang="ts">
import type { MachineFile } from '@/components/FileTree/MachineFile'
import { apiCall } from '@/api/utils'
import { useChatStore } from '@/stores/chat'
import Button from 'primevue/button'
import { ref } from 'vue'

const props = defineProps<{
  directory: MachineFile
  open?: boolean
  currentPath?: string // Pass the current path down the tree
  enableDownload: boolean
  overridingDirectoryName?: string
}>()

const fileDownloadingInProgress = ref(false)
const downloadFile = async (file: MachineFile) => {
  try {
    fileDownloadingInProgress.value = true
    // Construct the full path to the file
    const fullPath = props.currentPath ? `${props.currentPath}/${file.name}` : file.name
    const chatStore = useChatStore()

    const queryParams = new URLSearchParams({
      executionEnvironmentTemplateId: chatStore.activeExecutionEnvironmentTemplateId!.toString(),
      chatId: chatStore.activeChatId!.toString(),
      pathToFile: fullPath
    }).toString()

    // Send a request to the download endpoint
    const response = await apiCall(`download?${queryParams}`)

    if (!response.ok) {
      throw new Error('Failed to download file')
    }

    // Trigger the file download
    const blob = await response.blob()
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = file.name
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    window.URL.revokeObjectURL(url)
  } catch (error) {
    console.error('Error downloading file:', error)
  } finally {
    fileDownloadingInProgress.value = false
  }
}

const getChildPath = (child: MachineFile): string => {
  // Construct the path for the child directory
  return props.currentPath ? `${props.currentPath}/${child.name}` : child.name
}
</script>

<style scoped>
.element {
  padding-left: 20px;
}

.file-download-button {
  padding: 2px;
}

.file {
  padding: 5px;
}
</style>
