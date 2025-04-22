using AcessoSeguroWebAssembly.Models;

namespace AcessoSeguroWebAssembly.Models
{
    public class UsuarioInfo
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string TipoUsuario { get; set; } = string.Empty;
    }
}