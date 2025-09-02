export const useAuthCookies = () => {
  // Use relaxed cookie settings for better compatibility
  // This allows tokens to work across different access methods (localhost, IP, domain)
  const getCookieOptions = (maxAge: number) => {
    return {
      maxAge,
      secure: false, // Allow HTTP access
      sameSite: 'lax' as const, // Allow cross-site requests
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
