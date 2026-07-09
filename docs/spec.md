# No Big Words Specification

## Product Overview

**No Big Words** is a web application for rewriting text in two distinct ways:

1. **No Big Words**: a strict, playful mode that tries to express ideas using only a small allowed vocabulary, even when the result is awkward or funny.
2. **Explain Like I'm 10**: a more natural AI-assisted mode that rewrites text for clarity and understanding rather than strict vocabulary compliance.

Suggested tagline:

> Say hard things with only small words. Badly, if needed.

The product is intentionally opinionated. In strict mode, charm matters more than elegance. The app should feel fast, clear, and fun to experiment with.

## Goals

- Deliver a single-page app that rewrites text live as the user types.
- Make **No Big Words** mode fast and mostly local-first.
- Use OpenAI only when needed in strict mode, and openly in the explain mode.
- Show users what changed, what could not be changed, and whether the result still breaks the allowed-word rules.
- Keep the architecture simple, testable, and easy to evolve.

## Non-Goals

- User accounts
- Saved history
- Collaboration
- Payments
- Complex admin tools
- Perfect grammar
- Perfect semantic accuracy
- Full natural-language understanding

## Target Users

- Curious writers who want a fun language constraint
- Teachers or parents who want simpler wording
- Developers or technical writers exploring plain-language rewrites
- People who enjoy the "Thing Explainer" style of forced simplification

## Product Principles

- **Fast first**: strict mode should feel immediate.
- **Transparent**: the app should explain what it changed and where it struggled.
- **Playful, not precious**: awkward output is acceptable in strict mode.
- **Safe fallbacks**: AI should be bounded, validated, and clearly labeled.
- **Simple to ship**: prefer straightforward code over heavy abstractions.

## User Stories

- As a user, I can paste technical text and quickly see a smaller-word version.
- As a user, I can switch between strict and natural explanation modes.
- As a user, I can see which words are still too big.
- As a user, I can copy the rewritten output with one click.
- As a user, I can clear both editors and start over quickly.
- As a user, I can understand whether the output came from local rules or AI help.
- As a developer, I can add or refine word replacements without changing core logic.
- As a developer, I can test the rewrite engine without depending on OpenAI.

## Core Modes

### No Big Words

This is the default mode and the core identity of the product.

Behavior:

- Uses a local allowed-word list.
- Detects words not present in the allowed list.
- Replaces disallowed words using a local replacement dictionary.
- Supports one-word and multi-word replacements.
- Applies simple inflection handling before giving up.
- Avoids OpenAI unless local rewriting cannot produce a good enough result.
- Validates the final rewritten text against the allowed list.
- Highlights remaining disallowed words in the output.
- Updates live with a debounce so typing stays responsive.

Examples:

- `planet` -> `world`
- `astronaut` -> `space person`
- `telescope` -> `far-seeing thing`
- `microscope` -> `small-seeing thing`
- `oxygen` -> `air part we need`
- `photograph` -> `picture`
- `automobile` -> `road car`

### Explain Like I'm 10

This mode optimizes for understanding rather than strict vocabulary limits.

Behavior:

- Uses the OpenAI API for the rewrite.
- Does not enforce the strict allowed-word list.
- Rewrites input so a typical 10-year-old could follow it.
- Prefers short sentences and natural phrasing.
- Explains jargon if it must use it.
- Clearly marks the mode as AI-powered.

## User Experience and UI Design

## Page Structure

The application should be a clean single-page experience with:

- App title: **No Big Words**
- Tagline
- Short mode description
- Mode selector
- Input editor
- Output editor or output pane
- Copy output button
- Clear button
- Validation summary
- Unknown-word list
- Optional replacement details panel

## Layout

### Desktop

- Two-column layout
- Left column: input editor and helper text
- Right column: rewritten output, validation, and replacement details

### Mobile

- Stacked layout
- Input on top
- Output below
- Summary cards and details collapsed below output

## UI Sections

### Header

- Title
- Tagline
- One-line explanation of the current mode

### Mode Selector

- Segmented control or radio-button group
- Options:
  - `No Big Words`
  - `Explain Like I'm 10`
- Mode switch updates helper text and badges immediately

### Input Editor

- Large textarea with placeholder text
- Live character and word count
- Debounced rewrite trigger
- Optional sample-text button in later versions

### Output Pane

- Read-only textarea or styled text pane
- Highlights disallowed words in strict mode
- Displays source badge when AI fallback contributed

### Action Row

- `Copy output`
- `Clear`
- Optional `Swap example` or `Try sample` in later versions

### Validation Summary

- Total words
- Allowed words
- Disallowed words
- Allowed percentage

### Unknown Words Panel

- Lists words that could not be replaced locally
- Shows count and occurrence order
- In strict mode, indicates whether AI fallback was attempted

