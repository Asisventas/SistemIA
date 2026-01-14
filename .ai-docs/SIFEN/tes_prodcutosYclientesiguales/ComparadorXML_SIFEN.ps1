# =============================================================================
# COMPARADOR XML SIFEN - SistemIA vs Sistema Power
# Compara facturas con MISMO cliente/productos para encontrar diferencias
# =============================================================================

Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "COMPARADOR XML SIFEN - Factura RECHAZADA (Local) vs APROBADA (Power)" -ForegroundColor Yellow
Write-Host "=" * 80 -ForegroundColor Cyan

$carpeta = Split-Path -Parent $MyInvocation.MyCommand.Path
$archivoLocal = Join-Path $carpeta "FacturaElectronica_252_SistemaLocal.xml"
$archivoPower = Join-Path $carpeta "xml06_sistemapower.txt"

# Cargar XMLs
[xml]$xmlLocal = Get-Content $archivoLocal -Encoding UTF8
[xml]$xmlPowerRaw = Get-Content $archivoPower -Encoding UTF8

# Extraer el rDE interno del SOAP del sistema Power
$ns = New-Object System.Xml.XmlNamespaceManager($xmlPowerRaw.NameTable)
$ns.AddNamespace("ns2", "http://ekuatia.set.gov.py/sifen/xsd")
$ns.AddNamespace("env", "http://www.w3.org/2003/05/soap-envelope")
$rdeNode = $xmlPowerRaw.SelectSingleNode("//ns2:xContenDE/rDE", $ns)

# Si no se encuentra, intentar otro path
if (-not $rdeNode) {
    $rdeNode = $xmlPowerRaw.SelectSingleNode("//*[local-name()='rDE']")
}

$xmlPower = New-Object System.Xml.XmlDocument
$xmlPower.LoadXml($rdeNode.OuterXml)

Write-Host ""
Write-Host "ARCHIVOS COMPARADOS:" -ForegroundColor Magenta
Write-Host "  LOCAL (RECHAZADA): $archivoLocal" -ForegroundColor Red
Write-Host "  POWER (APROBADA):  $archivoPower" -ForegroundColor Green
Write-Host ""

# =============================================================================
# COMPARACION DE DATOS
# =============================================================================

function Comparar-Campo {
    param($nombre, $valorLocal, $valorPower)
    
    $igual = $valorLocal -eq $valorPower
    $color = if ($igual) { "Green" } else { "Red" }
    $icono = if ($igual) { "[OK]" } else { "[!!]" }
    
    Write-Host "$icono $nombre" -ForegroundColor $color -NoNewline
    if (-not $igual) {
        Write-Host ""
        Write-Host "     LOCAL: '$valorLocal'" -ForegroundColor Yellow
        Write-Host "     POWER: '$valorPower'" -ForegroundColor Cyan
    } else {
        Write-Host " = '$valorLocal'" -ForegroundColor DarkGray
    }
}

$diferencias = @()

Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "1. DATOS GENERALES DEL DOCUMENTO" -ForegroundColor Yellow
Write-Host "=" * 80 -ForegroundColor Cyan

# CDC
$cdcLocal = $xmlLocal.rDE.DE.Id
$cdcPower = $xmlPower.rDE.DE.Id
Comparar-Campo "CDC (Id)" $cdcLocal $cdcPower

# Digito Verificador
$dvLocal = $xmlLocal.rDE.DE.dDVId
$dvPower = $xmlPower.rDE.DE.dDVId
Comparar-Campo "Digito Verificador (dDVId)" $dvLocal $dvPower

# Fecha Firma
$fechaFirmaLocal = $xmlLocal.rDE.DE.dFecFirma
$fechaFirmaPower = $xmlPower.rDE.DE.dFecFirma
Comparar-Campo "Fecha Firma (dFecFirma)" $fechaFirmaLocal $fechaFirmaPower

# Sistema Facturacion
$sisFacLocal = $xmlLocal.rDE.DE.dSisFact
$sisFacPower = $xmlPower.rDE.DE.dSisFact
Comparar-Campo "Sistema Facturacion (dSisFact)" $sisFacLocal $sisFacPower

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "2. TIMBRADO (gTimb)" -ForegroundColor Yellow
Write-Host "=" * 80 -ForegroundColor Cyan

$timbLocal = $xmlLocal.rDE.DE.gTimb
$timbPower = $xmlPower.rDE.DE.gTimb

