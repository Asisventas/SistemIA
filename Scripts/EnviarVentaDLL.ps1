# Script para generar XML de venta y enviar con sifen.dll
# Ejecutar: .\Scripts\EnviarVentaDLL.ps1 -IdVenta 273

param(
    [int]$IdVenta = 273
)

$ErrorActionPreference = "Stop"

# Configuración
$server = "SERVERSIS\SQL2022"
$database = "asiswebapp"
$user = "sa"
$password = "%L4V1CT0R14"

$dllPath = "C:\asis\SistemIA\.ai-docs\SIFEN\Datos_nuevos_envios_dll_sifen\Sifen_Fuente\Sifen.dll"
# Certificado se obtiene de Sociedades
$urlLote = "https://sifen-test.set.gov.py/de/ws/async/recibe-lote.wsdl"
$urlQR = "https://ekuatia.set.gov.py/consultas-test/qr?"
$urlConsultaCDC = "https://sifen-test.set.gov.py/de/ws/consultas/consulta.wsdl"

Write-Host "=== ENVIO SIFEN CON DLL ===" -ForegroundColor Cyan
Write-Host "Venta ID: $IdVenta"
Write-Host "Fecha: $(Get-Date)"

# Conectar a BD
$connectionString = "Server=$server;Database=$database;User Id=$user;Password=$password;TrustServerCertificate=True"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()
Write-Host "Conectado a BD" -ForegroundColor Green

# Consultar venta
$queryVenta = @"
SELECT 
    v.IdVenta, v.Fecha, v.Total, v.Establecimiento, v.PuntoExpedicion, 
    v.NumeroFactura, v.CodigoSeguridad, v.FormaPago, v.Estado, v.Timbrado,
    c.IdCliente, c.RazonSocial as NombreCliente, c.RUC as RucCliente, c.DV as DVCliente,
    c.NaturalezaReceptor, c.TipoDocumentoIdentidadSifen, c.NumeroDocumentoIdentidad,
    so.RUC as RucEmisor, so.DV as DVEmisor, so.Nombre as NombreEmpresa, so.Direccion as DireccionEmisor,
    so.Telefono as TelefonoEmisor, so.Email as EmailEmisor, so.TipoContribuyente,
    so.IdCsc, so.Csc, so.PathCertificadoP12, so.PasswordCertificadoP12,
    ca.VigenciaDel
FROM Ventas v
JOIN Clientes c ON v.IdCliente = c.IdCliente
LEFT JOIN Sociedades so ON so.IdSociedad = 1
JOIN Cajas ca ON v.IdCaja = ca.id_caja
WHERE v.IdVenta = $IdVenta
"@

$cmd = New-Object System.Data.SqlClient.SqlCommand($queryVenta, $conn)
$reader = $cmd.ExecuteReader()
if (-not $reader.Read()) {
    Write-Host "ERROR: Venta $IdVenta no encontrada" -ForegroundColor Red
    $conn.Close()
    exit 1
}

