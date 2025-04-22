using System.ComponentModel.DataAnnotations;

namespace AcessoSeguro.Models
{
    public class Usuario
    {
        // 🆔 Identificador único do usuário (chave primária)
        public int Id { get; set; }

        // 🧑 Nome completo do usuário (obrigatório, com no máximo 100 caracteres)
        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }

        // 📧 Email do usuário (obrigatório, validado como email e limitado a 100 caracteres)
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        // 🔐 Senha protegida como hash (obrigatória)
        [Required]
        public string SenhaHash { get; set; }

        // 🖼️ Foto de perfil armazenada como array de bytes (opcional)
        public byte[]? FotoPerfil { get; set; }

        // 📂 Nome original do arquivo da foto (opcional)
        public string? NomeFoto { get; set; }

        // 📄 Tipo MIME da foto (ex: image/jpeg, image/png) (opcional)
        public string? TipoFoto { get; set; }

        // 🕓 Data de cadastro do usuário (valor padrão: data e hora atual)
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        // ✅ Indica se o usuário está ativo (valor padrão: true)
        public bool Ativo { get; set; } = true;

        // 👤 Tipo do usuário (pode ser "Cliente", "Admin", etc. — valor padrão: "Cliente")
        public string TipoUsuario { get; set; } = "Cliente";
    }
}
