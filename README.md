# KC.Tik4Net

Async .NET 10+ API client for MikroTik RouterOS — a modern, opinionated successor to the original [tik4net](https://github.com/danikf/tik4net) library.

## Status

- Production-ready **low-level** RouterOS client, tested against RouterOS 7.21.x.
- **Async-only** API surface.
- **.NET 10+** only.
- Growing **high-level** helpers — still a work in progress, but the implemented parts are suitable for normal use.
- No NuGet package yet — consume via source or project reference.

## Why this library?

KC.Tik4Net began as a clone of [tik4net](https://github.com/danikf/tik4net), but has since diverged into a different library with a simpler, async-first design.

- built for **modern .NET**, not broad legacy compatibility
- **async/await** throughout
- explicit encoding handling, including non-ASCII text
- less reflection, more direct and readable mappings

It is **not** a drop-in replacement for tik4net. If you need the original synchronous API or older .NET targets, use the original project.

## Very small example

```csharp
await using var connection = await ConnectionFactory.ConnectAsync(
    TikConnectionType.Api,
    host,
    user,
    password,
    cancellationToken);

var command = connection.CreateCommand("/system/identity/print");
var rows = await command.ExecuteListAsync(cancellationToken);
var identity = rows.Single().GetResponseField("name");
```

## Documentation and examples

- **Low-level API guide:** [`LowLevelApi.md`](LowLevelApi.md)
- **High-level API guide:** [`HighLevelApi.md`](HighLevelApi.md)
- **Test harness:** [`TestHarness`](TestHarness)

- **Low-level API guide:** [`LowLevelApi.md`](LowLevelApi.md)  
  Explains the core async client, connections, and raw RouterOS commands.
- **High-level API guide:** [`HighLevelApi.md`](HighLevelApi.md)  
  Describes the emerging high-level services (e.g. firewall address lists) and models.
- **Test harness:** [`TestHarness`](TestHarness)  
  A small console application that shows realistic usage of both low-level and
  selected high-level APIs against a RouterOS device and doubles as a regression
  test for the library.
The harness is the best place to see real code that connects, runs commands, and exercises encoding and high-level helpers end to end.

There is currently **no NuGet package**.

1. Clone the repository.
2. Add a project reference to `KC.Tik4Net`.
3. Target **.NET 10.0** or later.
4. Start with `LowLevelApi.md`, `HighLevelApi.md`, and `TestHarness`.

## Contributing and expectations

This library is developed to support real projects, not to provide complete RouterOS coverage or broad support guarantees.

- Issues and PRs are welcome.
- Small, focused PRs with tests are preferred.
- Contributions should stay async-only and avoid reflection-heavy patterns.

## License and credits

KC.Tik4Net is licensed under the **Apache License, Version 2.0** — see [`LICENSE`](./LICENSE).

The project began as a clone of the original tik4net library by **Daniel Frantik**, but has since diverged significantly while continuing under the same Apache License 2.0.

## Further reading

- AiStudio overview: <https://www.kctech.dk/aistudio/aistudio-presentation>
- Refactoring tik4net into KC.Tik4Net: <https://www.kctech.dk/aistudio/refactoring-tik4net>
