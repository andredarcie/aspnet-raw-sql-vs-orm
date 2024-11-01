docker-compose up --build

Set-ExecutionPolicy RemoteSigned

.\TesteApi.ps1

docker-compose -f docker-compose.yml up --build

docker-compose -f docker-compose.monitoring.yml up --build
