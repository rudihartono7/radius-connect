<template>
  <header class="bg-white shadow-sm border-b border-gray-200">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex justify-between items-center h-16">
        <!-- Logo and Brand -->
        <div class="flex items-center">
          <NuxtLink to="/" class="flex items-center">
            <div class="w-8 h-8 bg-blue-600 rounded-lg flex items-center justify-center">
              <span class="text-white font-bold text-sm">R</span>
            </div>
            <span class="ml-2 text-xl font-semibold text-gray-900">RadiusConnect</span>
          </NuxtLink>
        </div>

        <!-- Navigation -->
        <nav class="hidden md:flex space-x-8">
          <NuxtLink
            v-for="item in navigationItems"
            :key="item.name"
            :to="item.href"
            :class="[
              $route.path === item.href
                ? 'text-blue-600 border-blue-600'
                : 'text-gray-500 border-transparent hover:text-gray-700 hover:border-gray-300',
              'inline-flex items-center px-1 pt-1 border-b-2 text-sm font-medium transition-colors duration-200'
            ]"
          >
            {{ item.name }}
          </NuxtLink>
        </nav>

        <!-- User Menu -->
        <div class="flex items-center space-x-4">
          <!-- Notifications -->
          <button class="p-2 text-gray-400 hover:text-gray-500 relative">
            <BellIcon class="h-6 w-6" />
            <span v-if="notificationCount > 0" class="absolute -top-1 -right-1 bg-red-500 text-white text-xs rounded-full h-5 w-5 flex items-center justify-center">
              {{ notificationCount }}
            </span>
          </button>

          <!-- User Dropdown -->
          <Menu as="div" class="relative">
            <MenuButton class="flex items-center space-x-2 text-sm rounded-full focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500">
              <div class="w-8 h-8 bg-gray-300 rounded-full flex items-center justify-center">
                <span class="text-gray-700 font-medium">
                  {{ userInitials }}
                </span>
              </div>
              <span class="hidden md:block text-gray-700">{{ authStore.currentUser?.username }}</span>
              <ChevronDownIcon class="h-4 w-4 text-gray-400" />
            </MenuButton>

            <transition
              enter-active-class="transition ease-out duration-100"
              enter-from-class="transform opacity-0 scale-95"
              enter-to-class="transform opacity-100 scale-100"
              leave-active-class="transition ease-in duration-75"
              leave-from-class="transform opacity-100 scale-100"
              leave-to-class="transform opacity-0 scale-95"
            >
              <MenuItems class="absolute right-0 mt-2 w-48 bg-white rounded-md shadow-lg py-1 z-50">
                <MenuItem v-slot="{ active }">
                  <NuxtLink
                    to="/profile"
                    :class="[
                      active ? 'bg-gray-100' : '',
                      'block px-4 py-2 text-sm text-gray-700'
                    ]"
                  >
                    Profile
                  </NuxtLink>
                </MenuItem>
                <MenuItem v-slot="{ active }">
                  <NuxtLink
                    to="/settings"
                    :class="[
                      active ? 'bg-gray-100' : '',
                      'block px-4 py-2 text-sm text-gray-700'
                    ]"
                  >
                    Settings
                  </NuxtLink>
                </MenuItem>
                <div class="border-t border-gray-100"></div>
                <MenuItem v-slot="{ active }">
                  <button
                    @click="handleLogout"
                    :class="[
                      active ? 'bg-gray-100' : '',
                      'block w-full text-left px-4 py-2 text-sm text-gray-700'
                    ]"
                  >
                    Sign out
                  </button>
                </MenuItem>
              </MenuItems>
            </transition>
          </Menu>
        </div>
      </div>
    </div>
  </header>
</template>

<script setup lang="ts">
import { Menu, MenuButton, MenuItem, MenuItems } from '@headlessui/vue'
import { BellIcon, ChevronDownIcon } from '@heroicons/vue/24/outline'

const authStore = useAuthStore()
const router = useRouter()

const notificationCount = ref(0)

const navigationItems = computed(() => {
  const items = [
    { name: 'Dashboard', href: '/' },
    { name: 'Users', href: '/users' },
    // { name: 'Groups', href: '/groups' },
    { name: 'RADIUS Users', href: '/radius/users' },
    { name: 'RADIUS Groups', href: '/radius/groups' },
    { name: 'Sessions', href: '/sessions' },
    { name: 'Audit', href: '/audit' },
  ]
  
  // Filter based on user roles - Admin and Manager can see all items
  if (!authStore.isAdmin && !authStore.isManager) {
    return items.filter(item => item.name === 'Dashboard')
  }
  
  return items
})

const userInitials = computed(() => {
  const user = authStore.currentUser
  if (!user) return 'U'
  
  const firstName = user.firstName || ''
  const lastName = user.lastName || ''
  
  if (firstName && lastName) {
    return `${firstName[0]}${lastName[0]}`.toUpperCase()
  }
  
  return user.username.substring(0, 2).toUpperCase()
})

const handleLogout = async () => {
  await authStore.logout()
  await router.push('/login')
}
</script>