# Leer datos de la venta
$venta = @{
    IdVenta = $reader["IdVenta"]
    Fecha = [DateTime]$reader["Fecha"]
    Total = [decimal]$reader["Total"]
    Establecimiento = $reader["Establecimiento"].ToString().PadLeft(3, '0')
    PuntoExpedicion = $reader["PuntoExpedicion"].ToString().PadLeft(3, '0')
    NumeroFactura = $reader["NumeroFactura"].ToString()
    CodigoSeguridad = $reader["CodigoSeguridad"]
    FormaPago = $reader["FormaPago"]
    NombreCliente = $reader["NombreCliente"]
    RucCliente = $reader["RucCliente"]
    DVCliente = $reader["DVCliente"]
    NaturalezaReceptor = if ($reader["NaturalezaReceptor"] -ne [DBNull]::Value) { [int]$reader["NaturalezaReceptor"] } else { 2 }
    TipoDocIdentidad = if ($reader["TipoDocumentoIdentidadSifen"] -ne [DBNull]::Value) { [int]$reader["TipoDocumentoIdentidadSifen"] } else { 1 }
    NumDocIdentidad = if ($reader["NumeroDocumentoIdentidad"] -ne [DBNull]::Value) { $reader["NumeroDocumentoIdentidad"].ToString() } else { "44444401" }
    RucEmisor = $reader["RucEmisor"]
    DVEmisor = $reader["DVEmisor"]
    NombreEmpresa = $reader["NombreEmpresa"]
    DireccionEmisor = $reader["DireccionEmisor"]
    TelefonoEmisor = $reader["TelefonoEmisor"]
    EmailEmisor = $reader["EmailEmisor"]
    IdCsc = if ($reader["IdCsc"] -ne [DBNull]::Value) { $reader["IdCsc"].ToString() } else { "1" }
    Csc = if ($reader["Csc"] -ne [DBNull]::Value) { $reader["Csc"].ToString() } else { "ABCD0000000000000000000000000000" }
    CertPath = if ($reader["PathCertificadoP12"] -ne [DBNull]::Value) { $reader["PathCertificadoP12"].ToString() } else { "C:\SistemIA\Certificados\WEN.pfx" }
    CertPassword = if ($reader["PasswordCertificadoP12"] -ne [DBNull]::Value) { $reader["PasswordCertificadoP12"].ToString() } else { "" }
    Timbrado = $reader["Timbrado"]
    VigenciaDel = if ($reader["VigenciaDel"] -ne [DBNull]::Value) { [DateTime]$reader["VigenciaDel"] } else { Get-Date }
}
$reader.Close()

Write-Host "Venta cargada: $($venta.NombreCliente) - Total: $($venta.Total)" -ForegroundColor Green

# Consultar detalles
$queryDetalles = @"
SELECT 
    vd.Cantidad, vd.PrecioUnitario, vd.Importe as Subtotal,
    p.IdProducto, p.CodigoInterno, p.Descripcion as NombreProducto,
    ti.Porcentaje as PorcentajeIVA, ti.CodigoSifen
FROM VentasDetalles vd
JOIN Productos p ON vd.IdProducto = p.IdProducto
LEFT JOIN TiposIva ti ON vd.IdTipoIVA = ti.IdTipoIVA
WHERE vd.IdVenta = $IdVenta
"@

$cmdDet = New-Object System.Data.SqlClient.SqlCommand($queryDetalles, $conn)
$readerDet = $cmdDet.ExecuteReader()
$detalles = @()
while ($readerDet.Read()) {
    $porc = if ($readerDet["PorcentajeIVA"] -ne [DBNull]::Value) { [decimal]$readerDet["PorcentajeIVA"] } else { 10 }
    # Determinar afectación: 10% o 5% = Gravado (1), 0% = Exento (3)
    $afec = if ($porc -gt 0) { 1 } else { 3 }
    $detalles += @{
        Cantidad = [decimal]$readerDet["Cantidad"]
        PrecioUnitario = [decimal]$readerDet["PrecioUnitario"]
        Subtotal = [decimal]$readerDet["Subtotal"]
        CodigoInterno = $readerDet["CodigoInterno"]
        NombreProducto = $readerDet["NombreProducto"]
        PorcentajeIVA = $porc
        AfectacionSifen = $afec
    }
}
$readerDet.Close()
$conn.Close()

Write-Host "Detalles cargados: $($detalles.Count) items" -ForegroundColor Green

# Generar CDC
$tipoDoc = "01"
$rucSinDV = $venta.RucEmisor.ToString().PadLeft(8, '0')
$dvEmisor = $venta.DVEmisor.ToString()
$est = $venta.Establecimiento
$ptoExp = $venta.PuntoExpedicion
$numDoc = $venta.NumeroFactura
$tipoContrib = "1"  # Persona física
$fechaStr = $venta.Fecha.ToString("yyyyMMdd")
$tipoEmision = "1"
$codSegDB = $venta.CodigoSeguridad
$codSeg = if ($codSegDB -and $codSegDB -ne [DBNull]::Value -and $codSegDB.ToString().Trim() -ne "") { 
    $codSegDB.ToString().PadLeft(9, '0') 
} else { 
    (Get-Random -Minimum 100000000 -Maximum 999999999).ToString() 
}

# CDC sin DV (43 dígitos)
$cdcBase = "$tipoDoc$rucSinDV$dvEmisor$est$ptoExp$numDoc$tipoContrib$fechaStr$tipoEmision$codSeg"