Comparar-Campo "Tipo DE (iTiDE)" $timbLocal.iTiDE $timbPower.iTiDE
Comparar-Campo "Nro Timbrado (dNumTim)" $timbLocal.dNumTim $timbPower.dNumTim
Comparar-Campo "Establecimiento (dEst)" $timbLocal.dEst $timbPower.dEst
Comparar-Campo "Punto Expedicion (dPunExp)" $timbLocal.dPunExp $timbPower.dPunExp
Comparar-Campo "Nro Documento (dNumDoc)" $timbLocal.dNumDoc $timbPower.dNumDoc
Comparar-Campo "Fecha Inicio Timbrado (dFeIniT)" $timbLocal.dFeIniT $timbPower.dFeIniT

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "3. EMISOR (gEmis)" -ForegroundColor Yellow
Write-Host "=" * 80 -ForegroundColor Cyan

$emisLocal = $xmlLocal.rDE.DE.gDatGralOpe.gEmis
$emisPower = $xmlPower.rDE.DE.gDatGralOpe.gEmis

Comparar-Campo "RUC Emisor (dRucEm)" $emisLocal.dRucEm $emisPower.dRucEm
Comparar-Campo "DV Emisor (dDVEmi)" $emisLocal.dDVEmi $emisPower.dDVEmi
Comparar-Campo "Tipo Contribuyente (iTipCont)" $emisLocal.iTipCont $emisPower.iTipCont
Comparar-Campo "Nombre Emisor (dNomEmi)" $emisLocal.dNomEmi $emisPower.dNomEmi
Comparar-Campo "Direccion (dDirEmi)" $emisLocal.dDirEmi $emisPower.dDirEmi
Comparar-Campo "Numero Casa (dNumCas)" $emisLocal.dNumCas $emisPower.dNumCas
Comparar-Campo "Telefono (dTelEmi)" $emisLocal.dTelEmi $emisPower.dTelEmi
Comparar-Campo "Email (dEmailE)" $emisLocal.dEmailE $emisPower.dEmailE

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "4. RECEPTOR (gDatRec)" -ForegroundColor Yellow
Write-Host "=" * 80 -ForegroundColor Cyan

$recLocal = $xmlLocal.rDE.DE.gDatGralOpe.gDatRec
$recPower = $xmlPower.rDE.DE.gDatGralOpe.gDatRec

Comparar-Campo "Naturaleza (iNatRec)" $recLocal.iNatRec $recPower.iNatRec
Comparar-Campo "Tipo Operacion (iTiOpe)" $recLocal.iTiOpe $recPower.iTiOpe
Comparar-Campo "Pais (cPaisRec)" $recLocal.cPaisRec $recPower.cPaisRec
Comparar-Campo "Tipo Contribuyente (iTiContRec)" $recLocal.iTiContRec $recPower.iTiContRec
Comparar-Campo "RUC Receptor (dRucRec)" $recLocal.dRucRec $recPower.dRucRec
Comparar-Campo "DV Receptor (dDVRec)" $recLocal.dDVRec $recPower.dDVRec
Comparar-Campo "Nombre Receptor (dNomRec)" $recLocal.dNomRec $recPower.dNomRec
Comparar-Campo "Numero Casa Receptor (dNumCasRec)" $recLocal.dNumCasRec $recPower.dNumCasRec

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "5. ITEMS (gCamItem)" -ForegroundColor Yellow
Write-Host "=" * 80 -ForegroundColor Cyan

$itemLocal = $xmlLocal.rDE.DE.gDtipDE.gCamItem
$itemPower = $xmlPower.rDE.DE.gDtipDE.gCamItem

Comparar-Campo "Codigo Interno (dCodInt)" $itemLocal.dCodInt $itemPower.dCodInt
Comparar-Campo "Descripcion (dDesProSer)" $itemLocal.dDesProSer $itemPower.dDesProSer
Comparar-Campo "Unidad Medida (cUniMed)" $itemLocal.cUniMed $itemPower.cUniMed
Comparar-Campo "Cantidad (dCantProSer)" $itemLocal.dCantProSer $itemPower.dCantProSer
Comparar-Campo "Precio Unitario (dPUniProSer)" $itemLocal.gValorItem.dPUniProSer $itemPower.gValorItem.dPUniProSer
Comparar-Campo "Total Bruto (dTotBruOpeItem)" $itemLocal.gValorItem.dTotBruOpeItem $itemPower.gValorItem.dTotBruOpeItem

Write-Host ""
Write-Host "--- IVA del Item ---" -ForegroundColor Magenta
$ivaLocal = $itemLocal.gCamIVA
$ivaPower = $itemPower.gCamIVA

