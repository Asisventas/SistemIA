$file = "c:\asis\SistemIA\wwwroot\js\funciones.js"
$content = Get-Content $file -Raw -Encoding UTF8

# Reemplazar el bloque de columnas de detalle (formato viejo por formato nuevo)
$oldDetail = @"
        // Nueva distribuci.n de columnas: CANT. | DESCRIPCI.N | EXENTA | 5% | 10%
        const colCant = margen;
        const colDesc = margen + anchoUtil * 0.12;
        const colExenta = margen + anchoUtil * 0.55;
        const colIva5 = margen + anchoUtil * 0.70;
        const colIva10 = margen + anchoUtil * 0.85;
        
        doc.text('CANT.', colCant, y);
        doc.text('DESCRIPCION', colDesc, y);
        doc.text('EXENTA', colExenta, y);
        doc.text('5%', colIva5, y);
        doc.text('10%', colIva10, y);
        y += 10;
        
        doc.setFont('courier', 'normal');
        
        ticketData.items.forEach(item => {
            const cantStr = item.cantidad.toString();
            const exentaStr = item.exenta || '';
            const iva5Str = item.gravada5 || '';
            const iva10Str = item.gravada10 || '';
            
            const maxDescChars = 22;
            const lineasDesc = dividirTextoChars(item.descripcion, maxDescChars);
            
            doc.text(cantStr, colCant, y);
            doc.text(lineasDesc[0], colDesc, y);
            doc.text(exentaStr, colExenta, y);
            doc.text(iva5Str, colIva5, y);
            doc.text(iva10Str, colIva10, y);
            y += 9;
            
            for (let i = 1; i < lineasDesc.length; i++) {
                doc.text(lineasDesc[i], colDesc, y);
                y += 9;
            }
        });
"@

$newDetail = @"
        // Formato ticket: CANT. | DESCRIPCION | IMPORTE | IVA
        const colCant = margen;
        const colDesc = margen + anchoUtil * 0.10;
        const colImporte = margen + anchoUtil * 0.68;
        const colTiva = margen + anchoUtil * 0.88;
        
        doc.text('CANT.', colCant, y);
        doc.text('DESCRIPCION', colDesc, y);
        doc.text('IMPORTE', colImporte, y);
        doc.text('IVA', colTiva, y);
        y += 10;
        
        doc.setFont('courier', 'normal');
        
        ticketData.items.forEach(item => {
            const cantStr = item.cantidad.toString();
            const importeStr = item.total || '';
            let ivaCode = '';
            if (item.gravada10 && item.gravada10 !== '') ivaCode = '10';
            else if (item.gravada5 && item.gravada5 !== '') ivaCode = '5';
            
            const maxDescChars = 28;
            const lineasDesc = dividirTextoChars(item.descripcion, maxDescChars);
            
            doc.text(cantStr, colCant, y);
            doc.text(lineasDesc[0], colDesc, y);
            doc.text(importeStr, colImporte, y);
            doc.text(ivaCode, colTiva, y);
            y += 9;
            
            for (let i = 1; i < lineasDesc.length; i++) {
                doc.text(lineasDesc[i], colDesc, y);
                y += 9;
            }
        });
"@

# Usar regex para matchear con caracteres unicode problemáticos
$pattern = '// Nueva distribuci.n de columnas: CANT\. \| DESCRIPCI.N \| EXENTA \| 5% \| 10%\s+const colCant = margen;\s+const colDesc = margen \+ anchoUtil \* 0\.12;\s+const colExenta = margen \+ anchoUtil \* 0\.55;\s+const colIva5 = margen \+ anchoUtil \* 0\.70;\s+const colIva10 = margen \+ anchoUtil \* 0\.85;\s+doc\.text\(.CANT\..,.+\s+doc\.text\(.DESCRIPCION.,.+\s+doc\.text\(.EXENTA.,.+\s+doc\.text\(.5%.,.+\s+doc\.text\(.10%.,.+\s+y \+= 10;\s+doc\.setFont\(.courier., .normal.\);\s+ticketData\.items\.forEach\(item => \{\s+const cantStr = item\.cantidad\.toString\(\);\s+const exentaStr = item\.exenta \|\| ....;\s+const iva5Str = item\.gravada5 \|\| ....;\s+const iva10Str = item\.gravada10 \|\| ....;\s+const maxDescChars = 22;\s+const lineasDesc = dividirTextoChars\(item\.descripcion, maxDescChars\);\s+doc\.text\(cantStr, colCant, y\);\s+doc\.text\(lineasDesc\[0\], colDesc, y\);\s+doc\.text\(exentaStr, colExenta, y\);\s+doc\.text\(iva5Str, colIva5, y\);\s+doc\.text\(iva10Str, colIva10, y\);\s+y \+= 9;\s+for \(let i = 1; i < lineasDesc\.length; i\+\+\) \{\s+doc\.text\(lineasDesc\[i\], colDesc, y\);\s+y \+= 9;\s+\}\s+\}\);'

$count = ([regex]::Matches($content, $pattern)).Count
Write-Host "Matches found: $count"

$content = [regex]::Replace($content, $pattern, $newDetail)

Set-Content $file -Value $content -Encoding UTF8 -NoNewline
Write-Host "Done"
