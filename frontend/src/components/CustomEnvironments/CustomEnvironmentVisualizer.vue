<template>
  <div class="custom-environment-option">
    {{ customEnvironment.name }}
    <p class="building-info" v-if="customEnvironment.buildInProgress">Building...</p>
    <div class="editing-group" v-else>
      <CustomEnvironmentEditor :custom-environment="customEnvironment" />

      <Button
        :loading="customEnvironmentsStore.deletingEnvironmentsList.has(customEnvironment.id)"
        severity="danger"
        @click="customEnvironmentsStore.deleteCustomEnvironment(customEnvironment.id)"
        clickable
        >Delete
      </Button>
    </div>
  </div>
</template>
<script setup lang="ts">
import {
  useCustomEnvironmentsStore,
  type CustomEnvironmentInfo
} from '@/stores/customEnvironmentsStore'
import Button from 'primevue/button'
import CustomEnvironmentEditor from './CustomEnvironmentEditor.vue'

const props = defineProps<{ customEnvironment: CustomEnvironmentInfo }>()

const customEnvironmentsStore = useCustomEnvironmentsStore()
</script>
<style scoped>
.custom-environment-option {
  display: flex;
  flex-direction: row;
  align-items: center;
  gap: 10px;
  padding: 10px;
  border: 1px solid white;
}
.editing-group {
  display: flex;
  flex-direction: row;
  align-items: center;
  flex: 1;
  gap: 15px;
  justify-content: center;
  border-left: 1px solid white;
}
.building-info {
  border-left: 1px solid white;
  padding-left: 10px;
}
</style>
