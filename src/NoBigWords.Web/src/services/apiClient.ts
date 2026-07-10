import type { RewriteRequest, RewriteResponse } from '../types/contracts'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5183'

export async function rewriteText(request: RewriteRequest): Promise<RewriteResponse> {
  const response = await fetch(`${apiBaseUrl}/api/rewrite`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  })

  if (!response.ok) {
    throw new Error(`Rewrite request failed with status ${response.status}.`)
  }

  return response.json() as Promise<RewriteResponse>
}
