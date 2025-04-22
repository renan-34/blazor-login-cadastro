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

        public async Task<bool> LoginAsync(UsuarioLoginDTO dto)
        {
            var response = await _http.PostAsJsonAsync("api/usuarios/login", dto);

            if (!response.IsSuccessStatusCode)
                return false;

            var loginResult = await response.Content.ReadFromJsonAsync<LoginResult>();

            if (loginResult is null || string.IsNullOrEmpty(loginResult.Token))
                return false;

            Token = loginResult.Token;
            UsuarioLogado = loginResult.Usuario;

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", Token);

            return true;
        }

        public void Logout()
        {
            Token = null;
            UsuarioLogado = null;
            _http.DefaultRequestHeaders.Authorization = null;
        }
    }
}
