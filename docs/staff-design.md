# Staff Side Design

## Overview
The staff side is the team behind customer operations. This is used by actual staff members.

## Scope
**V1 (building now):** category table with routing and default priority; staff accounts on ASP.NET Core Identity; staff login; L1 dashboard of tickets at their level; claim a ticket; view conversation and internal notes; reply; internal note; escalate L1 to L2 with a required note; status transitions; staff TOTP, built last so the core workflow can be tested with password-only login.

**Second pass:** work statuses, auto-assignment and the ticket pool, deadline-ordered queues, Manager UI for staff accounts, engineering referral, file uploads, SignalR, ticket history.

**Third pass:** Admin role, Manager-defined roles with permissions, category tree and management UI, engineer & customer surveys, linked tickets, KPI dashboard, disposable-email blocking.

Sections below marked *(second pass)* describe the target design, not what is built.

## Roles and permissions
| Role | Created by | Scope |
|------|------------|--------|
| Admin | (seeded) | Create Manager accounts, overview the entire system, turn off/on services on demand, review logs, performance monitoring |
| Manager | Admin | Create Staff Accounts, reassign tickets, deactivate staff accounts, change staff role - not a customer facing role|
| Advisor L1 | Manager | Respond to tickets, escalate to another level, leave internal notes, review internal articles, access templates, review internal notes, change work statuses, sign out, sign in, review active tickets, change the logging of a case|
| Advisor L2 | Manager | everything in L1 + escalate to engineering + certain tickets that are otherwise not handled by L1 |
| Engineer | Manager | Review incoming escalations, respond - not a customer facing role|

L1 and L2 are separate tiers to the outside, but internally they are one `Advisor` role carrying a `Level`, since the distinction is scope rather than job function.

Roles are stored in Identity's role tables rather than an enum column, so a staff member can hold more than one role and roles are rows rather than code. Only advisors carry a `Level`; this rule is enforced in the staff service, because the role no longer lives on the staff row where a check constraint could see it.

*(Third pass)* A Manager creates a job role and ticks the permissions it grants, stored as role claims. Code then checks permissions such as `CanReassignTickets` instead of role names, and the `Level` rule becomes "the role grants working tickets at a level" rather than "is an Advisor".

How staff accounts are created :
=> Managers create staff accounts, they only specify the details such as their email (which should be a corporate email but for demo purposes this is TBD), full legal name, job role and the tools to customize their experience such as a signature or the interface

The first Manager cannot be created by a Manager, so it is seeded at startup from configuration. The seeder is idempotent and also provides demo advisors and sample tickets. It creates accounts through the staff service rather than inserting rows, so the same rules apply as for any other account.

In V1 the Manager UI does not exist yet, so the seeder is the only way staff accounts are created.

Staff accounts and user accounts are deactivated, never deleted *(second pass)*. A staff member's messages, assignments and history rows all reference their account, so deleting it would either be blocked by the foreign keys or erase the audit trail.

## Staff authentication
Staff accounts use ASP.NET Core Identity, registered with `AddIdentityCore` so the customer cookie stays the app's default scheme. `Staff` inherits from Identity's user class, so login fields (email, password hash, security stamp, lockout) and staff fields (display name, level, signature) share one table.

Staff two-factor authentication uses authenticator apps (TOTP) rather than emailed codes.

The security stamp is re-validated on every staff request, so banning a staff member or changing their role takes effect immediately. Each check loads the staff row and rebuilds the cookie's claims, a few small queries; staff are few and staff pages already query the database, so the cost is negligible. Customer sessions are re-validated on a longer interval.