### Replacement Details Panel

- Collapsible by default on mobile
- Shows original word, replacement, source, and confidence

## Visual Design Direction

- Simple, bright, and approachable
- Light-first design with clear contrast
- Skeleton CSS is acceptable as a base, with a thin custom stylesheet for spacing, badges, panels, and highlights
- Avoid a cluttered "tool dashboard" feel; the main interaction should remain text-in, text-out

## Functional Requirements

### Input and Rewrite Flow

- The app rewrites text after a short debounce, such as 250 to 400 ms.
- Empty input clears the output and summary.
- Very large inputs should show a friendly limit or process by paragraph in future versions.
- In-flight rewrite requests should be canceled when new typing occurs.

### Strict-Mode Validation

- Every tokenized word in output is checked against the allowed vocabulary.
- Words still outside the allowed list are surfaced visually and in metadata.
- Replacement phrases must themselves be validated word-by-word.

### Transparency

- The UI should show whether each replacement came from:
  - allowed as-is
  - local dictionary
  - inflection rule
  - OpenAI fallback

## Proposed Information Architecture

- `/docs/spec.md`
- `/src/NoBigWords.Api`
- `/src/NoBigWords.Core`
- `/src/NoBigWords.Infrastructure`
- `/src/NoBigWords.Web`
- `/tests/NoBigWords.Tests`
- `/data/allowed-words.txt`
- `/data/replacements.json`
- `/data/common-words-10000.txt`

## Technical Stack

- **Backend**: ASP.NET Core Web API on the latest .NET LTS
- **Frontend**: Vue 3 with TypeScript
- **Styling**: Skeleton CSS plus app-specific CSS
- **Persistence**: none for v1 unless lightweight telemetry or editable dictionaries are later added
- **Database if needed**: SQLite
- **Data access if needed**: Dapper
- **Tests**: xUnit

## Backend Architecture

## High-Level Design

Use a layered architecture with constructor-injected services:

- **API layer**: request validation, HTTP contracts, controllers/endpoints
- **Application/Core layer**: rewrite orchestration, tokenization, validation, replacement logic
- **Infrastructure layer**: file-backed vocabulary providers, replacement dictionary loading, OpenAI client

## Key Services

- `IRewriteService`
- `ITokenizer`
- `IAllowedWordsProvider`
- `IReplacementDictionary`
- `IWordNormalizer`
- `IWordInflectionService`
- `IValidationService`
- `IAiRewriteService`

## Request Flow

1. Controller receives `RewriteRequest`.
2. `IRewriteService` branches by mode.
3. Strict mode runs local rewrite pipeline first.
4. If strict mode hits unresolved terms, optional targeted AI fallback is attempted.
5. Final text is validated.
6. Structured metadata is returned to the client.

## Frontend Architecture

## Application Structure

Suggested Vue structure:

- `App.vue`
- `components/ModeSelector.vue`
- `components/InputEditor.vue`
- `components/OutputPane.vue`
- `components/ValidationSummary.vue`
- `components/UnknownWordsPanel.vue`
- `components/ReplacementsPanel.vue`
- `services/apiClient.ts`
- `stores/rewriteStore.ts` or simple composables for v1
- `types/contracts.ts`

## Frontend Responsibilities

- Capture user input and selected mode
- Debounce rewrite calls
- Cancel stale requests
- Render validation and unknown-word state
- Highlight output issues in strict mode
- Surface AI usage clearly in explain mode and fallback cases

## State Model

Suggested client state:

- `inputText`
- `selectedMode`
- `isLoading`
- `rewriteResponse`
- `errorMessage`
- `lastCopiedAt`

## API Design

## Endpoints

### `POST /api/rewrite`

Primary rewrite endpoint. Selects local-first strict mode or full AI explain mode based on the request.

Request:

```json
{
  "text": "The astronaut used a telescope to observe the planet.",
  "mode": "NoBigWords"
}
```

Response:

```json
{
  "originalText": "The astronaut used a telescope to observe the planet.",
  "rewrittenText": "The space person used a far-seeing thing to see the world.",
  "replacements": [
    {
      "original": "astronaut",
      "replacement": "space person",
      "startIndex": 4,
      "length": 9,
      "source": "LocalDictionary",
      "confidence": 0.9
    }
  ],
  "unknownWords": [],
  "validation": {
    "totalWords": 12,
    "allowedWords": 12,
    "disallowedWords": 0,
    "allowedPercentage": 100.0
  }
}
```

### `POST /api/validate`

Validates arbitrary text against the allowed-word list without rewriting.

### `GET /api/vocabulary/allowed`

Returns vocabulary metadata for tooling and debugging. For v1, this may return a count and version rather than the full list if payload size becomes awkward.

### `GET /api/replacements`

