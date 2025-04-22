using System.ComponentModel.DataAnnotations;

namespace AcessoSeguroWebAssembly.Models
{
    public class UsuarioLoginDTO
    {
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Email inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        public string Senha { get; set; } = string.Empty;
        //[Required(ErrorMessage = "O email é obrigatório")]
        //[EmailAddress(ErrorMessage = "Email inválido")]
        // public string Email { get; set; } = string.Empty;

        //[Required(ErrorMessage = "A senha é obrigatória")]
        // public string Senha { get; set; } = string.Empty;
    }
}