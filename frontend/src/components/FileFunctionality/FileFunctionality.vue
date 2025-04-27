<template>
  <div>
    <div class="uploaders">
      <DirectoryUploader :upload-url="uploadUrl" v-model="uploadingFeedback" />
      <FileUploader :upload-url="uploadUrl" v-model="uploadingFeedback" />
    </div>
    <label v-if="uploadingFeedback">{{ uploadingFeedback }}</label
    ><br />
    <template v-if="directoryScanningConnectionState == 'open' && scannedStructure">
      <Directory
        :enableDownload="enableDownload"
        :directory="scannedStructure"
        :open="true"
        :overriding-directory-name="overridingDirectoryName"
      />
    </template>
    <p v-else-if="directoryScanningConnectionState == 'initializing'">
      Loading environment, feel free to ask the chat any questions...
    </p>
    <p v-else-if="directoryScanningConnectionState == 'failed'">
      The connection to the environment has been interrupted, please reopen the chat.
    </p>
  </div>
</template>
<script setup lang="ts">
import type { MachineFile } from '@/components/FileTree/MachineFile'
import { onUnmounted, ref, watch } from 'vue'
import { apiCallJsonBody } from '@/api/utils'
import Directory from '@/components/FileTree/Directory.vue'
import FileUploader from '@/components/Uploaders/FileUploader.vue'
import DirectoryUploader from '@/components/Uploaders/DirectoryUploader.vue'
import { apiConfig } from '@/api/apiConfig'

const uploadingFeedback = ref('')

const props = defineProps<{
  shouldInitiateConnection: boolean
  machineStartingData?: Record<string, string>
  sseStreamConnectionUrl: string | undefined
  enableDownload: boolean
  uploadUrl: string
  overridingDirectoryName?: string
}>()

type DirectoryScanData = {
  message: string
  structure: MachineFile
}
const scannedStructure = defineModel<MachineFile>('scannedStructure')

const directoryScanningConnectionState = ref('initializing')

function connectToSSEStream() {
  const eventSource = new EventSource(`${apiConfig.apiUrl}${props.sseStreamConnectionUrl!}`, {
    withCredentials: true
  })

  // Handle incoming messages
  eventSource.onmessage = (event) => {
    try {
      const data = JSON.parse(event.data) as DirectoryScanData
      scannedStructure.value = data.structure
    } catch (error) {
      console.log('Error reading directory data:', event.data)
    }
  }

  // Handle connection opened
  eventSource.onopen = () => {
    directoryScanningConnectionState.value = 'open'
  }

  // Handle errors
  eventSource.onerror = (error) => {
    console.error('Directory scanning connection error: ', error)
    eventSource.close()
    directoryScanningConnectionState.value = 'failed'
  }

  // Return the EventSource instance so it can be closed if needed
  return eventSource
}

let sseStream: EventSource | undefined
watch(
  () => props.shouldInitiateConnection,
  async (newValue, oldValue) => {
    if (newValue && !oldValue) {
      props.machineStartingData
        ? await apiCallJsonBody('chat/startMachine', props.machineStartingData)
        : undefined

      sseStream = connectToSSEStream()
    }
  },
  { immediate: true }
)

onUnmounted(() => {
  sseStream?.close()
})
</script>
<style scoped>
.uploaders {
  display: flex;
  flex-direction: row;
  gap: 10px;
}
</style>
