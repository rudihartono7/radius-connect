<template>
  <div class="space-y-4">
    <div class="flex items-center justify-between">
      <h3 class="text-lg font-medium text-gray-900">{{ title }}</h3>
      <UiButton
        type="button"
        @click="addAttribute"
        variant="secondary"
        size="sm"
        class="flex items-center"
      >
        <PlusIcon class="h-4 w-4 mr-1" />
        Add Attribute
      </UiButton>
    </div>

    <div v-if="attributes.length === 0" class="text-center py-8 text-gray-500">
      <div class="text-sm">No attributes configured</div>
      <div class="text-xs mt-1">Click "Add Attribute" to get started</div>
    </div>

    <div v-else class="space-y-3">
      <div
        v-for="(attribute, index) in attributes"
        :key="index"
        class="flex items-center space-x-3 p-3 bg-gray-50 rounded-lg"
      >
        <!-- Attribute Name -->
        <div class="flex-1">
          <UiInput
            v-model="attribute.attribute"
            placeholder="Attribute name (e.g., Session-Timeout)"
            @input="updateAttribute(index, 'attribute', $event.target.value)"
          />
        </div>

        <!-- Operator -->
        <div class="w-20">
          <UiSelect
            :model-value="attribute.op"
            @update:model-value="updateAttribute(index, 'op', $event)"
          >
            <option value=":=">:=</option>
            <option value="==">==</option>
            <option value="!=">!=</option>
            <option value="<="><=</option>
            <option value=">=">>=</option>
            <option value="<"><</option>
            <option value=">">></option>
            <option value="=~">=~</option>
            <option value="!~">!~</option>
            <option value="+=">+=</option>
          </UiSelect>
        </div>

        <!-- Value -->
        <div class="flex-1">
          <UiInput
            v-model="attribute.value"
            placeholder="Value"
            @input="updateAttribute(index, 'value', $event.target.value)"
          />
        </div>

        <!-- Remove Button -->
        <UiButton
          type="button"
          @click="removeAttribute(index)"
          variant="danger"
          size="sm"
        >
          <TrashIcon class="h-4 w-4" />
        </UiButton>
      </div>
    </div>

    <!-- Common Attributes Helper -->
    <div v-if="showCommonAttributes" class="mt-4">
      <div class="text-sm font-medium text-gray-700 mb-2">Common Attributes:</div>
      <div class="flex flex-wrap gap-2">
        <button
          v-for="common in commonAttributes"
          :key="common.attribute"
          type="button"
          @click="addCommonAttribute(common)"
          class="px-3 py-1 text-xs bg-blue-100 text-blue-700 rounded-full hover:bg-blue-200 transition-colors"
        >
          {{ common.attribute }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { PlusIcon, TrashIcon } from '@heroicons/vue/24/outline'
import type { RadiusAttribute } from '~/types'

interface Props {
  modelValue: RadiusAttribute[]
  title: string
  showCommonAttributes?: boolean
}

interface Emits {
  (e: 'update:modelValue', value: RadiusAttribute[]): void
}

const props = withDefaults(defineProps<Props>(), {
  showCommonAttributes: true
})

const emit = defineEmits<Emits>()

const attributes = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value)
})

const commonAttributes = [
  { attribute: 'Session-Timeout', op: ':=', value: '3600' },
  { attribute: 'Idle-Timeout', op: ':=', value: '1800' },
  { attribute: 'Simultaneous-Use', op: ':=', value: '1' },
  { attribute: 'Service-Type', op: ':=', value: 'Framed-User' },
  { attribute: 'Framed-Protocol', op: ':=', value: 'PPP' },
  { attribute: 'Framed-MTU', op: ':=', value: '1500' },
  { attribute: 'Filter-Id', op: ':=', value: '' },
  { attribute: 'WISPr-Bandwidth-Max-Up', op: ':=', value: '1000000' },
  { attribute: 'WISPr-Bandwidth-Max-Down', op: ':=', value: '1000000' },
  { attribute: 'Cleartext-Password', op: ':=', value: '' },
  { attribute: 'Tunnel-Type', op: ':=', value: '' },
  { attribute: 'Tunnel-Medium-Type', op: ':=', value: '' },
  { attribute: 'Tunnel-Private-Group-Id', op: ':=', value: '' },
]

const addAttribute = () => {
  const newAttributes = [...attributes.value, { attribute: '', op: ':=', value: '' }]
  attributes.value = newAttributes
}

const removeAttribute = (index: number) => {
  const newAttributes = attributes.value.filter((_, i) => i !== index)
  attributes.value = newAttributes
}

const updateAttribute = (index: number, field: keyof RadiusAttribute, value: string) => {
  const newAttributes = [...attributes.value]
  newAttributes[index] = { ...newAttributes[index], [field]: value } as RadiusAttribute
  attributes.value = newAttributes
}

const addCommonAttribute = (common: RadiusAttribute) => {
  const newAttributes = [...attributes.value, { ...common }]
  attributes.value = newAttributes
}
</script>