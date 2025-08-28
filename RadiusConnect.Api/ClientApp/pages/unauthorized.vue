<template>
  <div class="min-h-screen bg-gray-50 flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full text-center">
      <div class="w-16 h-16 bg-red-100 rounded-full flex items-center justify-center mx-auto mb-4">
        <ExclamationTriangleIcon class="w-8 h-8 text-red-600" />
      </div>
      <h1 class="text-2xl font-semibold text-gray-900 mb-2">Access Denied</h1>
      <p class="text-gray-600 mb-6">
        You don't have permission to access this page. Please contact your administrator if you believe this is an error.
      </p>
      <div class="space-y-3">
        <UiButton @click="goBack" variant="primary" full-width>
          Go Back
        </UiButton>
        <UiButton @click="goHome" variant="outline" full-width>
          Go to Dashboard
        </UiButton>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ExclamationTriangleIcon } from '@heroicons/vue/24/outline'

definePageMeta({
  layout: false
})

const router = useRouter()

const goBack = () => {
  router.back()
}

const goHome = () => {
  router.push('/')
}
</script>