# Calcular DV del CDC (módulo 11)
$factores = @(2,3,4,5,6,7,2,3,4,5,6,7,2,3,4,5,6,7,2,3,4,5,6,7,2,3,4,5,6,7,2,3,4,5,6,7,2,3,4,5,6,7,2)
$suma = 0
$chars = $cdcBase.ToCharArray()
[Array]::Reverse($chars)
for ($i = 0; $i -lt $chars.Count; $i++) {
    $suma += [int]::Parse($chars[$i].ToString()) * $factores[$i]
}
$resto = $suma % 11
$dvCdc = if ($resto -le 1) { 0 } else { 11 - $resto }

$cdc = "$cdcBase$dvCdc"
Write-Host "CDC generado: $cdc" -ForegroundColor Yellow

# Calcular totales según items
$subExe = 0
$sub5 = 0
$sub10 = 0
$iva5 = 0
$iva10 = 0

foreach ($det in $detalles) {
    $subtotal = [Math]::Round($det.Subtotal, 0)
    if ($det.AfectacionSifen -eq 3) {
        $subExe += $subtotal
    } elseif ($det.PorcentajeIVA -eq 5) {
        $sub5 += $subtotal
        $iva5 += [Math]::Round($subtotal / 21, 0)
    } else {
        $sub10 += $subtotal
        $iva10 += [Math]::Round($subtotal / 11, 0)
    }
}

$totalIva = $iva5 + $iva10
$baseGrav5 = $sub5 - $iva5
$baseGrav10 = $sub10 - $iva10
$totalBaseGrav = $baseGrav5 + $baseGrav10
$totalOpe = [Math]::Round($venta.Total, 0)

# Generar items XML
$itemsXml = ""
foreach ($det in $detalles) {
    $afecIva = $det.AfectacionSifen
    $desAfecIva = switch ($afecIva) { 1 { "Gravado IVA" } 2 { "Exonerado" } 3 { "Exento" } 4 { "Gravado parcial" } default { "Gravado IVA" } }
    $tasaIva = switch ($afecIva) { 1 { $det.PorcentajeIVA } 3 { 0 } default { $det.PorcentajeIVA } }
    $propIva = switch ($tasaIva) { 10 { 100 } 5 { 100 } default { 0 } }
    
    $precioUnit = [Math]::Round($det.PrecioUnitario, 0)
    $totalItem = [Math]::Round($det.Subtotal, 0)
    
    # Calcular IVA del item
    if ($tasaIva -eq 10) {
        $ivaItem = [Math]::Round($totalItem / 11, 0)
        $baseGravItem = $totalItem - $ivaItem
    } elseif ($tasaIva -eq 5) {
        $ivaItem = [Math]::Round($totalItem / 21, 0)
        $baseGravItem = $totalItem - $ivaItem
    } else {
        $ivaItem = 0
        $baseGravItem = 0
    }
    
    $basExeItem = if ($afecIva -eq 3) { $totalItem } else { 0 }
    
    $itemsXml += @"

				<gCamItem>
					<dCodInt>$($det.CodigoInterno)</dCodInt>
					<dDesProSer>$($det.NombreProducto)</dDesProSer>
					<cUniMed>77</cUniMed>
					<dDesUniMed>UNI</dDesUniMed>
					<dCantProSer>$($det.Cantidad.ToString("0.0000", [System.Globalization.CultureInfo]::InvariantCulture))</dCantProSer>
					<gValorItem>
						<dPUniProSer>$precioUnit</dPUniProSer>
						<dTotBruOpeItem>$totalItem</dTotBruOpeItem>
						<gValorRestaItem>
							<dDescItem>0</dDescItem>
							<dPorcDesIt>0.00</dPorcDesIt>
							<dTotOpeItem>$totalItem</dTotOpeItem>
						</gValorRestaItem>
					</gValorItem>
					<gCamIVA>
						<iAfecIVA>$afecIva</iAfecIVA>
						<dDesAfecIVA>$desAfecIva</dDesAfecIVA>
						<dPropIVA>$propIva</dPropIVA>
						<dTasaIVA>$tasaIva</dTasaIVA>
						<dBasGravIVA>$baseGravItem</dBasGravIVA>
						<dLiqIVAItem>$ivaItem</dLiqIVAItem>
						<dBasExe>$basExeItem</dBasExe>
					</gCamIVA>
				</gCamItem>
"@
}

