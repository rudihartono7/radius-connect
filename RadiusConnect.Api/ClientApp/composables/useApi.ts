import { ref, readonly } from 'vue'
import type { ApiResponse } from '~/types'

export function useApi<T = any>() {
  const data = ref<T | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  const execute = async (apiCall: () => Promise<ApiResponse<T>>) => {
    loading.value = true
    error.value = null
    
    try {
      const response = await apiCall()
      data.value = response.data
      return { success: true, data: response.data }
    } catch (err: any) {
      error.value = err.message || 'An error occurred'
      return { success: false, error: error.value }
    } finally {
      loading.value = false
    }
  }

  const reset = () => {
    data.value = null
    loading.value = false
    error.value = null
  }

  return {
    data: readonly(data),
    loading: readonly(loading),
    error: readonly(error),
    execute,
    reset
  }
}
