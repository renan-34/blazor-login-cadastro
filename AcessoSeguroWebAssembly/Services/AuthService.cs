using System.Net.Http.Headers;
using System.Net.Http.Json;
using AcessoSeguroWebAssembly.Models;

namespace AcessoSeguroWebAssembly.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;

        public string? Token { get; private set; }
        public UsuarioInfo? UsuarioLogado { get; private set; }

        public AuthService(HttpClient http)
        {
            _http = http;
        }

        public async Task<(bool sucesso, string mensagem)> LoginAsync(UsuarioLoginDTO dto)
        {
            var response = await _http.PostAsJsonAsync("api/usuarios/login", dto);

            if (!response.IsSuccessStatusCode)
            {
                // Tenta ler a mensagem de erro como texto simples
                var mensagemErro = await response.Content.ReadAsStringAsync();
                return (false, mensagemErro);
            }

            var loginResult = await response.Content.ReadFromJsonAsync<LoginResult>();

            if (loginResult is null || string.IsNullOrEmpty(loginResult.Token))
                return (false, "Token inválido ou resposta malformada.");

            Token = loginResult.Token;
            UsuarioLogado = loginResult.Usuario;

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", Token);

            return (true, "Login realizado com sucesso.");
        }

        public void Logout()
        {
            Token = null;
            UsuarioLogado = null;
            _http.DefaultRequestHeaders.Authorization = null;
        }
    }
}
