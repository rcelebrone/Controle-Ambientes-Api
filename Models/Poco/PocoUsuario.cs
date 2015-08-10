using System;
using System.Net.Mail;

namespace TemGente.Models.Poco
{
    public class PocoUsuario
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public DateTime DataHora { get; set; }
        public int Verificado { get; set; }

        public bool ValidaEmail
        {
            get
            {
                try
                {
                    var m = new MailAddress(Email);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public bool ValidaSenha
        {
            get
            {
                return !string.IsNullOrEmpty(Senha) && Senha.Length > 4;
            }
        }

        public bool ValidaNome
        {
            get
            {
                return !string.IsNullOrEmpty(Nome) && Nome.Length > 2;
            }
        }
    }
}