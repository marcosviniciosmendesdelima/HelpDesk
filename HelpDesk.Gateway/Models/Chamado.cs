using System;
using System.Text.Json.Serialization; // Adicione esta linha!

namespace HelpDesk.Gateway.Models
{
    public class Chamado
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("titulo")]
        public string Titulo { get; set; } = string.Empty;

        [JsonPropertyName("descricao")]
        public string Descricao { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = "Aberto";

        [JsonPropertyName("prioridade")]
        public string Prioridade { get; set; } = "Média";

        [JsonPropertyName("datacriacao")]
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}