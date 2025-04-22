namespace AcessoSeguroWebAssembly.Models
{
    public class LoginResult
    {
        public string Token { get; set; } = string.Empty;
        public UsuarioInfo Usuario { get; set; } = new();
    }
}
