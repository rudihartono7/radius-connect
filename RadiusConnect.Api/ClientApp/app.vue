<template>
  <div>
    <NuxtLayout>
      <NuxtPage />
    </NuxtLayout>
  </div>
</template>

<script setup lang="ts">
const authStore = useAuthStore()

// Initialize auth on app start
onMounted(() => {
  authStore.initializeAuth()
})
</script>
