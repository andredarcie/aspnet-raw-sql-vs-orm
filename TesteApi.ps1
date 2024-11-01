# Função para imprimir cabeçalho
function Write-Header {
    param([string]$Title)
    Write-Host "`n=== $Title ===" -ForegroundColor Cyan
}

# Função para imprimir o resultado formatado
function Write-Result {
    param([Parameter(Mandatory=$true)][psobject]$Response)
    $Response | Format-List
}

# Função para exibir detalhes de erro
function Write-ErrorDetails {
    param([Parameter(Mandatory=$true)][System.Management.Automation.ErrorRecord]$ErrorRecord)
    Write-Host "`nErro durante a execução:" -ForegroundColor Red
    Write-Host $ErrorRecord.Exception.Message -ForegroundColor Red
    
    if ($ErrorRecord.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($ErrorRecord.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        $reader.DiscardBufferedData()
        $responseBody = $reader.ReadToEnd()
        Write-Host "Detalhes da Resposta de Erro:" -ForegroundColor Yellow
        Write-Host $responseBody -ForegroundColor Yellow
    }
}

# Configurações da API
$BaseUrl = "http://localhost:8080/api/records"
$SleepIntervalSeconds = 1

try {
    # Teste de criação de registro
    Write-Header "Criando Novo Registro"
    $responseCreate = Invoke-RestMethod -Uri "$BaseUrl/create" -Method Post -ErrorAction Stop
    Write-Result $responseCreate

    # Espera configurável para garantir que o registro esteja disponível
    Start-Sleep -Seconds $SleepIntervalSeconds

    # Teste de leitura do último registro
    Write-Header "Lendo Último Registro"
    $responseRead = Invoke-RestMethod -Uri "$BaseUrl/read" -Method Get -ErrorAction Stop
    Write-Result $responseRead

    Write-Host "`nTestes concluídos com sucesso!" -ForegroundColor Green
}
catch {
    Write-Host "Entrou no bloco de erro" -ForegroundColor Red
    Write-ErrorDetails -ErrorRecord $_
}
