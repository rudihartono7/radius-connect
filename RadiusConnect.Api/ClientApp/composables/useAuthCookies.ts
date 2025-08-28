export const useAuthCookies = () => {
  const getTokenCookie = () => useCookie('auth-token', {
    maxAge: 60 * 60 * 24 * 7, // 7 days
    secure: true,
    sameSite: 'strict'
  })

  const getRefreshTokenCookie = () => useCookie('refresh-token', {
    maxAge: 60 * 60 * 24 * 30, // 30 days
    secure: true,
    sameSite: 'strict'
  })

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
