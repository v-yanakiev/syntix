import { apiConfig } from './apiConfig'

export async function apiCall(endpoint: string, options: object = {}) {
  const url = `${apiConfig.apiUrl}${endpoint}`
  const extendedOptions = { ...options, credentials: 'include' } as object
  const response = await fetch(url, extendedOptions)

  return response
}

export function apiCallJsonBody(endpoint: string, body: object | undefined = undefined, method = 'POST') {
  return apiCall(endpoint, {
    method,
    headers: {
      'Content-Type': 'application/json'
    },
    body: body ? JSON.stringify(body) : undefined
  })
}