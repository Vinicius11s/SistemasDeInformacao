using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniEventosAPI.Data;
using UniEventosAPI.DTO;
using UniEventosAPI.Models;
using UniEventosAPI.Service;

namespace UniEventosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventosController : ControllerBase
    {
        private readonly IEventoService _service;

        public EventosController(IEventoService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var eventos = await _service.Listar();

            return Ok(eventos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _service.BuscarPorId(id);

            if (response is null)
                return NotFound();

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] EventoCreateDTO dto)
        {
            var response = await _service.Cadastrar(dto);

            return CreatedAtAction(
                    nameof(GetById),
                    new { id = response.Id },
                    response
                );

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.Excluir(id);

            if (!ok)
                return NotFound();

            return NoContent();
        }
    }
}
