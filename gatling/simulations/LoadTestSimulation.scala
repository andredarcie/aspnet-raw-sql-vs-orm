import io.gatling.core.Predef._
import io.gatling.http.Predef._
import scala.concurrent.duration._

class LoadTestSimulation extends Simulation {

  val baseUrl = sys.env.getOrElse("BASE_URL", "http://localhost:8080")

  val httpProtocol = http
    .baseUrl(baseUrl)
    .acceptHeader("application/json")

  val scn = scenario("Violent Load Test - Create Only")
    .exec(
      http("Create Record")
        .post("/create")
        .check(status.is(200))
    )

  setUp(
    scn.inject(
      atOnceUsers(1000) // Lança 1000 usuários simultaneamente para criar registros
    )
  ).protocols(httpProtocol) 
}