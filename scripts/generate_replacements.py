#!/usr/bin/env python3
from __future__ import annotations

import csv
import json
import re
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable


ROOT = Path(__file__).resolve().parent.parent
DATA = ROOT / "data"
ALLOWED_PATH = DATA / "allowed-words-normalized.txt"
SOURCE_CSV_PATH = DATA / "top-15000-common-words-excluding-allowed.csv"
SEED_REPLACEMENTS_PATH = DATA / "replacements.json"
GENERATED_REPLACEMENTS_PATH = DATA / "replacements.generated.json"
REVIEW_PATH = DATA / "replacements.review-needed.psv"
REPORT_PATH = DATA / "replacements.generation-report.json"

WORD_RE = re.compile(r"[a-z]+(?:'[a-z]+)?")


@dataclass(frozen=True)
class Entry:
    replacement: str
    confidence: float
    notes: str

    def to_json(self) -> dict[str, object]:
        return {
            "replacement": self.replacement,
            "confidence": round(self.confidence, 2),
            "notes": self.notes,
        }


CONTRACTIONS = {
    "it's": "it is",
    "you're": "you are",
    "that's": "that is",
    "i've": "i have",
    "there's": "there is",
    "they're": "they are",
    "we're": "we are",
    "she's": "she is",
    "you'll": "you will",
    "what's": "what is",
    "let's": "let us",
    "you've": "you have",
    "haven't": "have not",
    "he's": "he is",
    "won't": "will not",
    "don't": "do not",
    "can't": "can not",
    "didn't": "did not",
    "doesn't": "does not",
    "isn't": "is not",
    "aren't": "are not",
    "wasn't": "was not",
    "wouldn't": "would not",
    "couldn't": "could not",
    "i'll": "i will",
    "i'm": "i am",
    "i'd": "i would",
    "who's": "who is",
    "they've": "they have",
    "we've": "we have",
    "you'd": "you would",
    "we'd": "we would",
}

MONTHS = {
    "january", "february", "march", "april", "may", "june",
    "july", "august", "september", "october", "november", "december",
}

PEOPLE_NAMES = {
    "john", "james", "david", "michael", "lord", "mom", "mr", "dr", "st",
}

PLACE_NAMES = {
    "york", "london", "washington", "england", "uk", "china",
}