# Datos del receptor
$natRec = $venta.NaturalezaReceptor
$tiOpe = if ($natRec -eq 1) { 1 } else { 2 }  # B2B o B2C
$tipoIdRec = $venta.TipoDocIdentidad
$desTipoId = switch ($tipoIdRec) { 1 { "Cédula paraguaya" } 2 { "Pasaporte" } 3 { "Cédula extranjera" } 4 { "Carnet residencia" } 5 { "Innominado" } default { "Cédula paraguaya" } }
$numIdRec = $venta.NumDocIdentidad

# Receptor XML
$receptorXml = if ($natRec -eq 1) {
    # Contribuyente
    @"
				<gDatRec>
					<iNatRec>1</iNatRec>
					<iTiOpe>1</iTiOpe>
					<cPaisRec>PRY</cPaisRec>
					<dDesPaisRe>Paraguay</dDesPaisRe>
					<iTiContRec>2</iTiContRec>
					<dRucRec>$($venta.RucCliente)</dRucRec>
					<dDVRec>$($venta.DVCliente)</dDVRec>
					<dNomRec>$($venta.NombreCliente)</dNomRec>
					<dNumCasRec>0</dNumCasRec>
				</gDatRec>
"@
} else {
    # No contribuyente
    @"
				<gDatRec>
					<iNatRec>2</iNatRec>
					<iTiOpe>2</iTiOpe>
					<cPaisRec>PRY</cPaisRec>
					<dDesPaisRe>Paraguay</dDesPaisRe>
					<iTipIDRec>$tipoIdRec</iTipIDRec>
					<dDTipIDRec>$desTipoId</dDTipIDRec>
					<dNumIDRec>$numIdRec</dNumIDRec>
					<dNomRec>$($venta.NombreCliente)</dNomRec>
					<dNumCasRec>0</dNumCasRec>
				</gDatRec>
"@
}

# Condición de venta
$condOpe = if ($venta.FormaPago -eq "CONTADO") { 1 } else { 2 }

# Forma de pago
$condicionXml = if ($condOpe -eq 1) {
    @"
				<gCamCond>
					<iCondOpe>1</iCondOpe>
					<dDCondOpe>Contado</dDCondOpe>
					<gPaConEIni>
						<iTiPago>1</iTiPago>
						<dDesTiPag>Efectivo</dDesTiPag>
						<dMonTiPag>$totalOpe</dMonTiPag>
						<cMoneTiPag>PYG</cMoneTiPag>
						<dDMoneTiPag>Guarani</dDMoneTiPag>
					</gPaConEIni>
				</gCamCond>
"@
} else {
    @"
				<gCamCond>
					<iCondOpe>2</iCondOpe>
					<dDCondOpe>Crédito</dDCondOpe>
					<gPagCred>
						<iCondCred>1</iCondCred>
						<dDCondCred>Plazo</dDCondCred>
						<dPlazoCre>30 dias</dPlazoCre>
					</gPagCred>
				</gCamCond>
"@
}

# Placeholder para DigestValue (la DLL lo reemplaza)
$digestPlaceholder = "665569394474586a4f4a396970724970754f344c434a75706a457a73645766664846656d573270344c69593d"

# QR base
$fechaQR = $venta.Fecha.ToString("yyyy-MM-ddTHH:mm:ss")
$fechaQRHex = -join ($fechaQR.ToCharArray() | ForEach-Object { '{0:x2}' -f [int]$_ })
$idCscVal = $venta.IdCsc.TrimStart('0')
if (-not $idCscVal) { $idCscVal = "1" }
$cscVal = $venta.Csc

$qrData = "nVersion=150&amp;Id=$cdc&amp;dFeEmiDE=$fechaQRHex&amp;dNumIDRec=$numIdRec&amp;dTotGralOpe=$totalOpe&amp;dTotIVA=$totalIva&amp;cItems=$($detalles.Count)&amp;DigestValue=$digestPlaceholder&amp;IdCSC=$idCscVal$cscVal"