Comparar-Campo "Afectacion IVA (iAfecIVA)" $ivaLocal.iAfecIVA $ivaPower.iAfecIVA
Comparar-Campo "Descripcion Afectacion (dDesAfecIVA)" $ivaLocal.dDesAfecIVA $ivaPower.dDesAfecIVA
Comparar-Campo "Proporcion IVA (dPropIVA)" $ivaLocal.dPropIVA $ivaPower.dPropIVA
Comparar-Campo "Tasa IVA (dTasaIVA)" $ivaLocal.dTasaIVA $ivaPower.dTasaIVA
Comparar-Campo "Base Gravada IVA (dBasGravIVA)" $ivaLocal.dBasGravIVA $ivaPower.dBasGravIVA
Comparar-Campo "Liquidacion IVA Item (dLiqIVAItem)" $ivaLocal.dLiqIVAItem $ivaPower.dLiqIVAItem
Comparar-Campo "Base Exenta (dBasExe)" $ivaLocal.dBasExe $ivaPower.dBasExe

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "6. TOTALES (gTotSub)" -ForegroundColor Yellow
Write-Host "=" * 80 -ForegroundColor Cyan

$totLocal = $xmlLocal.rDE.DE.gTotSub
$totPower = $xmlPower.rDE.DE.gTotSub

Comparar-Campo "Subtotal Exenta (dSubExe)" $totLocal.dSubExe $totPower.dSubExe
Comparar-Campo "Subtotal 5% (dSub5)" $totLocal.dSub5 $totPower.dSub5
Comparar-Campo "Subtotal 10% (dSub10)" $totLocal.dSub10 $totPower.dSub10
Comparar-Campo "Total Operacion (dTotOpe)" $totLocal.dTotOpe $totPower.dTotOpe
Comparar-Campo "Total General (dTotGralOpe)" $totLocal.dTotGralOpe $totPower.dTotGralOpe

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "7. FIRMA DIGITAL (Signature)" -ForegroundColor Yellow
Write-Host "=" * 80 -ForegroundColor Cyan

$sigLocal = $xmlLocal.rDE.Signature
$sigPower = $xmlPower.rDE.Signature

$sigInfoLocal = $sigLocal.SignedInfo
$sigInfoPower = $sigPower.SignedInfo

Comparar-Campo "Canonicalizacion" $sigInfoLocal.CanonicalizationMethod.Algorithm $sigInfoPower.CanonicalizationMethod.Algorithm
Comparar-Campo "Metodo Firma" $sigInfoLocal.SignatureMethod.Algorithm $sigInfoPower.SignatureMethod.Algorithm
Comparar-Campo "Transform 1" $sigInfoLocal.Reference.Transforms.Transform[0].Algorithm $sigInfoPower.Reference.Transforms.Transform[0].Algorithm
Comparar-Campo "Transform 2" $sigInfoLocal.Reference.Transforms.Transform[1].Algorithm $sigInfoPower.Reference.Transforms.Transform[1].Algorithm
Comparar-Campo "Metodo Digest" $sigInfoLocal.Reference.DigestMethod.Algorithm $sigInfoPower.Reference.DigestMethod.Algorithm

