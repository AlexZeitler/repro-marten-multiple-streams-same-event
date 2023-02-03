using Marten;
using Marten.Events.Aggregation;
using Marten.Events.Projections;
using Npgsql;
using Shouldly;

namespace MultipleStreamsWithSameEvent.Tests3;

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
    session.Events.Append(teamStreamId, assigned);
    session.Events.Append(employeeStreamId, assigned);
    await session.SaveChangesAsync();

    var employee = session.Load<Employee>(employeeStreamId);
    employee.Teams.ShouldContain(x => x == teamStreamId);

    var team = session.Load<Team>(teamStreamId);
    team.ShouldNotBeNull();
    team.Employees.ShouldContain(e => e == assigned.EmployeeId);
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
        _.Projections.Add<EmployeeProjection>(ProjectionLifecycle.Inline);
        _.Projections.Add<TeamProjection>(ProjectionLifecycle.Inline);
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
  public List<Guid> Employees { get; set; }
}

public class Employee
{
  public Guid Id { get; set; }
  public string Name { get; set; }
  public string Email { get; set; }
  public List<Guid> Teams { get; set; }
}

public class TeamProjection : SingleStreamAggregation<Team>
{
  public Team Create(
    TeamFounded founded
  )
  {
    return new Team
    {
      Name = founded.Name,
      Employees = new()
    };
  }

  public void Apply(
    EmployeeAssignedToTeam assignedToTeam,
    Team team
  )
  {
    team.Employees.Add(assignedToTeam.EmployeeId);
  }
}

public class EmployeeProjection : SingleStreamAggregation<Employee>
{
  public Employee Create(
    EmployeeRegistered registered
  )
  {
    return new Employee
    {
      Name = registered.Firstname,
      Teams = new()
    };
  }

  public void Apply(
    EmployeeAssignedToTeam assignedToTeam,
    Employee employee
  )
  {
    employee.Teams.Add(assignedToTeam.TeamId);
  }
}
