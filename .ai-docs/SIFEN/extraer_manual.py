#!/usr/bin/env python3
"""
Script para extraer contenido del Manual Técnico SIFEN v150
Extrae: texto completo, imágenes, tablas
"""
import fitz  # PyMuPDF
import os
import json
from pathlib import Path

# Rutas
PDF_PATH = r"C:\asis\SistemIA\.ai-docs\SIFEN\Manual_Tecnico_v150.pdf"
OUTPUT_DIR = r"C:\asis\SistemIA\.ai-docs\SIFEN\Manual_Extraido"
IMAGES_DIR = os.path.join(OUTPUT_DIR, "imagenes")

def crear_directorios():
    """Crear directorios de salida"""
    os.makedirs(OUTPUT_DIR, exist_ok=True)
    os.makedirs(IMAGES_DIR, exist_ok=True)
    print(f"✓ Directorios creados: {OUTPUT_DIR}")

def extraer_texto_completo(doc):
    """Extraer todo el texto del PDF"""
    texto_completo = []
    for num_pag, pagina in enumerate(doc, 1):
        texto = pagina.get_text("text")
        if texto.strip():
            texto_completo.append(f"\n{'='*80}\n")
            texto_completo.append(f"PÁGINA {num_pag}\n")
            texto_completo.append(f"{'='*80}\n")
            texto_completo.append(texto)
    
    # Guardar texto completo
    ruta_texto = os.path.join(OUTPUT_DIR, "manual_completo.txt")
    with open(ruta_texto, "w", encoding="utf-8") as f:
        f.write("".join(texto_completo))
    print(f"✓ Texto extraído: {ruta_texto}")
    return texto_completo

def extraer_imagenes(doc):
    """Extraer todas las imágenes del PDF"""
    imagenes_extraidas = []
    
    for num_pag, pagina in enumerate(doc, 1):
        lista_imagenes = pagina.get_images(full=True)
        
        for idx, img_info in enumerate(lista_imagenes, 1):
            xref = img_info[0]
            try:
                imagen_base = doc.extract_image(xref)
                imagen_bytes = imagen_base["image"]
                extension = imagen_base["ext"]
                
                nombre_archivo = f"pag{num_pag:03d}_img{idx:02d}.{extension}"
                ruta_imagen = os.path.join(IMAGES_DIR, nombre_archivo)
                
                with open(ruta_imagen, "wb") as f:
                    f.write(imagen_bytes)
                
                imagenes_extraidas.append({
                    "pagina": num_pag,
                    "archivo": nombre_archivo,
                    "tamaño_bytes": len(imagen_bytes),
                    "extension": extension
                })
            except Exception as e:
                print(f"  ⚠ Error imagen pág {num_pag}: {e}")
    
    print(f"✓ Imágenes extraídas: {len(imagenes_extraidas)}")
    return imagenes_extraidas

def buscar_secciones_xml(texto_completo):
    """Buscar secciones relevantes sobre XML"""
    secciones_relevantes = {
        "formato_xml": [],
        "codificacion": [],
        "campos_obligatorios": [],
        "formas_envio": [],
        "ejemplos_xml": [],
        "errores": [],
        "firma_digital": []
    }
    
    texto_unido = "".join(texto_completo).lower()
    
    # Buscar patrones relevantes
    if "utf-8" in texto_unido or "codificación" in texto_unido:
        secciones_relevantes["codificacion"].append("Sección encontrada sobre codificación")
    
    if "<rde" in texto_unido or "renvide" in texto_unido:
        secciones_relevantes["ejemplos_xml"].append("Sección encontrada con ejemplos XML")
    
    return secciones_relevantes

def extraer_tablas_por_pagina(doc):
    """Extraer texto estructurado de cada página (tablas)"""
    tablas = []
    for num_pag, pagina in enumerate(doc, 1):
        # Intentar extraer como bloques de texto
        bloques = pagina.get_text("blocks")
        for bloque in bloques:
            if len(bloque) >= 5:
                texto = bloque[4] if isinstance(bloque[4], str) else ""
                # Detectar posibles tablas (múltiples columnas)
                if "|" in texto or "\t" in texto or texto.count("  ") > 3:
                    tablas.append({
                        "pagina": num_pag,
                        "contenido": texto[:500]  # Primeros 500 chars
                    })
    return tablas

def generar_indice(doc):
    """Generar índice del documento"""
    toc = doc.get_toc()  # Table of contents
    indice = []
    for nivel, titulo, pagina in toc:
        indice.append({
            "nivel": nivel,
            "titulo": titulo,
            "pagina": pagina
        })
    return indice

def main():
    print("="*60)
    print("EXTRACCIÓN MANUAL TÉCNICO SIFEN v150")
    print("="*60)
    
    # Verificar que existe el PDF
    if not os.path.exists(PDF_PATH):
        print(f"❌ No se encontró el PDF: {PDF_PATH}")
        return
    
    print(f"✓ PDF encontrado: {PDF_PATH}")
    
    # Crear directorios
    crear_directorios()
    
    # Abrir documento
    doc = fitz.open(PDF_PATH)
    print(f"✓ PDF abierto: {doc.page_count} páginas")
    
    # Extraer metadatos
    metadata = doc.metadata
    print(f"✓ Metadatos: Título='{metadata.get('title', 'N/A')}', Páginas={doc.page_count}")
    
    # Extraer índice (TOC)
    indice = generar_indice(doc)
    print(f"✓ Índice extraído: {len(indice)} entradas")
    
    # Extraer texto completo
    texto_completo = extraer_texto_completo(doc)
    
    # Extraer imágenes
    imagenes = extraer_imagenes(doc)
    
    # Generar resumen JSON
    resumen = {
        "pdf_original": PDF_PATH,
        "total_paginas": doc.page_count,
        "metadatos": metadata,
        "indice": indice,
        "imagenes_extraidas": len(imagenes),
        "lista_imagenes": imagenes
    }
    
    ruta_resumen = os.path.join(OUTPUT_DIR, "resumen_extraccion.json")
    with open(ruta_resumen, "w", encoding="utf-8") as f:
        json.dump(resumen, f, indent=2, ensure_ascii=False)
    print(f"✓ Resumen guardado: {ruta_resumen}")
    
    # Cerrar documento
    doc.close()
    
    print("\n" + "="*60)
    print("EXTRACCIÓN COMPLETADA")
    print(f"  - Texto: {OUTPUT_DIR}\\manual_completo.txt")
    print(f"  - Imágenes: {IMAGES_DIR}\\")
    print(f"  - Resumen: {ruta_resumen}")
    print("="*60)

if __name__ == "__main__":
    main()
