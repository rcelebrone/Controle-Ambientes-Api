using System;
using System.Linq;
using System.Web.Mvc;
using TemGente.Models;
using TemGente.Models.Poco;
using TemGente.Models.Enumeradores;
using System.Security;

[assembly: AllowPartiallyTrustedCallers]
namespace TemGente.Controllers
{
    public class ApiController : Controller
    {
        private string ErroRequisicaoNegada
        {
            get { return "Sua requisição negada"; }
        }

        // POST: /Api/Autenticacao
        [HttpPost]
        [Throttle(Name = "Autenticar", Seconds = 5)]
        public JsonResult Autenticar(PocoUsuario poco)
        {
            ApiResposta resposta = new ApiResposta();
            try
            {
                if(poco.ValidaEmail && poco.ValidaSenha)
                {
                    Usuario usuario = new Usuario();
                    usuario.Poco = poco;
                    var retorno = usuario.Autenticar();
                    resposta.Retorno = (retorno ? Retornos.verdadeiro : Retornos.falso).ToString();
                    resposta.Mensagem = usuario.Mensagem;
                    resposta.Id = usuario.Id;
                }
                else
                {
                    throw new Exception(ErroRequisicaoNegada);
                }
            }
            catch (Exception e)
            {
                resposta.Retorno = Retornos.falso.ToString();
                resposta.Mensagem = "Autenticar o usuário : " + e.Message;
                Util.LogError(e);
            }

            return Json(resposta, "application/json", JsonRequestBehavior.AllowGet);
        }

        // POST: /Api/CriarUsuario
        [HttpPost]
        [Throttle(Name = "CriarUsuario", Seconds = 600)]
        public JsonResult CriarUsuario(PocoUsuario poco)
        {
            ApiResposta resposta = new ApiResposta();
            try
            {
                if (poco.ValidaEmail && poco.ValidaSenha)
                {
                    Usuario usuario = new Usuario();
                    usuario.Poco = poco;
                    var retorno = usuario.Autenticar(true);
                    resposta.Retorno = (retorno ? Retornos.verdadeiro : Retornos.falso).ToString();
                    resposta.Mensagem = usuario.Mensagem;
                    resposta.Id = usuario.Id;
                }
                else
                {
                    throw new Exception(ErroRequisicaoNegada);
                }

            }
            catch (Exception e)
            {
                resposta.Retorno = Retornos.falso.ToString();
                resposta.Mensagem = "Criar usuário : " + e.Message;
                //se acontecer um erro, vou remover o controle de acesso para liberar uma proxima tentativa
                System.Web.HttpRuntime.Cache.Remove("CriarUsuario-"+Request.UserHostAddress);
                Util.LogError(e);
            }

            return Json(resposta, "application/json", JsonRequestBehavior.AllowGet);
        }

        // POST: /Api/CriarLocal
        [HttpPost]
        [Throttle(Name = "CriarLocal", Seconds = 20)]
        public JsonResult CriarLocal(PocoLocal poco)
        {
            ApiResposta resposta = new ApiResposta();
            try
            {
                if (poco.ValidaNome)
                {
                    Local local = new Local();
                    local.Poco = poco;
                    var retorno = local.Adicionar();
                    resposta.Retorno = (retorno ? Retornos.verdadeiro : Retornos.falso).ToString();
                    resposta.Mensagem = local.Mensagem;
                }
                else
                {
                    throw new Exception(ErroRequisicaoNegada);
                }
            }
            catch (Exception e)
            {
                resposta.Retorno = Retornos.falso.ToString();
                resposta.Mensagem = "Criar loca : " + e.Message;
                Util.LogError(e);
            }

            return Json(resposta, "application/json", JsonRequestBehavior.AllowGet);
        }

        // POST: /Api/OcuparLocal
        [HttpPost]
        [Throttle(Name = "OcuparLocal", Seconds = 5)]
        public JsonResult OcuparLocal(PocoLocal poco, int IdUsuarioOcupar, DateTime? dataDe, DateTime? dataAte)
        {
            ApiResposta resposta = new ApiResposta();
            try
            {
                Local local = new Local();
                local.Poco = poco;
                local.DataDe = dataDe;
                local.DataAte = dataAte;
                var retorno = local.Ocupar(IdUsuarioOcupar);
                resposta.Retorno = (retorno ? Retornos.verdadeiro : Retornos.falso).ToString();
                resposta.Mensagem = local.Mensagem;
            }
            catch (Exception e)
            {
                resposta.Retorno = Retornos.falso.ToString();
                resposta.Mensagem = "Ocupar local: " + e.Message;
                Util.LogError(e);
            }

            return Json(resposta, "application/json", JsonRequestBehavior.AllowGet);
        }

        // POST: /Api/DesocuparLocal
        [HttpPost]
        [Throttle(Name = "DesocuparLocal", Seconds = 5)]
        public JsonResult DesocuparLocal(PocoLocal poco, int IdUsuarioDesocupar)
        {
            ApiResposta resposta = new ApiResposta();
            try
            {
                Local local = new Local();
                local.Poco = poco;
                var retorno = local.Desocupar(IdUsuarioDesocupar);
                resposta.Retorno = (retorno ? Retornos.verdadeiro : Retornos.falso).ToString();
                resposta.Mensagem = local.Mensagem;
            }
            catch (Exception e)
            {
                resposta.Retorno = Retornos.falso.ToString();
                resposta.Mensagem = "Desocupar local : " + e.Message;
                Util.LogError(e);
            }

            return Json(resposta, "application/json", JsonRequestBehavior.AllowGet);
        }

