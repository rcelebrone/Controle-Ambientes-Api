using System;

namespace TemGente.Models.Poco
{
    public class PocoLocal
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public string Nome { get; set; }
        public string Imagem { get; set; }
        public DateTime DataHora { get; set; }
        public int Agendavel { get; set; }
        public int Status { get; set; }

        public bool ValidaNome
        {
            get
            {
                return !string.IsNullOrEmpty(Nome) && Nome.Length > 2;
            }
        }
    }
}