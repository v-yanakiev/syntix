<template>
  <div class="labels-and-inputs">
    <div class="label-and-input">
      <label>Environment's name:</label>
      <InputText type="text" v-model="environmentName" :invalid="!environmentName" />
    </div>

    <div class="label-and-input">
      <label
        >Root directory (likely the same as the WORKDIR in your Dockerfile, cannot be '/''):</label
      >
      <InputText
        type="text"
        v-model="rootDirectory"
        :invalid="!isValidRootDirectory"
        placeholder="/myCustomWorkdir"
      />
    </div>

    <div class="label-and-input">
      <label
        >Path to the code file that the AI will create and/or overwrite (it should be in or under
        the root directory):</label
      >
      <InputText
        type="text"
        v-model="codeFilePath"
        :invalid="!codeFilePath"
        placeholder="./code.js"
      />
    </div>

    <div class="label-and-input">
      <label>Programming language of the code file:</label>
      <InputText
        type="text"
        v-model="programmingLanguage"
        :invalid="!programmingLanguage"
        placeholder="javascript"
      />
    </div>

    <div class="label-and-input">
      <label
        >Validation shell command that will be ran (in the root directory) after the AI writes
        code:</label
      >
      <InputText
        type="text"
        v-model="validationCommand"
        :invalid="!validationCommand"
        placeholder="node code.js"
      />
    </div>

    <div class="label-and-input">
      <label
        >Terminal command that will be ran (in the root directory) when installing a dependency ({0}
        will be replaced with the actual dependency at runtime):</label
      >
      <InputText
        type="text"
        v-model="dependencyInstallingTerminalCall"
        placeholder="npm i {0}"
        :invalid="!isValidDependencyInstallingTerminalCall"
      />
    </div>
  </div>
</template>
<script setup lang="ts">
import InputText from 'primevue/inputtext'
import { computed, watch } from 'vue'

const environmentName = defineModel('environmentName', { default: '' })
const validationCommand = defineModel('validationCommand', { default: '' })
const codeFilePath = defineModel('codeFilePath', { default: '' })
const dependencyInstallingTerminalCall = defineModel('dependencyInstallingTerminalCall', {
  default: ''
})
const rootDirectory = defineModel('rootDirectory', { default: '' })

const programmingLanguage = defineModel('programmingLanguage', { default: '' })

const isValid = defineModel('isValid', { default: true })

const isValidDependencyInstallingTerminalCall = computed(
  () => !!dependencyInstallingTerminalCall.value?.includes('{0}')
)

const isValidRootDirectory = computed(
  () => !!(rootDirectory.value != '/' && rootDirectory.value?.includes('/'))
)

watch(
  () => [
    environmentName.value,
    validationCommand.value,
    codeFilePath.value,
    dependencyInstallingTerminalCall.value,
    rootDirectory.value
  ],
  () => {
    isValid.value =
      !!environmentName.value &&
      !!validationCommand.value &&
      !!codeFilePath.value &&
      !!programmingLanguage.value &&
      isValidRootDirectory.value &&
      isValidDependencyInstallingTerminalCall.value
  },
  { immediate: true }
)
</script>
<style scoped>
.label-and-input {
  display: flex;
  flex-direction: row;
  align-items: center;
  gap: 10px;
}

.labels-and-inputs {
  display: flex;
  flex-direction: column;
  gap: 10px;
  padding-left: 10px;
}
</style>
