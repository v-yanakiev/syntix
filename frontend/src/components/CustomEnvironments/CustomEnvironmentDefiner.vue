<template>
  <div class="custom-environment-definer">
    <p>
      Here you can define the custom Docker image, inside of which the agent will execute code. Make
      sure to place the Dockerfile in the root of the environment defining directory. Note that CMD
      and ENTRYPOINT in the Dockerfile will not be executed.
    </p>
    <div
      class="environment-setup"
      v-if="
        customEnvironmentsStore.customEnvironmentCreatingMachineState.state ==
        CustomEnvironmentCreatingMachineStateEnum.Created
      "
    >
      <p>If you want to reset the definition environment, refresh the page.</p>
      <CustomEnvironmentTextsDefiner
        v-model:code-file-path="codeFilePath"
        v-model:environment-name="environmentName"
        v-model:validation-command="validationCommand"
        v-model:dependencyInstallingTerminalCall="dependencyInstallingTerminalCall"
        v-model:root-directory="rootDirectory"
        v-model:programming-language="programmingLanguage"
        v-model:is-valid="areTextsValid"
      />

      <FileFunctionality
        v-model:scanned-structure="scannedStructure"
        :enable-download="false"
        :should-initiate-connection="true"
        sse-stream-connection-url="environment/scanEnvironmentBuilderDirectory"
        upload-url="environment/upload"
        overridingDirectoryName="EnvironmentDefiningDirectory"
      />

      <p class="error-message" v-if="!scannedStructureHasADockerfileInTheRoot">
        No Dockerfile has been uploaded to the root directory yet.
      </p>
      <p class="error-message" v-if="!areTextsValid">
        You have to finish configuring the execution environment.
      </p>
      <Button
        severity="warn"
        @click="buildDockerImage"
        :disabled="!areTextsValid || !scannedStructureHasADockerfileInTheRoot"
        >Build Docker image</Button
      >
    </div>
    <div
      v-if="
        customEnvironmentsStore.customEnvironmentCreatingMachineState.state ==
        CustomEnvironmentCreatingMachineStateEnum.Blocked
      "
    >
      <p>
        An execution environment is being built. You'll be able to build a new environment after the
        current one finishes building.
      </p>
    </div>
    <div v-else>
      <div
        class="error-message"
        v-if="
          customEnvironmentsStore.customEnvironmentCreatingMachineState.state ==
          CustomEnvironmentCreatingMachineStateEnum.Failed
        "
      >
        <p>
          The environment failed to build! You can try again. Below, you can download the
          errorLog.txt.
        </p>
        <p>
          <a :href="getErrorDataUrl()" download="errorLog.txt"> Download errorLog.txt</a>
        </p>
      </div>

      <Button
        @click="customEnvironmentsStore.createCustomEnvironmentCreator()"
        :disabled="
          !areTextsValid ||
          customEnvironmentsStore.customEnvironmentCreatingMachineState.state ==
            CustomEnvironmentCreatingMachineStateEnum.Creating
        "
        >Create new environment
      </Button>
    </div>
  </div>
</template>

<script setup lang="ts">
import Button from 'primevue/button'
import {
  CustomEnvironmentCreatingMachineStateEnum,
  useCustomEnvironmentsStore,
  type CustomEnvironmentCreatingMachineFailedStateInformation
} from '@/stores/customEnvironmentsStore'
import FileFunctionality from '../FileFunctionality/FileFunctionality.vue'
import { computed, ref } from 'vue'
import CustomEnvironmentTextsDefiner from './Common/CustomEnvironmentTextsDefiner.vue'
import type { MachineFile } from '../FileTree/MachineFile'

const customEnvironmentsStore = useCustomEnvironmentsStore()

const environmentName = ref('')
const validationCommand = ref('')
const codeFilePath = ref('')
const dependencyInstallingTerminalCall = ref('')
const rootDirectory = ref('')
const programmingLanguage = ref('')
const areTextsValid = ref(true)

const scannedStructure = ref<MachineFile>()

const scannedStructureHasADockerfileInTheRoot = computed(() => {
  const result = scannedStructure.value?.children?.some((a) => a.name == 'Dockerfile')
  return result
})

async function buildDockerImage() {
  await customEnvironmentsStore.createCustomEnvironment(
    environmentName.value,
    validationCommand.value,
    codeFilePath.value,
    dependencyInstallingTerminalCall.value,
    rootDirectory.value,
    programmingLanguage.value
  )
}

function getErrorDataUrl() {
  const error = (
    customEnvironmentsStore.customEnvironmentCreatingMachineState as CustomEnvironmentCreatingMachineFailedStateInformation
  ).error
  const unquotedError = error.replace(/^"|"$/g, '')

  const blob = new Blob([unquotedError], { type: 'text/plain' })
  return URL.createObjectURL(blob)
}
</script>

<style scoped>
.error-message {
  color: red;
}

.environment-setup {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.custom-environment-definer {
  display: flex;
  flex-direction: column;
  gap: 10px;
}
</style>