Returns replacement metadata or a filtered list for development and diagnostics. This should not necessarily expose every internal note in production.

## Error Handling

- `400 Bad Request` for invalid payloads
- `413 Payload Too Large` for oversized input if limits are enforced
- `429 Too Many Requests` for AI-backed mode throttling
- `502/503` for upstream AI failures

Strict mode should prefer partial success with warnings over full failure.

## Data Model

Proposed C# contracts:

```csharp
public sealed record RewriteRequest(
    string Text,
    RewriteMode Mode
);

public enum RewriteMode
{
    NoBigWords,
    ExplainLikeImTen
}

public sealed record RewriteResponse(
    string OriginalText,
    string RewrittenText,
    IReadOnlyList<ReplacementResult> Replacements,
    IReadOnlyList<UnknownWord> UnknownWords,
    ValidationSummary Validation
);

public sealed record ReplacementResult(
    string Original,
    string Replacement,
    int StartIndex,
    int Length,
    ReplacementSource Source,
    double Confidence
);

public enum ReplacementSource
{
    AllowedAsIs,
    LocalDictionary,
    InflectionRule,
    OpenAiFallback
}

public sealed record UnknownWord(
    string Word,
    int StartIndex,
    int Length
);

public sealed record ValidationSummary(
    int TotalWords,
    int AllowedWords,
    int DisallowedWords,
    double AllowedPercentage
);
```

Suggested supporting types:

```csharp
public sealed record ReplacementEntry(
    string Replacement,
    double Confidence,
    string? Notes
);

public sealed record Token(
    string OriginalText,
    string NormalizedText,
    int StartIndex,
    int Length,
    bool IsWord,
    bool IsCapitalized,
    bool IsAllCaps
);
```

## Local Data Files

### Allowed Vocabulary

Path:

```text
/data/allowed-words.txt
```

Format:

- One word per line
- Lowercase
- Comments optional with `#`

### Replacement Dictionary

Path:

```text
/data/replacements.json
```

Format:

```json
{
  "planet": {
    "replacement": "world",
    "confidence": 0.95,
    "notes": "Good general replacement"
  },
  "astronaut": {
    "replacement": "space person",
    "confidence": 0.9,
    "notes": "Fun strict-mode replacement"
  }
}
```

### Optional Frequency List

Path:

```text
/data/common-words-10000.txt
```

Possible use cases:

- prioritizing likely replacements
- future scoring and suggestions
- tuning explainability

## Replacement Algorithm

## Goals of the Algorithm

- Be deterministic where possible
- Preserve punctuation and casing reasonably well
- Prefer local replacements first
- Return metadata rich enough for UI and tests

## Pipeline

1. Tokenize input into words, spaces, and punctuation-preserving tokens.
2. For each word token, normalize for lookup:
   - lowercase
   - trim punctuation artifacts
   - attempt singularization for simple plurals
   - attempt basic verb/root forms for simple endings
3. If the word is already allowed, keep it.
4. If not allowed, look for a direct dictionary replacement.
5. If not found, attempt inflection-aware lookup.
6. Validate candidate replacement phrase word-by-word.
7. If the replacement is valid, apply it and preserve casing as well as practical.
8. If still unresolved in strict mode, mark as unknown and optionally enqueue targeted AI fallback.
9. Rebuild the final output string.
10. Validate the full output and compute summary metrics.

## Tokenization Rules

- Keep punctuation tokens separate from word tokens
- Preserve whitespace locations when rebuilding output
- Treat apostrophes conservatively in v1
- Support Unicode letters only if needed later; ASCII-first behavior is acceptable for the first release

## Normalization Rules

Initial rules can stay intentionally simple:

- `planets` -> `planet`
- `studies` -> `study`
- `looked` -> `look`
- `looking` -> `look`

This should be rule-based, transparent, and easy to test. Avoid premature NLP complexity in v1.

## Capitalization Preservation

- `Planet` -> `World`
- `PLANET` -> `WORLD`
- `planet` -> `world`

For multi-word replacements, capitalize only the first replacement word when the source token is title case.

## Unknown Word Handling

If a word cannot be replaced locally:

- add it to `UnknownWords`
- leave the original token in place unless AI fallback succeeds
- include UI warnings

## OpenAI Strategy

## Explain Like I'm 10 Mode

This mode uses OpenAI for the full rewrite.

Prompt requirements:

- explain in simple language for a 10-year-old
- prefer short sentences
- keep the meaning
- define jargon when needed
- avoid unnecessary length

The response should still be wrapped in the same `RewriteResponse` shape, though replacement granularity may be coarser than in strict mode.

## Strict-Mode Fallback

Use OpenAI only when local logic cannot produce an acceptable replacement.

Rules:

