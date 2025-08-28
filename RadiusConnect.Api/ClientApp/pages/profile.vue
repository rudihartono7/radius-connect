<template>
  <div class="min-h-screen bg-gray-50 py-8">
    <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
      <!-- Header -->
      <div class="mb-8">
        <h1 class="text-3xl font-bold text-gray-900">My Profile</h1>
        <p class="mt-2 text-gray-600">Manage your account information and settings</p>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="flex justify-center items-center py-12">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="bg-red-50 border border-red-200 rounded-lg p-4 mb-6">
        <div class="flex">
          <div class="flex-shrink-0">
            <svg class="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
            </svg>
          </div>
          <div class="ml-3">
            <h3 class="text-sm font-medium text-red-800">Error loading profile</h3>
            <p class="mt-1 text-sm text-red-700">{{ error }}</p>
          </div>
        </div>
      </div>

      <!-- Profile Content -->
      <div v-else-if="user" class="space-y-6">
        <!-- Profile Card -->
        <div class="bg-white shadow rounded-lg">
          <div class="px-6 py-4 border-b border-gray-200">
            <h2 class="text-lg font-medium text-gray-900">Profile Information</h2>
          </div>
          <div class="px-6 py-4">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <!-- Avatar Section -->
              <div class="col-span-full flex items-center space-x-6">
                <div class="flex-shrink-0">
                  <div class="h-20 w-20 rounded-full bg-blue-600 flex items-center justify-center">
                    <span class="text-2xl font-bold text-white">
                      {{ getInitials(user.firstName, user.lastName) }}
                    </span>
                  </div>
                </div>
                <div>
                  <h3 class="text-xl font-semibold text-gray-900">
                    {{ user.firstName }} {{ user.lastName }}
                  </h3>
                  <p class="text-gray-600">@{{ user.username }}</p>
                  <div class="mt-2 flex items-center space-x-2">
                    <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium" 
                          :class="user.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'">
                      {{ user.isActive ? 'Active' : 'Inactive' }}
                    </span>
                    <span v-if="user.isTotpEnabled" 
                          class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                      2FA Enabled
                    </span>
                  </div>
                </div>
              </div>

              <!-- Basic Information -->
              <div class="space-y-4">
                <div>
                  <label class="block text-sm font-medium text-gray-700">Username</label>
                  <p class="mt-1 text-sm text-gray-900">{{ user.username }}</p>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700">Email</label>
                  <p class="mt-1 text-sm text-gray-900">{{ user.email }}</p>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700">First Name</label>
                  <p class="mt-1 text-sm text-gray-900">{{ user.firstName || 'Not provided' }}</p>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700">Last Name</label>
                  <p class="mt-1 text-sm text-gray-900">{{ user.lastName || 'Not provided' }}</p>
                </div>
              </div>

              <!-- Account Details -->
              <div class="space-y-4">
                <div>
                  <label class="block text-sm font-medium text-gray-700">Account Status</label>
                  <p class="mt-1 text-sm" :class="user.isActive ? 'text-green-600' : 'text-red-600'">
                    {{ user.isActive ? 'Active' : 'Inactive' }}
                  </p>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700">Two-Factor Authentication</label>
                  <p class="mt-1 text-sm" :class="user.isTotpEnabled ? 'text-green-600' : 'text-gray-600'">
                    {{ user.isTotpEnabled ? 'Enabled' : 'Disabled' }}
                  </p>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700">Last Login</label>
                  <p class="mt-1 text-sm text-gray-900">
                    {{ user.lastLogin ? formatDate(user.lastLogin) : 'Never' }}
                  </p>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700">Member Since</label>
                  <p class="mt-1 text-sm text-gray-900">{{ formatDate(user.createdAt) }}</p>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Roles Card -->
        <div class="bg-white shadow rounded-lg">
          <div class="px-6 py-4 border-b border-gray-200">
            <h2 class="text-lg font-medium text-gray-900">Roles & Permissions</h2>
          </div>
          <div class="px-6 py-4">
            <div v-if="authStore.currentUser?.roles && authStore.currentUser.roles.length > 0" class="flex flex-wrap gap-2">
              <span v-for="role in authStore.currentUser.roles" :key="role" 
                    class="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium"
                    :class="getRoleColor(role)">
                {{ role }}
              </span>
            </div>
            <p v-else class="text-gray-500 text-sm">No roles assigned</p>
          </div>
        </div>

        <!-- Actions Card -->
        <div class="bg-white shadow rounded-lg">
          <div class="px-6 py-4 border-b border-gray-200">
            <h2 class="text-lg font-medium text-gray-900">Account Actions</h2>
          </div>
          <div class="px-6 py-4">
            <div class="flex flex-wrap gap-4">
              <button @click="refreshProfile" 
                      class="inline-flex items-center px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500">
                <svg class="-ml-1 mr-2 h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                </svg>
                Refresh Profile
              </button>
              <NuxtLink to="/settings" 
                        class="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500">
                <svg class="-ml-1 mr-2 h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
                Account Settings
              </NuxtLink>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { User } from '~/types'

// Page metadata
definePageMeta({
  middleware: 'auth',
  title: 'My Profile'
})

// Reactive state
const user = ref<User | null>(null)
const loading = ref(true)
const error = ref<string | null>(null)

// Auth store
const authStore = useAuthStore()

// Load user profile on mount
onMounted(async () => {
  await loadProfile()
})

// Methods
const loadProfile = async () => {
  try {
    loading.value = true
    error.value = null
    
    const result = await authStore.getCurrentUser()
    if (result.success && result.user) {
      user.value = result.user
    } else {
      error.value = result.error || 'Failed to load profile'
    }
  } catch (err: any) {
    error.value = err.message || 'An unexpected error occurred'
  } finally {
    loading.value = false
  }
}

const refreshProfile = async () => {
  await loadProfile()
}

const getInitials = (firstName?: string, lastName?: string): string => {
  const first = firstName?.charAt(0)?.toUpperCase() || ''
  const last = lastName?.charAt(0)?.toUpperCase() || ''
  return first + last || 'U'
}

const formatDate = (dateString: string): string => {
  try {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    })
  } catch {
    return 'Invalid date'
  }
}

const getRoleColor = (role: string): string => {
  const colors: Record<string, string> = {
    'Admin': 'bg-red-100 text-red-800',
    'Manager': 'bg-yellow-100 text-yellow-800',
    'User': 'bg-blue-100 text-blue-800',
    'Moderator': 'bg-purple-100 text-purple-800'
  }
  return colors[role] || 'bg-gray-100 text-gray-800'
}

// SEO
useHead({
  title: 'My Profile - RadiusConnect',
  meta: [
    { name: 'description', content: 'View and manage your RadiusConnect account profile information' }
  ]
})
</script>

<style scoped>
/* Add any custom styles here if needed */
</style>