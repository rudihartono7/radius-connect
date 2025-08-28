<template>
  <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
    <div class="flex items-center justify-between">
      <div>
        <p class="text-sm font-medium text-gray-600">{{ title }}</p>
        <p class="text-2xl font-semibold text-gray-900">{{ value }}</p>
      </div>
      <div class="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
        <component :is="iconComponent" class="w-6 h-6 text-blue-600" />
      </div>
    </div>
    
    <div v-if="trend" class="mt-4 flex items-center">
      <component 
        :is="trendIcon" 
        class="w-4 h-4 mr-1" 
        :class="trendDirection === 'up' ? 'text-green-500' : 'text-red-500'"
      />
      <span 
        class="text-sm font-medium"
        :class="trendDirection === 'up' ? 'text-green-600' : 'text-red-600'"
      >
        {{ trend }}
      </span>
      <span class="text-sm text-gray-500 ml-1">from last month</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { 
  UsersIcon, 
  SignalIcon, 
  UserGroupIcon, 
  CheckCircleIcon,
  ArrowUpIcon,
  ArrowDownIcon
} from '@heroicons/vue/24/outline'

interface Props {
  title: string
  value: string | number
  icon: string
  trend?: string
  trendDirection?: 'up' | 'down'
}

const props = withDefaults(defineProps<Props>(), {
  trendDirection: 'up'
})

const iconComponent = computed(() => {
  const icons: Record<string, any> = {
    UsersIcon,
    SignalIcon,
    UserGroupIcon,
    CheckCircleIcon,
  }
  return icons[props.icon] || UsersIcon
})

const trendIcon = computed(() => {
  return props.trendDirection === 'up' ? ArrowUpIcon : ArrowDownIcon
})
</script>
