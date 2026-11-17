# Source structure and maintenance

- `src/ConnectionWatcher.Core/`: monitoring, rules, and persistence.
- `src/ConnectionWatcher.App/`: Windows Forms UI.
- `MainForm.*.cs`: partial files grouped by page, layout, and lifecycle.
- `tests/ConnectionWatcher.Tests/`: domain and local TCP scenarios.
- `tests/ConnectionWatcher.UiSmoke/`: languages, scaling, and dialogs.
- `scripts/`, `packaging/`: build and installer tools.
- `Directory.Build.props`: shared project-name property for references.

Partial files remain one class. Keep field initialization and constructor
order intact. UI resources, installer translations, and archived releases
are not source-formatting targets. Build metadata must evaluate identically.

## Readability rules

Keep authored code and comments within 80 characters per line. Use the
project formatter, then run this portable development check from the root:

```text
python tooling/check_source_width.py
```

The checker excludes generated output, vendor code, archived releases,
runtime data, and resources. It does not rewrite files. JSON, SQL, Markdown,
lockfiles, and installer localization are data/documents, not code-width
targets. Keep their values and historical provenance intact.

Group modules by responsibility and dependency direction. Keep public entry
points stable, avoid forwarding-only layers, and split a file when the parts
have independent responsibilities. Comments explain purpose, invariants,
units, timing, or non-obvious constraints; do not narrate obvious statements.
Prefer named helpers to deeply nested expressions or repeated magic values.

After moving code, verify public imports, builds, fixture tests, and affected
screens. Compare persisted formats, query order, translations, and input/output
contracts. Formatting checks do not establish behavioral equivalence.
