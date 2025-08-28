<template>
  <div class="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
    <div class="relative top-10 mx-auto p-5 border w-full max-w-2xl shadow-lg rounded-md bg-white">
      <div class="mt-3">
        <!-- Header -->
        <div class="flex items-center justify-between mb-4">
          <div>
            <h3 class="text-lg font-medium text-gray-900">
              {{ isEditing ? 'Edit Group' : 'Create New Group' }}
            </h3>
            <p class="text-sm text-gray-600">
              {{ isEditing ? 'Update group information and attributes' : 'Create a new RADIUS group with check and reply attributes' }}
            </p>
          </div>
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
        <form @submit.prevent="handleSubmit" class="space-y-6">
          <!-- Group Name -->
          <div>
            <label for="groupName" class="block text-sm font-medium text-gray-700 mb-1">
              Group Name *
            </label>
            <UiInput
              id="groupName"
              v-model="formData.groupName"
              placeholder="Enter group name"
              :disabled="isEditing || loading"
              required
              class="w-full"
            />
            <p class="text-xs text-gray-500 mt-1">
              {{ isEditing ? 'Group name cannot be changed after creation' : 'Choose a unique name for this group' }}
            </p>
          </div>

          <!-- Check Attributes -->
          <div class="border-t pt-6">
            <RadiusAttributesManager
              v-model="formData.checkAttributes"
              title="Check Attributes"
              :show-common-attributes="true"
            />
          </div>

          <!-- Reply Attributes -->
          <div class="border-t pt-6">
            <RadiusAttributesManager
              v-model="formData.replyAttributes"
              title="Reply Attributes"
              :show-common-attributes="true"
            />
          </div>

          <!-- Form Actions -->
          <div class="flex justify-end space-x-3 pt-6">
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
              {{ isEditing ? 'Update Group' : 'Create Group' }}
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
import type { RadiusGroup, CreateRadiusGroupRequest, UpdateRadiusGroupRequest, RadiusAttribute } from '~/types'
import RadiusAttributesManager from '~/components/RadiusAttributesManager.vue'

interface Props {
  group?: RadiusGroup | null
}

interface Emits {
  close: []
  saved: []
}

// Removed AttributeEntry interface as we now use RadiusAttribute

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

// Store
const radiusGroupsStore = useRadiusGroupsStore()

// Reactive data
const loading = ref(false)
const error = ref('')

const formData = reactive({
  groupName: '',
  checkAttributes: [] as RadiusAttribute[],
  replyAttributes: [] as RadiusAttribute[]
})

// Computed properties
const isEditing = computed(() => !!props.group)

// Removed attribute management methods as they're now handled by RadiusAttributesManager

const initializeForm = () => {
  if (props.group) {
    // Editing existing group
    formData.groupName = props.group.groupName
    formData.checkAttributes = props.group.checkAttributes || []
    formData.replyAttributes = props.group.replyAttributes || []
  } else {
    // Creating new group
    formData.groupName = ''
    formData.checkAttributes = []
    formData.replyAttributes = []
  }
}

const validateForm = (): boolean => {
  error.value = ''
  
  if (!formData.groupName.trim()) {
    error.value = 'Group name is required'
    return false
  }
  
  // Validate check attributes
  for (const attr of formData.checkAttributes) {
    if (attr.attribute.trim() && !attr.value.trim()) {
      error.value = 'All check attributes must have both name and value'
      return false
    }
    if (!attr.attribute.trim() && attr.value.trim()) {
      error.value = 'All check attributes must have both name and value'
      return false
    }
  }
  
  // Validate reply attributes
  for (const attr of formData.replyAttributes) {
    if (attr.attribute.trim() && !attr.value.trim()) {
      error.value = 'All reply attributes must have both name and value'
      return false
    }
    if (!attr.attribute.trim() && attr.value.trim()) {
      error.value = 'All reply attributes must have both name and value'
      return false
    }
  }
  
  return true
}

const handleSubmit = async () => {
  if (!validateForm()) return
  
  loading.value = true
  error.value = ''
  
  try {
    let result
    if (isEditing.value) {
      const updateData: UpdateRadiusGroupRequest = {
        checkAttributes: formData.checkAttributes,
        replyAttributes: formData.replyAttributes
      }
      result = await radiusGroupsStore.updateGroup(props.group!.groupName, updateData)
    } else {
      const createData: CreateRadiusGroupRequest = {
        groupName: formData.groupName.trim(),
        checkAttributes: formData.checkAttributes,
        replyAttributes: formData.replyAttributes
      }
      result = await radiusGroupsStore.createGroup(createData)
    }
    
    if (result.success) {
      emit('saved')
    } else {
      error.value = result.message || `Failed to ${isEditing.value ? 'update' : 'create'} group`
    }
  } catch (err) {
    console.error('Error saving group:', err)
    error.value = err instanceof Error ? err.message : `Failed to ${isEditing.value ? 'update' : 'create'} group`
  } finally {
    loading.value = false
  }
}

// Initialize form when component mounts or group prop changes
watch(
  () => props.group,
  () => {
    initializeForm()
  },
  { immediate: true }
)
</script>