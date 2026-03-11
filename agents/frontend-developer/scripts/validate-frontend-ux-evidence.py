#!/usr/bin/env python3
"""
Frontend UX Evidence Validator

Fails CI when frontend UI files change without an accompanying UX evidence artifact.

Evidence artifacts must live in:
  planning-mds/operations/evidence/frontend-ux/

Usage:
  python3 agents/frontend-developer/scripts/validate-frontend-ux-evidence.py
  python3 agents/frontend-developer/scripts/validate-frontend-ux-evidence.py --base <sha> --head <sha>
"""

from __future__ import annotations

import argparse
import re
import subprocess
import sys
from pathlib import Path
from typing import Iterable, List, Sequence, Tuple


EVIDENCE_DIR = Path("planning-mds/operations/evidence/frontend-ux")
EVIDENCE_EXCLUDE = {"README.md", "TEMPLATE.md"}

UI_SOURCE_ROOT = "experience/src/"
UI_SOURCE_EXTENSIONS = {".tsx", ".jsx", ".css", ".scss", ".sass", ".less"}

REQUIRED_HEADINGS = (
    "## Commands Executed",
    "## Light/Dark Screenshots",
    "## UX Checklist",
    "## Deferred Items",
)

REQUIRED_COMMAND_CHECKBOXES = (
    "pnpm --dir experience lint",
    "pnpm --dir experience lint:theme",
    "pnpm --dir experience build",
    "pnpm --dir experience test",
    "pnpm --dir experience test:visual:theme",
)

REQUIRED_CHECKLIST_CHECKBOXES = (
    "No clickable non-interactive wrappers.",
    "Keyboard and focus behavior validated for touched flows.",
    "Modal/popover/tabs accessibility behavior validated.",
    "Light and dark theme readability/contrast verified.",
    "Responsive behavior validated for affected UI flows.",
)


def run_command(command: Sequence[str]) -> Tuple[int, str, str]:
    proc = subprocess.run(command, capture_output=True, text=True, check=False)
    return proc.returncode, proc.stdout, proc.stderr


def is_valid_ref(ref: str) -> bool:
    code, _, _ = run_command(["git", "rev-parse", "--verify", ref])
    return code == 0


def changed_files_for_range(base: str, head: str) -> List[str]:
    code, out, err = run_command(
        ["git", "diff", "--name-only", "--diff-filter=ACMR", f"{base}..{head}"]
    )
    if code != 0:
        raise RuntimeError(err.strip() or "git diff failed")
    return [line.strip() for line in out.splitlines() if line.strip()]


def resolve_range(base: str, head: str) -> Tuple[str, str]:
    if base and head:
        if is_valid_ref(base) and is_valid_ref(head):
            return base, head
        raise ValueError(f"Invalid git refs provided: base='{base}' head='{head}'")

    default_head = head or "HEAD"
    default_base = base or "HEAD~1"

    if is_valid_ref(default_base) and is_valid_ref(default_head):
        return default_base, default_head

    raise ValueError(
        "Could not determine a valid git diff range. "
        "Provide --base and --head explicitly."
    )


def detect_ui_changes(changed_files: Iterable[str]) -> List[str]:
    result: List[str] = []
    for raw in changed_files:
        normalized = raw.replace("\\", "/")
        if normalized == "experience/index.html":
            result.append(raw)
            continue

        if not normalized.startswith(UI_SOURCE_ROOT):
            continue

        suffix = Path(normalized).suffix.lower()
        if suffix in UI_SOURCE_EXTENSIONS:
            result.append(raw)

    return result


def detect_changed_evidence_files(changed_files: Iterable[str]) -> List[Path]:
    result: List[Path] = []
    for raw in changed_files:
        path = Path(raw)
        if path.suffix != ".md":
            continue
        if path.name in EVIDENCE_EXCLUDE:
            continue
        if str(path).replace("\\", "/").startswith(str(EVIDENCE_DIR).replace("\\", "/") + "/"):
            result.append(path)
    return result


def has_checked_backtick_item(content: str, value: str) -> bool:
    pattern = rf"(?mi)^\s*-\s*\[[xX]\]\s*`{re.escape(value)}`\s*$"
    return re.search(pattern, content) is not None


def has_checked_plain_item(content: str, value: str) -> bool:
    pattern = rf"(?mi)^\s*-\s*\[[xX]\]\s*{re.escape(value)}\s*$"
    return re.search(pattern, content) is not None


def has_checked_unavailable_item(content: str, command: str) -> bool:
    pattern = (
        rf"(?mi)^\s*-\s*\[[xX]\]\s*Command unavailable:\s*`{re.escape(command)}`"
        rf"(?:\s*[-:]\s*.*)?$"
    )
    return re.search(pattern, content) is not None


