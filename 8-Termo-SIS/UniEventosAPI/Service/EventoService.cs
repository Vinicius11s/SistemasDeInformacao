using Microsoft.EntityFrameworkCore;
using UniEventosAPI.Data;
using UniEventosAPI.DTO;
using UniEventosAPI.Models;

namespace UniEventosAPI.Service
{
    public class EventoService : IEventoService
    {

        private readonly AppDbContext _context;

        public EventoService(AppDbContext context) => _context = context;

        public async Task<List<EventoResponseDTO>> Listar()
        {
            return await _context.Eventos
                .AsNoTracking()
                .Select(e => new EventoResponseDTO
                {
                    Id = e.Id,
                    Titulo = e.Titulo,
                    Descricao = e.Descricao,
                    Local = e.Local,
                    Data = e.Data,
                    Status = e.Status.ToString()
                })
                .ToListAsync();
        }

        public async Task<EventoResponseDTO?> BuscarPorId(int id)
        {
            return await _context.Eventos
                .Where(e => e.Id == id)
                .AsNoTracking()
                .Select(e => new EventoResponseDTO
                {
                    Id = e.Id,
                    Titulo = e.Titulo,
                    Descricao = e.Descricao,
                    Local = e.Local,
                    Data = e.Data,
                    Status = e.Status.ToString()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<EventoResponseDTO> Cadastrar(EventoCreateDTO dto)
        {
            var evento = new Evento
            {
                Titulo = dto.Titulo,
                Descricao = dto.Descricao,
                Local = dto.Local,
                Data = dto.Data,
                Status = StatusEvento.Aberto
            };

            _context.Eventos.Add(evento);

            await _context.SaveChangesAsync();

            return new EventoResponseDTO
            {
                Id = evento.Id,
                Titulo = evento.Titulo,
                Descricao = evento.Descricao,
                Local = evento.Local,
                Data = evento.Data,
                Status = evento.Status.ToString()
            };
        }

        public async Task<bool> Excluir(int id)
        {
            var evento = await _context.Eventos.FindAsync(id);

            if (evento is null)
                return false;

            _context.Eventos.Remove(evento);

            await _context.SaveChangesAsync();

            return true;
        }

    }
}
