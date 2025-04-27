// src/composables/usePostHog.ts
import posthog from 'posthog-js'

export function usePostHog() {
  posthog.init('phc_EQ7wl1iGiCvwZs8CrqMW8yxYpBq0SHhZKcPEkB3fhWa', {
    api_host: '/good',
    ui_host: 'https://eu.posthog.com'
  })

  return {
    posthog
  }
}