- Send only the smallest useful span, ideally a word, phrase, or sentence fragment
- Ask for replacements using only allowed words where possible
- Validate model output afterward
- Retry once if the result still violates the allowed list
- If the retry still fails, return the best result with warnings

Recommended fallback strategy:

1. Complete local pass
2. Collect unresolved tokens by sentence
3. Ask the AI for a strict small-word rewrite of only those sentences
4. Validate AI output
5. Accept only validated or improved results

## AI Safety and Operational Concerns

- Time out upstream AI calls aggressively
- Log prompt and response metadata without storing sensitive text long-term in v1
- Keep AI calls behind an interface for testability
- Make AI usage visible in the UI

## Interfaces

```csharp
public interface IRewriteService
{
    Task<RewriteResponse> RewriteAsync(RewriteRequest request, CancellationToken cancellationToken = default);
}

public interface IAllowedWordsProvider
{
    bool IsAllowed(string word);
}

public interface IReplacementDictionary
{
    bool TryGetReplacement(string word, out ReplacementEntry replacement);
}

public interface IAiRewriteService
{
    Task<string> RewriteAsync(string text, RewriteMode mode, CancellationToken cancellationToken = default);
}
```

## Testing Strategy

Use xUnit and constructor injection throughout so fake providers and mock AI services can be swapped in cleanly.

## Unit Tests

- Tokenization splits words and punctuation correctly
- Allowed-word validation works for exact and normalized matches
- Local dictionary replacement returns expected replacements
- Multi-word replacement preserves spacing and punctuation
- Capitalization preservation works for lowercase, title case, and all-caps
- Punctuation preservation survives rewrite and rebuild
- Unknown words are reported with correct location metadata
- Validation summary counts words accurately
- OpenAI fallback is called only when needed in strict mode
- Explain mode always routes through the AI abstraction

## Integration Tests

- `POST /api/rewrite` returns expected JSON contract
- Strict mode completes fully local requests without AI
- Strict mode returns warnings when unresolved terms remain
- Explain mode handles mocked AI output end to end
- Frontend-to-API contract tests verify mode names and payload shapes

## Manual QA Checklist

- Typing feels responsive
- Copy button copies exact output
- Clear button resets all UI state
- Mode switch updates labels and behavior immediately
- Disallowed words are visibly highlighted
- Unknown words panel matches backend metadata
- Mobile layout is readable and usable

## Observability

For v1, lightweight structured logs are enough:

- rewrite mode
- text length
- local-only vs AI-assisted path
- unknown word count
- AI failure count

Avoid collecting user content beyond what is operationally necessary.

## First Milestone Implementation Plan

## Milestone Goal

Ship a usable v1 that proves the product loop:

- type text
- rewrite locally in strict mode
- show summary and unknown words
- switch to AI explain mode

## Milestone Scope

### Backend

- Create ASP.NET Core Web API solution
- Implement file-backed allowed-word provider
- Implement file-backed replacement dictionary
- Implement tokenizer, normalizer, validator, and rewrite service
- Add `POST /api/rewrite` and `POST /api/validate`
- Stub `GET` diagnostics endpoints if time allows
- Add AI abstraction with a mock or placeholder implementation behind configuration

### Frontend

- Create Vue + TypeScript SPA
- Build single-page layout with mode selector and two editors
- Add debounced API calls
- Render validation summary, unknown words, and replacement details
- Add copy and clear actions

### Testing

- Unit tests for core rewrite logic
- One or two integration tests for API contracts

## Milestone Acceptance Criteria

- Strict mode rewrites known sample inputs locally
- Unresolved words appear in metadata and UI
- Explain mode returns AI-backed output when configured
- App works on desktop and mobile widths
- Core services are covered by unit tests

## Future Roadmap

- User-editable replacement dictionary
- Import/export replacement dictionary
- Shareable rewritten links
- Dark mode
- A stricter "Thing Explainer" mode
- Educational notes explaining each replacement
- Browser extension
- VS Code extension
- NuGet package for the rewrite engine
- API-only package
- Ranked replacement suggestions by confidence
- Community-submitted replacements with moderation

## Open Questions

- What exact allowed-word list should define strict mode: a curated custom list, a Thing Explainer-style list, or a frequency-based starter list?
- Should strict mode leave unresolved words unchanged, redact them visually, or try harder with AI by default?
- Should `/api/vocabulary/allowed` expose the full list publicly or only metadata?
- What text length limits should v1 enforce for AI-backed mode?

## Recommended Defaults

- Start with a curated file-based allowed vocabulary and replacement dictionary committed to the repo
- Keep v1 stateless with no database
- Use local-first strict rewriting with optional sentence-level AI fallback
- Use a simple read-only output pane rather than a second editable textarea
- Place the spec in `/docs/spec.md` and treat it as the implementation source of truth for v1
