using System.Collections.Generic;

namespace TemGente.Models
{
    public class ApiResposta
    {
        private string retorno = Enumeradores.Retornos.falso.ToString();
        public string Retorno
        {
            get { return retorno; }
            set { retorno = value; }
        }

        private string mensagem = "Falha monstruosa :/ Não foi possivel criar o retorno da api";
        public string Mensagem
        {
            get { return mensagem; }
            set { mensagem = value; }
        }

        public List<Poco.PocoLocal> Lista { get; set; }

        public int Id { get; set; }
    }
}