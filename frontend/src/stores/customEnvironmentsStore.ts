import { defineStore } from 'pinia'
import { ref } from 'vue'
import { apiCall, apiCallJsonBody } from '@/api/utils'

export type CustomEnvironmentInfo = {
  id: number
  name: string
  codeFile: string
  afterChangesValidationCommand: string
  dependencyInstallingTerminalCall: string
  rootDirectory: string
  programmingLanguage: string
  buildInProgress: boolean
}

export enum CustomEnvironmentCreatingMachineStateEnum {
  None = 'None',
  Blocked = 'Blocked',
  Creating = 'Creating',
  Created = 'Created',
  Failed = 'Failed'
}
export type CustomEnvironmentCreatingMachineStateInformation =
  | {
      state: CustomEnvironmentCreatingMachineStateEnum
    }
  | CustomEnvironmentCreatingMachineFailedStateInformation

export type CustomEnvironmentCreatingMachineFailedStateInformation = {
  state: CustomEnvironmentCreatingMachineStateEnum.Failed
  error: string
}

export const useCustomEnvironmentsStore = defineStore('customEnvironments', () => {
  const customEnvironments = ref<CustomEnvironmentInfo[] | undefined>()
  const customEnvironmentCreatingMachineState =
    ref<CustomEnvironmentCreatingMachineStateInformation>({
      state: CustomEnvironmentCreatingMachineStateEnum.None
    })

  loadCustomEnvironments()
  let interval = setInterval(loadCustomEnvironments, 5000)

  async function loadCustomEnvironments() {
    const upToDateCustomEnvironments = (await (
      await apiCall('environment/list')
    ).json()) as CustomEnvironmentInfo[]

    customEnvironments.value = upToDateCustomEnvironments

    if (upToDateCustomEnvironments.some((a) => a.buildInProgress)) {
      customEnvironmentCreatingMachineState.value = {
        state: CustomEnvironmentCreatingMachineStateEnum.Blocked
      }
    } else if (
      customEnvironmentCreatingMachineState.value.state ==
      CustomEnvironmentCreatingMachineStateEnum.Blocked
    ) {
      customEnvironmentCreatingMachineState.value = {
        state: CustomEnvironmentCreatingMachineStateEnum.None
      }
    }
  }

  function resetToInitialState() {
    customEnvironments.value = []
    customEnvironmentCreatingMachineState.value = {
      state: CustomEnvironmentCreatingMachineStateEnum.None
    }
    deletingEnvironmentsList.value.clear()
    clearInterval(interval)
  }

  async function createCustomEnvironmentCreator() {
    customEnvironmentCreatingMachineState.value = {
      state: CustomEnvironmentCreatingMachineStateEnum.Creating
    }

    try {
      const response = await apiCallJsonBody('environment/startEnvironmentBuilder')

      if (!response.ok) {
        throw new Error()
      }
    } catch (e) {
      customEnvironmentCreatingMachineState.value = {
        state: CustomEnvironmentCreatingMachineStateEnum.None
      }
      return
    }

    customEnvironmentCreatingMachineState.value = {
      state: CustomEnvironmentCreatingMachineStateEnum.Created
    }
  }

  const deletingEnvironmentsList = ref(new Set<number>())
  async function deleteCustomEnvironment(id: number) {
    try {
      deletingEnvironmentsList.value.add(id)

      clearInterval(interval)
      interval = setInterval(loadCustomEnvironments, 5000)
      const response = await apiCallJsonBody(`environment/delete/${id}`, undefined, 'DELETE')
      if (response.ok) {
        customEnvironments.value!.splice(customEnvironments.value!.findIndex((a) => a.id == id))
      }
    } finally {
      deletingEnvironmentsList.value.delete(id)
    }
  }

  async function createCustomEnvironment(
    environmentName: string,
    validationCommand: string,
    codeFilePath: string,
    dependencyInstallingTerminalCall: string,
    rootDirectory: string,
    programmingLanguage: string
  ) {
    try {
      clearInterval(interval)
      interval = setInterval(loadCustomEnvironments, 5000)

      customEnvironmentCreatingMachineState.value = {
        state: CustomEnvironmentCreatingMachineStateEnum.Blocked
      }
      customEnvironments.value!.push({
        id: 0,
        name: environmentName,
        codeFile: codeFilePath,
        afterChangesValidationCommand: validationCommand,
        dependencyInstallingTerminalCall: dependencyInstallingTerminalCall,
        rootDirectory: rootDirectory,
        programmingLanguage: programmingLanguage,
        buildInProgress: true
      })

      const response = await apiCallJsonBody('environment/build', {
        environmentName: environmentName,
        validationCommand: validationCommand,
        codeFilePath: codeFilePath,
        dependencyInstallingTerminalCall: dependencyInstallingTerminalCall,
        rootDirectory: rootDirectory,
        programmingLanguage: programmingLanguage
      })

      if (!response.ok) {
        const errorContent = await response.text()
        const decodedError = errorContent.startsWith('"') ? JSON.parse(errorContent) : errorContent

        throw new Error(decodedError || response.statusText)
      }
    } catch (e: any) {
      const message = 'message' in e ? (e.message as string) : ''
      customEnvironmentCreatingMachineState.value = {
        state: CustomEnvironmentCreatingMachineStateEnum.Failed,
        error: message
      }
      clearInterval(interval)
      interval = setInterval(loadCustomEnvironments, 5000)

      return
    }

    customEnvironmentCreatingMachineState.value = {
      state: CustomEnvironmentCreatingMachineStateEnum.None
    }
  }

  async function editCustomEnvironment(
    environmentId: number,
    environmentName: string,
    validationCommand: string,
    codeFilePath: string,
    dependencyInstallingTerminalCall: string,
    rootDirectory: string,
    programmingLanguage: string
  ) {
    clearInterval(interval)
    interval = setInterval(loadCustomEnvironments, 5000)

    const newEnvironmentInfo: CustomEnvironmentInfo = {
      id: environmentId,
      buildInProgress: false,
      name: environmentName,
      afterChangesValidationCommand: validationCommand,
      codeFile: codeFilePath,
      dependencyInstallingTerminalCall: dependencyInstallingTerminalCall,
      rootDirectory: rootDirectory,
      programmingLanguage: programmingLanguage
    }

    const savedEnvironmentId = customEnvironments.value!.findIndex((a) => a.id == environmentId)
    customEnvironments.value![savedEnvironmentId] = newEnvironmentInfo

    const editRequestResponse = await apiCallJsonBody('environment/edit', newEnvironmentInfo, 'PUT')

    if (!editRequestResponse.ok) {
      throw new Error()
    }
  }

  return {
    resetToInitialState,
    customEnvironments,
    deleteCustomEnvironment,
    createCustomEnvironment,
    createCustomEnvironmentCreator,
    customEnvironmentCreatingMachineState,
    deletingEnvironmentsList,
    editCustomEnvironment,
    loadCustomEnvironments
  }
})