MANUAL = {
    "government": "group in power",
    "public": "all people",
    "support": "help",
    "including": "with",
    "based": "built on",
    "national": "whole country",
    "american": "from the us",
    "season": "part of the year",
    "general": "not just one kind",
    "united": "joined",
    "per": "for each",
    "president": "top leader",
    "health": "body state",
    "post": "put up",
    "video": "moving picture",
    "social": "about people",
    "court": "law place",
    "level": "step",
    "research": "study",
    "university": "big school",
    "international": "between lands",
    "due": "must be paid",
    "series": "set",
    "community": "local group",
    "data": "facts",
    "de": "of",
    "million": "a lot",
    "deal": "buy and sell act",
    "report": "tell about",
    "development": "making bigger",
    "major": "big",
    "media": "news group",
    "gonna": "going to",
    "market": "buy and sell place",
    "non": "not",
    "political": "about power",
    "according": "by",
    "available": "there to use",
    "education": "learning",
    "former": "before",
    "list": "set of names",
    "current": "now",
    "example": "one like it",
    "experience": "thing that happened to you",
    "program": "plan",
    "type": "kind",
    "process": "way",
    "action": "act",
    "self": "yourself",
    "cost": "what you pay",
    "department": "part",
    "energy": "power",
    "issue": "question",
    "price": "what you pay",
    "results": "things you get",
    "term": "word",
    "america": "the us",
    "date": "day",
    "project": "plan",
    "site": "place",
    "account": "story",
    "co": "group",
    "include": "have with",
    "period": "time",
    "record": "write down",
    "similar": "almost the same",
    "total": "all together",
    "club": "group",
    "common": "seen a lot",
    "film": "movie",
    "likely": "may well",
    "military": "war group",
    "security": "safe state",
    "english": "that land talk",
    "industry": "work field",
    "online": "on the computer",
    "private": "not open",
    "present": "now",
    "result": "thing you get",
    "design": "plan",
    "king": "top man",
    "policy": "way of doing things",
    "society": "people around us",
    "average": "middle",
    "medical": "body care",
    "original": "first",
    "park": "green place",
    "performance": "how well it works",
    "role": "part",
    "worth": "how much it matters",
    "bill": "paper asking for money",
    "director": "top person",
    "provide": "give",
    "relationship": "tie",
    "source": "where it comes from",
    "value": "how much it matters",
    "evidence": "sign",
    "official": "from those in power",
    "production": "making",
    "rate": "how fast",
    "tax": "state take",
    "amount": "how much",
    "federal": "whole-country",
    "league": "group",
    "management": "running",
    "match": "game",
    "model": "one to look at",
    "range": "spread",
    "review": "look back at",
    "science": "study of how things work",
    "trade": "buy and sell",
    "various": "many kinds of",
    "character": "story person",
    "chief": "top",
    "football": "ball game",
    "natural": "from the world",
    "property": "thing you own",
    "quality": "how good it is",
    "style": "way",
    "vote": "pick",
    "amazing": "very good",
    "complete": "whole",
    "economic": "money",
    "involved": "in it",
    "related": "tied to",
    "terms": "words",
    "title": "name",
    "article": "writing piece",
    "damn": "bad",
    "decision": "choice",
    "french": "from that land",
    "release": "let out",
    "technology": "machine way",
    "website": "computer place",
    "code": "special words",
    "council": "group",
    "currently": "now",
    "election": "people pick",
    "european": "from that land",
    "financial": "money",
    "foreign": "from another land",
    "increase": "get bigger",
    "legal": "by the law",
    "race": "run game",
    "staff": "work people",
    "super": "very good",
    "union": "joined group",
    "career": "work life",
    "daily": "every day",
    "difficult": "hard",
    "loss": "what was lost",
    "modern": "new",
    "popular": "liked by many",
    "published": "put out",
    "county": "part of a state",
    "british": "from that land",
    "central": "middle",
    "al": "person name",
    "mom": "mother",
    "reviewed": "looked back at",
    "published": "put out",
    "officials": "people in power",
    "performance": "how well it works",
    "policy": "way of doing things",
    "management": "running",
    "federal": "whole-country",
    "medical": "body care",
    "economic": "money",
    "technology": "machine way",
    "currently": "now",
    "foreign": "from another land",
    "career": "work life",
    "popular": "liked by many",
    "decision": "choice",
    "relationship": "tie",
    "source": "where it comes from",
    "evidence": "sign",
    "character": "story person",
    "property": "thing you own",
    "quality": "how good it is",
    "article": "writing piece",
    "website": "computer place",
    "security": "safe state",
    "industry": "work field",
    "service": "help work",
    "special": "not like the rest",
    "position": "place",
    "possible": "can be",
    "probably": "may well",
    "problem": "hard thing",
    "promise": "say you will",
    "question": "ask thing",
    "re": "about",
    "test": "try",
    "version": "kind",
    "army": "war group",
    "australia": "place name",
    "goal": "end point",
    "internet": "computer world",
    "practice": "do again and again",
    "rules": "ways",
    "success": "doing well",
    "access": "way in",
    "base": "bottom part",
    "quickly": "fast",
    "quietly": "with no sound",
    "radio": "sound box",
    "rather": "more like",
    "realize": "see that",
    "reason": "why",
    "receive": "get",
    "recognize": "know again",
    "refuse": "say no",
    "remain": "stay",
    "remember": "keep in mind",
    "remove": "take away",
    "repeat": "say again",
    "reply": "answer",
    "return": "come back",
    "reveal": "show",
    "serious": "not funny",
    "seriously": "in a real way",
    "service": "help work",
    "settle": "come to rest",
    "several": "more than one",
    "similarity": "how much they are the same",
    "simple": "easy",
    "simply": "in an easy way",
    "situation": "how things are",
    "slightly": "a little",
    "soldier": "war person",
    "somebody": "some person",
    "somehow": "in some way",
    "someone": "some person",
    "something": "some thing",
    "sometimes": "now and then",
    "somewhere": "some place",
    "sound": "noise",
    "south": "down side",
    "space": "room",
    "speak": "talk",
    "spend": "use up",
    "spirit": "inside part",
    "spread": "move out",
    "stage": "show place",
    "station": "stop place",
    "stomach": "food bag",
    "story": "what happened",
    "straight": "not bent",
    "strange": "not known",
    "street": "road",
    "stretch": "pull out",
    "strike": "hit",
    "strong": "full of power",
    "student": "school person",
    "study": "learn",
    "stuff": "things",
    "stupid": "not bright",
    "suddenly": "all at once",
    "suggest": "put out as an idea",
    "surface": "top side",
    "surprise": "did not see it coming",
    "system": "set way",
    "teacher": "school person",
    "television": "picture box",
    "terrible": "very bad",
    "through": "from side to side",
    "together": "with each other",
    "tomorrow": "next day",
    "toward": "in the way of",
    "travel": "go far",
    "trouble": "hard thing",
    "trust": "believe",
    "truth": "what is true",
    "understand": "get",
    "unless": "if not",
    "usual": "most times",
    "usually": "most times",
    "village": "small town",
    "visit": "go see",
    "voice": "person sound",
    "wash": "clean with water",
    "watch": "look at",
    "water": "water",
    "weather": "sky state",
    "weight": "how heavy it is",
    "whether": "if",
    "whisper": "soft talk",
    "window": "see-through hole",
    "winter": "cold time of year",
    "without": "with no",
    "woman": "woman",
    "wonder": "think hard about",
    "wooden": "made of wood",
    "work": "work",
    "world": "world",
    "worry": "fear in your mind",
    "write": "put down words",
    "wrong": "not right",
}


