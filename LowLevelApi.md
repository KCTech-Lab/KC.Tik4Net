# tik4net (current)

## Requirements

- .NET: `net10.0` (as currently targeted by the solution)
- RouterOS: MikroTik RouterOS with API enabled
  - API (plain): TCP `8728`
  - API-SSL (TLS): TCP `8729`
- Credentials with sufficient permissions for the commands you execute

Notes:
- The library is **async-first**. Synchronous execution is not supported.
- Cancellation is treated as **transactional**: if an in-flight operation is canceled, the connection is closed to avoid leaving the protocol stream half-consumed.

---

## Usage

### 1) Connect

Use `ConnectionFactory` for convenience.

- Plain API:
  - `ConnectionFactory.ConnectAsync(TikConnectionType.Api, host, user, password, ct)`
- API over TLS:
  - `ConnectionFactory.ConnectAsync(TikConnectionType.ApiSsl, host, user, password, ct)`

Always `await using` the connection so it is closed even on exceptions/cancellation.

### 2) Execute a command (list)

If you want all returned rows buffered:

- Create a command:
  - `var cmd = connection.CreateCommand("/system/identity/print");`
- Execute:
  - `var rows = await cmd.ExecuteListAsync(ct);`

Returned items are `ITikReSentence` (RouterOS `!re` rows).

### 3) Execute a command (stream)

For large outputs (preferred for scans):

- `await foreach (var row in cmd.ExecuteStreamAsync().WithCancellation(ct)) { ... }`

Important:
- The stream method intentionally has **no** `CancellationToken` parameter to keep builds warning-free; cancellation is applied by calling `.WithCancellation(ct)` at the enumeration site.

### 4) Low-level execution (raw rows)

If you need full sentence-level control (including `!trap` / `!done`):

- Build command rows yourself (first row is the command, then parameters):
  - `new[] { "/ip/address/print", "?disabled=false", "=.proplist=address,interface" }`
- Enumerate sentences:
  - `await foreach (var s in connection.ExecuteStreamAsync(rows).WithCancellation(ct)) { ... }`

If you want a buffered list of all sentences:

- `var sentences = await connection.ExecuteAsync(rows, ct);`

### 5) Close

- `await connection.CloseAsync(ct);` or rely on `await using`.

---

## Behavioral notes

- **Single-flight per connection:** only one in-flight execute is allowed per `ITikConnection`. Parallelism requires multiple connections.
- **Cancellation closes the connection:** treat router interactions as transactions; on cancel, create a new connection.
- **Timeouts:** `SendTimeout` / `ReceiveTimeout` exist on `ITikConnection` (milliseconds). Prefer cancellation for end-to-end control.

---

## Quick checklist for parallel scans

- Create a **connection pool** (N connections) rather than sharing one connection across N tasks.
- Use streaming (`ExecuteStreamAsync`) and apply cancellation with `.WithCancellation(ct)`.
- On any `OperationCanceledException`, dispose the connection and recreate.
