<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full space-y-8">
      <div>
        <div class="mx-auto h-12 w-12 bg-blue-600 rounded-lg flex items-center justify-center">
          <span class="text-white font-bold text-xl">R</span>
        </div>
        <h2 class="mt-6 text-center text-3xl font-extrabold text-gray-900">
          Sign in to RadiusConnect
        </h2>
        <p class="mt-2 text-center text-sm text-gray-600">
          Manage your RADIUS infrastructure
        </p>
      </div>
      
      <form class="mt-8 space-y-6" @submit.prevent="handleLogin">
        <div class="space-y-4">
          <UiInput
            v-model="form.username"
            label="Username"
            type="text"
            required
            :error="errors.username"
            placeholder="Enter your username"
          />
          
          <UiInput
            v-model="form.password"
            label="Password"
            type="password"
            required
            :error="errors.password"
            placeholder="Enter your password"
          />
          
          <UiInput
            v-if="requireTotp"
            v-model="form.totpCode"
            label="TOTP Code"
            type="text"
            required
            :error="errors.totpCode"
            placeholder="Enter 6-digit code"
            maxlength="6"
          />
        </div>

        <div v-if="error" class="bg-red-50 border border-red-200 rounded-lg p-4">
          <div class="flex">
            <div class="flex-shrink-0">
              <ExclamationTriangleIcon class="h-5 w-5 text-red-400" />
            </div>
            <div class="ml-3">
              <p class="text-sm text-red-700">{{ error }}</p>
            </div>
          </div>
        </div>

        <div>
          <UiButton
            type="submit"
            variant="primary"
            size="lg"
            :loading="authStore.loading"
            full-width
          >
            Sign in
          </UiButton>
        </div>

        <div class="flex items-center justify-between">
          <div class="text-sm">
            <NuxtLink to="/forgot-password" class="font-medium text-blue-600 hover:text-blue-500">
              Forgot your password?
            </NuxtLink>
          </div>
          <div class="text-sm">
            <NuxtLink to="/register" class="font-medium text-blue-600 hover:text-blue-500">
              Create account
            </NuxtLink>
          </div>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ExclamationTriangleIcon } from '@heroicons/vue/24/outline'

definePageMeta({
  layout: false
})

const authStore = useAuthStore()
const router = useRouter()

const form = reactive({
  username: '',
  password: '',
  totpCode: '',
})

const errors = reactive({
  username: '',
  password: '',
  totpCode: '',
})

const error = ref('')
const requireTotp = ref(false)

const validateForm = () => {
  errors.username = ''
  errors.password = ''
  errors.totpCode = ''
  
  if (!form.username.trim()) {
    errors.username = 'Username is required'
  }
  
  if (!form.password) {
    errors.password = 'Password is required'
  }
  
  if (requireTotp.value && !form.totpCode.trim()) {
    errors.totpCode = 'TOTP code is required'
  }
  
  return !errors.username && !errors.password && (!requireTotp.value || !errors.totpCode)
}

const handleLogin = async () => {
  error.value = ''
  
  if (!validateForm()) {
    return
  }
  
  const loginData = {
    username: form.username.trim(),
    password: form.password,
    ...(requireTotp.value && { totpCode: form.totpCode.trim() })
  }
  
  const result = await authStore.login(loginData)
  
  if (result.success) {
    // Check if TOTP is required
    if (result.data?.data?.requireTotp) {
      requireTotp.value = true
      return
    }
    
    // Login successful, redirect to dashboard
    await router.push('/')
  } else {
    error.value = result.error || 'Login failed'
  }
}

// Redirect if already authenticated
onMounted(() => {
  if (authStore.isAuthenticated) {
    router.push('/')
  }
})
</script>