Rotating the stamp signs a staff member out on every device at once. Signing out one specific device remotely (a lost phone, or a Manager ending one of an agent's sessions) is not possible with this design; see Decisions and alternatives.

*(Second pass)* A SignalR connection is authenticated once when it opens and is not re-checked afterwards, so a ban must also close the staff member's open connections.

## Availability *(second pass)*
When first logged in, the status sits on Idle and has to be switched to a relevant status straight away depending on advisor scheduling.

Statuses are :
Available - work status
Training - transition training, newhires sit on this status, not a work status
Break - scheduled breaks in accordance with business rules, not a work status
Lunch - scheduled lunch in accordance with business rules, not a work status
Offline - when a case needs further research and followup before a reply, not a work status

## Ticket categories
Categories live in their own table rather than an enum, so the routing rules are data instead of a `switch` in code.

A row carries the level that handles that question (`HandledByLevel`) and the priority tickets default to (`DefaultPriority`), which the advisor can adjust on triage.

Parent/child subcategories are third pass: a row holds a parent ID, and a null parent means the row is itself a parent.

## Assignment *(second pass)*
A database query looks at all available advisors and renders them by whoever has the least and assigns constantly until theres unavailable advisors at that point the tickets sit on a ticket pool to wait until an advisor is available to take more tickets. If a ticket is closed it moves out of the advisor queue, if the user decides to reply it gets pushed back into that advisors queue depending on severity.

In V1 there is no assignment engine. Tickets at an advisor's level appear unclaimed and an advisor claims one manually. Auto-assignment later sets the same column, so nothing built now is thrown away.

## Queue and deadlines *(second pass)*
Mission Critical - response within 4 hours
High - response in 1 business day
Standard - response in 2 business days
Low - response in 3 days

Deadlines are stored on the ticket rather than computed, so they survive transfers and changes to the SLA rules. `DueAt` is set at creation from the priority, and on escalation the elapsed wait time carries over rather than the clock restarting.

Tickets are ordered by the values above, and one approaching its deadline moves to the top of the queue. Assignment itself is sticky - the advisor's view re-sorts, the ticket is not reassigned.

## Escalation
L1 to L2: escalation is triggered by L1, no special explanation required but notes are required to indicate why they are transferring (business rule)

Escalating flips the ticket's level and clears the assignment, and the note is written as an internal message. The status is retained; the ticket may sit in the waiting pool until an L2 picks it up.

L2 to L1: generally not allowed in exceptional cases it can happen (i.e advisor leaving)

Engineering referral *(second pass)*: engineers receive ticket escalations with the template in front of them, they will be able to review any attachments and respond to a separate conversation linked to that ticket. Engineering team is not related to the ticket any other way however

## Ticket statuses
Open — awaiting advisor action
AwaitingInteraction — advisor has replied, waiting on the customer
Resolved — advisor considers it handled (customer can still reply)
Closed — terminal (set automatically 5 days after Resolved, or by the customer)

| From | To | Who |
|------|-----|-----|
| Open | AwaitingInteraction | Advisor posts a reply |
| AwaitingInteraction | Open | Customer replies |
| Open / AwaitingInteraction | Resolved | Advisor |
| Resolved | Open | Customer replies within the 5 day reopen window |
| Resolved | Closed | System, 5 days after Resolved |
| Open / AwaitingInteraction / Resolved | Closed | Customer ends it |

A customer can close a ticket at any point, including while it is still Open.

Closed is terminal. The reply box is hidden, and the server rejects any reply to a closed ticket regardless of what the client sends. Reopening uses the same ticket, not a new one.

## Ticket history *(second pass)*
Every action is recorded: when a reply is posted and by who, who escalated where, who placed an internal note, who uploaded what.

Replies and internal notes are already auditable because each is its own row, but escalations, claims, status and priority changes mutate the ticket row and leave no trace. History writes will live inside the service methods alongside the change itself, so an action cannot be performed without being recorded.

Deferred from V1 to keep the core advisor workflow first. To keep it cheap to add, every ticket change in V1 already goes through a single service method, never through a page writing to the ticket directly, so adding history is one write per method.

## Decisions and alternatives
- Deadline based assignment is better but other alternatives featured a point based system which would have been difficult to adjust and prioritizing things based purely off of priority which wouldnt be good as less severe tickets sit waiting forever so the obvious choice is a mix of wait time and priority for assignment
- Websockets are the best solution to tracking stuff in real time, whether a response was made to a ticket the advisor is following up one, work status switch, whether a ticket was reassigned etc
- Categories are a table rather than an enum because routing and default priority are business rules that should be data a Manager can change, not a `switch` that needs a redeploy. The parent/child tree is deferred because a nullable self-referencing column is cheap to add later, whereas the foreign key on the ticket is expensive to retrofit once there are real tickets.
- Sessions are stateless: claims are signed into the cookie rather than held in a server-side store, so the app scales horizontally with no shared session store or sticky sessions. The cost is that a cookie cannot be revoked instantly, which matters because Managers ban accounts and change roles. This is handled with a security stamp, a value on the account, copied into the cookie at sign-in and re-checked periodically (on every request for staff), so rotating it invalidates every outstanding cookie at once and covers bans, role changes and password resets with one mechanism. Caching the stamps in process memory was considered and rejected: it would be per-instance, so a revoked session could survive on whichever instance had not seen the change, undoing the reason for choosing stateless cookies in the first place.
- Server-side sessions were considered for staff, since the stamp is checked on every staff request anyway. Sessions would add per-device remote sign-out and a list of active sessions, at the cost of a session table, a custom ticket store, a cleanup job for expired sessions and a database write per request if activity is tracked; role changes would still need the stored sessions updated. Instant revocation is the only V1 requirement and the stamp check provides it with one setting. If per-device sign-out is needed later, the cookie handler's `SessionStore` accepts a server-side store without changes to Identity, pages or authorization. Sessions were also ruled out for presence (who is actually online): a valid session does not mean the person is at their desk, whereas a SignalR connection drops when the tab closes.
- Reply-by-email was skipped in favour of a notification containing a link to the ticket. Inbound mail needs an unguessable per-ticket reply token because `From:` headers are trivially forged, plus quoted-text and signature stripping that never fully works. The link drives customers to the portal where ticket state is visible anyway.
- Customer authentication is hand-built and staff authentication uses ASP.NET Core Identity. Building the customer side by hand was deliberate, to understand the mechanics; Identity was chosen for staff because its built-in TOTP, security stamp, lockout and roles match the staff requirements. In production one system would be chosen for both.
- `AddIdentityCore` rather than `AddIdentity`: `AddIdentity` registers its own cookies as the app's default scheme, which would take over the customer login. `AddIdentityCore` adds only the requested services and leaves cookie configuration to the app.
- Staff login and staff data share one table (`Staff` inherits from Identity's user class). The alternative, a separate Identity account table linked one-to-one to a `Staff` profile, keeps credentials out of business data and would allow swapping the login provider (for example corporate single sign-on) without touching `Staff`. It costs two rows per staff member and an extra lookup at login, which is not worth it at this project's scale; it is the better fit for a larger system.
- Roles moved from a `StaffRole` enum to Identity's role tables. The enum was simpler and type-safe, but moving later to Manager-defined roles would have required a data migration; role tables are the foundation that plan builds on. Role names become strings in code, so they are kept as constants in one place.
- The "only advisors have a Level" rule moved from a check constraint into the staff service, since a check constraint can only read its own row. A database trigger was considered: it would also catch direct SQL, but it hides the rule in the database and the SQLite test suite would never run it.
- Ticket history was moved to the second pass. Recovering history from before the table exists is impossible, but this project has only demo data, so deferring it loses nothing.
- Ticket IDs are sequential integers. Enumeration is already prevented by ownership being enforced in the query rather than by the URL being unguessable. At scale an opaque public identifier would be added alongside the integer key, to avoid leaking ticket volume, while keeping the integer for index performance.

## Open questions
- Staff email: corporate domain vs free addresses for the demo
- Whether SLA deadlines should count business days only, and how holidays are handled
- Whether L2 to L1 de-escalation is modelled at all or handled by a Manager reassigning
- Ticket history actor *(second pass)*: an explicit `ActorType` column (User/Staff/System, numbered from 1 so a forgotten value fails the check constraint) vs two nullable foreign keys where both null means System, following the existing `Messages` pattern. Must be decided before the history table is built.
- Demo access with mandatory TOTP: an interviewer logging in as a demo advisor cannot be expected to enrol an authenticator for someone else's account. Options: exempt demo accounts behind a clearly named setting, show the demo account's current code on the demo login page, let visitors enrol their own authenticator on first login, or keep TOTP mandatory and demonstrate it in a recording.
