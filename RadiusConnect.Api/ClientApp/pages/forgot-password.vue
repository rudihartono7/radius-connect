<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full space-y-8">
      <div>
        <div class="mx-auto h-12 w-12 bg-blue-600 rounded-lg flex items-center justify-center">
          <span class="text-white font-bold text-xl">R</span>
        </div>
        <h2 class="mt-6 text-center text-3xl font-extrabold text-gray-900">
          Forgot your password?
        </h2>
        <p class="mt-2 text-center text-sm text-gray-600">
          Enter your email address and we'll send you a link to reset your password.
        </p>
      </div>
      
      <form class="mt-8 space-y-6" @submit.prevent="handleForgotPassword">
        <div>
          <UiInput
            v-model="email"
            label="Email Address"
            type="email"
            required
            :error="error"
            placeholder="Enter your email address"
          />
        </div>

        <div v-if="success" class="bg-green-50 border border-green-200 rounded-lg p-4">
          <div class="flex">
            <div class="flex-shrink-0">
              <CheckCircleIcon class="h-5 w-5 text-green-400" />
            </div>
            <div class="ml-3">
              <p class="text-sm text-green-700">{{ success }}</p>
            </div>
          </div>
        </div>

        <div>
          <UiButton
            type="submit"
            variant="primary"
            size="lg"
            :loading="loading"
            full-width
          >
            Send Reset Link
          </UiButton>
        </div>

        <div class="text-center">
          <NuxtLink to="/login" class="font-medium text-blue-600 hover:text-blue-500">
            Back to Sign In
          </NuxtLink>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { CheckCircleIcon } from '@heroicons/vue/24/outline'

definePageMeta({
  layout: false
})

const authStore = useAuthStore()
const router = useRouter()

const email = ref('')
const error = ref('')
const success = ref('')
const loading = ref(false)

const handleForgotPassword = async () => {
  if (!email.value.trim()) {
    error.value = 'Email is required'
    return
  }

  loading.value = true
  error.value = ''
  success.value = ''

  try {
    const result = await authStore.forgotPassword(email.value.trim())
    if (result.success) {
      success.value = 'If an account with that email exists, we have sent a password reset link.'
      email.value = ''
    } else {
      error.value = result.error || 'Failed to send reset link'
    }
  } catch (err: any) {
    error.value = err.message || 'An error occurred'
  } finally {
    loading.value = false
  }
}

// Redirect if already authenticated
onMounted(() => {
  if (authStore.isAuthenticated) {
    router.push('/')
  }
})
</script>
