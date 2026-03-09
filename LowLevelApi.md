# Low-level API

## Overview

The low-level layer exposes the RouterOS API protocol through an async-first .NET surface.
It is intended for callers who want direct control over command paths, parameters, and
response handling without depending on higher-level abstractions.

This layer is the foundation of the library and is intended to remain relatively stable.

## Requirements

- **Target framework:** .NET 10+
- **RouterOS:** API enabled on the target device
  - API: TCP `8728`
  - API-SSL: TCP `8729`
- **Permissions:** credentials with access to the commands you execute

## Core types

- `ConnectionFactory` — creates and opens connections
- `ITikConnection` — represents a RouterOS API session
- `ITikCommand` — builds and executes a single command
- `ITikCommandParameter` — represents one encoded command parameter
- `ITikSentence` — base abstraction for returned sentences
- `ITikReSentence` — data sentence (`!re`)
- `ITikTrapSentence` — error sentence (`!trap`)
- `ITikDoneSentence` — completion sentence (`!done`)

## Usage

Use the low-level API when you want RouterOS command paths and parameters to remain explicit in
application code.

Typical flow:

1. Open a connection with `ConnectionFactory.ConnectAsync(...)` or create one explicitly.
2. Build a command or send raw rows.
3. Execute the command.
4. Read returned `!re`, `!trap`, and `!done` sentences as needed.

`TestHarness/Tests/LowLevelSanityTests.cs` shows a complete end-to-end scenario against a real
RouterOS device, including identity reads and address-list updates.

### Buffered execution

Use buffered execution when you want the full result materialized before processing.
This fits straightforward reads, validation steps, and small result sets.

### Streaming execution

Use streaming execution when result size is unknown, potentially large, or naturally processed as
it arrives.
This is the preferred pattern for scans, listings, and longer-running reads.

### Cancellation

Pass a cancellation token into connection and execution calls so the caller controls the lifetime
of network operations explicitly.

## Creating a connection

Use `ConnectionFactory` as the standard entry point.

### Plain API

- `ConnectionFactory.ConnectAsync(TikConnectionType.Api, host, user, password, cancellationToken)`

### API over TLS

- `ConnectionFactory.ConnectAsync(TikConnectionType.ApiSsl, host, user, password, cancellationToken)`

Prefer `await using` so the connection is closed cleanly even when execution fails.

## Executing commands

### Buffered execution

Use `ExecuteListAsync` when you want the full result materialized before processing.
This fits smaller result sets and straightforward read operations.

Typical flow:

1. Create a command from the connection.
2. Add parameters if needed.
3. Await `ExecuteListAsync`.

### Streaming execution

Use `ExecuteStreamAsync` when result size is unknown, large, or naturally processed as a stream.
Apply cancellation at the enumeration site with `.WithCancellation(cancellationToken)`.

### Sentence-level execution

If you need direct protocol control, `ITikConnection` can execute raw command rows and expose all
returned sentence types, including `!trap` and `!done`.

This is useful when building custom abstractions or when the higher-level command helpers are not
the right fit for the operation.

## Parameters

Commands support explicit RouterOS parameters through `ITikCommandParameter` and helper methods on
`ITikCommand`.

Common patterns include:

- query filters such as `?list=my-list`
- field assignments such as `=comment=example`
- property selection through `.proplist`

Parameter handling is intentionally explicit so the request shape stays visible in calling code.

## Working with responses

RouterOS replies are modeled as sentences:

- `!re` contains returned data
- `!trap` reports a command-level failure
- `!done` marks successful completion

For `!re` sentences, fields can be read through:

- `GetResponseField`
- `TryGetResponseField`
- `GetResponseFieldOrDefault`

For `!done` sentences, optional return values can be read through:

- `GetResponseWord`
- `GetResponseWordOrDefault`

## Concurrency model

A single `ITikConnection` supports one in-flight execution at a time.

If you need parallel work, create multiple connections rather than sharing one connection across
concurrent operations.

## Cancellation behavior

Cancellation is handled conservatively.
If an in-flight operation is canceled, the connection may be closed to avoid leaving the protocol
stream in a partially consumed state.

Treat cancellation as transactional and create a new connection after a canceled operation when
you need predictable continuation.

## Timeouts

`ITikConnection` exposes send and receive timeout properties.
For end-to-end control, cancellation tokens remain the preferred mechanism.

## Error handling

Command failures are surfaced through trap sentences and exception types in the public API.
When you build directly on the low-level layer, keep both protocol-level errors and transport-level
failures in mind.

## TestHarness reference

The `TestHarness` project contains practical low-level examples against a real RouterOS device,
including connection setup, command execution, explicit encoding verification, and end-to-end
validation.

If you want a working integration example before writing your own abstraction layer, start there.
