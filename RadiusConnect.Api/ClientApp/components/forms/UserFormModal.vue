<template>
  <div class="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
    <div class="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
      <div class="mt-3">
        <div class="flex items-center justify-between mb-4">
          <h3 class="text-lg font-medium text-gray-900">
            {{ user ? 'Edit User' : 'Create User' }}
          </h3>
          <button
            @click="$emit('close')"
            class="text-gray-400 hover:text-gray-600"
          >
            <XMarkIcon class="w-6 h-6" />
          </button>
        </div>
        
        <form @submit.prevent="handleSubmit" class="space-y-4">
          <UiInput
            v-model="form.username"
            label="Username"
            type="text"
            required
            :error="errors.username"
            placeholder="Enter username"
          />
          
          <UiInput
            v-model="form.email"
            label="Email"
            type="email"
            required
            :error="errors.email"
            placeholder="Enter email"
          />
          
          <UiInput
            v-model="form.firstName"
            label="First Name"
            type="text"
            required
            :error="errors.firstName"
            placeholder="Enter first name"
          />
          
          <UiInput
            v-model="form.lastName"
            label="Last Name"
            type="text"
            required
            :error="errors.lastName"
            placeholder="Enter last name"
          />
          
          <UiInput
            v-if="!user"
            v-model="form.password"
            label="Password"
            type="password"
            required
            :error="errors.password"
            placeholder="Enter password"
          />
          
          <UiInput
            v-if="!user"
            v-model="form.confirmPassword"
            label="Confirm Password"
            type="password"
            required
            :error="errors.confirmPassword"
            placeholder="Confirm password"
          />
          
          <div class="flex justify-end space-x-3 pt-4">
            <UiButton
              type="button"
              variant="secondary"
              @click="$emit('close')"
            >
              Cancel
            </UiButton>
            <UiButton
              type="submit"
              variant="primary"
              :loading="loading"
            >
              {{ user ? 'Update' : 'Create' }}
            </UiButton>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { XMarkIcon } from '@heroicons/vue/24/outline'
import type { User } from '~/types'

interface Props {
  user: User | null
}

const props = defineProps<Props>()

const emit = defineEmits<{
  close: []
  saved: [user: User]
}>()

const usersStore = useUsersStore()

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

const loading = ref(false)

// Initialize form with user data if editing
onMounted(() => {
  if (props.user) {
    form.username = props.user.username
    form.email = props.user.email
    form.firstName = props.user.firstName || ''
    form.lastName = props.user.lastName || ''
  }
})

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
  
  if (!form.firstName.trim()) {
    errors.firstName = 'First name is required'
  }
  
  if (!form.lastName.trim()) {
    errors.lastName = 'Last name is required'
  }
  
  if (!props.user && !form.password) {
    errors.password = 'Password is required'
  } else if (!props.user && form.password.length < 6) {
    errors.password = 'Password must be at least 6 characters'
  }
  
  if (!props.user && form.password !== form.confirmPassword) {
    errors.confirmPassword = 'Passwords do not match'
  }
  
  return !Object.values(errors).some(error => error)
}

const handleSubmit = async () => {
  if (!validateForm()) {
    return
  }
  
  loading.value = true
  
  try {
    let result
    
    if (props.user) {
      // Update existing user
      result = await usersStore.updateUser(props.user.id, {
        username: form.username.trim(),
        email: form.email.trim(),
        firstName: form.firstName.trim(),
        lastName: form.lastName.trim(),
      })
    } else {
      // Create new user
      result = await usersStore.createUser({
        username: form.username.trim(),
        email: form.email.trim(),
        password: form.password,
        firstName: form.firstName.trim(),
        lastName: form.lastName.trim(),
      })
    }
    
    if (result.success && result.user) {
      emit('saved', result.user)
    } else {
      // Handle error
      console.error('Failed to save user:', result.error)
    }
  } catch (error) {
    console.error('Error saving user:', error)
  } finally {
    loading.value = false
  }
}
</script>
