using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TemGente.Models.Database;
using TemGente.Models.Poco;

namespace TemGente.Models
{
    public class Agendamento
    {
        /*
         * Propriedades com acesso via controller
         */
        public string Mensagem { get; set; }
        private PocoAgendamento Poco { get; set; }

        public Agendamento()
        {
            throw new Exception("Essa classe só pode ser instanciada com os parametros idLocal, idUsuario, dataDe, dataAte.");
        }

        public Agendamento(int idLocal, int idUsuario)
        {
            Poco = new PocoAgendamento() { IdLocal = idLocal, IdUsuario = idUsuario };
        }

        public Agendamento(int idLocal, int idUsuario, DateTime dataDe, DateTime dataAte) 
        {
            Poco = new PocoAgendamento() { IdLocal = idLocal, IdUsuario = idUsuario, DataDe = dataDe, DataAte = dataAte };
        }

        /*
        * Métodos com acesso via controller
        */
        internal bool Agendar()
        {
            PocoAgendamento agendamentoExterno = Poco;

            var agendamentoInterno = Get(agendamentoExterno.IdLocal, agendamentoExterno.DataDe, agendamentoExterno.DataAte);
            
            //pode agendar pois não existem agendamentos nesse range de data e hora
            if (agendamentoInterno == 0)
            {
                var cadastrado = Add(agendamentoExterno.IdLocal, agendamentoExterno.IdUsuario, agendamentoExterno.DataDe, agendamentoExterno.DataAte);

                if (cadastrado) {
                    Mensagem = "Horário foi agendado com sucesso de " + agendamentoExterno.DataDe + " até " + agendamentoExterno.DataAte;
                } else {
                    Mensagem = "Não foi possível realizar o agendamento no horário selecionado. " + Util.Alerta;
                }

                return cadastrado;
            }
            else
            {
                Mensagem = "Período selecionada não está disponível;" + Agendamentos();

                return false;
            }
        }

        internal bool Remover()
        {
            PocoAgendamento agendamentoExterno = Poco;

            var deletado = Del(agendamentoExterno.IdLocal, agendamentoExterno.IdUsuario);

            //agendamentos deletados com sucesso
            if (deletado)
            {
                Mensagem = "Agendamentos foram removidos";
            }
            else
            {
                Mensagem = "Não encontramos agendamentos para remover";
            }
            return deletado;
        }

        internal string Agendamentos() {
            var agendamentos = Get(Poco.IdLocal).ToList();
            var mensagem = string.Empty;

            if (agendamentos.Count > 0)
                mensagem = "<ul>";

            agendamentos.ForEach(delegate(PocoAgendamento a)
            {
                var periodo = a.DataDe.ToString("dd/MM/yyyy HH:mm") + " - " + a.DataAte.ToString("dd/MM/yyyy HH:mm");
                var pocoUsuario = new Usuario().Get(a.IdUsuario);

                //se estiver dentro do periodo, colocamos uma tag html para tornar o periodo negrito.
                if (DateTime.Now >= a.DataDe && DateTime.Now <= a.DataAte)
                    periodo = "<strong>" + periodo + "</strong>";

                //concatena o email com os horarios
                if (pocoUsuario.Id > 0)
                    periodo = "<li data-nome=\""+ pocoUsuario.Nome + "\">" + periodo + "</li>";

                mensagem += periodo;
            });

            if (agendamentos.Count > 0)
                return mensagem + "</ul>";
            else
                return "Não existem agendamentos nesse local";
        }

        /*
        * Métodos com acesso interno (privado)
        */
        private Dictionary<string, object> param;

        private IEnumerable<PocoAgendamento> Get(int idLocal)
        {
            var sql = @"
                SELECT Id, IdLocal, IdUsuario, DataDe, DataAte 
                FROM Agendamento Where IdLocal = @pidlocal order by DataDe asc
            ";
            param = new Dictionary<string, object>();
            param.Add("pidlocal", idLocal);

            return DB.Get<PocoAgendamento>(sql, param, CommandType.Text);
        }

        private int Get(int idLocal, DateTime dataDe, DateTime dataAte)
        {
            var sql = @"
                SELECT Id, IdLocal, IdUsuario, DataDe, DataAte 
                FROM Agendamento Where IdLocal = @pidlocal 
                AND (@pdatade <= DataDe AND @pdataate > DataDe AND @pdataate <= DataAte) 
                OR (@pdatade >= DataDe AND @pdatade <= DataAte) 
                OR (@pdataate >= DataDe AND @pdataate <= DataAte) 
                OR (@pdatade <= DataDe AND @pdataate >= DataAte) 
            ";
            param = new Dictionary<string, object>();
            param.Add("pidlocal", idLocal);
            param.Add("pdatade", dataDe);
            param.Add("pdataate", dataAte);

            return DB.Get<PocoAgendamento>(sql, param, CommandType.Text).Count();
        }
        
        private bool Add(int idLocal, int idusuario, DateTime dataDe, DateTime dataAte)
        {
            var sql = @"
                insert into Agendamento (IdLocal, IdUsuario, DataDe, DataAte) 
                values (@pidlocal, @pidusuario, @pdatade, @pdataate)
            ";

            param = new Dictionary<string, object>();
            param.Add("pdatade", dataDe);
            param.Add("pdataate", dataAte);
            param.Add("pidusuario", idusuario);
            param.Add("pidlocal", idLocal);

            return DB.Save(sql, param) > 0;
        }

        private bool Del(int idLocal, int idUsuario)
        {
            var sql = @"
                UPDATE Local set 
                    Status = 0,
                    IdUsuario = 0
                WHERE Id = @pidlocal AND IdUsuario = @pidusuario;

                DELETE FROM Agendamento 
                WHERE IdLocal = @pidlocal AND IdUsuario = @pidusuario;
            ";

            param = new Dictionary<string, object>();
            param.Add("pidusuario", idUsuario);
            param.Add("pidlocal", idLocal);

            return DB.Save(sql, param) > 0;
        }

    }
}