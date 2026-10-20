namespace UniEventosAPI.Models
{
    public class Evento
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Local { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
        public StatusEvento Status { get; set; }
    }
}
