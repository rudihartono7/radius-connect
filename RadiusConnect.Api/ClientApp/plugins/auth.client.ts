export default defineNuxtPlugin(() => {
  const authStore = useAuthStore()

  // Add response interceptor to handle token refresh
  addRouteMiddleware('auth-refresh', async (to, from) => {
    // Check if token is expired and refresh if needed
    const token = useCookie('auth-token').value
    if (token && !authStore.isAuthenticated) {
      try {
        await authStore.refreshToken()
      } catch (error) {
        // Token refresh failed, redirect to login
        authStore.clearAuth()
        return navigateTo('/login')
      }
    }
  }, { global: true })
})
