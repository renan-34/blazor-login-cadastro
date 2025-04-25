// Serviço responsável por gerar tokens JWT para autenticação

using System.IdentityModel.Tokens.Jwt; // Manipulação de tokens JWT
using System.Security.Claims; // Uso de claims (informações do usuário no token)
using System.Text; // Para codificação da chave secreta
using AcessoSeguro.Models; // Model 'Usuario' usado na geração do token
using Microsoft.IdentityModel.Tokens; // Tipos para segurança e criptografia do token

namespace AcessoSeguro.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;

        // Construtor que recebe o IConfiguration para acessar as configurações do appsettings.json
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Método responsável por gerar um token JWT com base nas informações do usuário
        public string GerarToken(Usuario usuario)
        {
            // Obtém a seção "Jwt" do appsettings.json
            var jwtSettings = _configuration.GetSection("Jwt");

            // Converte a chave secreta em array de bytes
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

            // Cria o manipulador de tokens JWT
            var tokenHandler = new JwtSecurityTokenHandler();

            // Define a descrição do token que será gerado
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                // Define as informações (claims) que estarão dentro do token
                Subject = new ClaimsIdentity(new[]
                {
                    // Claim com o ID do usuário (NameIdentifier é o padrão para identificação do usuário)
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),

                    // Claim com o e-mail do usuário
                    new Claim(ClaimTypes.Email, usuario.Email),

                    // Claim com o nome do usuário
                    new Claim(ClaimTypes.Name, usuario.Nome),

                    // Claim com o tipo/perfil do usuário (ex: "Admin", "Cliente", etc.)
                    new Claim(ClaimTypes.Role, usuario.TipoUsuario)
                }),

                // Define o tempo de validade do token (expira em 1 hora)
                Expires = DateTime.UtcNow.AddHours(1),

                // Emissor do token (opcional)
                Issuer = jwtSettings["Issuer"],

                // Público-alvo do token (opcional)
                Audience = jwtSettings["Audience"],

                // Define a assinatura do token com a chave secreta e algoritmo HMAC SHA256
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), // Chave de segurança simétrica
                    SecurityAlgorithms.HmacSha256Signature // Algoritmo de assinatura
                )
            };

            // Cria o token com base nas configurações acima
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Converte o token para string e retorna (ex: "eyJhbGciOiJIUzI1NiIsInR5cCI6...")
            return tokenHandler.WriteToken(token);
        }
    }
}
