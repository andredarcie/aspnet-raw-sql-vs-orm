using desafio.Services;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Configuração de logging
builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Criação do logger para o Program
var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();

// Log para verificar a string de conexão
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    + ";Pooling=true;Minimum Pool Size=20;Maximum Pool Size=500;Timeout=15;";

if (string.IsNullOrEmpty(connectionString))
{
    logger.LogError("A string de conexão 'DefaultConnection' está vazia ou não foi configurada.");
}
else
{
    logger.LogInformation("String de conexão obtida com sucesso.");
}

// Registrar o RecordsService no container de DI, passando a string de conexão
var recordsServiceLogger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<RecordsService>>();
var recordsService = new RecordsService(connectionString, recordsServiceLogger);
builder.Services.AddSingleton(recordsService);

// Inicialize o banco de dados uma vez na inicialização da aplicação
try
{
    recordsService.InitializeDatabase();
    logger.LogInformation("Banco de dados inicializado com sucesso.");
}
catch (Exception ex)
{
    logger.LogError(ex, "Erro ao inicializar o banco de dados.");
    throw;
}

var app = builder.Build();

// Configurações do ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