# Fecha de firma
$fechaFirma = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
$fechaEmision = $venta.Fecha.ToString("yyyy-MM-ddTHH:mm:ss")
$fechaIniTimb = $venta.VigenciaDel.ToString("yyyy-MM-dd")

# Timbrado
$timbrado = $venta.Timbrado.ToString().PadLeft(8, '0')

# Generar XML completo en formato DLL
$xmlInput = @"
<rLoteDE>
	<rDE xmlns="http://ekuatia.set.gov.py/sifen/xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:schemaLocation="http://ekuatia.set.gov.py/sifen/xsd siRecepDE_v150.xsd">
		<dVerFor>150</dVerFor>
		<DE Id="$cdc">
			<dDVId>$dvCdc</dDVId>
			<dFecFirma>$fechaFirma</dFecFirma>
			<dSisFact>1</dSisFact>
			<gOpeDE>
				<iTipEmi>1</iTipEmi>
				<dDesTipEmi>Normal</dDesTipEmi>
				<dCodSeg>$codSeg</dCodSeg>
			</gOpeDE>
			<gTimb>
				<iTiDE>1</iTiDE>
				<dDesTiDE>Factura electronica</dDesTiDE>
				<dNumTim>$timbrado</dNumTim>
				<dEst>$est</dEst>
				<dPunExp>$ptoExp</dPunExp>
				<dNumDoc>$numDoc</dNumDoc>
				<dFeIniT>$fechaIniTimb</dFeIniT>
			</gTimb>
			<gDatGralOpe>
				<dFeEmiDE>$fechaEmision</dFeEmiDE>
				<gOpeCom>
					<iTipTra>3</iTipTra>
					<dDesTipTra>Mixto (Venta de mercaderia y servicios)</dDesTipTra>
					<iTImp>1</iTImp>
					<dDesTImp>IVA</dDesTImp>
					<cMoneOpe>PYG</cMoneOpe>
					<dDesMoneOpe>Guarani</dDesMoneOpe>
					<gOblAfe>
						<cOblAfe>211</cOblAfe>
						<dDesOblAfe>IMPUESTO AL VALOR AGREGADO - GRAVADAS Y EXONERADAS - EXPORTADORES</dDesOblAfe>
					</gOblAfe>
				</gOpeCom>
				<gEmis>
					<dRucEm>$($venta.RucEmisor)</dRucEm>
					<dDVEmi>$($venta.DVEmisor)</dDVEmi>
					<iTipCont>1</iTipCont>
					<dNomEmi>$($venta.NombreEmpresa)</dNomEmi>
					<dDirEmi>$($venta.DireccionEmisor)</dDirEmi>
					<dNumCas>0</dNumCas>
					<cDepEmi>1</cDepEmi>
					<dDesDepEmi>CAPITAL</dDesDepEmi>
					<cDisEmi>1</cDisEmi>
					<dDesDisEmi>ASUNCION (DISTRITO)</dDesDisEmi>
					<cCiuEmi>1</cCiuEmi>
					<dDesCiuEmi>ASUNCION (DISTRITO)</dDesCiuEmi>
					<dTelEmi>$($venta.TelefonoEmisor)</dTelEmi>
					<dEmailE>$($venta.EmailEmisor)</dEmailE>
					<gActEco>
						<cActEco>47521</cActEco>
						<dDesActEco>COMERCIO AL POR MENOR DE ARTÍCULOS DE FERRETERÍA</dDesActEco>
					</gActEco>
				</gEmis>
$receptorXml
			</gDatGralOpe>
			<gDtipDE>
				<gCamFE>
					<iIndPres>1</iIndPres>
					<dDesIndPres>Operacion presencial</dDesIndPres>
				</gCamFE>
