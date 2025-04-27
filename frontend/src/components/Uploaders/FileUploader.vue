<template>
  <div class="upload-button-wrapper">
    <input type="file" ref="input" multiple class="hidden-input" @change="handleUpload" />
    <Button color="secondary" :loading="uploading" label="Upload Files" @click="triggerInput" />
  </div>
</template>
<script setup lang="ts">
import Button from 'primevue/button'
import { apiCall } from '@/api/utils'
import { ref } from 'vue'

const props = defineProps<{ uploadUrl: string }>()

const input = ref<HTMLInputElement | null>(null)

const uploading = ref(false)
const uploadingFeedback = defineModel<string>()

const triggerInput = () => {
  input.value?.click()
}
async function handleUpload(event: Event) {
  const files = (event.target as HTMLInputElement).files
  if (!files) {
    return
  }

  const formData = new FormData()

  uploading.value = true
  uploadingFeedback.value = ''
  try {
    Array.from(files).forEach((file) => {
      formData.append('files', file)
    })

    const options = {
      method: 'POST',
      body: formData
    }

    const response = await apiCall(props.uploadUrl, options)

    if (!response.ok) {
      throw new Error(`Upload failed: ${response.statusText}`)
    }

    const result = await response.json()
    console.log('Files uploaded successfully:', result)
    uploadingFeedback.value = 'Upload success!'
  } catch (error) {
    console.error('Error uploading files:', error)
    uploadingFeedback.value = 'Upload failed!'
    throw error
  } finally {
    // Reset the input to allow uploading the same file again
    if (input.value) input.value.value = ''
    uploading.value = false
  }
}
</script>

<style scoped>
@import 'styling.css';
</style>
