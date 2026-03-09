# High-level API

## Overview

The high-level layer builds on top of the low-level RouterOS client and provides a focused,
handwritten set of services for common operations.

It is still a work in progress. What is implemented today is intended to be reliable and suitable
for normal use, but the overall surface area is intentionally incomplete and will grow selectively
over time.

The goal is explicit behavior, clear data shapes, and practical building blocks for real-world
.NET projects.

## Design

The high-level API follows a few consistent principles:

- **Async-first** — all operations are asynchronous
- **Explicit mapping** — no reflection-based object binding
- **Focused scope** — coverage grows where there is concrete need
- **Low-level alignment** — services stay close to RouterOS behavior

## Usage

Use the high-level API when you want a typed entry point and task-oriented service methods instead
of composing raw RouterOS commands yourself.

Typical flow:

1. Connect with `MikrotikClient.Connect(...)`.
2. Access a service group such as `System` or `Firewall`.
3. Call the relevant async method.
4. Dispose the client when finished.

`TestHarness/Tests/HighLevelAddressListTests.cs` shows a complete scenario that clears a list,
synchronizes a desired state, validates the effective result, and leaves the final router state
available for inspection.

### Service-oriented usage

Use the high-level API for focused operations where a typed method is easier to read and maintain
than manually composing low-level commands.

### Managed state

For address lists, the high-level layer can reconcile router state against a desired collection.
This keeps create, update, and delete logic in one place and makes calling code easier to reason
about.

### Selective coverage

The current high-level layer is useful where it exists, but it is not meant to cover RouterOS
broadly yet. If a needed operation is not exposed here, the low-level API remains the fallback.

## Entry point

### `MikrotikClient`

`MikrotikClient` is the public high-level entry point.
It owns a connected `ITikConnection` and exposes service areas through typed properties.

Current service groups:

- `System`
- `Firewall`

The client supports both synchronous disposal and asynchronous disposal.

## Lifecycle

Typical usage flow:

1. Connect with `MikrotikClient.Connect(...)`.
2. Use one or more service groups.
3. Dispose the client when finished.

A single client should be treated as a single active session. Avoid concurrent command execution
through the same instance.

## System service

### `SystemService`

Provides high-level access to RouterOS system operations.

Current capability:

- `GetIdentityAsync` — reads the configured router identity name

This reflects the intended style of the high-level API: narrow surface area, explicit behavior,
and simple return values.

## Firewall service

### `FirewallService`

Groups firewall-related high-level operations.

Current capability:

- `AddressLists` — access to firewall address-list operations

## Firewall address lists

### `FirewallAddressListService`

Provides CRUD-style operations and synchronization support for
`/ip/firewall/address-list`.

Available operations:

- `GetListAsync` — reads all entries in a named list
- `ReadAsync` — reads a single entry by RouterOS id
- `AddAsync` — adds a new entry
- `UpdateAsync` — updates mutable fields on an existing entry
- `DeleteAsync` — removes an entry by id
- `ClearAsync` — removes all entries from a named list
- `SyncListAsync` — reconciles router state with a desired set of entries

### `AddressListEntry`

Represents a single firewall address-list row.

The main properties are:

- `Id` — RouterOS internal id
- `List` — address-list name
- `Address` — IP address or CIDR value
- `Comment` — optional comment
- `Disabled` — whether the entry is disabled
- `Timeout` — optional RouterOS timeout string

### `AddressListSyncResult`

Represents the outcome of a synchronization run.

It reports:

- the number of entries added
- the number of entries removed
- the number of entries updated
- the effective final state returned from the router

## Synchronization semantics

`SyncListAsync` treats the supplied collection as the desired final contents of a specific address
list.

Behavior is intentionally explicit:

- entries missing on the router are added
- entries present on the router but absent from the desired set are removed
- entries present on both sides are updated when enforced fields differ

Matching is based on address within the specified list.

Optional fields are handled conservatively:

- during create, optional values are sent only when provided
- during update, `null` means preserve the existing router value
- `Disabled` is treated as an enforced field

## Scope and evolution

The low-level layer is intended to remain broadly capable and comparatively stable.
The high-level layer is intentionally narrower and still evolving.

That means two things at the same time:

- the currently implemented service methods are suitable for normal use
- the overall high-level coverage is not complete and should be treated as an expanding layer

## TestHarness reference

The `TestHarness` project includes end-to-end examples of the current high-level layer, including
address-list synchronization.

It serves both as executable usage documentation and as a regression check for the library.
