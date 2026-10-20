using UniEventosAPI.DTO;
using UniEventosAPI.Models;

namespace UniEventosAPI.Service
{
    public interface IEventoService
    {

        Task<List<EventoResponseDTO>> Listar();
        Task<EventoResponseDTO?> BuscarPorId(int id);
        Task<EventoResponseDTO> Cadastrar(EventoCreateDTO dto);
        Task<bool> Excluir(int id);
    }
}
