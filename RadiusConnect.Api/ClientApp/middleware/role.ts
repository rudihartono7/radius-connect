export default defineNuxtRouteMiddleware((to, from) => {
  const authStore = useAuthStore()
  
  // Get required roles from route meta
  const requiredRoles = to.meta.roles as string[] || []
  
  if (requiredRoles.length === 0) {
    return // No role requirements, allow access
  }
  
  // Check if user has any of the required roles
  const hasRequiredRole = requiredRoles.some(role => authStore.hasRole(role))
  
  if (!hasRequiredRole) {
    // User doesn't have required role, redirect to unauthorized page
    return navigateTo('/unauthorized')
  }
})
