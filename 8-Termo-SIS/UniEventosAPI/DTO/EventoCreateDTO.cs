namespace UniEventosAPI.DTO
{
    public class EventoCreateDTO
    {
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Local { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
    }
}
