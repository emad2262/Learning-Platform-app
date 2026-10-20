using Learning_Platform_app.Domain.Entities;
using Learning_Platform_app.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace LearningPlatform.ModelTests;

public sealed class AcademyDatabaseTests(DatabaseFixture database) : IClassFixture<DatabaseFixture>
{
    private const string InitialMigration = "20261020134327_InitialAcademySchema";

    [Fact]
    public async Task Migrations_apply_without_membership_entities_pending_changes_or_shadow_foreign_keys()
    {
        await using var context = database.CreateContext();
        Assert.Equal(2, (await context.Database.GetAppliedMigrationsAsync()).Count());
        Assert.Empty(await context.Database.GetPendingMigrationsAsync());
        Assert.False(context.Database.HasPendingModelChanges());
        var entities = context.Model.GetEntityTypes().ToArray();
        Assert.DoesNotContain(entities, entity => entity.ClrType.Name.Contains("Membership", StringComparison.Ordinal));
        Assert.All(entities.Where(x => x.ClrType.Namespace == typeof(Academy).Namespace), entity =>
        {
            Assert.NotNull(entity.FindPrimaryKey());
            Assert.DoesNotContain(entity.GetForeignKeys().SelectMany(x => x.Properties), x => x.IsShadowProperty());
        });
    }

    [Fact]
    public async Task One_account_can_have_student_profiles_in_two_academies()
    {
        await using var context = database.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync();
        var graph = await SeedAsync(context);
        context.AddRange(Student(graph.User, graph.First), Student(graph.User, graph.Second));
        await context.SaveChangesAsync();
        Assert.Equal(2, await context.Students.CountAsync(x => x.UserId == graph.User.Id));
    }

    [Fact]
    public async Task Duplicate_student_profile_for_one_user_and_academy_is_rejected()
    {
        await using var context = database.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync();
        var graph = await SeedAsync(context);
        context.Add(Student(graph.User, graph.First));
        await context.SaveChangesAsync();
        context.Add(Student(graph.User, graph.First));
        await AssertSqlConstraintAsync(context, 2601, 2627);
    }

    [Fact]
    public async Task Duplicate_instructor_profile_for_one_user_and_academy_is_rejected()
    {
        await using var context = database.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync();
        var graph = await SeedAsync(context);
        context.Add(Instructor(graph.User, graph.First));
        await context.SaveChangesAsync();
        context.Add(Instructor(graph.User, graph.First));
        await AssertSqlConstraintAsync(context, 2601, 2627);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Profiles_cannot_reference_a_nonexistent_academy(bool isStudent)
    {
        await using var context = database.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync();
        var graph = await SeedAsync(context);
        if (isStudent)
            context.Add(new Student { AcademyId = int.MaxValue, UserId = graph.User.Id, Name = "Student" });
        else
            context.Add(new Instructor { AcademyId = int.MaxValue, UserId = graph.User.Id, FullName = "Instructor" });
        await AssertSqlConstraintAsync(context, 547);
    }

    [Fact]
    public async Task An_academy_must_have_an_existing_owner_account()
    {
        await using var context = database.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync();
        context.Add(new Academy
        {
            Name = "No owner", Slug = $"no-owner-{Guid.NewGuid():N}",
            Email = "academy@example.test", OwnerUserId = Guid.NewGuid().ToString("N")
        });
        await AssertSqlConstraintAsync(context, 547);
    }

    [Fact]
    public async Task The_same_account_can_be_an_owner_student_and_instructor()
    {
        await using var context = database.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync();
        var graph = await SeedAsync(context);
        context.AddRange(Student(graph.User, graph.First), Instructor(graph.User, graph.First));
        await context.SaveChangesAsync();
        Assert.Equal(graph.User.Id, graph.First.OwnerUserId);
        Assert.Single(graph.User.StudentProfiles);
        Assert.Single(graph.User.InstructorProfiles);
    }

    [Fact]
    public async Task Row_version_rejects_a_stale_academy_update()
    {
        await using var firstContext = database.CreateContext();
        var graph = await SeedAsync(firstContext);
        await using var secondContext = database.CreateContext();
        var stale = await secondContext.Academies.SingleAsync(x => x.Id == graph.First.Id);
        graph.First.Name = "First update";
        await firstContext.SaveChangesAsync();
        stale.Name = "Stale overwrite";
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => secondContext.SaveChangesAsync());
    }

