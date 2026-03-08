# HighLevel API (tik4net.HighLevel)

Hand-written high-level API on top of **tik4net** (low-level RouterOS API).

Design goals:
- async-first
- no reflection
- predictable mapping (hand-coded)
- a `MikrotikClient` instance owns one connection and is intended for single-flight usage

## Project
- Project: `tik4net.HighLevel`
- Namespace root: `tik4net.HighLevel`

## Connection lifecycle
Create a client, connect, use services, disconnect.

- Cancellation: pass `CancellationToken` for long operations.
- Concurrency: avoid concurrent commands on the same `MikrotikClient` instance.

## Entry point

### MikrotikClient
File: `tik4net.HighLevel/MikrotikClient.cs`

- `ConnectAsync(host, user, password, cancellationToken)`
- `Disconnect()` / `Dispose()`

Services:
- `System` → `SystemService`
- `Firewall` → `FirewallService`

## System

### SystemService
File: `tik4net.HighLevel/System/SystemService.cs`

- `GetIdentityAsync(cancellationToken)`
  - returns the router identity name.

## Firewall

### FirewallService
File: `tik4net.HighLevel/Firewall/FirewallService.cs`

- `AddressLists` → `FirewallAddressListService`

## Firewall Address Lists

### AddressListEntry
File: `tik4net.HighLevel/Firewall/AddressLists/AddressListEntry.cs`

Represents one `/ip/firewall/address-list` row.

Properties:
- `Id` (RouterOS `.id`, opaque)
- `List` (list name)
- `Address` (ip/cidr)
- `Comment` (optional)
- `Disabled` (bool)
- `Timeout` (optional, RouterOS duration string; countdown when read back)

Notes:
- `(List, Address)` is treated as unique for syncing.
- `Timeout` readback is not stable to the second (it counts down).

### FirewallAddressListService
File: `tik4net.HighLevel/Firewall/AddressLists/FirewallAddressListService.cs`

Core operations:
- `GetListAsync(listName, cancellationToken)`
  - fetches entries for a specific list name.
  - uses `.proplist` to limit payload for efficiency.

- `ClearAsync(listName, cancellationToken)`
  - removes all entries in the named list.

- `AddAsync(entry, cancellationToken)`
- `UpdateAsync(entry, cancellationToken)`
- `DeleteAsync(id, cancellationToken)`

Field semantics (important):
- **Create** (`AddAsync`): optional fields (`Comment`, `Timeout`) are only sent when non-null.
  - if `Timeout` is null during create, RouterOS default applies.
- **Update / Sync**: optional fields that are null are treated as **preserve existing**.
  - `Disabled` is always enforced.

### Sync

- `SyncListAsync(listName, desiredEntries, cancellationToken)`
  - reconciles router state to match `desiredEntries` for the specified `listName`.

Matching:
- The sync key is `Address` within the given `listName`.

Actions:
- add: address in desired but not on router
- remove: address on router but not in desired
- update: address exists on both sides but one or more enforced fields differ

Efficiency:
- 1 read (`GetListAsync` with `.proplist`) + one write per changed row.
- diff is O(n) using dictionaries keyed by address.

### AddressListSyncResult
File: `tik4net.HighLevel/Firewall/AddressLists/AddressListSyncResult.cs`

- `Added`
- `Removed`
- `Updated`
- `Effective` (the router’s list after sync)

## Test Harness
High level integration scenario:
- `TestHarness/Tests/HighLevelAddressListTests.cs`

Low level sanity check:
- `TestHarness/Tests/LowLevelSanityTests.cs`
