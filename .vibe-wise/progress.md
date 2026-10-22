# Learning Progress

## Pending decision
Decision: Step 7, Swagger polish + automated tests (incl. parallel test, expiry and event-ended cases not verifiable via HTTP)
Learner decided: integration tests (WebApplicationFactory) and injected TimeProvider (no criterion stated).
Step 7 implemented (approved): TimeProvider injected, Swagger info/tags/summaries, eventual.Tests (xUnit+Shouldly, WebApplicationFactory, FakeTimeProvider, per-test InMemory DB), 26 tests. Result: 24 pass, 2 fail (parallel reservations, parallel payments).
Diagnosis (verified): EF InMemory SaveChanges is not atomic. When the concurrency check fails, inserts in the same SaveChanges (new Order / new Tickets) are already persisted: 20 parallel reservations on 3 slots left 6 orders in DB (1 reported to client, available -3); 10 parallel payments left 21 tickets. Earlier AI statement "InMemory enforces concurrency tokens" was incomplete: it detects the conflict but does not roll back. Earlier curl checks passed by low contention (false confidence).
Learner chose B PostgreSQL via Docker (no criterion stated). Blocker found 2026-10-22: Docker is not installed on this machine (no docker in PATH, no Docker Desktop); Z:\APIs\ecommerce no longer exists.
Update: Docker Desktop is installed (CLI 29.9.2 at %LOCALAPPDATA%\Programs\DockerDesktop\resources\bin, not in this session's PATH); engine was not running (no docker_engine pipe, no WSL distros).
Stage: implementation approval (Postgres migration checkpoint shown, not yet answered); learner must start Docker Desktop for verification.
Update 2026-10-22: restructure committed by learner (4a55ba3), root is Z:\Eventual, Docker engine running (server 29.9.2). Learner asked to proceed with docker compose for Postgres; Build checkpoint on compose design asked, awaiting answer.
Learner answers (compose): API + Postgres both in compose (no criterion stated); named volume to keep data across containers (correct reasoning); user/password/db in .env or Docker secrets; tests stay InMemory/mocked. AI flagged: tests-on-InMemory contradicts the migration goal (the 2 failing concurrency tests stay failing); asked for criterion on API-in-container and to revisit test DB choice. Awaiting reply.
Learner: Docker secrets (criterion: educational); API in compose (criterion: wants to learn Docker; believed API must run in container to test, AI corrected: both modes possible). Asked whether SQLite for tests solves it; AI explained SQLite (atomic, but different engine/locking, shared in-memory connection serializes) vs Testcontainers Postgres. Awaiting learner choice.
Learner decided: both run modes (compose full with Docker secrets; postgres-only + dotnet run with .NET user-secrets); tests on Postgres but not the dev container. Learner proposed periodic dump/restore and asked about a "homologação" instance with full CRUD. AI clarified: no staging instance exists; Testcontainers already gives a separate disposable container; dump/restore is backup/seed, not test isolation; shared persistent DB causes test interference. Asked learner to pick ephemeral (Testcontainers) vs persistent test/homolog service and state what dump/restore would serve. Awaiting reply.
Learner chose A Testcontainers (no criterion stated; dump/restore question unanswered -> not in scope). Implementation checkpoint approved (AI additions accepted: EF migrations applied on startup in Development, AddKeyPerFile for secrets, one container per run + new database per test, UTC dates, ports 5432/8080).
Implemented 2026-10-22 (AI): Npgsql provider, Dockerfile (multi-stage), compose.yaml (postgres:17 + api, volume eventual-data, healthcheck, file secret db_password -> Database__Password), user-secrets, migration Initial, Testcontainers factory, docs. Verified: dotnet test 26/26 pass (concurrency tests now pass); compose full mode POST 201 then down/up keeps data; postgres-only + dotnet run reads data via user-secrets.
Blocker found: Z: is a network share (\PMBMZ-ERP2\Dev); Docker Desktop cannot bind-mount files from it, so file-based secret fails on compose up. Verified workaround (temporary override, not committed): secret source `environment: DB_PASSWORD`. Decision pending with learner.
Interrupted 2026-10-22 by restructure request: learner chose folders/projects/namespaces Eventual.Api + Eventual.Tests + Eventual.slnx (done by AI, build ok, same 24/26 tests). Root Z:\APIs to be renamed to Z:\Eventual by learner. Found existing git repo inside Eventual.Api (remote github.com/GuFerreiraV/eventual-api, learner's commits); learner chose to move that repo up to the root: done (.git and .gitignore now at Z:\APIs root, history and remote kept; git sees old paths deleted + Eventual.Api/ Eventual.Tests/ untracked until learner commits). .vibe-wise/ is untracked and not ignored.

## Check-in
Learner decisions: POST /events/{eventId}/check-in with { code } (scoped to event); window opens 30 min before start, closes at end; response shows buyer name, lote name, checkedInAt, code (AI refined learner's field list: lote name instead of id, drop order status, no email/amount); double scan: first passes, next blocked (behavior right, mechanism named by AI).
Step 6 done (AI implemented, verified via HTTP: 200, 409 reuse with time, 400 other event, 404 unknown code/event, 400 missing code, 409 not open yet; 10 parallel scans -> 1 x 200, 9 x 409). Event-ended (closed) path not verifiable via HTTP.

## Payment and tickets
Learner decisions: fake gateway driven by test token in body (tok_approved / tok_declined); decline keeps order Pending for retry until expiry; concurrent duplicate payment: first wins, second gets the paid order without error (UX decision). Mechanism (concurrency token on Order) explained by AI after learner did not identify it.
Step 5 done (AI implemented, verified via HTTP: 402 decline then retry approved, 200 idempotent repeat with same tickets, 404, 400 missing token, 10 parallel payments -> 10 x 200 and exactly Quantity unique tickets). Expired-on-pay path not verified (needs 15 min). Whether parallel requests actually hit the conflict path was not instrumented.
Needs reinforcement:
- Recognizing the same check-then-act race in a new entity and reusing the known mechanism (missed on payment and on check-in; states correct behavior but not the mechanism).

## Orders and reservation
Learner decisions: POST /lotes/{loteId}/orders (nested, no criterion stated); max 5 tickets per order -> exception; orders allowed after event start, not after event end.
Step 4 done (AI implemented, verified via HTTP: 201 Pendent + expiresAt, available decreases, 400 limit, 409 event ended, 409 sold out, 404 lote/order, 400 validation; 20 parallel reservations on 3-slot lote -> exactly 3 x 201, 17 x 409). Expired display not verified (needs 15 min wait).

## Routes and API surface
Learner decisions: nested /events/{eventId}/lotes plus flat GET /lotes/{id} (learner reasoned around nested-route weakness); MVP create + read only; future rules: reducing Quantity below reserved/sold and deleting event/lote with paid orders must be rejected.
Step 3 done (AI implemented, verified via HTTP: 201 + Location, 200 lists/detail with available, 404 event/lote, 400 date order and lote validation).

## Domain model and purchase flow
Introduced (AI explained):
- Real gateways confirm asynchronously via webhook; sync fake vs async simulation tradeoff.
- Stock reservation vs. take-on-payment; abandoned reservations; lazy vs. background expiration.
- Check-then-act race when two purchases read the same availability.
- InMemory provider: no transactions/locks, but enforces optimistic concurrency tokens; optimistic token vs in-process lock tradeoff.
Learner decisions (persistence/concurrency): EF Core InMemory for MVP; optimistic concurrency token on Batch (criteria unstated).
Demonstrated understanding:
- Completed step 1 (entities, DbContext with 3 relationships + unique index on Ticket.Code, InMemory registration); build passes. Initially confused which side holds the collection in 1-N; resolved after explanation and a guided fill-in table.
- Drew entity relationships with correct cardinalities (Lote-Event N:1, Order-Lote N:1, Ticket-Order N:1) on their own before writing EF config.
Learner decisions (choices, criteria mostly unstated):
- Order 1-N Ticket; payment via abstraction with real gateway deferred (criterion: MVP first, consistent with earlier choice); sync fake gateway; reserve on order creation; lazy expiration.
Needs reinforcement:
- Stating the criterion behind choices.

## Error handling and HTTP status
Step 2 done (AI implemented, learner approved): learner chose category bases + switch (idea A); verified 404/409/409/400/500 via temporary controller (removed).
Step 2 review 1: exceptions done (one file each, learner choice); handler mapping filled but TicketAlreadyUsed mapped to 422 (confirmed design was 409); else branch empty -> Status null -> handler would throw; Program.cs imports eventual.Exceptions instead of eventual.Handlers (build error); logger unused.
Learner reasoning: exception.Message safe "because more detail" (usefulness, not safety; 500 leak side not addressed); unmapped domain exception falls to generic 500 (correct prediction; good/bad tradeoff not evaluated).
Demonstrated understanding:
- Chose to reuse framework validation instead of duplicating the rule in the service; accepted generic 500 for infrastructure failures (learner reasoning).
- Mapped business failures to fitting status codes: 404 missing resource, 409 state conflict (sold out, reused QR), 422 invalid business data, 503 dependency unavailable (learner answers; criterion not stated).
Introduced:
- [ApiController] automatic 400 ValidationProblemDetails before the action runs (AI explained).
- Domain exceptions vs. infrastructure failures; not leaking exception details (AI explained).

## Layered architecture
Introduced:
- Compiler does not enforce layer boundaries in a single project (AI explained).
- Service layer holds business rules (AI explained after learner described it as "communications").
Demonstrated understanding:
- Dependency direction Controller -> Service -> Data; HTTP concerns (status codes) stay in the controller; service is HTTP-agnostic (learner reasoning).
- Chose single project by weighing complexity against MVP scope (learner reasoning).
- Placed the batch stock rule in the service layer (learner answer; rationale not stated).
Needs reinforcement:
- Articulating why a rule belongs in a given layer.

Handoff 2026-10-22: learner moving to another machine and new conversation; accepted current state ("sem problemas"), network-drive secret issue left as is (may not apply on new machine). On new machine: recreate secrets/db_password.txt and user-secrets; appsettings.json is gitignored. Next: commit this step, then continue roadmap (Unknowns in project-map).
