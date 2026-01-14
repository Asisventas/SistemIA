# Script para calcular el hash del QR - PRUEBAS con IdCSC
$cdc = "01004952197001002000007120260110634465737"
$digestHex = "35686859635a79364156783648566c48414c4b4c79345471764f4e42616c4b5a6a30526932416c786c633d"
$hashAprobado = "f34bdd20371b2d9d5ea8d67a41b997a167ca86beffe8d7fa8eb002c5494c0226"
$csc = "ABCD0000000000000000000000000000"
$fechaHex = "323032362d30312d31305430303a30303a3030"
$rucRec = "80033703"
$totOpe = "1000"
$totIVA = "0"
$cItems = "1"

$sha = [System.Security.Cryptography.SHA256]::Create()

# PRUEBA 1: IdCSC=1 (lo que hacemos actualmente)
Write-Host "=== PRUEBA 1: IdCSC=1 (TrimStart ceros) ==="
$params1 = "nVersion=150&Id=$cdc&dFeEmiDE=$fechaHex&dRucRec=$rucRec&dTotGralOpe=$totOpe&dTotIVA=$totIVA&cItems=$cItems&DigestValue=$digestHex&IdCSC=1"
$hash1 = [BitConverter]::ToString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($params1 + $csc))).Replace("-","").ToLower()
Write-Host "Hash: $hash1"

# PRUEBA 2: IdCSC=0001 (como el manual)
Write-Host ""
Write-Host "=== PRUEBA 2: IdCSC=0001 (con ceros) ==="
$params2 = "nVersion=150&Id=$cdc&dFeEmiDE=$fechaHex&dRucRec=$rucRec&dTotGralOpe=$totOpe&dTotIVA=$totIVA&cItems=$cItems&DigestValue=$digestHex&IdCSC=0001"
$hash2 = [BitConverter]::ToString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($params2 + $csc))).Replace("-","").ToLower()
Write-Host "Hash: $hash2"

# PRUEBA 3: Con nVersion=142 (version vieja)
Write-Host ""
Write-Host "=== PRUEBA 3: nVersion=142, IdCSC=0001 ==="
$params3 = "nVersion=142&Id=$cdc&dFeEmiDE=$fechaHex&dRucRec=$rucRec&dTotGralOpe=$totOpe&dTotIVA=$totIVA&cItems=$cItems&DigestValue=$digestHex&IdCSC=0001"
$hash3 = [BitConverter]::ToString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($params3 + $csc))).Replace("-","").ToLower()
Write-Host "Hash: $hash3"

Write-Host ""
Write-Host "Hash aprobado: $hashAprobado"
Write-Host ""
Write-Host "Match? P1: $($hash1 -eq $hashAprobado), P2: $($hash2 -eq $hashAprobado), P3: $($hash3 -eq $hashAprobado)"
