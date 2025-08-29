<template>
  <div>
    <NuxtLayout>
      <NuxtPage />
    </NuxtLayout>
  </div>
</template>

<script setup lang="ts">
const authStore = useAuthStore()

// Initialize auth immediately on app start (both client and server)
authStore.initializeAuth()
</script>
