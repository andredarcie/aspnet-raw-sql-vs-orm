using Npgsql;

namespace desafio.Services
{
    public class RecordsService
    {
        private readonly string _connectionString;
        private readonly ILogger<RecordsService> _logger;

        public RecordsService(string connectionString, ILogger<RecordsService> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        private NpgsqlConnection CreateConnection()
        {
            try
            {
                var connection = new NpgsqlConnection(_connectionString);
                connection.Open();
                return connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao conectar ao banco de dados PostgreSQL.");
                throw;
            }
        }

        public void InitializeDatabase()
        {
            _logger.LogInformation("Iniciando a criação da tabela 'Records' se não existir...");
            try
            {
                using var connection = CreateConnection();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Records (
                        Id SERIAL PRIMARY KEY,
                        Name TEXT NOT NULL,
                        Age INTEGER NOT NULL,
                        CreatedAt TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
                    );
                ";
                command.ExecuteNonQuery();
                _logger.LogInformation("Tabela 'Records' criada ou já existente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inicializar a tabela 'Records'.");
                throw;
            }
        }

        public int CreateRecord(string name, int age)
        {
            using var connection = CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Records (Name, Age) 
                VALUES (@name, @age) 
                RETURNING Id;
            ";
            command.Parameters.AddWithValue("name", name);
            command.Parameters.AddWithValue("age", age);
            
            var newId = Convert.ToInt32(command.ExecuteScalar());
            _logger.LogInformation("Novo registro criado com Id={Id}, Name={Name}, Age={Age}", newId, name, age);
            return newId;
        }

        public (int Id, string Name, int Age)? GetLastRecord()
        {
            using var connection = CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Records ORDER BY Id DESC LIMIT 1;";

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                var id = reader.GetInt32(0);
                var name = reader.GetString(1);
                var age = reader.GetInt32(2);

                _logger.LogInformation("Último registro lido com sucesso: Id={Id}, Name={Name}, Age={Age}", id, name, age);
                return (id, name, age);
            }

            _logger.LogWarning("Nenhum registro encontrado.");
            return null;
        }
    }
}
