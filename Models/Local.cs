using System.Collections.Generic;
using System.Data;
using System.Linq;
using TemGente.Models.Database;
using TemGente.Models.Poco;
using System;

namespace TemGente.Models
{
    public class Local
    {
        /*
         * Propriedades com acesso via controller
         */
        public string Mensagem { get; set; }
        public PocoLocal Poco { get; set; }
        public DateTime? DataAte { get; set; }
        public DateTime? DataDe { get; set; }

        /*
        * Métodos com acesso via controller
        */
        internal bool Adicionar()
        {
            PocoLocal localExterno = Poco;

            if (string.IsNullOrEmpty(localExterno.Imagem))
                localExterno.Imagem = Util.Base64LocalDefault;

            var cadastrou = Add(localExterno);
            if (cadastrou)
            {
                Mensagem = localExterno.Nome + ", cadastrado com sucesso";
            }
            else
            {
                Mensagem = "Estranho!!! Não conseguimos cadastrar esse local. " + Util.Alerta;
                return false;
            }

            return cadastrou;
        }

        internal PocoLocal Carregar()
        {
            var local = Get(Poco.Id);

            //Antes de tudo, verificamos se o local existe
            if (local == null)
            {
                Mensagem = "Estranho!!! Local não foi encontrado. " + Util.Alerta;
                return null;
            }
            
            //Retornamos na mensagem da api os horarios agendados
            if (local.Agendavel == 1)
            {
                //atualiza os locais de acordo com a data e horario atual
                UpdateLocalXAgendamento(Poco.Id);

                //atualiza os dados do local, caso o status tenha alterado 
                local = Get(Poco.Id);

                Mensagem = new Agendamento(Poco.Id, Poco.IdUsuario).Agendamentos();
            }
            else if (local.Status == 0)
            {
                Mensagem = local.Nome + " está LIVRE, coloque em uso e aproveite!";
            }
            else if (local.Status == 1)
            {
                Mensagem = local.Nome + " está OCUPADO!";
            }

            return local;
        }

        internal bool Ocupar(int idUsuario)
        {
            var local = Get(Poco.Id);
            //se for agendavel, tentaremos agendar
            if (local.Agendavel == 1)
            {
                if (!DataDe.HasValue || !DataAte.HasValue)
                {
                    Mensagem = "Esse local é privado, por favor, informe o período que pretende utilizá-lo";
                    return false;
                }

                if (DataAte.Value <= DataDe.Value)
                {
                    Mensagem = "O período final, deve ser superior ao período inicial";
                    return false;
                }

                var agendamento = new Agendamento(local.Id, idUsuario, DataDe.Value, DataAte.Value);

                //tentaremos agendar, se não for possivel, a class agendamento vai nos avisar o motivo
                var agendado = agendamento.Agendar();

                //retorno para essa class a mensagem da class agendamento
                Mensagem = agendamento.Mensagem;

                return agendado;

            }
            else if (local.Status == 0)
            {
                local.Status = 1;
                local.IdUsuario = idUsuario;
                
                var atualizado = UpdateLocalUsuario(local);
                if (atualizado)
                {
                    //TODO: push alertando todos os usuario desse local, que o local foi colocado em uso
                    Mensagem = "É sua vez! Você colocou " + local.Nome + " em uso";
                }
                else
                {
                    Mensagem = "Estranho!!! Não foi possível colocar esse local em uso. " + Util.Alerta;
                }
                return atualizado;
            }
            else
            {
                Mensagem = local.Nome + " está ocupado";
                return false;
            }
        }

        internal bool Desocupar(int idUsuario)
        {
            var local = Get(Poco.Id);
            //(apenas se estiver sendo usado pelo proprio usuario)
            if (local.Status == 1)
            {
                if (local.IdUsuario != idUsuario)
                {
                    Mensagem = "Você está tentando sair de um local que você não entrou o.O";
                    return false;
                }

                local.Status = 0;
                local.IdUsuario = 0;
                var atualizado = UpdateLocalUsuario(local);
                if (atualizado)
                {
                    //TODO: push alertando todos os usuario desse local, que o local foi desocupado
                    Mensagem = local.Nome + " foi desocupado. Obrigado por avisar :)";
                }
                else
                {
                    Mensagem = "Estranho!!! Não foi possível colocar esse local em uso. " + Util.Alerta;
                }
                return atualizado;
            }
            else
            {
                Mensagem = local.Nome + " está livre";
                return false;
            }
        }

        /*
        * Métodos com acesso interno (privado)
        */
        private Dictionary<string, object> param;

        private PocoLocal Get(int id)
        {
            var sql = @"
                SELECT Id, IdUsuario, Nome, Imagem, DataHora, Status, Agendavel
                FROM Local
                Where Id = @pid
            ";
            param = new Dictionary<string, object>();
            param.Add("pid", id);

            return DB.Get<PocoLocal>(sql, param, CommandType.Text).FirstOrDefault();
        }

        private bool Add(PocoLocal local)
        {
            var sql = @"
                insert into Local (Nome, Imagem, Agendavel) values (@pnome, @pimagem, @pagendavel);

                insert into UsuarioLocal (IdUsuario, IdLocal) values (@pidUsuario,  LAST_INSERT_ID());
            ";

            param = new Dictionary<string, object>();
            param.Add("pnome", local.Nome);
            param.Add("pimagem", local.Imagem);
            param.Add("pidUsuario", local.IdUsuario);
            param.Add("pagendavel", local.Agendavel);

            return DB.Save(sql, param) > 0;
        }

        private bool UpdateLocalUsuario(PocoLocal local)
        {
            var sql = @"
                update Local set 
                Status = @pstatus,
                IdUsuario = @pidUsuario
                where Id = @pid
            ";

            param = new Dictionary<string, object>();
            param.Add("pstatus", local.Status);
            param.Add("pidUsuario", local.IdUsuario);
            param.Add("pid", local.Id);

            return DB.Save(sql, param) > 0;
        }

        private void UpdateLocalXAgendamento(int idLocal)
        {
            var sql = @"
                UPDATE Local ll
                SET 
                    ll.Status = 0,
                    ll.IdUsuario = 0
                WHERE ll.Id = @pidlocal;

                UPDATE Local l
                LEFT JOIN Agendamento a on a.IdLocal = l.Id
                SET 
                    l.Status = 1,
                    l.IdUsuario = a.IdUsuario
                WHERE l.Id = @pidlocal
                AND Now() >= a.DataDe
                AND Now() <= a.DataAte;
            ";
            param = new Dictionary<string, object>();
            param.Add("pidlocal", idLocal);

            DB.Save(sql, param);
        }
    }
}