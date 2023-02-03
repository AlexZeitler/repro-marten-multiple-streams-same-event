using Marten;
using Marten.Events.Projections;
using Npgsql;
using Shouldly;

namespace MultipleStreamsWithSameEvent.Tests;

public class SingleTenantTests
{
  [Fact]
  public async Task ShouldAddAssignEmployeeToTeam()
  {
    var store = TestEventStore.GetSingleTenantEventStoreForTestDatabase();
    await using var session = store.OpenSession();

    var registered = new EmployeeRegistered(
      "Jane",
      "Doe",
      "jd@acme.inc"
    );

    var founded = new TeamFounded("Martens");

    var employeeStreamId = Guid.NewGuid();
    var teamStreamId = Guid.NewGuid();

    session.Events.StartStream<Employee>(employeeStreamId, registered);
    session.Events.StartStream<Team>(teamStreamId, founded);
    await session.SaveChangesAsync();

    var assigned = new EmployeeAssignedToTeam(teamStreamId, employeeStreamId);
    session.Events.Append(employeeStreamId, assigned);
    session.Events.Append(teamStreamId, assigned);
    await session.SaveChangesAsync();

    var employee = session.Load<Employee>(employeeStreamId);
    var team = session.Load<Team>(teamStreamId);

    employee.Teams.ShouldContain(x=> x == teamStreamId);
    team.Employees.ShouldContain(x=> x == employeeStreamId);
  }
}

public static class TestEventStore
{
  public static IDocumentStore GetSingleTenantEventStoreForTestDatabase()
  {
    var connectionString = new NpgsqlConnectionStringBuilder()
    {
      Pooling = false,
      Port = 5454,
      Host = "localhost",
      CommandTimeout = 20,
      Database = "marten_test",
      Password = "Password12!",
      Username = "postgres"
    }.ToString();

    return DocumentStore.For(
      _ =>
      {
        _.Connection(connectionString);
        _.Projections.SelfAggregate<Employee>(ProjectionLifecycle.Inline);
        _.Projections.SelfAggregate<Team>(ProjectionLifecycle.Inline);
      }
    );
  }
}

public record EmployeeRegistered(
  string Firstname,
  string Lastname,
  string Email
);

public record TeamFounded(
  string Name
);

public record EmployeeAssignedToTeam(
  Guid TeamId,
  Guid EmployeeId
);

public class Team
{
  public Guid Id { get; set; }
  public string Name { get; set; }
  public List<Guid> Employees => new();

  public void Apply(
    TeamFounded founded
  ) =>
    Name = founded.Name;

  public void Apply(
    EmployeeAssignedToTeam assignedToTeam
  )
  {
    Employees.Add(assignedToTeam.EmployeeId);
  }
}

public class Employee
{
  public Guid Id { get; set; }
  public string Name { get; set; }
  public string Email { get; set; }
  public List<Guid> Teams => new();

  public void Apply(
    EmployeeRegistered registered
  )
  {
    Name = $"{registered.Firstname} {registered.Lastname}";
    Email = registered.Email;
  }

  public void Apply(
    EmployeeAssignedToTeam assignedToTeam
  )
  {
    Teams.Add(assignedToTeam.TeamId);
  }
}
