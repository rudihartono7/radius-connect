<template>
  <div class="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
    <div class="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
      <div class="mt-3">
        <!-- Header -->
        <div class="flex items-center justify-between mb-4">
          <h3 class="text-lg font-medium text-gray-900">Add User to Group</h3>
          <button
            @click="$emit('close')"
            class="text-gray-400 hover:text-gray-600"
          >
            <XMarkIcon class="h-6 w-6" />
          </button>
        </div>

        <!-- Form -->
        <form @submit.prevent="handleSubmit" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Group: {{ groupName }}
            </label>
          </div>

          <UiInput
            v-model="form.username"
            label="Username"
            type="text"
            required
            :error="errors.username"
            placeholder="Enter username to add"
          />

          <UiInput
            v-model="form.priority"
            label="Priority (Optional)"
            type="number"
            :error="errors.priority"
            placeholder="Enter priority (1-10)"
            min="1"
            max="10"
          />

          <!-- Error Display -->
          <div v-if="error" class="bg-red-50 border border-red-200 rounded-lg p-3">
            <p class="text-sm text-red-700">{{ error }}</p>
          </div>

          <!-- Actions -->
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
              Add User
            </UiButton>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { XMarkIcon } from '@heroicons/vue/24/outline'

interface Props {
  groupName: string
}

const props = defineProps<Props>()
const emit = defineEmits<{ close: []; 'user-added': [] }>()

const groupsStore = useGroupsStore()

const form = reactive({
  username: '',
  priority: ''
})

const errors = reactive({
  username: '',
  priority: ''
})

const error = ref('')
const loading = ref(false)

const validateForm = () => {
  errors.username = ''
  errors.priority = ''
  
  if (!form.username.trim()) {
    errors.username = 'Username is required'
  }
  
  if (form.priority && (isNaN(Number(form.priority)) || Number(form.priority) < 1 || Number(form.priority) > 10)) {
    errors.priority = 'Priority must be a number between 1 and 10'
  }
  
  return !Object.values(errors).some(error => error)
}

const handleSubmit = async () => {
  if (!validateForm()) {
    return
  }

  loading.value = true
  error.value = ''

  try {
    const result = await groupsStore.addUserToGroup(
      props.groupName,
      form.username.trim(),
      form.priority ? Number(form.priority) : undefined
    )

    if (result.success) {
      emit('user-added')
    } else {
      error.value = result.error || 'Failed to add user to group'
    }
  } catch (err: any) {
    error.value = err.message || 'An error occurred'
  } finally {
    loading.value = false
  }
}
</script>
