# Usa uma imagem base para Gatling
FROM denvazh/gatling

# Define o diretório de trabalho
WORKDIR /opt/gatling

# Copia o arquivo de simulação Gatling para o container
COPY simulations /opt/gatling/user-files/simulations

# Executa o Gatling com a simulação especificada
ENTRYPOINT ["bin/gatling.sh", "-s", "LoadTestSimulation"]