def load_allowed() -> set[str]:
    return {line.strip() for line in ALLOWED_PATH.read_text(encoding="utf-8").splitlines() if line.strip()}


def load_candidates() -> list[dict[str, str]]:
    with SOURCE_CSV_PATH.open(encoding="utf-8", newline="") as handle:
        return list(csv.DictReader(handle))


def load_seed_entries() -> dict[str, Entry]:
    data = json.loads(SEED_REPLACEMENTS_PATH.read_text(encoding="utf-8"))
    return {
        key: Entry(
            replacement=value["replacement"],
            confidence=float(value["confidence"]),
            notes=value["notes"],
        )
        for key, value in data.items()
    }


def phrase_tokens(phrase: str) -> list[str]:
    return WORD_RE.findall(phrase.lower())


def validate_phrase(phrase: str, allowed: set[str]) -> bool:
    tokens = phrase_tokens(phrase)
    return bool(tokens) and all(token in allowed for token in tokens)


def require_valid(phrase: str, allowed: set[str]) -> str:
    if not validate_phrase(phrase, allowed):
        raise ValueError(f"Replacement phrase is not fully allowed: {phrase!r}")
    return phrase


def normalize_manual(allowed: set[str], seed_entries: dict[str, Entry]) -> dict[str, Entry]:
    merged = dict(seed_entries)

    for word, replacement in CONTRACTIONS.items():
        merged[word] = Entry(require_valid(replacement, allowed), 0.99, "Auto-generated contraction expansion")

    for word, replacement in MANUAL.items():
        clean = replacement.replace("-", " ")
        merged[word] = Entry(require_valid(clean, allowed), 0.9, "Auto-generated manual starter replacement")

    for month in MONTHS:
        merged[month] = Entry(require_valid("month name", allowed), 0.45, "Auto-generated category fallback for month names")

    for name in PEOPLE_NAMES:
        if name == "dr":
            merged[name] = Entry(require_valid("doctor", allowed), 0.8, "Auto-generated abbreviation replacement")
        elif name == "mr":
            merged[name] = Entry(require_valid("man", allowed), 0.6, "Auto-generated title replacement")
        elif name == "st":
            merged[name] = Entry(require_valid("road", allowed), 0.35, "Auto-generated abbreviation fallback; meaning may vary")
        else:
            merged[name] = Entry(require_valid("person name", allowed), 0.45, "Auto-generated category fallback for names")

    for place in PLACE_NAMES:
        merged[place] = Entry(require_valid("place name", allowed), 0.45, "Auto-generated category fallback for place names")

    return merged


