<template>
  <div class="flex h-screen bg-gray-100">
    <!-- Sidebar -->
    <div class="flex flex-col w-64 bg-white shadow-lg">
      <!-- Logo -->
      <div class="flex items-center justify-center h-16 px-4 bg-blue-600">
        <NuxtLink to="/" class="flex items-center">
          <div class="w-8 h-8 bg-white rounded-lg flex items-center justify-center">
            <span class="text-blue-600 font-bold text-sm">R</span>
          </div>
          <span class="ml-2 text-xl font-semibold text-white">RadiusConnect</span>
        </NuxtLink>
      </div>

      <!-- Navigation -->
      <nav class="flex-1 px-4 py-6 space-y-2">
        <NuxtLink
          v-for="item in navigationItems"
          :key="item.name"
          :to="item.href"
          :class="[
            $route.path === item.href
              ? 'bg-blue-50 text-blue-700 border-r-2 border-blue-700'
              : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900',
            'group flex items-center px-3 py-2 text-sm font-medium rounded-md transition-colors duration-200'
          ]"
        >
          <component
            :is="item.icon"
            :class="[
              $route.path === item.href ? 'text-blue-500' : 'text-gray-400 group-hover:text-gray-500',
              'mr-3 h-5 w-5'
            ]"
          />
          {{ item.name }}
        </NuxtLink>
      </nav>

      <!-- User Section -->
      <div class="px-4 py-4 border-t border-gray-200">
        <!-- Notifications -->
        <div class="flex items-center justify-between mb-4">
          <button class="p-2 text-gray-400 hover:text-gray-500 relative rounded-md hover:bg-gray-100">
            <BellIcon class="h-5 w-5" />
            <span v-if="notificationCount > 0" class="absolute -top-1 -right-1 bg-red-500 text-white text-xs rounded-full h-4 w-4 flex items-center justify-center">
              {{ notificationCount }}
            </span>
          </button>
        </div>

        <!-- User Menu -->
        <Menu as="div" class="relative">
          <MenuButton class="flex items-center w-full px-3 py-2 text-sm rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500">
            <div class="w-8 h-8 bg-gray-300 rounded-full flex items-center justify-center">
              <span class="text-gray-700 font-medium text-sm">
                {{ userInitials }}
              </span>
            </div>
            <div class="ml-3 flex-1 text-left">
              <p class="text-sm font-medium text-gray-700">{{ authStore.currentUser?.username }}</p>
              <p class="text-xs text-gray-500">{{ authStore.currentUser?.email }}</p>
            </div>
            <ChevronUpIcon class="h-4 w-4 text-gray-400" />
          </MenuButton>

          <transition
            enter-active-class="transition ease-out duration-100"
            enter-from-class="transform opacity-0 scale-95"
            enter-to-class="transform opacity-100 scale-100"
            leave-active-class="transition ease-in duration-75"
            leave-from-class="transform opacity-100 scale-100"
            leave-to-class="transform opacity-0 scale-95"
          >
            <MenuItems class="absolute bottom-full left-0 right-0 mb-2 bg-white rounded-md shadow-lg py-1 z-50">
              <MenuItem v-slot="{ active }">
                <NuxtLink
                  to="/profile"
                  :class="[
                    active ? 'bg-gray-100' : '',
                    'block px-4 py-2 text-sm text-gray-700'
                  ]"
                >
                  <UserIcon class="inline h-4 w-4 mr-2" />
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
                  <CogIcon class="inline h-4 w-4 mr-2" />
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
                  <ArrowRightOnRectangleIcon class="inline h-4 w-4 mr-2" />
                  Sign out
                </button>
              </MenuItem>
            </MenuItems>
          </transition>
        </Menu>
      </div>
    </div>

    <!-- Main Content -->
    <div class="flex-1 flex flex-col overflow-hidden">
      <!-- Top Bar -->
      <header class="bg-white shadow-sm border-b border-gray-200 px-6 py-4">
        <div class="flex items-center justify-between">
          <h1 class="text-2xl font-semibold text-gray-900">{{ pageTitle }}</h1>
          <div class="flex items-center space-x-4">
            <!-- Additional header actions can go here -->
          </div>
        </div>
      </header>

      <!-- Page Content -->
      <main class="flex-1 overflow-y-auto bg-gray-50">
        <slot />
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Menu, MenuButton, MenuItem, MenuItems } from '@headlessui/vue'
import {
  BellIcon,
  ChevronUpIcon,
  UserIcon,
  CogIcon,
  ArrowRightOnRectangleIcon,
  HomeIcon,
  UsersIcon,
  UserGroupIcon,
  ServerIcon,
  ClipboardDocumentListIcon,
  DocumentTextIcon
} from '@heroicons/vue/24/outline'

const authStore = useAuthStore()
const router = useRouter()
const route = useRoute()

const notificationCount = ref(0)

const navigationItems = computed(() => {
  const items = [
    { name: 'Dashboard', href: '/', icon: HomeIcon },
    { name: 'Users', href: '/users', icon: UsersIcon },
    { name: 'Groups', href: '/groups', icon: UserGroupIcon },
    { name: 'RADIUS Users', href: '/radius/users', icon: UsersIcon },
    { name: 'RADIUS Groups', href: '/radius/groups', icon: UserGroupIcon },
    { name: 'Sessions', href: '/sessions', icon: ServerIcon },
    { name: 'Audit', href: '/audit', icon: DocumentTextIcon },
  ]
  
  // Filter based on user roles - Admin and Manager can see all items
  if (!authStore.isAdmin && !authStore.isManager) {
    return items.filter(item => item.name === 'Dashboard')
  }
  
  return items
})

const pageTitle = computed(() => {
  const currentItem = navigationItems.value.find(item => item.href === route.path)
  return currentItem?.name || 'RadiusConnect'
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