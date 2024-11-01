using Microsoft.Data.Sqlite;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Configuração de logging
builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuração da string de conexão
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=/data/sample.db";

// Verificação e criação do diretório do banco de dados
try
{
    var dbDirectory = Path.GetDirectoryName(connectionString.Replace("Data Source=", ""));
    
    if (string.IsNullOrEmpty(dbDirectory))
    {
        throw new InvalidOperationException("Diretório do banco de dados inválido.");
    }

    Directory.CreateDirectory(dbDirectory);
}
catch (Exception ex)
{
    Console.WriteLine($"Erro ao criar diretório do banco de dados: {ex.Message}");
}

// Método para criar conexão com tratamento de erros
static SqliteConnection CreateConnection(string connectionString)
{
    try
    {
        var connection = new SqliteConnection(connectionString);
        connection.Open();
        return connection;
    }
    catch (SqliteException ex)
    {
        Console.WriteLine($"Erro ao conectar ao banco de dados: {ex.Message}");
        throw;
    }
}

// Método para criar tabela
static void CreateTableIfNotExists(SqliteConnection connection)
{
    using var command = connection.CreateCommand();
    command.CommandText = @"
        CREATE TABLE IF NOT EXISTS Records (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            Age INTEGER NOT NULL,
            CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
        );
    ";
    command.ExecuteNonQuery();
}

var app = builder.Build();

// Inicialização do banco de dados
try
{
    using var connection = CreateConnection(connectionString);
    CreateTableIfNotExists(connection);
    app.Logger.LogInformation("Banco de dados inicializado com sucesso.");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Falha na inicialização do banco de dados.");
}

// Configurações do ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Endpoint de criação de registro
app.MapPost("/create", (ILogger<Program> logger) =>
{
    try
    {
        var random = new Random();
        var name = $"User{random.Next(1, 1000)}";
        var age = random.Next(18, 100);
        
        using var connection = CreateConnection(connectionString);
        
        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Records (Name, Age) 
            VALUES (@name, @age);
            SELECT last_insert_rowid();
        ";
        
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@age", age);
        
        var newId = Convert.ToInt32(command.ExecuteScalar());
        
        return Results.Ok(new { 
            Id = newId, 
            Message = "Record created", 
            Name = name, 
            Age = age 
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erro ao criar um novo registro.");
        return Results.Problem("Erro ao criar o registro.");
    }
});

// Endpoint de leitura do último registro
app.MapGet("/read", (ILogger<Program> logger) =>
{
    try
    {
        using var connection = CreateConnection(connectionString);
        
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Records ORDER BY Id DESC LIMIT 1;";
        
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            var id = reader.GetInt32(0);
            var name = reader.GetString(1);
            var age = reader.GetInt32(2);
            
            logger.LogInformation(
                "Último registro lido com sucesso: Id={Id}, Name={Name}, Age={Age}", 
                id, name, age
            );
            
            return Results.Ok(new { 
                Id = id, 
                Name = name, 
                Age = age
            });
        }

        logger.LogWarning("Nenhum registro encontrado.");
        return Results.NotFound("No record found");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erro ao ler o último registro.");
        return Results.Problem("Erro ao ler o registro.");
    }
});

app.MapGet("/health", () => Results.Ok("Healthy")).AllowAnonymous();

app.Run();