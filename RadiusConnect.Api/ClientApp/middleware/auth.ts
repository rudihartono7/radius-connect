export default defineNuxtRouteMiddleware(async (to, from) => {
  const authStore = useAuthStore()
  
  // Initialize auth if not already done
  if (!authStore.isAuthenticated) {
    await authStore.initializeAuth()
  }
  
  // Check if user is authenticated
  if (!authStore.isAuthenticated) {
    // Redirect to login if not authenticated
    return navigateTo('/login')
  }
  
  // If user is authenticated but trying to access login page, redirect to dashboard
  if (to.path === '/login') {
    return navigateTo('/')
  }
})
