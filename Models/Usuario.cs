using System.Collections.Generic;
using System.Data;
using System.Linq;
using TemGente.Models.Database;
using TemGente.Models.Poco;

namespace TemGente.Models
{
    public class Usuario
    {

        /*
         * Propriedades com acesso via controller
         */
        public int Id { get; set; }
        public string Mensagem { get; set; }
        public PocoUsuario Poco { get; set; }

        /*
        * Métodos com acesso via controller
        */
        internal bool Autenticar()
        {
            return Autenticar(false);
        }

        internal bool Autenticar(bool cadastrar)
        {
            PocoUsuario usuarioExterno = Poco;
            PocoUsuario usuarioInterno = Get(usuarioExterno.Email);

            //Email está cadastrado na base
            if (usuarioInterno != null)
            {
                //Senhas iguais
                if (usuarioExterno.Senha == usuarioInterno.Senha)
                {
                    //Usuário verificado
                    if (usuarioInterno.Verificado == 1)
                    {
                        Id = usuarioInterno.Id;
                        Mensagem =
                            "Usuário verificado";

                        return true;
                    }
                    //Usuário não verificado
                    else
                    {
                        var hash = Util.GerarHashMd5(usuarioInterno.Email + usuarioInterno.Senha);

                        var url = Util.Config("Api:Url") + "/ValidarAcesso?email=" + usuarioInterno.Email + "&hash=" + hash;

                        var conteudo = "Olá " + usuarioInterno.Nome + ", para validar seu acesso no aplicativo, por favor, <a href='" + url + "' target='_blank'>clique aqui</a> ou copie e cole a URL (" + url + ") no browser";

                        Util.EnviaEmail(usuarioInterno.Email, "Validar acesso no aplicativo", conteudo);

                        Mensagem =
                            "Usuário não verificado, acesse sua conta de email [" + usuarioExterno.Email + "] e clique no link que enviamos, para validar o seu acesso";

                        return false;
                    }
                }
                //Senhas diferentes
                else
                {
                    Mensagem =
                        "A senha informada não é valida, verifique os dados e tente novamente";
                    return false;
                }
            }
            //Usuário não existe
            else
            {
                //solicitação de cadastro de usuário
                if (cadastrar)
                {
                    if (!usuarioExterno.ValidaNome)
                    {
                        Mensagem =
                            "Informe o nome do usuário com 3 ou mais caracteres";
                        return false;
                    }

                    if (!usuarioExterno.ValidaEmail)
                    {
                        Mensagem =
                            "Email informado não é valido";
                        return false;
                    }

                    var cadastrou = Add(usuarioExterno);
                    if (cadastrou)
                    {

                        var hash = Util.GerarHashMd5(usuarioExterno.Email + usuarioExterno.Senha);

                        var url = Util.Config("Api:Url") + "/ValidarAcesso?email=" + usuarioExterno.Email + "&hash=" + hash;

                        var conteudo = "Olá " + usuarioExterno.Nome + ", seu cadastro foi realizado com sucesso. Para validar seu acesso no aplicativo, por favor, <a href='" + url + "' target='_blank'>clique aqui</a> ou copie e cole a URL (" + url + ") no browser";

                        Util.EnviaEmail(usuarioExterno.Email, "Cadastrado com sucesso. Confirme seu acesso.", conteudo);

                        Mensagem =
                            "Usuário cadastrado com sucesso, acesse sua conta de email [" + usuarioExterno.Email + "] e clique no link que enviamos, para validar o seu acesso";
                    }
                    else
                    {
                        Mensagem = "Estranho!!! Não foi possivel cadastrar o usuário [" + usuarioExterno.Email + "]." + Util.Alerta;
                    }

                    return false;
                }
                //tentou logar com usuário que não existe
                else
                {
                    Mensagem =
                        "Usuário informado não existe";
                    return false;
                }
            }
        }

        /*
        * Metodos staticos e publicos
        */
        public static bool ValidaHash(string emailExterna, string hashExterna)
        {
            var newUsuario = new Usuario();

            var oUsusario = newUsuario.Get(emailExterna);

            if (oUsusario != null)
            {
                var hashInterna = Util.GerarHashMd5(oUsusario.Email + oUsusario.Senha);

                if (hashInterna == hashExterna)
                {
                    return newUsuario.UpdateStatus(oUsusario.Id, 1);
                }
            }

            return false;

        }

        /*
        * Métodos com acesso interno (privado)
        */
        private Dictionary<string, object> param;

        public PocoUsuario Get(string email)
        {
            var sql = @"
                select Id,Nome,Email,Senha,DataHora,Verificado 
                from Usuario 
                where Email = @pemail
            ";
            param = new Dictionary<string, object>();
            param.Add("pemail", email);

            return DB.Get<PocoUsuario>(sql, param, CommandType.Text).FirstOrDefault();
        }

        public PocoUsuario Get(int id)
        {
            var sql = @"
                select Id,Nome,Email,Senha,DataHora,Verificado 
                from Usuario 
                where Id = @pid
            ";
            param = new Dictionary<string, object>();
            param.Add("pid", id);

            return DB.Get<PocoUsuario>(sql, param, CommandType.Text).FirstOrDefault();
        }

        private bool Add(PocoUsuario usuario)
        {
            var sql = @"
                insert into Usuario (Nome,Email,Senha,DataHora) 
                values (@pnome,@pemail,@psenha,Now())
            ";

            param = new Dictionary<string, object>();
            param.Add("pnome", usuario.Nome);
            param.Add("pemail", usuario.Email);
            param.Add("psenha", usuario.Senha);

            return DB.Save(sql, param) > 0;
        }

        private bool UpdateStatus(int id, int verificado)
        {
            var sql = @"
                update Usuario 
                    set Verificado = @pverificado
                where Id = @pid
            ";

            param = new Dictionary<string, object>();
            param.Add("pid", id);
            param.Add("pverificado", verificado);

            return DB.Save(sql, param) > 0;
        }
    }
}