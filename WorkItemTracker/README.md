

# Work Item Tracker

A small in-memory console app for managing users and work items, written in C# (.NET 8) with no external packages.

## Run

```
dotnet run
```

Sample data (3+ users, 5+ work items covering every status and priority, one unassigned) is loaded on start.

## Structure

```
Program.cs          Menu loop, option handlers, table output, ordering menu (UI only)
ConsoleUI/          ConsoleInput: prompt helpers that re-ask on invalid input
Domain/             Rules and data. Never touches Console.
  User, WorkItem, Priority, Status
  UserService, WorkItemService
  DomainException
  IIdGenerator, SequentialIdGenerator
  PriorityOrdering, NewestFirstOrdering, TitleOrdering
SampleData          Startup data (C5)
```

Two layers: **Domain** decides what is allowed, **Program/ConsoleUI** reads input and prints. Domain never calls `Console`, so the rules can be reused or tested without a UI (C1).

## Where each requirement lives

| Requirement | Where |
|---|---|
| R1 add user, next Id | `UserService` |
| R2 duplicate name (ignoring case) | `UserService`, `StringComparison.OrdinalIgnoreCase` |
| R3 create work item, assignee must exist | `WorkItemService.CreateWorkItem` |
| R4 assign, replaces previous, Done can't be assigned | `WorkItem.AssignUser` (Done rule), `WorkItemService.AssignUserToWorkItem` (lookups) |
| R5 status moves forward one step | `WorkItem.UpdateStatus` |
| R6 InProgress needs an assignee | `WorkItem.UpdateStatus` |
| R7 rejection: nothing changes, clear message, app continues | `DomainException`, caught once in the `Program` loop |
| Q1 listing (all / status / assignee, unknown user rejected) | `WorkItemService.GetAllWorkItems`, `GetWorkItemsByStatus`, `GetWorkItemsByAssignee` |
| Q2 ordering (priority, then Id) | `PriorityOrdering`, the default comparer |
| Q3 summary incl. zero counts | `WorkItemService.GetStatusSummary` (loops over every `Status` value) |
| C1 separate UI and logic | Domain vs Program/ConsoleUI |
| C2 invalid input never crashes | `ConsoleInput` helpers, menu `default` case |
| C3 table with Id, Title, Priority, Status, Assignee, Created | `Program.PrintList` |
| C4 confirmations | option handlers in `Program` |
| C5 sample data | `SampleData` |

## Main decisions and why

- **Rules live where the data they need lives.** Rules that need only one object's own state are on that object: R5/R6 in `WorkItem.UpdateStatus`, "Done can't be assigned" in `WorkItem.AssignUser`. Rules that need a collection or another service are in the services: duplicate names, "user exists", "work item exists", Id issuing, queries.
- **One exception type for rejections.** Every business-rule failure throws `DomainException`. `Program` catches only that type, in one `try/catch` inside the loop, prints `Error: <message>` and shows the menu again. Any other exception is a bug and is allowed to surface.
- **Constructors validate too.** `User` and `WorkItem` cannot be created in an invalid state, so the invariant holds no matter who constructs them. The validators are `public static` so services can validate *before* taking an Id.
- **A rejection never burns an Id.** Services validate everything and resolve lookups first, and ask the Id generator last.
- **`IIdGenerator` is a deliberate, small over-engineering.** It applies dependency inversion (services depend on an abstraction, with one `SequentialIdGenerator` instance per sequence, created in `Program`), but nothing needs a second implementation yet. I kept it to show the pattern and accept the cost of one extra interface.
- **Enum order is meaningful.** `Priority` is High=0 to Low=2, so an ascending sort puts High first. `Status` is ToDo, InProgress, Done, so R5 reduces to "new minus current equals 1". Reordering these enums would change behavior.
- **Listings are snapshots.** Ordering ends in `ToList().AsReadOnly()`, so callers get a fixed, read-only list and not a live query over internal state.
- **Assignee filtering compares Ids**, not object references, so it keeps working if the same user is represented by different instances.
- **Time is stored in UTC** and converted with `ToLocalTime()` only when displayed.

## Choosing the order of a listing (optional extra)

Options 4, 5 and 6 ask for an ordering. Enter selects the default, Priority.

An ordering is an `IComparer<WorkItem>`, and the listing methods accept one:

- `PriorityOrdering`: priority, then Id ascending (default, also used when no comparer is passed)
- `NewestFirstOrdering`: `CreatedAt` newest first, then Id highest first
- `TitleOrdering`: title A to Z ignoring case, then Id ascending

`WorkItemService.OrderWorkItems` is the only place a sort is applied. Each comparer defines its own rule and tie-break.

**To add a fourth ordering:**

1. Add a class that implements `IComparer<WorkItem>` in `Domain`.
2. Add one entry to the `Orderings` list in `Program`.

The listing code and the existing orderings are not touched, and the menu is built from the list, so it needs no edit either.

