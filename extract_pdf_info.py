from pdfminer.high_level import extract_text

pdf_path = r'c:\asis\SistemIA\ManualSifen\Manual Técnico Versión 150.pdf'
text = extract_text(pdf_path)

# Guardar el texto extraído
with open(r'c:\asis\SistemIA\.ai-docs\SIFEN\Manual_Tecnico_v150_COMPLETO.txt', 'w', encoding='utf-8') as f:
    f.write(text)

# Buscar secciones de error 0160 y validaciones
lines = text.split('\n')

print("=== SECCIÓN DE VALIDACIONES DEL ÁREA DE DATOS (AE) ===")
for i, line in enumerate(lines):
    clean_line = line.strip()
    # Buscar sección de error AE01 (XML malformado) y su contexto
    if 'AE01' in clean_line and ('XML' in clean_line.lower() or 'malformado' in clean_line.lower()):
        start = max(0, i-5)
        end = min(len(lines), i+50)
        for j in range(start, end):
            print(f'{j}: {lines[j]}')
        print("\n" + "="*60 + "\n")
        break

print("\n=== BUSCANDO EJEMPLO DE XML RECHAZADO CON 0160 ===")
for i, line in enumerate(lines):
    if '0160' in line and 'Rechazado' in line:
        start = max(0, i-10)
        end = min(len(lines), i+10)
        for j in range(start, end):
            print(f'{j}: {lines[j]}')
        print("\n")
        break

print("\n=== BUSCANDO VALIDACIONES DE FIRMA DIGITAL ===")
for i, line in enumerate(lines):
    if 'AD01' in line or 'Firma difiere' in line:
        start = max(0, i-3)
        end = min(len(lines), i+20)
        for j in range(start, end):
            print(f'{j}: {lines[j]}')
        print("\n")
        break

print("\nTEXTO COMPLETO GUARDADO EN: .ai-docs/SIFEN/Manual_Tecnico_v150_COMPLETO.txt")
