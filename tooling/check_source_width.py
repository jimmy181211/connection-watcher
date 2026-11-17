"""Check authored code only; never rewrite resources or generated output."""

import os
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SUFFIXES = {
    ".py", ".kt", ".kts", ".cs", ".csproj", ".props", ".targets",
    ".js", ".mjs", ".ts", ".tsx", ".css", ".html", ".ps1",
}
SKIP = {
    ".git", ".venv", "node_modules", ".codex_deps", ".openai",
    ".next", ".vinext", ".wrangler", "__pycache__", ".ruff_cache",
    ".pytest_cache", "bin", "obj", "dist", "data", "staging",
    "work", "outputs", "artifacts", "logs", "tmp", "evidence",
    "backup-output", "test-artifacts", "Final-Share", ".vercel",
    ".tools", ".npm-cache", "02_任务支撑材料", "learning",
}
GENERATED = {"worker-configuration.d.ts", "next-env.d.ts"}
failures = []
checked = 0
for directory, folders, names in os.walk(ROOT):
    relative = Path(directory).relative_to(ROOT).as_posix()
    folders[:] = [name for name in folders if name not in SKIP]
    if relative in {"public/ort", "android/app/build", "android/build"}:
        folders.clear()
        continue
    for name in names:
        path = Path(directory) / name
        if path.suffix not in SUFFIXES or name in GENERATED:
            continue
        checked += 1
        for number, line in enumerate(
            path.read_text("utf-8-sig").splitlines(), 1
        ):
            if len(line) > 80:
                failures.append(
                    f"{path.relative_to(ROOT)}:{number}: {len(line)}"
                )
print(f"Checked {checked} authored source files; {len(failures)} violations.")
for failure in failures:
    print(failure)
raise SystemExit(bool(failures))
