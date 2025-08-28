<template>
  <div class="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
    <div class="relative top-10 mx-auto p-5 border w-4/5 max-w-4xl shadow-lg rounded-md bg-white">
      <div class="mt-3">
        <!-- Header -->
        <div class="flex items-center justify-between mb-4">
          <h3 class="text-lg font-medium text-gray-900">
            {{ isEditing ? 'Edit RADIUS User' : 'Create RADIUS User' }}
          </h3>
          <button
            @click="$emit('close')"
            class="text-gray-400 hover:text-gray-600"
          >
            <XMarkIcon class="h-6 w-6" />
          </button>
        </div>

        <!-- Error Display -->
        <div v-if="error" class="mb-4">
          <div class="bg-red-50 border border-red-200 rounded-lg p-3">
            <div class="flex">
              <div class="flex-shrink-0">
                <ExclamationTriangleIcon class="h-5 w-5 text-red-400" />
              </div>
              <div class="ml-3">
                <p class="text-sm text-red-700">{{ error }}</p>
              </div>
            </div>
          </div>
        </div>

        <!-- Form -->
        <form @submit.prevent="handleSubmit" class="space-y-4">
          <!-- Username -->
          <div>
            <label for="username" class="block text-sm font-medium text-gray-700">
              Username <span class="text-red-500">*</span>
            </label>
            <UiInput
              id="username"
              v-model="form.username"
              type="text"
              :disabled="isEditing"
              :error="errors.username"
              placeholder="Enter username"
              required
            />
          </div>

          <!-- Password -->
          <div v-if="!isEditing || showPasswordField">
            <label for="password" class="block text-sm font-medium text-gray-700">
              Password <span v-if="!isEditing" class="text-red-500">*</span>
            </label>
            <UiInput
              id="password"
              v-model="form.password"
              type="password"
              :error="errors.password"
              placeholder="Enter password"
              :required="!isEditing"
            />
          </div>

          <!-- Show Password Field Toggle for Editing -->
          <div v-if="isEditing && !showPasswordField" class="text-center">
            <button
              type="button"
              @click="showPasswordField = true"
              class="text-sm text-blue-600 hover:text-blue-800"
            >
              Change Password
            </button>
          </div>

          <!-- Check Attributes Section -->
          <div class="border-t pt-4">
            <RadiusAttributesManager
              v-model="form.checkAttributes"
              title="Check Attributes"
              :show-common-attributes="true"
            />
          </div>

          <!-- Reply Attributes Section -->
          <div class="border-t pt-4">
            <RadiusAttributesManager
              v-model="form.replyAttributes"
              title="Reply Attributes"
              :show-common-attributes="true"
            />
          </div>

          <!-- Form Actions -->
          <div class="flex justify-end space-x-3 pt-4">
            <UiButton
              type="button"
              variant="secondary"
              @click="$emit('close')"
              :disabled="loading"
            >
              Cancel
            </UiButton>
            <UiButton
              type="submit"
              variant="primary"
              :loading="loading"
            >
              {{ isEditing ? 'Update User' : 'Create User' }}
            </UiButton>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { 
  XMarkIcon, 
  ExclamationTriangleIcon
} from '@heroicons/vue/24/outline'
import type { RadiusUser, CreateRadiusUserRequest, UpdateRadiusUserRequest, RadiusAttribute } from '~/types'

interface Props {
  user?: RadiusUser | null
}

interface Emits {
  close: []
  saved: []
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

// Store
const radiusUsersStore = useRadiusUsersStore()

// Computed
const isEditing = computed(() => !!props.user)

// Reactive data
const loading = ref(false)
const error = ref('')
const showPasswordField = ref(false)

const form = reactive({
  username: '',
  password: '',
  group: [] as string[],
  checkAttributes: [] as RadiusAttribute[],
  replyAttributes: [] as RadiusAttribute[]
})

const errors = reactive({
  username: '',
  password: ''
})

// Removed attributeEntries as we now use RadiusAttributesManager 

// Methods
const clearErrors = () => {
  errors.username = ''
  errors.password = ''
  error.value = ''
}

const validateForm = () => {
  clearErrors()
  let isValid = true

  // Username validation
  if (!form.username.trim()) {
    errors.username = 'Username is required'
    isValid = false
  } else if (form.username.length < 3) {
    errors.username = 'Username must be at least 3 characters'
    isValid = false
  }

  // Password validation (only for new users or when changing password)
  if (!isEditing.value || showPasswordField.value) {
    if (!form.password.trim()) {
      errors.password = 'Password is required'
      isValid = false
    } else if (form.password.length < 6) {
      errors.password = 'Password must be at least 6 characters'
      isValid = false
    }
  }

  return isValid
}

// Removed attribute management methods as they're now handled by RadiusAttributesManager

const handleSubmit = async () => {
  if (!validateForm()) {
    return
  }

  loading.value = true
  error.value = ''

  try {
    if (isEditing.value && props.user) {
      // Update existing user
      const updateData: UpdateRadiusUserRequest = {
        checkAttributes: form.checkAttributes,
        replyAttributes: form.replyAttributes,
        groups: form.group
      }

      if (showPasswordField.value && form.password) {
        updateData.password = form.password
      }

      const result = await radiusUsersStore.updateUser(props.user.username, updateData)
      
      if (result.success) {
        emit('saved')
      } else {
        error.value = result.message || 'Failed to update RADIUS user'
      }
    } else {
      // Create new user
      const createData: CreateRadiusUserRequest = {
        username: form.username,
        password: form.password,
        checkAttributes: form.checkAttributes,
        replyAttributes: form.replyAttributes,
        groups: form.group
      }

      const result = await radiusUsersStore.createUser(createData)
      
      if (result.success) {
        emit('saved')
      } else {
        error.value = result.message || 'Failed to create RADIUS user'
      }
    }
  } catch (err) {
    console.error('Error submitting form:', err)
    error.value = err instanceof Error ? err.message : 'An unexpected error occurred'
  } finally {
    loading.value = false
  }
}

// Initialize form when user prop changes
watch(
  () => props.user,
  (newUser) => {
    if (newUser) {
      form.username = newUser.username
      form.password = ''
      showPasswordField.value = false
      form.checkAttributes = newUser.checkAttributes || []
      form.replyAttributes = newUser.replyAttributes || []
    } else {
      // Reset form for new user
      form.username = ''
      form.password = ''
      form.checkAttributes = []
      form.replyAttributes = []
      showPasswordField.value = false
    }
    clearErrors()
  },
  { immediate: true }
)
</script>