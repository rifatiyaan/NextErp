using AutoMapper;
using NextErp.Application.Commands.Module;
using NextErp.Application.DTOs;
using NextErp.Application.Handlers.CommandHandlers.Module;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Module;

public class CreateBulkModulesHandlerTests : HandlerTestBase
{
    private static readonly IMapper Mapper = BuildMapper();

    private static IMapper BuildMapper()
    {
        var cfg = new MapperConfiguration(c =>
            c.AddMaps(typeof(NextErp.Application.ApplicationAssemblyMarker).Assembly));
        return cfg.CreateMapper();
    }

    private CreateBulkModulesHandler BuildHandler() => new(Db, Mapper);

    private static DTOs.Module.Request.Create.Hierarchical Hier(string title, params DTOs.Module.Request.Create.Hierarchical[] children) =>
        new()
        {
            Title = title,
            Type = ModuleType.Module,
            Children = children.ToList()
        };

    [Fact]
    public async Task Two_parents_two_children_each_creates_six_modules_total()
    {
        var sut = BuildHandler();
        var cmd = new CreateBulkModulesCommand(new DTOs.Module.Request.Create.Bulk
        {
            Modules = new List<DTOs.Module.Request.Create.Hierarchical>
            {
                Hier("P1", Hier("P1-C1"), Hier("P1-C2")),
                Hier("P2", Hier("P2-C1"), Hier("P2-C2")),
            }
        });

        var response = await sut.Handle(cmd, CancellationToken.None);

        var rows = await Db.Modules.AsNoTracking().ToListAsync();
        rows.Should().HaveCount(6);

        // The response only enumerates parents, but tracks total successes.
        response.Modules.Should().HaveCount(2);
        response.SuccessCount.Should().Be(6); // 2 parents + 4 children
        response.FailureCount.Should().Be(0);
        response.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Empty_input_creates_zero_modules_and_reports_zero_counts()
    {
        var sut = BuildHandler();
        var cmd = new CreateBulkModulesCommand(new DTOs.Module.Request.Create.Bulk
        {
            Modules = new List<DTOs.Module.Request.Create.Hierarchical>()
        });

        var response = await sut.Handle(cmd, CancellationToken.None);

        (await Db.Modules.AsNoTracking().AnyAsync()).Should().BeFalse();
        response.Modules.Should().BeEmpty();
        response.SuccessCount.Should().Be(0);
        response.FailureCount.Should().Be(0);
        response.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Children_have_ParentId_pointing_at_newly_created_parent_ids()
    {
        var sut = BuildHandler();
        var cmd = new CreateBulkModulesCommand(new DTOs.Module.Request.Create.Bulk
        {
            Modules = new List<DTOs.Module.Request.Create.Hierarchical>
            {
                Hier("Alpha", Hier("AlphaChild1"), Hier("AlphaChild2")),
                Hier("Beta", Hier("BetaChild1")),
            }
        });

        await sut.Handle(cmd, CancellationToken.None);

        var rows = await Db.Modules.AsNoTracking().ToListAsync();

        var alpha = rows.Single(r => r.Title == "Alpha");
        var beta = rows.Single(r => r.Title == "Beta");

        rows.Where(r => r.Title.StartsWith("AlphaChild"))
            .Should().HaveCount(2)
            .And.AllSatisfy(c => c.ParentId.Should().Be(alpha.Id));

        rows.Where(r => r.Title.StartsWith("BetaChild"))
            .Should().HaveCount(1)
            .And.AllSatisfy(c => c.ParentId.Should().Be(beta.Id));

        // Cross-check: parents themselves have no parent.
        alpha.ParentId.Should().BeNull();
        beta.ParentId.Should().BeNull();
    }
}