    [Fact]
    public async Task Membership_removal_and_rollback_preserve_owner_and_profile_account_links()
    {
        var upgradeDatabase = new DatabaseFixture();
        try
        {
            await using var context = upgradeDatabase.CreateContext();
            var migrator = context.GetService<IMigrator>();
            await migrator.MigrateAsync(InitialMigration);
            var user = User();
            context.Add(user);
            await context.SaveChangesAsync();
            await context.Database.ExecuteSqlInterpolatedAsync($"""
                DECLARE @academyId int, @membershipId int;
                INSERT INTO [Academies] ([Name], [Slug], [Email], [IsActive], [CreatedAt])
                VALUES (N'Existing academy', 'existing-academy', 'academy@example.test', 1, SYSUTCDATETIME());
                SET @academyId = SCOPE_IDENTITY();
                INSERT INTO [AcademyMemberships] ([AcademyId], [UserId], [IsActive], [JoinedAt])
                VALUES (@academyId, {user.Id}, 1, SYSUTCDATETIME());
                SET @membershipId = SCOPE_IDENTITY();
                INSERT INTO [AcademyMembershipRoles] ([AcademyId], [MembershipId], [Role])
                VALUES (@academyId, @membershipId, 0);
                INSERT INTO [Students] ([AcademyId], [MembershipId], [Name], [Points], [IsActive], [CreatedAt])
                VALUES (@academyId, @membershipId, N'Existing student', 10, 1, SYSUTCDATETIME());
                INSERT INTO [Instructors] ([AcademyId], [MembershipId], [FullName], [IsActive], [CreatedAt])
                VALUES (@academyId, @membershipId, N'Existing instructor', 1, SYSUTCDATETIME());
                """);

            await migrator.MigrateAsync();
            var studentId = (await context.Students.SingleAsync()).Id;
            var instructorId = (await context.Instructors.SingleAsync()).Id;
            Assert.Equal(user.Id, (await context.Academies.SingleAsync()).OwnerUserId);
            Assert.Equal(user.Id, (await context.Students.SingleAsync()).UserId);
            Assert.Equal(user.Id, (await context.Instructors.SingleAsync()).UserId);

            await migrator.MigrateAsync(InitialMigration);
            await migrator.MigrateAsync();
            context.ChangeTracker.Clear();
            Assert.Equal(user.Id, (await context.Academies.SingleAsync()).OwnerUserId);
            var student = await context.Students.SingleAsync();
            var instructor = await context.Instructors.SingleAsync();
            Assert.Equal(studentId, student.Id);
            Assert.Equal(instructorId, instructor.Id);
            Assert.Equal(user.Id, student.UserId);
            Assert.Equal(user.Id, instructor.UserId);
            Assert.Equal(10, student.Points);
        }
        finally
        {
            await upgradeDatabase.DisposeAsync();
        }
    }

    [Fact]
    public async Task Membership_removal_stops_without_data_loss_if_an_existing_academy_has_no_owner()
    {
        var upgradeDatabase = new DatabaseFixture();
        try
        {
            await using var context = upgradeDatabase.CreateContext();
            var migrator = context.GetService<IMigrator>();
            await migrator.MigrateAsync(InitialMigration);
            await context.Database.ExecuteSqlRawAsync("""
                INSERT INTO [Academies] ([Name], [Slug], [Email], [IsActive], [CreatedAt])
                VALUES (N'Unassigned academy', 'unassigned-academy', 'academy@example.test', 1, SYSUTCDATETIME());
                """);
            var error = await Assert.ThrowsAsync<SqlException>(() => migrator.MigrateAsync());
            Assert.Equal(50001, error.Number);
            Assert.Single(await context.Database.GetAppliedMigrationsAsync());
            Assert.Equal(1, await context.Database.SqlQueryRaw<int>("SELECT COUNT(*) AS [Value] FROM [Academies]").SingleAsync());
            Assert.Equal(1, await context.Database.SqlQueryRaw<int>(
                "SELECT COUNT(*) AS [Value] FROM sys.tables WHERE [name] = 'AcademyMemberships'").SingleAsync());
        }
        finally
        {
            await upgradeDatabase.DisposeAsync();
        }
    }

    private static Student Student(ApplicationUser user, Academy academy) => new()
    {
        Academy = academy, User = user, Name = "Student"
    };

    private static Instructor Instructor(ApplicationUser user, Academy academy) => new()
    {
        Academy = academy, User = user, FullName = "Instructor"
    };

    private static ApplicationUser User()
    {
        var key = Guid.NewGuid().ToString("N");
        return new ApplicationUser
        {
            UserName = key, NormalizedUserName = key.ToUpperInvariant(),
            Email = $"{key}@example.test", NormalizedEmail = $"{key}@EXAMPLE.TEST".ToUpperInvariant(),
            DisplayName = "Test user"
        };
    }

    private static async Task<TestGraph> SeedAsync(ApplicationDbContext context)
    {
        var user = User();
        var first = Academy("first", user);
        var second = Academy("second", user);
        context.AddRange(first, second);
        await context.SaveChangesAsync();
        return new TestGraph(user, first, second);
    }

    private static Academy Academy(string label, ApplicationUser owner) => new()
    {
        Name = label, Slug = $"{label}-{Guid.NewGuid():N}", Email = "academy@example.test", Owner = owner
    };

    private static async Task AssertSqlConstraintAsync(ApplicationDbContext context, params int[] expectedNumbers)
    {
        var error = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        var sqlError = Assert.IsType<SqlException>(error.InnerException);
        Assert.Contains(sqlError.Number, expectedNumbers);
    }

    private sealed record TestGraph(ApplicationUser User, Academy First, Academy Second);
}

public sealed class DatabaseFixture : IAsyncLifetime
{
    private readonly string databaseName = $"LearningPlatform_ModelTests_{Guid.NewGuid():N}";
    private string ConnectionString => new SqlConnectionStringBuilder
    {
        DataSource = Environment.GetEnvironmentVariable("LEARNINGPLATFORM_TEST_SERVER") ?? @"(localdb)\MSSQLLocalDB",
        InitialCatalog = databaseName,
        IntegratedSecurity = true,
        TrustServerCertificate = true,
        MultipleActiveResultSets = false
    }.ConnectionString;

    public ApplicationDbContext CreateContext() => new(
        new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(ConnectionString).Options);

    public async Task InitializeAsync()
    {
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        var target = new SqlConnectionStringBuilder(ConnectionString).InitialCatalog;
        if (target != databaseName || !target.StartsWith("LearningPlatform_ModelTests_", StringComparison.Ordinal))
            throw new InvalidOperationException("Refusing to delete a database not created by this test fixture.");
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
    }
}
