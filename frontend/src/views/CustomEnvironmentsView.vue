<template>
  <div class="custom-environment-view">
    <div class="custom-environments" v-if="customEnvironmentsStore.customEnvironments">
      <div
        class="custom-environments-list"
        v-if="customEnvironmentsStore.customEnvironments.length"
      >
        <p>Custom environments:</p>
        <CustomEnvironmentVisualizer
          v-for="customEnvironment in customEnvironmentsStore.customEnvironments"
          :custom-environment="customEnvironment"
          :key="customEnvironment.id"
        />
      </div>
      <CustomEnvironmentDefiner />
    </div>
    <div v-else>Loading custom execution environments...</div>
  </div>
</template>

<script setup lang="ts">
import { useCustomEnvironmentsStore } from '@/stores/customEnvironmentsStore'
import CustomEnvironmentDefiner from '@/components/CustomEnvironments/CustomEnvironmentDefiner.vue'
import CustomEnvironmentVisualizer from '@/components/CustomEnvironments/CustomEnvironmentVisualizer.vue'

const customEnvironmentsStore = useCustomEnvironmentsStore()
</script>

<style scoped>
.custom-environments-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}
.custom-environments {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.custom-environment-view {
  display: flex;
  flex-direction: column;
  gap: 10px;
}
</style>
