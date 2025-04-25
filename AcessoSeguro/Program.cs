// Importação dos namespaces necessários para a aplicação
using System.Text;                            // Para codificar a chave JWT
using AcessoSeguro.Components;                 // Componentes Razor do projeto
using AcessoSeguro.Services;                  // Serviços personalizados (ex: autenticação)
using Microsoft.AspNetCore.Authentication.JwtBearer; // Middleware para autenticação JWT
using Microsoft.IdentityModel.Tokens;         // Validação e geração de tokens JWT
using Microsoft.OpenApi.Models;               // Swagger para documentação da API

// Cria o builder da aplicação Web
var builder = WebApplication.CreateBuilder(args);

// 🔐 CONFIGURAÇÃO DE AUTENTICAÇÃO JWT
// Obtém a chave secreta do JWT do arquivo appsettings.json
var jwtKey = builder.Configuration["Jwt:Key"];
// Converte a chave para um array de bytes, necessária para a assinatura do token
var key = Encoding.ASCII.GetBytes(jwtKey!);

// Registra o serviço de autenticação usando o esquema JWT Bearer
builder.Services.AddAuthentication(options =>
{
    // Define o esquema padrão de autenticação e desafio como JWT
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true; // Exige HTTPS para segurança
    options.SaveToken = true; // Armazena o token após a autenticação
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true, // Valida a assinatura do token
        IssuerSigningKey = new SymmetricSecurityKey(key), // Define a chave para validar o token
        ValidateIssuer = false, // Desativa validação do emissor
        ValidateAudience = false // Desativa validação do público-alvo
    };
});

// 🔐 REGISTRA A AUTORIZAÇÃO
builder.Services.AddAuthorization(); // Habilita uso de [Authorize] nos endpoints

// ✅ REGISTRO DE SERVIÇOS CUSTOMIZADOS
builder.Services.AddScoped<UsuarioService>(); // Serviço para cadastro/login de usuários
builder.Services.AddScoped<TokenService>();   // Serviço responsável por gerar tokens JWT

// 🌐 CONFIGURAÇÃO DO CORS (Cross-Origin Resource Sharing)
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        //front                  // swagger/backend
        // policy.WithOrigins( "https://localhost:7037", "https://localhost:7172/")
        policy.WithOrigins("http://localhost:5260", "https://localhost:7037", "https://localhost:7172")
               // Permite requisições apenas desse endereço (Blazor WASM)
               .AllowAnyHeader()     // Aceita qualquer cabeçalho (ex: Authorization)
              .AllowAnyMethod()     // Permite todos os métodos HTTP (GET, POST, PUT, etc.)
              .AllowCredentials();  // Permite envio de cookies e tokens
    });
});

// 🧱 REGISTRO DOS COMPONENTES RAZOR DO SERVIDOR
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(); // Permite componentes interativos no servidor (Blazor Server)

// REGISTRO DOS CONTROLLERS (API REST)
builder.Services.AddControllers(); // Permite usar controllers [ApiController]

// 📘 CONFIGURAÇÃO DO SWAGGER (documentação da API)
builder.Services.AddEndpointsApiExplorer(); // Explora os endpoints automaticamente
builder.Services.AddSwaggerGen(c =>
{
    // Cria a versão 1 da documentação da API
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AcessoSeguro API", Version = "v1" });

    // Define a autenticação JWT no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Authorization: Bearer {seu_token}\"",
        Name = "Authorization", // Nome do cabeçalho
        In = ParameterLocation.Header, // Onde o token será enviado
        Type = SecuritySchemeType.ApiKey, // Tipo do esquema de segurança
        Scheme = "Bearer"
    });

    // Adiciona o requisito de segurança para todas as rotas
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {} // Escopos (nenhum específico)
        }
    });
});

// 💥 CONSTRUÇÃO DA APLICAÇÃO (gera o pipeline final)
var app = builder.Build();

// 🌍 CONFIGURAÇÃO DE MIDDLEWARES
if (!app.Environment.IsDevelopment())
{
    // Em ambiente de produção, redireciona para página de erro genérica
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts(); // Força HTTPS com cabeçalho de segurança HSTS
}
else
{
    // Em desenvolvimento, habilita Swagger para teste da API
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // Define o endpoint e o título da UI do Swagger
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AcessoSeguro API v1");
        c.RoutePrefix = "swagger"; // Acesso via /swagger
    });
}

app.UseHttpsRedirection(); // Redireciona HTTP para HTTPS automaticamente

// 🧠 APLICA CORS (deve vir antes da autenticação)
app.UseCors("PermitirFrontend");

app.UseAuthentication(); // Ativa a autenticação JWT
app.UseAuthorization();  // Ativa a autorização ([Authorize])

app.UseAntiforgery(); // Protege contra ataques CSRF em formulários

// Mapeia arquivos estáticos (wwwroot)
app.MapStaticAssets();

// Mapeia componentes Razor (Blazor Server)
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

// Mapeia os endpoints dos controllers da API
app.MapControllers();

// 🚀 Inicia a aplicação
app.Run();

