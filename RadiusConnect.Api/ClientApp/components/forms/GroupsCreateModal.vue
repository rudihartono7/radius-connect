<template>
  <div class="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
    <div class="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
      <div class="mt-3">
        <!-- Header -->
        <div class="flex items-center justify-between mb-4">
          <h3 class="text-lg font-medium text-gray-900">Create RADIUS Group</h3>
          <button
            @click="$emit('close')"
            class="text-gray-400 hover:text-gray-600"
          >
            <XMarkIcon class="h-6 w-6" />
          </button>
        </div>

        <!-- Form -->
        <form @submit.prevent="handleSubmit" class="space-y-4">
          <UiInput
            v-model="form.groupName"
            label="Group Name"
            type="text"
            required
            :error="errors.groupName"
            placeholder="Enter group name"
          />

          <!-- Check Attributes -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Check Attributes (Optional)
            </label>
            <div class="space-y-2">
              <div v-for="(value, key) in form.checkAttributes" :key="`check-${key}`" class="flex space-x-2">
                <input
                  v-model="checkAttributeKeys[key]"
                  type="text"
                  placeholder="Attribute"
                  class="flex-1 px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
                <input
                  v-model="form.checkAttributes[key]"
                  type="text"
                  placeholder="Value"
                  class="flex-1 px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
                <button
                  type="button"
                  @click="removeCheckAttribute(key)"
                  class="px-2 py-2 text-red-600 hover:text-red-800"
                >
                  <TrashIcon class="h-4 w-4" />
                </button>
              </div>
              <button
                type="button"
                @click="addCheckAttribute"
                class="text-sm text-blue-600 hover:text-blue-800"
              >
                + Add Check Attribute
              </button>
            </div>
          </div>

          <!-- Reply Attributes -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Reply Attributes (Optional)
            </label>
            <div class="space-y-2">
              <div v-for="(value, key) in form.replyAttributes" :key="`reply-${key}`" class="flex space-x-2">
                <input
                  v-model="replyAttributeKeys[key]"
                  type="text"
                  placeholder="Attribute"
                  class="flex-1 px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
                <input
                  v-model="form.replyAttributes[key]"
                  type="text"
                  placeholder="Value"
                  class="flex-1 px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
                <button
                  type="button"
                  @click="removeReplyAttribute(key)"
                  class="px-2 py-2 text-red-600 hover:text-red-800"
                >
                  <TrashIcon class="h-4 w-4" />
                </button>
              </div>
              <button
                type="button"
                @click="addReplyAttribute"
                class="text-sm text-blue-600 hover:text-blue-800"
              >
                + Add Reply Attribute
              </button>
            </div>
          </div>

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
              Create Group
            </UiButton>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { XMarkIcon, TrashIcon } from '@heroicons/vue/24/outline'

const emit = defineEmits<{ close: []; saved: [group: any] }>()

const groupsStore = useGroupsStore()

const form = reactive({
  groupName: '',
  checkAttributes: {} as Record<string, string>,
  replyAttributes: {} as Record<string, string>
})

const checkAttributeKeys = reactive({} as Record<string, string>)
const replyAttributeKeys = reactive({} as Record<string, string>)

const errors = reactive({
  groupName: ''
})

const error = ref('')
const loading = ref(false)

const validateForm = () => {
  errors.groupName = ''
  
  if (!form.groupName.trim()) {
    errors.groupName = 'Group name is required'
  } else if (!/^[a-zA-Z0-9_-]+$/.test(form.groupName.trim())) {
    errors.groupName = 'Group name can only contain letters, numbers, underscores, and hyphens'
  }
  
  return !Object.values(errors).some(error => error)
}

const addCheckAttribute = () => {
  const key = `check_${Date.now()}`
  checkAttributeKeys[key] = ''
  form.checkAttributes[key] = ''
}

const removeCheckAttribute = (key: string) => {
  delete checkAttributeKeys[key]
  delete form.checkAttributes[key]
}

const addReplyAttribute = () => {
  const key = `reply_${Date.now()}`
  replyAttributeKeys[key] = ''
  form.replyAttributes[key] = ''
}

const removeReplyAttribute = (key: string) => {
  delete replyAttributeKeys[key]
  delete form.replyAttributes[key]
}

const handleSubmit = async () => {
  if (!validateForm()) {
    return
  }

  loading.value = true
  error.value = ''

  try {
    // Process attributes
    const processedCheckAttributes: Record<string, string> = {}
    const processedReplyAttributes: Record<string, string> = {}

    Object.keys(form.checkAttributes).forEach(key => {
      const attrKey = checkAttributeKeys[key]
      const attrValue = form.checkAttributes[key]
      if (attrKey && attrValue) {
        processedCheckAttributes[attrKey] = attrValue
      }
    })

    Object.keys(form.replyAttributes).forEach(key => {
      const attrKey = replyAttributeKeys[key]
      const attrValue = form.replyAttributes[key]
      if (attrKey && attrValue) {
        processedReplyAttributes[attrKey] = attrValue
      }
    })

    const result = await groupsStore.createGroup({
      groupName: form.groupName.trim(),
      checkAttributes: Object.keys(processedCheckAttributes).length > 0 ? processedCheckAttributes : undefined,
      replyAttributes: Object.keys(processedReplyAttributes).length > 0 ? processedReplyAttributes : undefined
    })

    if (result.success) {
      emit('saved', result.data)
    } else {
      error.value = result.error || 'Failed to create group'
    }
  } catch (err: any) {
    error.value = err.message || 'An error occurred'
  } finally {
    loading.value = false
  }
}
</script>