        // POST: /Api/AdicionarNoLocal
        [HttpPost]
        [Throttle(Name = "AdicionarNoLocal", Seconds = 10)]
        public JsonResult AdicionarNoLocal(string EmailUsuario, int IdLocal)
        {
            ApiResposta resposta = new ApiResposta();
            try
            {
                UsuarioLocal ub = new UsuarioLocal();
                ub.IdLocal = IdLocal;
                var retorno = ub.Vincular(EmailUsuario);
                resposta.Retorno = (retorno ? Retornos.verdadeiro : Retornos.falso).ToString();
                resposta.Mensagem = ub.Mensagem;
            }
            catch (Exception e)
            {
                resposta.Retorno = Retornos.falso.ToString();
                resposta.Mensagem = "Adicionar no local : " + e.Message;
                Util.LogError(e);
            }

            return Json(resposta, "application/json", JsonRequestBehavior.AllowGet);
        }

        // POST: /Api/SairDoLocal
        [HttpPost]
        [Throttle(Name = "SairDoLocal", Seconds = 30)]
        public JsonResult SairDoLocal(int IdUsuario, int IdLocal)
        {
            ApiResposta resposta = new ApiResposta();
            try
            {
                try
                {
                    Local local = new Local();
                    local.Poco = new PocoLocal() { Id = IdLocal, IdUsuario = IdUsuario };
                    local.Desocupar(IdUsuario);
                }
                catch { }

                UsuarioLocal ub = new UsuarioLocal() { IdLocal = IdLocal, IdUsuario = IdUsuario };
                var retorno = ub.Desvincular();
                resposta.Retorno = (retorno ? Retornos.verdadeiro : Retornos.falso).ToString();
                resposta.Mensagem = ub.Mensagem;
            }
            catch (Exception e)
            {
                resposta.Retorno = Retornos.falso.ToString();
                resposta.Mensagem = "Sair do local : " + e.Message;
                Util.LogError(e);
            }

            return Json(resposta, "application/json", JsonRequestBehavior.AllowGet);
        }

        // POST: /Api/RemoverAgendamentos
        [HttpPost]
        [Throttle(Name = "RemoverAgendamentos", Seconds = 10)]
        public JsonResult RemoverAgendamentos(int IdUsuario, int IdLocal)
        {
            ApiResposta resposta = new ApiResposta();
            try
            {
                Agendamento agendamento = new Agendamento(IdLocal, IdUsuario);
                var retorno = agendamento.Remover();
                resposta.Retorno = (retorno ? Retornos.verdadeiro : Retornos.falso).ToString();
                resposta.Mensagem = agendamento.Mensagem;
            }
            catch (Exception e)
            {
                resposta.Retorno = Retornos.falso.ToString();
                resposta.Mensagem = "Remover agendamento : " + e.Message;
                Util.LogError(e);
            }

            return Json(resposta, "application/json", JsonRequestBehavior.AllowGet);
        }

        // GET: /Api/VerificaSeLocalEstaOcupado
        [HttpGet]
        public JsonResult VerificaSeLocalEstaOcupado(int IdLocal)
        {
            ApiResposta resposta = new ApiResposta();
            try
            {
                Local local = new Local() { Poco = new PocoLocal() { Id = IdLocal } };

                var carregado = local.Carregar();

                if(carregado == null)
                    throw new Exception(local.Mensagem);

                var agendavel = carregado.Agendavel == 1;
                var ocupado = carregado.Status == 1;

                //esta ocupado e é agendavel
                if (ocupado && agendavel)
                    resposta.Retorno = Retornos.ocupado_agendavel.ToString();
                //está livre e é agendavel
                else if (!ocupado && agendavel)
                    resposta.Retorno = Retornos.livre_agendavel.ToString();
                //está ocupado e não é agendavel
                else if (ocupado && !agendavel)
                    resposta.Retorno = Retornos.ocupado_simples.ToString();
                //está livre e não é agendavel
                else if (!ocupado && !agendavel)
                    resposta.Retorno = Retornos.livre_simples.ToString();

                resposta.Mensagem = local.Mensagem;
            }
            catch (Exception e)
            {
                resposta.Retorno = Retornos.falso.ToString();
                resposta.Mensagem = "Verifica local : " + e.Message;
                Util.LogError(e);
            }

            return Json(resposta, "application/json", JsonRequestBehavior.AllowGet);
        }

        // GET: /Api/ListaLocais
        [HttpGet]
        public JsonResult ListaLocais(int IdUsuario)
        {
            ApiResposta resposta = new ApiResposta();
            try
            {
                UsuarioLocal ub = new UsuarioLocal();
                ub.IdUsuario = IdUsuario;

                resposta.Id = IdUsuario;
                resposta.Lista = ub.Locais();
                if (resposta.Lista != null)
                {
                    var retorno = resposta.Lista.Count() > 0;
                    resposta.Retorno = (retorno ? Retornos.verdadeiro : Retornos.falso).ToString();
                }
                resposta.Mensagem = ub.Mensagem;
            }
            catch (Exception e)
            {
                resposta.Retorno = Retornos.falso.ToString();
                resposta.Mensagem = "Lista locais : " + e.Message;
                Util.LogError(e);
            }

            return Json(resposta, "application/json", JsonRequestBehavior.AllowGet);
        }

        // GET: /Api/ValidaHash
        [HttpGet]
        public ContentResult ValidaHash(string email, string hash)
        {
            var validado = Usuario.ValidaHash(email, hash);
            var retorno = string.Empty;
            if (validado)
            {
                retorno = "Acesso liberado com sucesso para o e-mail " + email;
            }
            else
            {
                retorno = "Ops! Algo terrivel aconteceu, essa URL não é valida. :'(";
            }

            return Content(retorno);
        }
        
    }
}
