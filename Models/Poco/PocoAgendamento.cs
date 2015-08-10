using System;

namespace TemGente.Models.Poco
{
    public class PocoAgendamento
    {
        public int Id { get; set; }
        public int IdLocal { get; set; }
        public int IdUsuario { get; set; }
        public DateTime DataDe { get; set; }
        public DateTime DataAte { get; set; }
        
    }
}