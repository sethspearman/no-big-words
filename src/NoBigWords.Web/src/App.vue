<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { rewriteText } from './services/apiClient'
import type { RewriteMode, RewriteResponse } from './types/contracts'

const inputText = ref('The astronaut used a telescope to observe the planet.')
const selectedMode = ref<RewriteMode>('NoBigWords')
const response = ref<RewriteResponse | null>(null)
const errorMessage = ref('')
const isLoading = ref(false)

let pendingRequest = 0
let debounceHandle: number | undefined

const modeDescription = computed(() =>
  selectedMode.value === 'NoBigWords'
    ? 'Strict, local-first, and cheerfully awkward when needed.'
    : "AI-assisted and tuned for an actual 10-year-old's understanding."
)

const unknownWordList = computed(() => response.value?.unknownWords ?? [])
const replacementList = computed(() => response.value?.replacements ?? [])
const validation = computed(() => response.value?.validation)

async function runRewrite() {
  if (!inputText.value.trim()) {
    response.value = null
    errorMessage.value = ''
    return
  }

  const requestId = ++pendingRequest
  isLoading.value = true
  errorMessage.value = ''

  try {
    const next = await rewriteText({
      text: inputText.value,
      mode: selectedMode.value,
    })

    if (requestId === pendingRequest) {
      response.value = next
    }
  } catch (error) {
    if (requestId === pendingRequest) {
      errorMessage.value = error instanceof Error ? error.message : 'Rewrite failed.'
    }
  } finally {
    if (requestId === pendingRequest) {
      isLoading.value = false
    }
  }
}

watch([inputText, selectedMode], () => {
  window.clearTimeout(debounceHandle)
  debounceHandle = window.setTimeout(() => {
    void runRewrite()
  }, 300)
}, { immediate: true })

async function copyOutput() {
  if (!response.value?.rewrittenText) {
    return
  }

  await navigator.clipboard.writeText(response.value.rewrittenText)
}

function clearAll() {
  inputText.value = ''
  response.value = null
  errorMessage.value = ''
}
</script>

<template>
  <main class="page-shell">
    <section class="hero">
      <p class="eyebrow">No Big Words</p>
      <h1>Say hard things with only small words.</h1>
      <p class="tagline">Badly, if needed.</p>
      <p class="intro">{{ modeDescription }}</p>
    </section>

    <section class="toolbar">
      <label class="mode-picker">
        <input v-model="selectedMode" type="radio" value="NoBigWords" />
        <span>No Big Words</span>
      </label>
      <label class="mode-picker">
        <input v-model="selectedMode" type="radio" value="ExplainLikeImTen" />
        <span>Explain Like I'm 10</span>
      </label>
      <div class="toolbar-actions">
        <button class="ghost-button" type="button" @click="clearAll">Clear</button>
        <button class="primary-button" type="button" :disabled="!response?.rewrittenText" @click="copyOutput">
          Copy output
        </button>
      </div>
    </section>

    <section class="editor-grid">
      <article class="panel">
        <div class="panel-header">
          <h2>Input</h2>
          <span>{{ inputText.length }} chars</span>
        </div>
        <textarea
          v-model="inputText"
          class="editor"
          placeholder="Paste or type something complicated here..."
        />
      </article>

      <article class="panel">
        <div class="panel-header">
          <h2>Output</h2>
          <span v-if="response?.usedAi" class="badge">AI-assisted</span>
        </div>
        <div class="output-pane">
          <p v-if="response?.rewrittenText">{{ response.rewrittenText }}</p>
          <p v-else class="placeholder">Your rewrite will appear here.</p>
        </div>
        <p v-if="response?.message" class="message">{{ response.message }}</p>
        <p v-if="errorMessage" class="error">{{ errorMessage }}</p>
        <p v-if="isLoading" class="loading">Rewriting...</p>
      </article>
    </section>

    <section class="details-grid">
      <article class="panel compact-panel">
        <h3>Validation</h3>
        <dl v-if="validation" class="stats-grid">
          <div>
            <dt>Total words</dt>
            <dd>{{ validation.totalWords }}</dd>
          </div>
          <div>
            <dt>Allowed words</dt>
            <dd>{{ validation.allowedWords }}</dd>
          </div>
          <div>
            <dt>Disallowed words</dt>
            <dd>{{ validation.disallowedWords }}</dd>
          </div>
          <div>
            <dt>Allowed %</dt>
            <dd>{{ validation.allowedPercentage }}</dd>
          </div>
        </dl>
        <p v-else class="placeholder">Validation stats will show up here.</p>
      </article>

      <article class="panel compact-panel">
        <h3>Unknown words</h3>
        <ul v-if="unknownWordList.length" class="pill-list">
          <li v-for="word in unknownWordList" :key="`${word.word}-${word.startIndex}`">{{ word.word }}</li>
        </ul>
        <p v-else class="placeholder">No unresolved words right now.</p>
      </article>

      <article class="panel compact-panel">
        <h3>Replacements</h3>
        <ul v-if="replacementList.length" class="replacement-list">
          <li v-for="item in replacementList" :key="`${item.original}-${item.startIndex}`">
            <strong>{{ item.original }}</strong>
            <span>{{ item.replacement }}</span>
            <small>{{ item.source }} · {{ item.confidence }}</small>
          </li>
        </ul>
        <p v-else class="placeholder">Applied replacements will show up here.</p>
      </article>
    </section>
  </main>
</template>
