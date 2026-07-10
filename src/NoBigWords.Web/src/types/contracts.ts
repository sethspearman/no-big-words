export type RewriteMode = 'NoBigWords' | 'ExplainLikeImTen'

export interface RewriteRequest {
  text: string
  mode: RewriteMode
}

export interface ReplacementResult {
  original: string
  replacement: string
  startIndex: number
  length: number
  source: 'AllowedAsIs' | 'LocalDictionary' | 'InflectionRule' | 'OpenAiFallback'
  confidence: number
}

export interface UnknownWord {
  word: string
  startIndex: number
  length: number
}

export interface ValidationSummary {
  totalWords: number
  allowedWords: number
  disallowedWords: number
  allowedPercentage: number
}

export interface RewriteResponse {
  originalText: string
  rewrittenText: string
  replacements: ReplacementResult[]
  unknownWords: UnknownWord[]
  validation: ValidationSummary
  usedAi: boolean
  message?: string | null
}
