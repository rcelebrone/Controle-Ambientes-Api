using System.Collections.Generic;
using System.Data;
using System.Linq;
using TemGente.Models.Database;
using TemGente.Models.Poco;

namespace TemGente.Models
{
    public class UsuarioLocal
    {

        /*
        * Propriedades com acesso via controller
        */
        public string Mensagem { get; set; }
        public int IdUsuario { get; set; }
        public int IdLocal { get; set; }

        /*
        * Métodos com acesso via controller
        */
        internal bool Vincular(string email)
        {
            //verificando se o ID do usuário foi informado.
            if (string.IsNullOrEmpty(email))
            {
                Mensagem = "Estranho!!! Email do usuário parece não ter sido informado." + Util.Alerta;
                return false;
            }

            //verificando se o ID do local foi informado.
            if (IdLocal == 0)
            {
                Mensagem = "Estranho!!! Não conseguimos identificar o local." + Util.Alerta;
                return false;
            }

            var usuario = new Usuario().Get(email);

            if (usuario != null)
            {
                var cadastrado = Add(email, IdLocal);
                if (cadastrado)
                    Mensagem = "Usuário do email [" + email + "] foi adicionado ao local com sucesso";
                else
                    Mensagem = "Não foi possível vincular o usuário do email [" + email + "] a esse local, pode ser que ele já seja um usuário desse local. " + Util.Alerta;

                return cadastrado;
            }
            else {
                var conteudo = "Olá " + email + ", um dos nossos usuários está te convidando para utilizar o aplicativo " + Util.Config("App:Nome") + ". Para conhecer esse aplicativo, acesse " + Util.Config("Api:Url") + " ou <a href='" + Util.Config("Api:Url") + "' target='_blank'>clique aqui</a>. Caso já conheça o aplicativo " + Util.Config("App:Nome") + " e deseja instalar, você pode <a href='" + Util.Config("App:Url:GooglePlay") + "' target='_blank'>clicar aqui para baixar a versão do Android na Google Play</a> ou <a href='" + Util.Config("App:Url:AppleStore") + "' target='_blank'>clicar aqui para baixar a versão do iPhone na Apple Store</a>. Estamos te aguardando :)";

                Util.EnviaEmail(email, "Convite para download do aplicativo " + Util.Config("App:Nome") + ".", conteudo);

                Mensagem = "Não encontramos o email ["+email+"] em nossa base de dados, então enviamos um convite para esse email, com os links para download do aplicativo :D";

                return false;
            }
        }

        internal bool Desvincular()
        {
            //verificando se o ID do usuário foi informado.
            if (IdUsuario == 0)
            {
                Mensagem = "Estranho!!! Não conseguimos identificar o usuário." + Util.Alerta;
                return false;
            }

            //verificando se o ID do local foi informado.
            if (IdLocal == 0)
            {
                Mensagem = "Estranho!!! Não conseguimos identificar o local." + Util.Alerta;
                return false;
            }

            var removeu = Del(IdUsuario, IdLocal);
            if (removeu)
                Mensagem = "Usuário saiu do local";
            else
                Mensagem = "Não foi possível sair do local, pode ser que você não seja usuário desse local. " + Util.Alerta;

            return removeu;
        }

        internal List<PocoLocal> Locais()
        {
            //verificando se o ID do usuário foi informado.
            if (IdUsuario == 0)
            {
                Mensagem = "Estranho!!! Não conseguimos identificar o usuário." + Util.Alerta;
                return null;
            }

            var locais = Get(IdUsuario);
            if (locais != null && locais.Count() > 0)
            {
                Mensagem = "Lista dos locais do usuário";
                return locais.ToList();
            }
            else
            {
                Mensagem = "Usuário não possui locais";
                return null;
            }
        }

        /*
        * Métodos com acesso interno (privado)
        */
        private Dictionary<string, object> param;

        private IEnumerable<PocoLocal> Get(int idUsuario)
        {
            var sql = @"
                select b.Id,b.Nome,b.Imagem,b.DataHora
                from Local b
                inner join UsuarioLocal ub on ub.IdLocal = b.Id
                where ub.IdUsuario = @pidusuario
            ";
            param = new Dictionary<string, object>();
            param.Add("pidusuario", idUsuario);

            return DB.Get<PocoLocal>(sql, param, CommandType.Text);
        }

        private bool Add(string email, int idLocal)
        {
            var sql = @"
                insert into UsuarioLocal (IdUsuario, IdLocal) 
                values ((select Id from Usuario where Email = @pemail), @pidlocal)
            ";

            param = new Dictionary<string, object>();
            param.Add("pemail", email);
            param.Add("pidlocal", idLocal);

            return DB.Save(sql, param) > 0;
        }

        private bool Del(int idUsuario, int idLocal)
        {
            var sql = @"
                delete from UsuarioLocal where IdUsuario = @pidUsuario and IdLocal = @pidLocal;

                delete from Local where Id not in (select ub.IdLocal from UsuarioLocal ub);
            ";

            param = new Dictionary<string, object>();
            param.Add("pidUsuario", idUsuario);
            param.Add("pidLocal", idLocal);

            return DB.Save(sql, param) > 0;
        }
    }
}