def derive_stems(word: str) -> Iterable[tuple[str, str]]:
    seen: set[tuple[str, str]] = set()

    def add(stem: str, kind: str) -> Iterable[tuple[str, str]]:
        item = (stem, kind)
        if stem and stem != word and item not in seen:
            seen.add(item)
            yield item

    if word.endswith("ies") and len(word) > 4:
        yield from add(word[:-3] + "y", "plural")
    if word.endswith("ing") and len(word) > 5:
        yield from add(word[:-3], "ing")
        yield from add(word[:-3] + "e", "ing")
    if word.endswith("ed") and len(word) > 4:
        yield from add(word[:-2], "past")
        yield from add(word[:-1], "past")
    if word.endswith("es") and len(word) > 4:
        yield from add(word[:-2], "plural")
    if word.endswith("s") and len(word) > 3:
        yield from add(word[:-1], "plural")
    if word.endswith("er") and len(word) > 4:
        yield from add(word[:-2], "agent")
    if word.endswith("est") and len(word) > 5:
        yield from add(word[:-3], "superlative")
    if word.endswith("ly") and len(word) > 4:
        yield from add(word[:-2], "adverb")
    if word.endswith("ness") and len(word) > 6:
        yield from add(word[:-4], "state")
    if word.endswith("ment") and len(word) > 6:
        yield from add(word[:-4], "event")
    if word.endswith("tion") and len(word) > 6:
        yield from add(word[:-4] + "e", "event")
        yield from add(word[:-4], "event")
    if word.endswith("sion") and len(word) > 6:
        yield from add(word[:-4], "event")
    if word.endswith("ity") and len(word) > 5:
        yield from add(word[:-3], "state")
    if word.endswith("al") and len(word) > 4:
        yield from add(word[:-2], "about")
    if word.endswith("ic") and len(word) > 4:
        yield from add(word[:-2], "about")
    if word.endswith("ive") and len(word) > 5:
        yield from add(word[:-3], "about")


def pluralize_phrase(base: str, allowed: set[str]) -> str:
    tokens = phrase_tokens(base)
    if len(tokens) == 1:
        candidate = tokens[0] + "s"
        if candidate in allowed:
            return candidate
    phrase = f"many {base}"
    if validate_phrase(phrase, allowed):
        return phrase
    return base


def transform_from_base(kind: str, base_entry: Entry, allowed: set[str]) -> Entry | None:
    base = base_entry.replacement

    transforms = {
        "plural": (pluralize_phrase(base, allowed), base_entry.confidence - 0.1, "Auto-generated plural fallback from simpler base form"),
        "past": (base, base_entry.confidence - 0.15, "Auto-generated past-tense fallback from simpler base form"),
        "ing": (base, base_entry.confidence - 0.15, "Auto-generated -ing fallback from simpler base form"),
        "agent": (f"person who {base}", base_entry.confidence - 0.12, "Auto-generated person-form fallback from simpler base form"),
        "superlative": (f"most {base}", base_entry.confidence - 0.16, "Auto-generated superlative fallback from simpler base form"),
        "adverb": (f"in a {base} way", base_entry.confidence - 0.16, "Auto-generated adverb fallback from simpler base form"),
        "state": (f"{base} state", base_entry.confidence - 0.18, "Auto-generated state-form fallback from simpler base form"),
        "event": (f"{base} thing", base_entry.confidence - 0.18, "Auto-generated event-form fallback from simpler base form"),
        "about": (f"about {base}", base_entry.confidence - 0.18, "Auto-generated adjective fallback from simpler base form"),
    }

    replacement, confidence, notes = transforms[kind]
    if validate_phrase(replacement, allowed):
        return Entry(replacement, max(0.25, confidence), notes)
    return None


