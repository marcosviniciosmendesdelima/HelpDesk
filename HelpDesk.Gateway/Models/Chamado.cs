namespace HelpDesk.Gateway.Models
{
    public class Chamado
    {
        public int Id { get; set; }

        public string Titulo { get; set; }

        public string Descricao { get; set; }

        public DateTime DataCriacao { get; set; }
    }
}