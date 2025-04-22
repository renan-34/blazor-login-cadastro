// Importações de namespaces necessários
using AcessoSeguro.Models; // Modelos de dados da aplicação
using Microsoft.AspNetCore.Cryptography.KeyDerivation; // Para gerar hash da senha com PBKDF2
using System.Data.SqlClient; // Para acesso ao SQL Server
using System.Security.Cryptography; // Para geração de salt aleatório

namespace AcessoSeguro.Services
{
    public class UsuarioService
    {
        private readonly string _connectionString;

        // Construtor: recebe as configurações do appsettings.json e armazena a string de conexão
        public UsuarioService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        // Método assíncrono para cadastrar um novo usuário
        public async Task<Usuario> CadastrarUsuarioAsync(UsuarioCadastroDTO dto)
        {
            // Cria um novo objeto Usuario e preenche seus dados com base no DTO recebido
            var usuario = new Usuario
            {
                Nome = dto.Nome,
                Email = dto.Email,
                SenhaHash = GerarHashSenha(dto.Senha), // Gera hash seguro da senha
                DataCadastro = DateTime.Now,
                TipoUsuario = "Cliente", // Define tipo padrão como Cliente
                Ativo = true
            };

            // Se foi enviada uma foto, armazena a imagem e metadados
            if (dto.Foto != null)
            {
                using var memoryStream = new MemoryStream(); // Stream para leitura do arquivo
                await dto.Foto.CopyToAsync(memoryStream); // Copia o conteúdo do arquivo para o stream
                usuario.FotoPerfil = memoryStream.ToArray(); // Converte em array de bytes
                usuario.NomeFoto = dto.Foto.FileName; // Nome do arquivo
                usuario.TipoFoto = dto.Foto.ContentType; // Tipo MIME (image/jpeg, etc.)
            }

            // Cria e abre conexão com o banco de dados
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // Comando SQL para inserir um novo usuário e retornar o ID gerado
            using var cmd = new SqlCommand(@"
                INSERT INTO Usuarios (Nome, Email, SenhaHash, FotoPerfil, NomeFoto, TipoFoto, DataCadastro, Ativo, TipoUsuario)
                OUTPUT INSERTED.Id
                VALUES (@Nome, @Email, @SenhaHash, @FotoPerfil, @NomeFoto, @TipoFoto, @DataCadastro, @Ativo, @TipoUsuario)", conn);

            // Define os parâmetros e seus tipos para evitar SQL Injection
            cmd.Parameters.Add("@Nome", System.Data.SqlDbType.NVarChar).Value = usuario.Nome;
            cmd.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar).Value = usuario.Email;
            cmd.Parameters.Add("@SenhaHash", System.Data.SqlDbType.NVarChar).Value = usuario.SenhaHash;
            cmd.Parameters.Add("@FotoPerfil", System.Data.SqlDbType.VarBinary).Value =
                usuario.FotoPerfil ?? (object)DBNull.Value;
            cmd.Parameters.Add("@NomeFoto", System.Data.SqlDbType.NVarChar).Value =
                usuario.NomeFoto ?? (object)DBNull.Value;
            cmd.Parameters.Add("@TipoFoto", System.Data.SqlDbType.NVarChar).Value =
                usuario.TipoFoto ?? (object)DBNull.Value;
            cmd.Parameters.Add("@DataCadastro", System.Data.SqlDbType.DateTime).Value = usuario.DataCadastro;
            cmd.Parameters.Add("@Ativo", System.Data.SqlDbType.Bit).Value = usuario.Ativo;
            cmd.Parameters.Add("@TipoUsuario", System.Data.SqlDbType.NVarChar).Value = usuario.TipoUsuario;

            // Executa o comando e captura o ID do novo usuário
            try
            {
                usuario.Id = (int)await cmd.ExecuteScalarAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao cadastrar usuário: " + ex.Message, ex);
            }

            return usuario;
        }

        // Método assíncrono para autenticar um usuário por email e senha
        public async Task<Usuario?> AutenticarUsuarioAsync(string email, string senha)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // Busca o usuário pelo e-mail
            using var cmd = new SqlCommand("SELECT * FROM Usuarios WHERE Email = @Email", conn);
            cmd.Parameters.AddWithValue("@Email", email);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!reader.HasRows)
                return null; // Usuário não encontrado

            await reader.ReadAsync();

            // Verifica se o hash da senha corresponde ao digitado
            var senhaHashSalva = reader["SenhaHash"].ToString();
            if (senhaHashSalva == null || !VerificarSenha(senha, senhaHashSalva))
                return null; // Senha incorreta

            // Retorna objeto Usuario com dados do banco
            return new Usuario
            {
                Id = (int)reader["Id"],
                Nome = reader["Nome"].ToString()!,
                Email = reader["Email"].ToString()!,
                TipoUsuario = reader["TipoUsuario"].ToString()!,
                Ativo = (bool)reader["Ativo"],
                DataCadastro = (DateTime)reader["DataCadastro"]
            };
        }

        // Método privado para gerar hash de senha com salt usando PBKDF2
        private string GerarHashSenha(string senha)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16); // Gera salt aleatório de 16 bytes
            byte[] hash = KeyDerivation.Pbkdf2(
                password: senha,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000, // Quantidade de iterações (segurança)
                numBytesRequested: 32 // Tamanho do hash resultante
            );

            // Retorna o hash no formato Base64, incluindo o salt e o hash separados por ponto
            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        // Retorna um usuário pelo ID
        public async Task<Usuario?> ObterUsuarioPorIdAsync(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand("SELECT * FROM Usuarios WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!reader.HasRows)
                return null;

            await reader.ReadAsync();

            return new Usuario
            {
                Id = (int)reader["Id"],
                Nome = reader["Nome"].ToString()!,
                Email = reader["Email"].ToString()!,
                TipoUsuario = reader["TipoUsuario"].ToString()!,
                Ativo = (bool)reader["Ativo"],
                DataCadastro = (DateTime)reader["DataCadastro"]
            };
        }

        // Método privado que verifica se a senha digitada bate com o hash armazenado
        private bool VerificarSenha(string senhaDigitada, string senhaHashSalva)
        {
            try
            {
                // Divide a string do hash salvo em duas partes: salt e hash
                var partes = senhaHashSalva.Split('.');
                var salt = Convert.FromBase64String(partes[0]); // Decodifica o salt
                var hashSalvo = partes[1]; // Hash armazenado

                // Gera hash da senha digitada com o mesmo salt
                var hashDigitado = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: senhaDigitada,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 10000,
                    numBytesRequested: 32
                ));

                // Compara hash gerado com o armazenado
                return hashDigitado == hashSalvo;
            }
            catch
            {
                return false; // Em caso de erro, considera a senha inválida
            }
        }
    }
}
