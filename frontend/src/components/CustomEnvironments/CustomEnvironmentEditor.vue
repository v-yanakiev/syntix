<template>
  <CustomEnvironmentTextsDefiner
    v-model:code-file-path="codeFilePath"
    v-model:environment-name="environmentName"
    v-model:validation-command="validationCommand"
    v-model:dependencyInstallingTerminalCall="dependencyInstallingTerminalCall"
    v-model:root-directory="rootDirectory"
    v-model:programming-language="programmingLanguage"
    v-model:is-valid="areTextsValid"
  />
  <Button @click="saveChanges" :disabled="savingDisabled" :loading="savingOngoing"
    >Save Edits
  </Button>
</template>
<script setup lang="ts">
import {
  useCustomEnvironmentsStore,
  type CustomEnvironmentInfo
} from '@/stores/customEnvironmentsStore'
import { computed, ref } from 'vue'
import CustomEnvironmentTextsDefiner from './Common/CustomEnvironmentTextsDefiner.vue'
import Button from 'primevue/button'

const props = defineProps<{ customEnvironment: CustomEnvironmentInfo }>()

const customEnvironmentsStore = useCustomEnvironmentsStore()

const environmentName = ref(props.customEnvironment.name)
const validationCommand = ref(props.customEnvironment.afterChangesValidationCommand)
const codeFilePath = ref(props.customEnvironment.codeFile)
const dependencyInstallingTerminalCall = ref(
  props.customEnvironment.dependencyInstallingTerminalCall
)
const rootDirectory = ref(props.customEnvironment.rootDirectory)

const programmingLanguage = ref(props.customEnvironment.programmingLanguage)

const areTextsValid = ref(true)

const savingOngoing = ref(false)

const savingDisabled = computed(() => {
  const noChanges =
    codeFilePath.value == props.customEnvironment.codeFile &&
    validationCommand.value == props.customEnvironment.afterChangesValidationCommand &&
    environmentName.value == props.customEnvironment.name &&
    dependencyInstallingTerminalCall.value ==
      props.customEnvironment.dependencyInstallingTerminalCall &&
    rootDirectory.value == props.customEnvironment.rootDirectory

  return noChanges || !areTextsValid.value
})

async function saveChanges() {
  savingOngoing.value = true

  try {
    await customEnvironmentsStore.editCustomEnvironment(
      props.customEnvironment.id,
      environmentName.value,
      validationCommand.value,
      codeFilePath.value,
      dependencyInstallingTerminalCall.value,
      rootDirectory.value,
      programmingLanguage.value
    )
  } finally {
    savingOngoing.value = false
  }
}
</script>
<style lang="css"></style>