$condicionXml
$itemsXml
			</gDtipDE>
			<gTotSub>
				<dSubExe>$subExe</dSubExe>
				<dSub5>$sub5</dSub5>
				<dSub10>$sub10</dSub10>
				<dTotOpe>$totalOpe</dTotOpe>
				<dTotDesc>0</dTotDesc>
				<dTotDescGlotem>0</dTotDescGlotem>
				<dTotAntItem>0</dTotAntItem>
				<dTotAnt>0</dTotAnt>
				<dPorcDescTotal>0</dPorcDescTotal>
				<dDescTotal>0</dDescTotal>
				<dAnticipo>0</dAnticipo>
				<dRedon>0</dRedon>
				<dTotGralOpe>$totalOpe</dTotGralOpe>
				<dIVA5>$iva5</dIVA5>
				<dIVA10>$iva10</dIVA10>
				<dTotIVA>$totalIva</dTotIVA>
				<dBaseGrav5>$baseGrav5</dBaseGrav5>
				<dBaseGrav10>$baseGrav10</dBaseGrav10>
				<dTBasGraIVA>$totalBaseGrav</dTBasGraIVA>
			</gTotSub>
		</DE>
		<gCamFuFD>
			<dCarQR>$qrData</dCarQR>
		</gCamFuFD>
	</rDE>
</rLoteDE>
"@

# Guardar XML generado (sin BOM)
$outputDir = "C:\asis\SistemIA\Debug\ultimo_$IdVenta"
if (-not (Test-Path $outputDir)) { New-Item -ItemType Directory -Path $outputDir -Force | Out-Null }
$xmlPath = "$outputDir\xml_input_dll.xml"
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($xmlPath, $xmlInput, $utf8NoBom)
Write-Host "XML guardado en: $xmlPath" -ForegroundColor Green
Write-Host "CDC: $cdc" -ForegroundColor Yellow
Write-Host "Total: $totalOpe Gs" -ForegroundColor Yellow

# Leer el XML del archivo (garantiza encoding correcto)
$xmlParaDLL = [System.IO.File]::ReadAllText($xmlPath, [System.Text.Encoding]::UTF8)

# Cargar y usar la DLL
Write-Host "`n=== CARGANDO DLL ===" -ForegroundColor Cyan
Add-Type -Path $dllPath
$sifen = New-Object Sifen.Sifen

Write-Host "DLL cargada correctamente" -ForegroundColor Green
Write-Host "Enviando a SIFEN..." -ForegroundColor Yellow

# Llamar firmarYEnviar
$resultado = $sifen.firmarYEnviar($urlLote, $urlQR, $xmlParaDLL, $venta.CertPath, $venta.CertPassword, "1")

Write-Host "`n=== RESULTADO ===" -ForegroundColor Cyan
Write-Host $resultado

# Parsear resultado
try {
    $json = $resultado | ConvertFrom-Json
    Write-Host "`nCódigo: $($json.codigo)" -ForegroundColor $(if ($json.codigo -eq "0300") { "Green" } else { "Red" })
    Write-Host "Mensaje: $($json.mensaje)"
    Write-Host "ID Lote: $($json.idLote)"
    
    if ($json.codigo -eq "0300") {
        Write-Host "`n¡EXITO! Lote recibido correctamente" -ForegroundColor Green
        Write-Host "Protocolo de consulta: $($json.idLote)" -ForegroundColor Yellow
        
        # Guardar resultado
        $json | ConvertTo-Json | Out-File "$outputDir\resultado_envio.json"
        
        # Consultar CDC después de 5 segundos
        Write-Host "`nEsperando 5 segundos para consultar CDC..."
        Start-Sleep -Seconds 5
        
        Write-Host "Consultando CDC: $cdc" -ForegroundColor Yellow
        $respConsulta = $sifen.consulta($urlConsultaCDC, $cdc, "2", $venta.CertPath, $venta.CertPassword)
        
        if ($respConsulta -match "0422") {
            Write-Host "CDC ENCONTRADO EN SIFEN!" -ForegroundColor Green
        } elseif ($respConsulta -match "0423") {
            Write-Host "CDC aún no procesado (normal si acaba de enviarse)" -ForegroundColor Yellow
        }
        
        $respConsulta | Out-File "$outputDir\consulta_cdc.xml"
        Write-Host "Respuesta consulta guardada en: $outputDir\consulta_cdc.xml"
    }
} catch {
    Write-Host "Error parseando resultado: $_" -ForegroundColor Red
}

Write-Host "`n=== FIN ===" -ForegroundColor Cyan
