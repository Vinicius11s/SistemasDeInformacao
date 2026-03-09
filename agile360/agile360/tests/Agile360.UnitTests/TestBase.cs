using Agile360.Domain.Interfaces;
using NSubstitute;

namespace Agile360.UnitTests;

/// <summary>
/// Base for unit tests with common mocks (IRepository, IUnitOfWork, ITenantProvider).
/// </summary>
public abstract class TestBase
{
    protected static IRepository<T> SubstituteRepository<T>() where T : Agile360.Domain.Entities.BaseEntity =>
        Substitute.For<IRepository<T>>();

    protected static IUnitOfWork SubstituteUnitOfWork()
    {
        var uow = Substitute.For<IUnitOfWork>();
        uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
        return uow;
    }

    protected static ITenantProvider SubstituteTenantProvider(Guid? advogadoId = null)
    {
        var provider = Substitute.For<ITenantProvider>();
        provider.GetCurrentAdvogadoId().Returns(advogadoId ?? Guid.NewGuid());
        return provider;
    }
}