def category_fallback(word: str, allowed: set[str]) -> Entry:
    if word in MONTHS:
        return Entry("month name", 0.45, "Auto-generated month-name fallback")

    if word in PEOPLE_NAMES or word.endswith("son"):
        return Entry("person name", 0.4, "Auto-generated name fallback")

    if word in PLACE_NAMES:
        return Entry("place name", 0.4, "Auto-generated place-name fallback")

    if word.endswith(("tion", "sion", "ment", "ness", "ity")):
        return Entry("thing", 0.3, "Auto-generated abstract-noun fallback")

    if word.endswith(("ing", "ed")):
        return Entry("do", 0.26, "Auto-generated verb-form fallback")

    if word.endswith(("er", "or", "ist")):
        return Entry("person", 0.28, "Auto-generated person-role fallback")

    if word.endswith(("ly",)):
        return Entry("in this way", 0.25, "Auto-generated adverb fallback")

    if word.endswith(("al", "ic", "ive", "ous", "ful", "less", "able", "ible")):
        return Entry("like this", 0.25, "Auto-generated adjective fallback")

    return Entry("thing", 0.22, "Auto-generated generic fallback")


def generate_entries() -> tuple[dict[str, Entry], list[dict[str, str]], dict[str, int]]:
    allowed = load_allowed()
    seed_entries = normalize_manual(allowed, load_seed_entries())
    candidates = load_candidates()
    generated: dict[str, Entry] = {}

    def resolve(word: str, depth: int = 0, trail: tuple[str, ...] = ()) -> Entry:
        if word in generated:
            return generated[word]

        if word in seed_entries:
            generated[word] = seed_entries[word]
            return generated[word]

        if depth < 3:
            for stem, kind in derive_stems(word):
                if stem in trail:
                    continue
                if stem in seed_entries or stem in generated:
                    base_entry = resolve(stem, depth + 1, trail + (word,))
                    derived = transform_from_base(kind, base_entry, allowed)
                    if derived:
                        generated[word] = derived
                        return derived

        fallback = category_fallback(word, allowed)
        generated[word] = fallback
        return fallback

    source_counts = {
        "seed_or_manual": 0,
        "morphology": 0,
        "fallback": 0,
    }

    review_rows: list[dict[str, str]] = []

    for row in candidates:
        word = row["word"].strip().lower()
        entry = resolve(word)
        if word in seed_entries:
            source_counts["seed_or_manual"] += 1
        elif "simpler base form" in entry.notes:
            source_counts["morphology"] += 1
        else:
            source_counts["fallback"] += 1

        if entry.confidence < 0.6:
            review_rows.append({
                "candidate_rank": row["candidate_rank"],
                "source_rank": row["source_rank"],
                "word": word,
                "replacement": entry.replacement,
                "confidence": f"{entry.confidence:.2f}",
                "notes": entry.notes,
            })

    return generated, review_rows, source_counts


def main() -> None:
    entries, review_rows, source_counts = generate_entries()

    ordered_words = [row["word"].strip().lower() for row in load_candidates()]
    ordered_json = {word: entries[word].to_json() for word in ordered_words}
    GENERATED_REPLACEMENTS_PATH.write_text(json.dumps(ordered_json, indent=2, ensure_ascii=True) + "\n", encoding="utf-8")

    with REVIEW_PATH.open("w", encoding="utf-8", newline="") as handle:
        writer = csv.DictWriter(
            handle,
            fieldnames=["candidate_rank", "source_rank", "word", "replacement", "confidence", "notes"],
            delimiter="|",
        )
        writer.writeheader()
        writer.writerows(review_rows)

    report = {
        "generated_entries": len(ordered_words),
        "review_needed_entries": len(review_rows),
        "source_counts": source_counts,
    }
    REPORT_PATH.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")

    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
