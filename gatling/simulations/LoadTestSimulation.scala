import io.gatling.core.Predef._
import io.gatling.http.Predef._
import scala.concurrent.duration._

class LoadTestSimulation extends Simulation {

  // Define a URL base usando a variável de ambiente BASE_URL ou localhost como padrão
  val baseUrl = sys.env.getOrElse("BASE_URL", "http://localhost:8080")

  // Configuração do protocolo HTTP
  val httpProtocol = http
    .baseUrl(baseUrl)
    .acceptHeader("application/json")

  // Cenário de teste de carga para o endpoint "/api/records/create"
  val scn = scenario("Violent Load Test - Create Only")
    .exec(
      http("Create Record")
        .post("/api/records/create") // Endpoint ajustado para o padrão de controller ASP.NET
        .check(status.is(200)) // Verifica se a resposta é 200 OK
    )

  // Configuração do plano de injeção de usuários simultâneos
  setUp(
    scn.inject(
      nothingFor(5.seconds), // Aguarda 5 segundos antes de começar, para garantir que o serviço esteja pronto
      atOnceUsers(1000) // Lança 10 usuários simultaneamente para criar registros
    )
  ).protocols(httpProtocol)
}