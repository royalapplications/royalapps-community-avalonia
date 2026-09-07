<script setup lang="ts">
import { computed } from 'vue'
import { useData, withBase } from 'vitepress'

const props = defineProps<{ name: string; alt: string; animated?: boolean }>()
const { isDark } = useData()
const source = computed(() => withBase(`/assets/demo/${props.name}-${isDark.value ? 'dark' : 'light'}.${props.animated ? 'gif' : 'png'}?v=borderless-2`))
</script>

<template>
  <img :key="source" :src="source" :alt="alt" class="demo-media" loading="lazy" />
</template>

<style scoped>
.demo-media { display: block; width: 100%; height: auto; border-radius: 8px; }
</style>