def has_checked_equivalent_item(content: str, command: str) -> bool:
    pattern = (
        rf"(?mi)^\s*-\s*\[[xX]\]\s*Equivalent command used for\s*"
        rf"`{re.escape(command)}`\s*:\s*`[^`]+`\s*$"
    )
    return re.search(pattern, content) is not None


def validate_evidence_file(path: Path) -> List[str]:
    if not path.exists():
        return [f"Evidence file not found in repository: {path}"]

    content = path.read_text(encoding="utf-8")
    errors: List[str] = []

    if not re.search(r"(?mi)^#\s+Frontend UX Audit Evidence\s*$", content):
        errors.append("Missing H1 title '# Frontend UX Audit Evidence'")

    for heading in REQUIRED_HEADINGS:
        if heading not in content:
            errors.append(f"Missing required heading: {heading}")

    if not re.search(r"(?mi)^\s*-\s*Date \(UTC\):\s*.+$", content):
        errors.append("Missing metadata line '- Date (UTC): ...'")

    if not re.search(r"(?mi)^\s*-\s*Scope:\s*.+$", content):
        errors.append("Missing metadata line '- Scope: ...'")

    for command in REQUIRED_COMMAND_CHECKBOXES:
        has_direct = has_checked_backtick_item(content, command)
        has_unavailable = has_checked_unavailable_item(content, command)
        has_equivalent = has_checked_equivalent_item(content, command)

        if has_direct or has_unavailable:
            continue

        if has_equivalent:
            errors.append(
                f"Equivalent command recorded for `{command}` but missing "
                f"`Command unavailable: `{command}`` marker"
            )
            continue

        errors.append(
            f"Missing command evidence for `{command}` "
            f"(checked command, or checked unavailable marker)"
        )

    for item in REQUIRED_CHECKLIST_CHECKBOXES:
        if not has_checked_plain_item(content, item):
            errors.append(f"Missing checked UX checklist item: {item}")

    if not has_checked_plain_item(content, "Light screenshot(s) attached or linked."):
        errors.append("Missing checked light screenshot evidence item")

    if not has_checked_plain_item(content, "Dark screenshot(s) attached or linked."):
        errors.append("Missing checked dark screenshot evidence item")

    return errors


def print_file_list(label: str, files: Sequence[str], max_items: int = 10) -> None:
    print(label)
    for path in files[:max_items]:
        print(f"  - {path}")
    if len(files) > max_items:
        print(f"  ... ({len(files) - max_items} more)")


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Validate frontend UX evidence artifacts for UI changes"
    )
    parser.add_argument("--base", default="", help="Base git ref/sha for diff range")
    parser.add_argument("--head", default="", help="Head git ref/sha for diff range")
    args = parser.parse_args()

    try:
        base, head = resolve_range(args.base, args.head)
    except ValueError as exc:
        print(f"[ERROR] {exc}")
        return 2

    print("Frontend UX evidence validation")
    print("-" * 60)
    print(f"[Range] {base}..{head}")

    try:
        changed_files = changed_files_for_range(base, head)
    except RuntimeError as exc:
        print(f"[ERROR] Unable to read changed files: {exc}")
        return 2

    if not changed_files:
        print("[PASS] No changed files in range")
        return 0

    ui_changes = detect_ui_changes(changed_files)
    if not ui_changes:
        print("[PASS] No frontend UI files changed; UX evidence not required.")
        return 0

    print_file_list("[Info] Frontend UI files changed:", ui_changes)

    evidence_files = detect_changed_evidence_files(changed_files)
    if not evidence_files:
        print("\n[FAIL] Frontend UI changed but no UX evidence file was updated.")
        print("Add an evidence file under:")
        print(f"  {EVIDENCE_DIR}/ux-audit-YYYY-MM-DD.md")
        print("Template:")
        print(f"  {EVIDENCE_DIR}/TEMPLATE.md")
        return 1

    print("\n[Info] Changed evidence files:")
    for file_path in evidence_files:
        print(f"  - {file_path}")

    valid_evidence_files: List[Path] = []
    validation_errors = {}

    for file_path in evidence_files:
        errors = validate_evidence_file(file_path)
        if errors:
            validation_errors[file_path] = errors
        else:
            valid_evidence_files.append(file_path)

    if not valid_evidence_files:
        print("\n[FAIL] Evidence file(s) found, but none satisfy required UX evidence format.")
        for file_path, errors in validation_errors.items():
            print(f"\n  {file_path}:")
            for err in errors:
                print(f"    - {err}")
        return 1

    print("\n[PASS] UX evidence requirements satisfied.")
    print("Valid evidence file(s):")
    for file_path in valid_evidence_files:
        print(f"  - {file_path}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
