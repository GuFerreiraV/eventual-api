# Project Map

## Purpose
eventual: API for an event platform. Organizers create events with ticket batches (price, quantity); participants buy tickets; entry is validated by reading a unique QR code. Portfolio project.

## Requirements
From the chosen idea (not yet refined with the learner):
- Organizer creates events and ticket batches
- Participant buys tickets without overselling a batch
- Each ticket has a QR code; check-in rejects reuse
- Payment: real gateway (sandbox + webhook) planned for a later phase
- Swagger documentation; idea mentions JWT roles, banner upload, versioning, deploy

## Components
Implemented by learner (step 1 done, builds): eventual/Models/Event.cs, Lote.cs, Order.cs, OrderStatus.cs (Pending, Paid, Declined, Expired), Ticket.cs; eventual/Data/EventualDbContext.cs (4 DbSets; Lote->Event, Order->Lote, Ticket->Order; unique index Ticket.Code); Program.cs registers AddDbContext with UseInMemoryDatabase("eventual"); WeatherForecast template removed.
Implemented step 2 (verified): eventual/Exceptions/ DomainException (abstract), NotFoundException and ConflictException (abstract categories), EventNotFoundException : NotFound, LoteSoldOutException and TicketAlreadyUsedException : Conflict; eventual/Handlers/DomainExceptionHandler.cs switch NotFound 404, Conflict 409, other DomainException 400, else log + generic 500; Program.cs AddProblemDetails, AddExceptionHandler, UseExceptionHandler. (Replaces approved type->status dictionary design.)
Implemented step 3 (verified): routes GET/POST /events, GET /events/{eventId}, GET/POST /events/{eventId}/lotes, GET /lotes/{id}; Controllers/EventsController.cs, LotesController.cs; Services/IEventService+EventService (uses ILoteService for detail lotes), ILoteService+LoteService (ToResponse expression computes Available = Quantity - Paid - non-expired Pending); Models/Dtos/ CreateEventRequest (IValidatableObject End > Start, Required/MaxLength), CreateLoteRequest (Price >= 0, Quantity >= 1), EventSummaryResponse, EventDetailResponse, LoteResponse; Exceptions/LoteNotFoundException : NotFound; services registered Scoped in Program.cs.
Implemented step 4 (verified): POST /lotes/{loteId}/orders, GET /orders/{id} (Controllers/OrdersController.cs); Services/IOrderService+OrderService (limit check, retry loop up to 3 with Lote.Version swap, ChangeTracker.Clear on DbUpdateConcurrencyException, then ReservationConflictException); Services/OrderRules.cs (MaxTicketsPerOrder 5, ReservationDuration 15 min, HoldsStock filter shared with LoteService, EffectiveStatus shows expired Pending as Expired without writing); Dtos CreateOrderRequest, OrderResponse, TicketResponse; Exceptions OrderNotFound (404), OrderLimitExceeded (DomainException -> 400), EventAlreadyEnded (409), ReservationConflict (409); OrderStatus serialized as string via JsonStringEnumConverter attribute.
Implemented step 5 (verified): POST /orders/{id}/payment (200 OrderResponse with tickets); Order.Version Guid [ConcurrencyCheck]; Services/Payments/IPaymentGateway (ChargeAsync(orderId as idempotency key, amount, token) -> PaymentResult) + FakePaymentGateway (tok_approved approves, anything else declines); OrderService.PayAsync (Paid -> idempotent 200; expired -> mark Expired + OrderExpiredException 409; decline -> PaymentDeclinedException 402, order stays Pending; approve -> Paid + Quantity tickets with new Guid codes, Version swap; on concurrency conflict reload and return paid order); Exceptions PaymentException category (402 in handler), PaymentDeclinedException, OrderExpiredException; Dtos PayOrderRequest; gateway registered Scoped. OrderStatus.Declined kept in enum, unused in MVP.
Implemented step 6 (verified): POST /events/{eventId}/check-in (Controllers/CheckInController.cs); Services/ICheckInService+CheckInService (event -> ticket -> belongs to event -> window [start-30min, end) -> already used -> set CheckedInAt + Ticket.Version swap; conflict -> TicketAlreadyUsed); Ticket.Version Guid [ConcurrencyCheck]; Dtos CheckInRequest, CheckInResponse(code, buyerName, loteName, checkedInAt); Exceptions TicketNotFound (404), TicketNotForEvent (DomainException 400), CheckInNotOpen (409), CheckInClosed (409); TicketAlreadyUsedException now takes checkedInAt. docs/FUNCIONAMENTO.md covers all 10 routes.
Template scaffold:
- eventual/Program.cs: controllers, AddOpenApi + MapOpenApi, NSwag Swagger UI pointing to /openapi/v1.json (Development only)
- eventual/eventual.csproj: net10.0; Microsoft.AspNetCore.OpenApi 10.0.13, Microsoft.EntityFrameworkCore.InMemory 10.0.13, NSwag.AspNetCore 14.7.1
- eventual/docs/ESCOPO.md, FUNCIONAMENTO.md: package and local-run notes

