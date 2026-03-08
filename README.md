# KC.Tik4Net

Async .NET 10+ client for MikroTik RouterOS – a modern, opinionated successor to the original [tik4net](https://github.com/danikf/tik4net) library.

## Status

- Production-ready **low-level** RouterOS client, tested against RouterOS 7.21.x.
- **Async-only** API surface – no synchronous calls.
- **.NET 10+** only (breaking change from the original .NET Standard library).
- Growing **high-level** helpers (e.g. firewall address lists) – early-stage and evolving.
- No NuGet package yet – consume via source / project reference.

---

## Why KC.Tik4Net?

The original [`tik4net`](https://github.com/danikf/tik4net) by **Daniel Frantik** is a solid synchronous .NET Standard library for the MikroTik RouterOS API.

KC.Tik4Net began as a **clone** of that codebase, but has since been **heavily rewritten** with a different philosophy:

- **Forward-looking** – the library targets the tooling and runtime environment of 2026, not the compatibility requirements of the past.
- **Async-only** – first-class `async/await` everywhere, no synchronous API.
- **Modern .NET** – built for **.NET 10+** only.
- **Transparent encoding** – explicit handling of Nordic (DK) and other non‑ASCII characters, with no hidden conversions.
- **Simpler internals** – heavy reflection removed in favor of explicit mappings.
- **Focused evolution** – the library grows according to the needs of my own projects, not as a general-purpose RouterOS abstraction.

KC.Tik4Net is **not** a drop-in replacement for tik4net.  
If you need the original synchronous API or older .NET targets, use <https://github.com/danikf/tik4net>.  
If you want a modern, async-first RouterOS client for current .NET, KC.Tik4Net is the intended path.

---

## Documentation and examples

For usage details and examples, see the included docs and harness:

- **Low-level API guide:** [`./LowLevel.md`](./KC.Tik4Net/Docs/LowLevel.md)  
  Explains the core async client, connections, and raw RouterOS commands.
- **High-level API guide:** [`./HighLevel.md`](./KC.Tik4Net/Docs/HighLevel.md)  
  Describes the emerging high-level services (e.g. firewall address lists) and models.
- **Test harness:** [`./TestHarness`](./KC.Tik4Net/TestHarness) the `TestHarness` project  
  A small console application that exercises both low-level and selected high-level APIs against a RouterOS device and doubles as a regression test.

The harness is the best place to see real code that connects, runs commands, and exercises encoding and high-level helpers end to end.

---

## Low-level API

The low-level API is intended to be **general-purpose and complete**:

- Speaks the RouterOS API protocol directly.
- Works with any RouterOS feature that exposes commands.
- Tested against RouterOS **7.21.x**.
- Fully **async** – connections, commands, and result streaming.

If you are comfortable with RouterOS command paths (`/ip/firewall/address-list`, etc.), you can treat the low-level API as a modern async “wire” client and build your own abstractions on top.

---

## High-level API

On top of the low-level client, KC.Tik4Net provides a **growing** set of strongly-typed helpers:

- Convenience models and services for selected areas (e.g. firewall address lists).
- Fully async.
- **Early-stage**: most of RouterOS is not covered yet.
- Expanded primarily to support my own projects.

The **low-level** layer is intended to remain stable.  
The **high-level** layer will evolve as needed and may introduce breaking changes.

---

## Getting started (no NuGet yet)

There is currently **no NuGet package**. Until the API settles:

1. Clone the repository:

   ```bash
   git clone https://github.com/KCTech-Lab/KC.Tik4Net.git
   ```

2. Add a project reference to `KC.Tik4Net`.
3. Target **.NET 10.0** (or later).
4. Explore the `TestHarness` project and the docs in `KC.Tik4Net/Docs`.

---

## Compatibility with tik4net

KC.Tik4Net was originally created by cloning the tik4net repository and then refactoring it heavily. Over time it has diverged so much that it should be treated as a **different library**:

- Async-only.
- Requires **.NET 10+**.
- Different namespaces and types.
- Redesigned internals (encoding, reflection, mapping).

If you depend on tik4net’s exact API or need older .NET targets, use the original project.

If you are starting a new RouterOS integration on modern .NET and want async-first behavior, KC.Tik4Net is the intended choice.

---

## Community expectations

KC.Tik4Net is released as a **reference implementation** and a solid starting point for working with the MikroTik RouterOS API from modern .NET.

You are welcome to:

- explore and learn from the code  
- clone or fork it  
- build on the low-level client  
- extend or replace the high-level helpers  
- integrate it into your own tools and services  

### A note on expectations

This is an **egoistic** library.  
It evolves according to the needs of my own projects, not the needs of the entire RouterOS ecosystem.

- I am not a general support channel for MikroTik.
- I may not respond quickly (or at all) to issues or feature requests.
- Development will follow my own project requirements.

Think of this as a **clean, modern codebase** you can build on — not a fully supported product.

---

## Maintenance and contributions

I use KC.Tik4Net in my own projects and will continue to evolve it, especially the high-level layer.

- No promise of full RouterOS coverage.
- Issues and PRs are welcome.
- Small, focused PRs with tests are preferred.
- Contributions must remain async-only and avoid reintroducing reflection-heavy patterns.

---

## License and credits

KC.Tik4Net is licensed under the **Apache License, Version 2.0** – see [`LICENSE`](./LICENSE).

The project began as a clone of the original tik4net library by **Daniel Frantik**, but has since diverged significantly. It continues under the same Apache License 2.0.

---

## AiStudio showcase and further reading

KC.Tik4Net is also a showcase of modern AI-assisted engineering using **AiStudio** at **KCtech**:

- AiStudio overview: <https://www.kctech.dk/aistudio/aistudio-presentation>  
- Refactoring tik4net into KC.Tik4Net: <https://www.kctech.dk/aistudio/refactoring-tik4net>

These articles describe how the original synchronous .NET Standard library was transformed into this async, .NET 10+ codebase.

