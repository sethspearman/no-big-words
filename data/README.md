# Data Notes

## `allowed-words.txt`

This file was generated from the xkcd Simple Writer site on July 8, 2026.

Source:

- `https://xkcd.com/simplewriter/`
- `https://xkcd.com/simplewriter/words.js?cache=6f3c4ed1c04dc0b6c30d46e2ea451210`

The site does not expose the word list as JSON. Instead, it serves a JavaScript file that assigns a pipe-delimited string to `window.__WORDS`.

For local use in this repo, that string was transformed into a newline-separated text file with one entry per line.

Important note:

- The resulting file contains `3,634` entries.
- This appears to include many inflected forms and variants, not just a bare 1,000-word root vocabulary.
- If we want a stricter source-of-truth later, we may want to derive a curated base-word list separately and keep this file as the "xkcd-compatible" vocabulary.

## `allowed-words-normalized.txt`

This file is a normalized, deduped derivative of `allowed-words.txt`.

Normalization applied:

- lowercase
- convert curly apostrophes (`’`) to straight apostrophes (`'`)
- remove duplicate normalized entries while preserving first-seen order

Result:

- `3,616` entries
- `18` normalization collisions collapsed

Examples of collapsed variants:

- `don't` and `don’t` -> `don't`
- `can't` and `can’t` -> `can't`
- `i'll` and `i’ll` -> `i'll`

This is probably a better starting point for the app's internal lookup set than the raw extracted file.

## Replacement Files

### `replacements.json`

This is the small curated seed dictionary.

Use it for:

- hand-written replacements we trust
- preserving intentionally funny or high-quality replacements
- feeding the larger generator pipeline

### `replacements.generated.json`

This is the large generated dictionary used by the app scaffold.

It is built from:

- `replacements.json` seed entries
- the 15,000-word non-allowed list
- contraction expansion
- manual starter mappings for high-signal words
- morphology-based fallbacks
- generic category fallbacks

### `replacements.review-needed.psv`

This is a ranked review backlog for weaker generated entries.

### `replacements.generation-report.json`

This is a small machine-readable summary of the latest generation pass.

### `top-15000-common-words-excluding-allowed.*`

These files are the source candidate list used for building the large generated replacement dictionary.

### `scripts/generate_replacements.py`

This script regenerates the full replacement dictionary and review files.
