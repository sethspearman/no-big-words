# Book Photos vs XKCD Simple Writer

This note compares a derived OCR extraction of the photographed book list against the xkcd Simple Writer normalized list.


## Caveat

- `book-base-words-derived.txt` is OCR-derived from the photographed list pages and should be treated as high-confidence but not perfect.
- The explanation page was excluded from the extracted word list.
- Some valid book words were likely missed by OCR, so `xkcd-only` counts are an upper bound rather than a perfect final diff.

## Counts

- Derived book base words: `916`
- XKCD normalized words: `3616`
- XKCD words not seen in the derived book set: `2700`

## Main Difference

- The photographed book pages appear to be a base-word list.
- The xkcd site expands that base list into many explicit forms such as plurals, tenses, `-er` forms, `-est` forms, and contractions.
- The book notes also say two common four-letter words were intentionally left off the printed page; those appear on the xkcd site.

## XKCD-Only Categories

These are heuristic buckets for quick comparison, not a linguistically exact stemming analysis.

- `profanity_or_omitted`: `6`
  sample: fucking, fucked, shits, shit, shat, fuck
- `apostrophe_or_contraction`: `18`
  sample: wouldn't, couldn't, doesn't, okay'd, ok'ing, didn't, aren't, wasn't, don't, who's, can't, isn't, won't, he's, ok'd, i'll, i'm, i'd
- `ing_forms`: `538`
  sample: understanding, disappearing, approaching, positioning, questioning, recognizing, remembering, discovering, interesting, controlling, considering, companying, continuing, stationing, stretching, soldiering, countering, suggesting, distancing, explaining
- `ed_forms`: `456`
  sample: disappeared, remembered, considered, controlled, recognized, questioned, positioned, approached, interested, discovered, companied, continued, travelled, countered, soldiered, distanced, explained, surprised, suggested, pretended
- `er_or_est_forms`: `332`
  sample: straightest, controller, straighter, rememberer, recognizer, questioner, traveller, explainer, brightest, stupidest, strongest, prettiest, forgetter, strangest, narrowest, soundest, sorriest, blackest, thinnest, craziest
- `ly_forms`: `3`
  sample: mostly, simply, smelly
- `plural_s_forms`: `1103`
  sample: understandings, conversations, informations, grandmothers, grandfathers, questionings, understands, travellings, questioners, recognizers, televisions, rememberers, expressions, controllers, cigarettes, spreadings, businesses, somebodies, darknesses, situations
- `other`: `244`
  sample: understood, attention, forgotten, colorful, yourself, children, neighbor, mountain, language, thought, visitor, special, without, trouble, brought, written, manage, hidden, wooden, within

## Explicitly Observed Omitted Printed Words

- `fuck`
- `shit`

## Files

- `data/book-base-words-derived.txt`
- `docs/book-vs-xkcd.md`
