# Actualizar appsettings.Development.json con la conexion correcta
$jsonContent = @{
    ConnectionStrings = @{
        DefaultConnection = "Server=SERVERSIS\SQL2022;Database=SistemIA_Central;User Id=sa;Password=%L4V1CT0R14;TrustServerCertificate=True;"
    }
    Logging = @{
        LogLevel = @{
            Default = "Debug"
            "Microsoft.AspNetCore" = "Warning"
        }
    }
    ApiKeySettings = @{
        RequireApiKey = $true
    }
}
$jsonContent | ConvertTo-Json -Depth 3 | Set-Content -Path "c:\asis\SistemIA.ServidorCentral\appsettings.Development.json" -Encoding UTF8

# También actualizar appsettings.json principal
$jsonProd = @{
    ConnectionStrings = @{
        DefaultConnection = "Server=SERVERSIS\SQL2022;Database=SistemIA_Central;User Id=sa;Password=%L4V1CT0R14;TrustServerCertificate=True;"
    }
    Kestrel = @{
        Endpoints = @{
            Http = @{ Url = "http://0.0.0.0:5100" }
            Https = @{ Url = "https://0.0.0.0:5101" }
        }
    }
    Logging = @{
        LogLevel = @{
            Default = "Information"
            "Microsoft.AspNetCore" = "Warning"
        }
    }
    ApiKeySettings = @{
        RequireApiKey = $true
    }
    AllowedHosts = "*"
}
$jsonProd | ConvertTo-Json -Depth 4 | Set-Content -Path "c:\asis\SistemIA.ServidorCentral\appsettings.json" -Encoding UTF8

Write-Host "Archivos de configuracion actualizados!" -ForegroundColor Green
Get-Content "c:\asis\SistemIA.ServidorCentral\appsettings.Development.json"
