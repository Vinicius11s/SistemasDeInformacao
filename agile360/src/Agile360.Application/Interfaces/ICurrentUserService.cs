namespace Agile360.Application.Interfaces;

public interface ICurrentUserService
{
    Guid AdvogadoId { get; }
    string Email { get; }
    string Nome { get; }
    bool IsAuthenticated { get; }
}
