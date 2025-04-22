namespace AcessoSeguro.Models
{
    public class UsuarioCadastroDTO
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }

        public IFormFile? Foto { get; set; }
    }
}
