using System.Net.Http.Headers;
using System.Net.Http.Json;
using AcessoSeguroWebAssembly.Models;
using Blazored.LocalStorage;

namespace AcessoSeguroWebAssembly.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;

        public string? Token { get; private set; }
        public UsuarioInfo? UsuarioLogado { get; private set; }

        public AuthService(HttpClient http, ILocalStorageService localStorage)
        {
            _http = http;
            _localStorage = localStorage;
        }

        public async Task<(bool sucesso, string mensagem)> LoginAsync(UsuarioLoginDTO dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/usuarios/login", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var mensagemErro = await response.Content.ReadAsStringAsync();
                    return (false, mensagemErro);
                }

                var loginResult = await response.Content.ReadFromJsonAsync<LoginResult>();

                if (loginResult is null || string.IsNullOrWhiteSpace(loginResult.Token))
                    return (false, "Resposta do servidor inválida. Token não encontrado.");

                // Armazena os dados em memória
                Token = loginResult.Token;
                UsuarioLogado = loginResult.Usuario;

                // Define o token no header para chamadas autenticadas
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", Token);

                // Persiste os dados no localStorage
                await _localStorage.SetItemAsync("authToken", Token);
                await _localStorage.SetItemAsync("usuarioLogado", UsuarioLogado);

                return (true, "Login realizado com sucesso.");
            }
            catch (Exception ex)
            {
                return (false, "Erro inesperado: " + ex.Message);
            }
        }

        public async Task LogoutAsync()
        {
            Token = null;
            UsuarioLogado = null;

            _http.DefaultRequestHeaders.Authorization = null;

            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("usuarioLogado");
        }

        public async Task InicializarAsync()
        {
            Token = await _localStorage.GetItemAsync<string>("authToken");
            UsuarioLogado = await _localStorage.GetItemAsync<UsuarioInfo>("usuarioLogado");

            if (!string.IsNullOrWhiteSpace(Token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", Token);
            }
        }
    }

    public class LoginResult
    {
        public string Token { get; set; }
        public UsuarioInfo Usuario { get; set; }
    }
    public class UsuarioInfo
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string TipoUsuario { get; set; }
    }

}
