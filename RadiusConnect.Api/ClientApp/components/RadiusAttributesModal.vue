<template>
  <div class="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
    <div class="relative top-10 mx-auto p-5 border w-4/5 max-w-4xl shadow-lg rounded-md bg-white">
      <div class="mt-3">
        <!-- Header -->
        <div class="flex items-center justify-between mb-4">
          <div>
            <h3 class="text-lg font-medium text-gray-900">
              Manage Attributes for {{ user?.username }}
            </h3>
            <p class="text-sm text-gray-600">
              Configure check and reply attributes for RADIUS authentication
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

        <!-- Loading State -->
        <div v-if="loading" class="flex justify-center items-center py-8">
          <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        </div>

        <!-- Attributes Content -->
        <div v-else class="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <!-- Check Attributes -->
          <div class="bg-gray-50 rounded-lg p-4">
            <div class="flex items-center justify-between mb-4">
              <h4 class="text-md font-medium text-gray-900">Check Attributes</h4>
              <button
                @click="addCheckAttribute"
                class="text-sm text-blue-600 hover:text-blue-800 flex items-center"
              >
                <PlusIcon class="h-4 w-4 mr-1" />
                Add Check
              </button>
            </div>

            <div v-if="checkAttributes.length === 0" class="text-sm text-gray-500 text-center py-4">
              No check attributes defined
            </div>

            <div v-else class="space-y-3">
              <div
                v-for="(attr, index) in checkAttributes"
                :key="`check-${index}`"
                class="bg-white rounded border p-3"
              >
                <div class="flex items-center justify-between mb-2">
                  <span class="text-sm font-medium text-gray-700">Check Attribute</span>
                  <button
                    @click="removeCheckAttribute(index)"
                    class="text-red-600 hover:text-red-800"
                  >
                    <TrashIcon class="h-4 w-4" />
                  </button>
                </div>
                <div class="space-y-2">
                  <UiInput
                    v-model="attr.name"
                    placeholder="Attribute name (e.g., User-Password)"
                    class="w-full"
                  />
                  <UiInput
                    v-model="attr.value"
                    placeholder="Attribute value"
                    class="w-full"
                  />
                </div>
              </div>
            </div>
          </div>

          <!-- Reply Attributes -->
          <div class="bg-gray-50 rounded-lg p-4">
            <div class="flex items-center justify-between mb-4">
              <h4 class="text-md font-medium text-gray-900">Reply Attributes</h4>
              <button
                @click="addReplyAttribute"
                class="text-sm text-blue-600 hover:text-blue-800 flex items-center"
              >
                <PlusIcon class="h-4 w-4 mr-1" />
                Add Reply
              </button>
            </div>

            <div v-if="replyAttributes.length === 0" class="text-sm text-gray-500 text-center py-4">
              No reply attributes defined
            </div>

            <div v-else class="space-y-3">
              <div
                v-for="(attr, index) in replyAttributes"
                :key="`reply-${index}`"
                class="bg-white rounded border p-3"
              >
                <div class="flex items-center justify-between mb-2">
                  <span class="text-sm font-medium text-gray-700">Reply Attribute</span>
                  <button
                    @click="removeReplyAttribute(index)"
                    class="text-red-600 hover:text-red-800"
                  >
                    <TrashIcon class="h-4 w-4" />
                  </button>
                </div>
                <div class="space-y-2">
                  <UiInput
                    v-model="attr.name"
                    placeholder="Attribute name (e.g., Session-Timeout)"
                    class="w-full"
                  />
                  <UiInput
                    v-model="attr.value"
                    placeholder="Attribute value"
                    class="w-full"
                  />
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Common Attributes Reference -->
        <div class="mt-6 bg-blue-50 rounded-lg p-4">
          <h5 class="text-sm font-medium text-blue-900 mb-2">Common RADIUS Attributes</h5>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4 text-xs text-blue-800">
            <div>
              <strong>Check Attributes:</strong>
              <ul class="mt-1 space-y-1">
                <li>• User-Password</li>
                <li>• CHAP-Password</li>
                <li>• NAS-IP-Address</li>
                <li>• Called-Station-Id</li>
              </ul>
            </div>
            <div>
              <strong>Reply Attributes:</strong>
              <ul class="mt-1 space-y-1">
                <li>• Session-Timeout</li>
                <li>• Idle-Timeout</li>
                <li>• Framed-IP-Address</li>
                <li>• Filter-Id</li>
              </ul>
            </div>
          </div>
        </div>

        <!-- Form Actions -->
        <div class="flex justify-end space-x-3 pt-6">
          <UiButton
            type="button"
            variant="secondary"
            @click="$emit('close')"
            :disabled="saving"
          >
            Cancel
          </UiButton>
          <UiButton
            type="button"
            variant="primary"
            @click="saveAttributes"
            :loading="saving"
          >
            Save Attributes
          </UiButton>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { 
  XMarkIcon, 
  ExclamationTriangleIcon, 
  PlusIcon, 
  TrashIcon 
} from '@heroicons/vue/24/outline'
import type { RadiusUser } from '~/types'

