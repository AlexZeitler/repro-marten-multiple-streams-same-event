# repro-marten-multiple-streams-same-event

## Usage

```bash
cd MultipleStreamsWithSameEvent.Tests/test-database
docker compose up -d
dotnet test
```

## Issue

```bash
Marten.Exceptions.ApplyEventException: Failure to apply event #28 (EmployeeAssignedToTeam { TeamId = 02fb7b33-f55f-47b9-938c-efd546869af4, EmployeeId ...

Marten.Exceptions.ApplyEventException
Failure to apply event #28 (EmployeeAssignedToTeam { TeamId = 02fb7b33-f55f-47b9-938c-efd546869af4, EmployeeId = 66242ba0-7cfa-4dea-9095-35219607b91e }.)
   at Marten.Events.Aggregation.AggregationRuntime`2.ApplyChangesAsync(DocumentSessionBase session, EventSlice`2 slice, CancellationToken cancellation, ProjectionLifecycle lifecycle)
   at Marten.Events.Aggregation.AggregationRuntime`2.ApplyAsync(IDocumentOperations operations, IReadOnlyList`1 streams, CancellationToken cancellation)
   at Marten.Events.EventGraph.ProcessEventsAsync(DocumentSessionBase session, CancellationToken token)
   at Marten.Internal.Sessions.DocumentSessionBase.SaveChangesAsync(CancellationToken token)
   at Marten.Internal.Sessions.DocumentSessionBase.SaveChangesAsync(CancellationToken token)
   at MultipleStreamsWithSameEvent.Tests.SingleTenantTests.ShouldAddAssignEmployeeToTeam() in /home/alex/src/repro-marten-multiple-streams-same-event/MultipleStreamsWithSameEvent.Tests/SingleTenantMultiStreamSameEventTests.cs:line 35
   at MultipleStreamsWithSameEvent.Tests.SingleTenantTests.ShouldAddAssignEmployeeToTeam() in /home/alex/src/repro-marten-multiple-streams-same-event/MultipleStreamsWithSameEvent.Tests/SingleTenantMultiStreamSameEventTests.cs:line 41
   at Xunit.Sdk.TestInvoker`1.<>c__DisplayClass48_1.<<InvokeTestMethodAsync>b__1>d.MoveNext() in C:\Dev\xunit\xunit\src\xunit.execution\Sdk\Frameworks\Runners\TestInvoker.cs:line 264
--- End of stack trace from previous location ---
   at Xunit.Sdk.ExecutionTimer.AggregateAsync(Func`1 asyncAction) in C:\Dev\xunit\xunit\src\xunit.execution\Sdk\Frameworks\ExecutionTimer.cs:line 48
   at Xunit.Sdk.ExceptionAggregator.RunAsync(Func`1 code) in C:\Dev\xunit\xunit\src\xunit.core\Sdk\ExceptionAggregator.cs:line 90

System.NullReferenceException
Object reference not set to an instance of an object.
   at MultipleStreamsWithSameEvent.Tests.Team.Apply(EmployeeAssignedToTeam assignedToTeam) in /home/alex/src/repro-marten-multiple-streams-same-event/MultipleStreamsWithSameEvent.Tests/SingleTenantMultiStreamSameEventTests.cs:line 104
   at Marten.Generated.EventStore.SingleStreamAggregationInlineHandler2045855437.ApplyEvent(IQuerySession session, EventSlice`2 slice, IEvent evt, Team aggregate, CancellationToken cancellationToken)
   at Marten.Events.Aggregation.AggregationRuntime`2.ApplyChangesAsync(DocumentSessionBase session, EventSlice`2 slice, CancellationToken cancellation, ProjectionLifecycle lifecycle)
```