<template>
  <select
    :value="modelValue"
    @change="handleChange"
    :class="[
      'block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm',
      'bg-white px-3 py-2 text-gray-900 placeholder-gray-500',
      'disabled:cursor-not-allowed disabled:bg-gray-50 disabled:text-gray-500',
      error ? 'border-red-300 focus:border-red-500 focus:ring-red-500' : 'border-gray-300'
    ]"
    :disabled="disabled"
  >
    <slot />
  </select>
  <p v-if="error" class="mt-1 text-sm text-red-600">{{ error }}</p>
</template>

<script setup lang="ts">
interface Props {
  modelValue?: string | number
  disabled?: boolean
  error?: string
}

interface Emits {
  (e: 'update:modelValue', value: string): void
}

const props = withDefaults(defineProps<Props>(), {
  disabled: false
})

const emit = defineEmits<Emits>()

const handleChange = (event: Event) => {
  const target = event.target as HTMLSelectElement
  emit('update:modelValue', target.value)
}
</script>