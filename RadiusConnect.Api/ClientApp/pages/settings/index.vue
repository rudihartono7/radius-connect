<template>
  <div class="min-h-screen bg-gray-50">
    <LayoutHeader />
    
    <main class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
      <!-- Page Header -->
      <div class="px-4 sm:px-0 mb-8">
        <div class="flex justify-between items-center">
          <div>
            <h1 class="text-2xl font-semibold text-gray-900">Settings</h1>
            <p class="mt-1 text-sm text-gray-600">
              Configure application settings and preferences
            </p>
          </div>
          <UiButton variant="primary">
            <CheckIcon class="w-4 h-4 mr-2" />
            Save Changes
          </UiButton>
        </div>
      </div>

      <!-- Content Placeholder -->
      <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-12 text-center">
        <div class="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
          <Cog6ToothIcon class="w-8 h-8 text-gray-400" />
        </div>
        <h3 class="text-lg font-medium text-gray-900 mb-2">Application Settings</h3>
        <p class="text-gray-600 mb-6">
          This page will allow you to configure various application settings and preferences.
        </p>
        <div class="text-sm text-gray-500">
          Features coming soon:
        </div>
        <ul class="text-sm text-gray-500 mt-2 space-y-1">
          <li>• User profile and preferences</li>
          <li>• Security settings and 2FA configuration</li>
          <li>• Notification preferences</li>
          <li>• System configuration options</li>
        </ul>
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import { CheckIcon, Cog6ToothIcon } from '@heroicons/vue/24/outline'

definePageMeta({
  middleware: ['auth']
})
</script>
