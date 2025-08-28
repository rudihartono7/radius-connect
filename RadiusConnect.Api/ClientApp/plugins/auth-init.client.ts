export default defineNuxtPlugin(async () => {
  const authStore = useAuthStore()
  
  // Initialize auth store on app startup
  // This ensures user data and roles are loaded before navigation
  await authStore.initializeAuth()
})