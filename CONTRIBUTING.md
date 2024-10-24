# Contributing to GDWeave

Most problems with contributions aren't hard blockers for merge, as they are usually stylistic and can be fixed later.

- Follow the .editorconfig at all times. Formatting will vary between editors, so it's okay to not have it 100% down!
  - Use LF line format.
  - Use K&R braces.
  - Use file-scoped namespaces.
  - Use `this.` for fields/properties.
  - Use `PascalCase` for namespaces, types, and public/static fields/properties. Use `camelCase` for private fields/properties. No prefixed underscore on private fields/properties.
  - Use `var` where possible.
  - Prefer fields to properties (unless in an interface). Fields can be `ref`d and are easier to deal with in the context of native code.
  - Use `readonly` where possible.
  - Prefer `const` over `static`.
  - Use primary constructors where possible.
- Keep in mind the contract for modding and API breakage.
  - Don't make changes to enums or interfaces that mods use.
  - Classes that shouldn't be exposed to mods should be `internal` or `private`.

## Loader

The loader is written in Rust. Use rustfmt for formatting and Clippy for linting.

Most changes do not require touching the loader.
