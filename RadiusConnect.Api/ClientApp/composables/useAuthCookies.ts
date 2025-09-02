export const useAuthCookies = () => {
  // Determine if we're in a secure context (HTTPS or localhost)
  const isSecureContext = () => {
    if (process.client) {
      return window.location.protocol === 'https:' || 
             window.location.hostname === 'localhost' ||
             window.location.hostname === '127.0.0.1'
    }
    return false
  }

  // Get cookie options based on environment
  const getCookieOptions = (maxAge: number) => {
    const secure = isSecureContext()
    return {
      maxAge,
      secure,
      sameSite: secure ? 'strict' as const : 'lax' as const,
      httpOnly: false // Must be false for client-side access
    }
  }

  const getTokenCookie = () => useCookie('auth-token', getCookieOptions(60 * 60 * 24 * 7)) // 7 days

  const getRefreshTokenCookie = () => useCookie('refresh-token', getCookieOptions(60 * 60 * 24 * 30)) // 30 days

  const setToken = (token: string) => {
    const cookie = getTokenCookie()
    cookie.value = token
  }

  const setRefreshToken = (token: string) => {
    const cookie = getRefreshTokenCookie()
    cookie.value = token
  }

  const getToken = () => {
    const cookie = getTokenCookie()
    return cookie.value
  }

  const getRefreshToken = () => {
    const cookie = getRefreshTokenCookie()
    return cookie.value
  }

  const clearTokens = () => {
    const tokenCookie = getTokenCookie()
    const refreshCookie = getRefreshTokenCookie()
    tokenCookie.value = null
    refreshCookie.value = null
  }

  return {
    setToken,
    setRefreshToken,
    getToken,
    getRefreshToken,
    clearTokens
  }
}
