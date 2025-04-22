// Controlador de Usuários com comentários explicativos
using AcessoSeguro.Models;
using AcessoSeguro.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AcessoSeguro.Controllers
{
    [Authorize] // Exige autenticação por padrão para todos os endpoints
    [ApiController] // Indica que essa classe é uma API REST
    [Route("api/[controller]")] // Rota base: api/usuarios
    public class UsuariosController : ControllerBase
    {
        private readonly UsuarioService _usuarioService;
        private readonly TokenService _tokenService;

        public UsuariosController(UsuarioService usuarioService, TokenService tokenService)
        {
            _usuarioService = usuarioService;
            _tokenService = tokenService;
        }

        // Endpoint público para cadastro
        [AllowAnonymous] // Permite acesso sem autenticação
        [HttpPost("cadastrar")] // POST api/usuarios/cadastrar
        public async Task<IActionResult> Cadastrar([FromForm] UsuarioCadastroDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState); // Validação de dados do formulário

            var usuario = await _usuarioService.CadastrarUsuarioAsync(dto);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(usuario); // Retorna o objeto do usuário cadastrado
        }

        // Endpoint de login e geração de token JWT
        [Authorize] // Requer autenticação, mas é sobrescrito por AllowAnonymous
        [AllowAnonymous]
        [HttpPost("login")] // POST api/usuarios/login
        public async Task<IActionResult> Login([FromBody] UsuarioLoginDTO dto)
        {
            var usuario = await _usuarioService.AutenticarUsuarioAsync(dto.Email, dto.Senha);

            if (usuario == null)
                return Unauthorized("Credenciais inválidas."); // Caso o login falhe

            var token = _tokenService.GerarToken(usuario); // Gera o token JWT

            return Ok(new
            {
                token, // Retorna o token JWT
                usuario = new
                {
                    usuario.Id,
                    usuario.Nome,
                    usuario.Email,
                    usuario.TipoUsuario
                }
            });
        }

        // Endpoint protegido para obter o perfil do usuário autenticado
        [Authorize] // Requer token válido
        [HttpGet("perfil")] // GET api/usuarios/perfil
        public async Task<IActionResult> ObterPerfil()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier); // Recupera o ID do token

            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim.Value); // Converte ID para int
            var usuario = await _usuarioService.ObterUsuarioPorIdAsync(userId);

            if (usuario == null)
                return NotFound("Usuário não encontrado.");

            return Ok(new
            {
                usuario.Id,
                usuario.Nome,
                usuario.Email,
                usuario.TipoUsuario,
                usuario.DataCadastro
            });
        }
    }
}