interface Props {
  user: RadiusUser | null
}

interface Emits {
  close: []
  saved: []
}

interface AttributeEntry {
  name: string
  value: string
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

// Store
const radiusUsersStore = useRadiusUsersStore()

// Reactive data
const loading = ref(false)
const saving = ref(false)
const error = ref('')
const checkAttributes = ref<AttributeEntry[]>([])
const replyAttributes = ref<AttributeEntry[]>([])

// Methods
const addCheckAttribute = () => {
  checkAttributes.value.push({ name: '', value: '' })
}

const removeCheckAttribute = (index: number) => {
  checkAttributes.value.splice(index, 1)
}

const addReplyAttribute = () => {
  replyAttributes.value.push({ name: '', value: '' })
}

const removeReplyAttribute = (index: number) => {
  replyAttributes.value.splice(index, 1)
}

const loadUserAttributes = async () => {
  if (!props.user) return

  loading.value = true
  error.value = ''

  try {
    const result = await radiusUsersStore.getUserAttributes(props.user.username)
    
    if (result && result.success && result.data) {
      // Parse existing attributes
      const attributes = result.data
      
      // Separate check and reply attributes based on common patterns
      // This is a simplified approach - in a real implementation, 
      // you might want to fetch this information from the API
      checkAttributes.value = []
      replyAttributes.value = []
      
      if (props.user.attributes) {
        Object.entries(props.user.attributes).forEach(([name, value]) => {
          // Common check attributes (this is a simplified categorization)
          if (name.includes('Password') || name.includes('NAS-') || name.includes('Called-Station')) {
            checkAttributes.value.push({ name, value })
          } else {
            // Assume everything else is a reply attribute
            replyAttributes.value.push({ name, value })
          }
        })
      }
    }
  } catch (err) {
    console.error('Error loading user attributes:', err)
    error.value = 'Failed to load user attributes'
  } finally {
    loading.value = false
  }
}

const saveAttributes = async () => {
  if (!props.user) return

  saving.value = true
  error.value = ''

  try {
    // Build combined attributes object
    const allAttributes: Record<string, string> = {}
    
    // Add check attributes
    checkAttributes.value.forEach(attr => {
      if (attr.name.trim() && attr.value.trim()) {
        allAttributes[attr.name.trim()] = attr.value.trim()
      }
    })
    
    // Add reply attributes
    replyAttributes.value.forEach(attr => {
      if (attr.name.trim() && attr.value.trim()) {
        allAttributes[attr.name.trim()] = attr.value.trim()
      }
    })

    // Update user with new attributes
    const result = await radiusUsersStore.updateUser(props.user.username, {
      attributes: allAttributes
    })

    if (result.success) {
      emit('saved')
    } else {
      error.value = result.message || 'Failed to save attributes'
    }
  } catch (err) {
    console.error('Error saving attributes:', err)
    error.value = err instanceof Error ? err.message : 'Failed to save attributes'
  } finally {
    saving.value = false
  }
}

// Initialize when user changes
watch(
  () => props.user,
  (newUser) => {
    if (newUser) {
      loadUserAttributes()
    } else {
      checkAttributes.value = []
      replyAttributes.value = []
    }
  },
  { immediate: true }
)
</script>