# Verificar si el certificado es el mismo
$certLocal = $sigLocal.KeyInfo.X509Data.X509Certificate
$certPower = $sigPower.KeyInfo.X509Data.X509Certificate
$certIgual = $certLocal -eq $certPower
$certColor = if ($certIgual) { "Green" } else { "Yellow" }
Write-Host "[$(if($certIgual){'OK'}else{'!!'})] Certificado X509" -ForegroundColor $certColor -NoNewline
if ($certIgual) {
    Write-Host " = IDENTICO" -ForegroundColor Green
} else {
    Write-Host " = MISMO CERTIFICADO (mismo contenido)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "8. QR CODE (gCamFuFD)" -ForegroundColor Yellow
Write-Host "=" * 80 -ForegroundColor Cyan

$qrLocal = $xmlLocal.rDE.gCamFuFD.dCarQR
$qrPower = $xmlPower.rDE.gCamFuFD.dCarQR

Write-Host "QR LOCAL:" -ForegroundColor Yellow
Write-Host $qrLocal -ForegroundColor DarkGray
Write-Host ""
Write-Host "QR POWER:" -ForegroundColor Cyan
Write-Host $qrPower -ForegroundColor DarkGray

# Analizar encoding de ampersand
Write-Host ""
Write-Host "--- Analisis de Encoding ---" -ForegroundColor Magenta
$tieneAmpLocal = $qrLocal -match '&amp;amp;'
$tieneAmpPower = $qrPower -match '&amp;amp;'
Write-Host "QR Local tiene '&amp;amp;' (doble encoding): $tieneAmpLocal" -ForegroundColor $(if($tieneAmpLocal){"Yellow"}else{"Green"})
Write-Host "QR Power tiene '&amp;amp;' (doble encoding): $tieneAmpPower" -ForegroundColor $(if($tieneAmpPower){"Green"}else{"Yellow"})

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "RESUMEN DE DIFERENCIAS CRITICAS" -ForegroundColor Red
Write-Host "=" * 80 -ForegroundColor Cyan

Write-Host ""
Write-Host "DIFERENCIAS ENCONTRADAS:" -ForegroundColor Red

# dBasExe
if ($ivaLocal.dBasExe -ne $ivaPower.dBasExe) {
    Write-Host "  [CRITICO] dBasExe: LOCAL='$($ivaLocal.dBasExe)' vs POWER='$($ivaPower.dBasExe)'" -ForegroundColor Red
    Write-Host "            El sistema local pone 1000, el power pone 0 (aunque sea exento)" -ForegroundColor Yellow
}

# dNumCas
if ($emisLocal.dNumCas -ne $emisPower.dNumCas) {
    Write-Host "  [DIFERENTE] dNumCas (Emisor): LOCAL='$($emisLocal.dNumCas)' vs POWER='$($emisPower.dNumCas)'" -ForegroundColor Yellow
}

# dDirEmi (espacio al inicio en Power)
if ($emisLocal.dDirEmi -ne $emisPower.dDirEmi) {
    Write-Host "  [DIFERENTE] dDirEmi: LOCAL='$($emisLocal.dDirEmi)' vs POWER='$($emisPower.dDirEmi)'" -ForegroundColor Yellow
}

# Nombre receptor (SRL vs S.R.L)
if ($recLocal.dNomRec -ne $recPower.dNomRec) {
    Write-Host "  [DIFERENTE] dNomRec: LOCAL='$($recLocal.dNomRec)' vs POWER='$($recPower.dNomRec)'" -ForegroundColor Yellow
}

# Email
if ($emisLocal.dEmailE -ne $emisPower.dEmailE) {
    Write-Host "  [DIFERENTE] dEmailE: LOCAL='$($emisLocal.dEmailE)' vs POWER='$($emisPower.dEmailE)'" -ForegroundColor Yellow
}

# Telefono
if ($emisLocal.dTelEmi -ne $emisPower.dTelEmi) {
    Write-Host "  [DIFERENTE] dTelEmi: LOCAL='$($emisLocal.dTelEmi)' vs POWER='$($emisPower.dTelEmi)'" -ForegroundColor Yellow
}

# Codigo interno
if ($itemLocal.dCodInt -ne $itemPower.dCodInt) {
    Write-Host "  [DIFERENTE] dCodInt: LOCAL='$($itemLocal.dCodInt)' vs POWER='$($itemPower.dCodInt)'" -ForegroundColor Yellow
}

# QR encoding
if ($tieneAmpLocal -ne $tieneAmpPower) {
    Write-Host "  [CRITICO] QR Encoding: LOCAL tiene simple '&amp;', POWER tiene doble '&amp;amp;'" -ForegroundColor Red
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Green
Write-Host "CONCLUSION - POSIBLE CAUSA DEL ERROR 0160" -ForegroundColor Green  
Write-Host "=" * 80 -ForegroundColor Green
Write-Host ""
Write-Host "La diferencia MAS PROBABLE que causa el rechazo es:" -ForegroundColor Yellow
Write-Host ""
Write-Host "  1. dBasExe = 1000 (LOCAL) vs 0 (POWER)" -ForegroundColor Red
Write-Host "     -> Para items EXENTOS, el sistema Power pone dBasExe=0" -ForegroundColor Yellow
Write-Host "     -> El sistema local pone dBasExe=1000 (el importe)" -ForegroundColor Yellow
Write-Host ""
Write-Host "  2. QR Encoding:" -ForegroundColor Red
Write-Host "     -> LOCAL: usa '&amp;' (encoding simple)" -ForegroundColor Yellow
Write-Host "     -> POWER: usa '&amp;amp;' (doble encoding)" -ForegroundColor Yellow
Write-Host ""

Write-Host "Ejecutado: $(Get-Date)" -ForegroundColor DarkGray
