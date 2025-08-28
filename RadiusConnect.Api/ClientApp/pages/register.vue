<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full space-y-8">
      <div>
        <div class="mx-auto h-12 w-12 bg-blue-600 rounded-lg flex items-center justify-center">
          <span class="text-white font-bold text-xl">R</span>
        </div>
        <h2 class="mt-6 text-center text-3xl font-extrabold text-gray-900">
          Create your account
        </h2>
        <p class="mt-2 text-center text-sm text-gray-600">
          Join RadiusConnect to manage your RADIUS infrastructure
        </p>
      </div>
      
      <form class="mt-8 space-y-6" @submit.prevent="handleRegister">
        <div class="space-y-4">
          <UiInput
            v-model="form.username"
            label="Username"
            type="text"
            required
            :error="errors.username"
            placeholder="Choose a username"
          />
          
          <UiInput
            v-model="form.email"
            label="Email Address"
            type="email"
            required
            :error="errors.email"
            placeholder="Enter your email"
          />
          
          <UiInput
            v-model="form.firstName"
            label="First Name"
            type="text"
            :error="errors.firstName"
            placeholder="Enter your first name"
          />
          
          <UiInput
            v-model="form.lastName"
            label="Last Name"
            type="text"
            :error="errors.lastName"
            placeholder="Enter your last name"
          />
          
          <UiInput
            v-model="form.password"
            label="Password"
            type="password"
            required
            :error="errors.password"
            placeholder="Choose a password"
          />
          
          <UiInput
            v-model="form.confirmPassword"
            label="Confirm Password"
            type="password"
            required
            :error="errors.confirmPassword"
            placeholder="Confirm your password"
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
            :loading="loading"
            full-width
          >
            Create Account
          </UiButton>
        </div>

        <div class="text-center">
          <span class="text-sm text-gray-600">Already have an account?</span>
          <NuxtLink to="/login" class="font-medium text-blue-600 hover:text-blue-500 ml-1">
            Sign in
          </NuxtLink>
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
  email: '',
  firstName: '',
  lastName: '',
  password: '',
  confirmPassword: '',
})

const errors = reactive({
  username: '',
  email: '',
  firstName: '',
  lastName: '',
  password: '',
  confirmPassword: '',
})

const error = ref('')
const loading = ref(false)

const validateForm = () => {
  errors.username = ''
  errors.email = ''
  errors.firstName = ''
  errors.lastName = ''
  errors.password = ''
  errors.confirmPassword = ''
  
  if (!form.username.trim()) {
    errors.username = 'Username is required'
  }
  
  if (!form.email.trim()) {
    errors.email = 'Email is required'
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) {
    errors.email = 'Invalid email format'
  }
  
  if (!form.password) {
    errors.password = 'Password is required'
  } else if (form.password.length < 6) {
    errors.password = 'Password must be at least 6 characters'
  }
  
  if (form.password !== form.confirmPassword) {
    errors.confirmPassword = 'Passwords do not match'
  }
  
  return !Object.values(errors).some(error => error)
}

const handleRegister = async () => {
  if (!validateForm()) {
    return
  }
  
  loading.value = true
  error.value = ''
  
  try {
    const result = await authStore.register({
      username: form.username.trim(),
      email: form.email.trim(),
      password: form.password,
      firstName: form.firstName.trim(),
      lastName: form.lastName.trim(),
    })
    
    if (result.success) {
      // Registration successful, redirect to login
      await router.push('/login')
    } else {
      error.value = result.error || 'Registration failed'
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
