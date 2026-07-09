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