Chosen (confirmed, not implemented):
- Learner: single project, layered folders; Controller -> Service -> Data; controllers own HTTP concerns; services are HTTP-agnostic and hold business rules (e.g., batch stock check)
- AI additions approved: folders Controllers/ Services/ Data/ Models/ (Models/Dtos separate from EF entities); service interfaces registered in DI; remove WeatherForecast template

- Error handling (confirmed, not implemented). Learner: services throw domain exceptions, global handler maps them; 404 event not found, 409 batch sold out, 409 QR already used; invalid date order via ASP.NET automatic model validation -> 400; infrastructure failures -> generic 500.
  AI additions approved: abstract DomainException base; handler holds type -> status map (exceptions carry no status); AddProblemDetails + IExceptionHandler; 500 logs and hides details; IValidatableObject on event-creation DTO; ProducesResponseType(ProblemDetails) per action.
- Purchase model (confirmed, not implemented). Learner: Event 1-N Batch; Order 1-N Ticket; IPaymentGateway with synchronous FakePaymentGateway in MVP (real gateway later); stock reserved on order creation with expiration; lazy expiration (availability ignores expired Pending; paying expired -> 409 + Expired); status Pending -> Paid | Declined | Expired; tickets issued on approval.
  AI additions approved: one batch per order (BatchId + Quantity on Order); BuyerName/BuyerEmail on Order until auth; ExpiresAt = CreatedAt + 15 min (configurable); TotalAmount snapshot; availability = Batch.Quantity - sum(Quantity of Paid or non-expired Pending); Ticket.Code random unique Guid (QR content); Ticket.CheckedInAt nullable. Base fields: Event(Id, Name, Description, Location, Start, End), Batch(Id, EventId, Name, Price, Quantity).
- Concurrency (confirmed, not implemented). Learner: optimistic concurrency token on Batch.
  AI additions approved: Batch.Version Guid [ConcurrencyCheck] changed on every reservation; on DbUpdateConcurrencyException retry up to 3 times (reload, recompute availability) then 409; only POST /orders touches Version; parallel test proving one winner for the last slot.
- Build order (AI proposal, learner writes the code): 1 structure + entities + DbContext; 2 error handling; 3 events/batches routes; 4 orders + reservation + concurrency; 5 fake payment + ticket issuing; 6 check-in; 7 Swagger polish + tests.

## Main Flow
Chosen: Client -> HTTPS -> Controller -> I*Service (rules) -> Data (DbContext) ; Controller maps results to status codes
Implemented today: Client -> HTTPS -> Controllers (WeatherForecast template)
Swagger UI (dev) -> /openapi/v1.json

## Data and Trust Boundaries
EF Core InMemory package referenced but no DbContext yet. Learner chose EF Core InMemory for the MVP (data lost on restart; no transactions; optimistic concurrency tokens enforced). No auth configured (UseAuthorization present, no scheme).

## Layout (2026-10-22)
Z:\APIs (to become Z:\Eventual): Eventual.slnx, Eventual.Api/ (Eventual.Api.csproj, namespace Eventual.Api), Eventual.Tests/ (namespace Eventual.Tests). Git repo at the root (moved from Eventual.Api/.git on 2026-10-22; remote GuFerreiraV/eventual-api).

## Build and Deployment
- dotnet dev-certs https --trust
- dotnet run --launch-profile https (from eventual/)
Deployment: none.

## Unknowns
Auth / organizer ownership, QR image generation, real payment gateway, API versioning, deployment target.
