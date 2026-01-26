# 🧠 Hub Central de IA - Especificación Técnica

## Documento para Claude - Implementación Completa

**Fecha:** 24 de enero de 2026  
**Versión:** 1.0  
**Autor:** Equipo de Desarrollo  
**Proyecto:** Hub Central de IA para Sistemas de Gestión

---

## 📋 Resumen Ejecutivo

Este documento especifica la implementación de un **Hub Central de Inteligencia Artificial** que servirá como punto único de consultas para múltiples sistemas de gestión empresarial. El Hub utilizará **Claude API (Anthropic)** para responder consultas basándose en conocimientos específicos de cada sistema.

### Sistemas a Integrar

| Sistema | Tecnología | Descripción |
|---------|------------|-------------|
| **Gasparini/Nextys** | PowerBuilder | Sistema de gestión legacy |
| **Sistema Angular** | Angular + Node.js | Sistema moderno en desarrollo |
| **SistemIA** | Blazor Server .NET 8 | Sistema de gestión con SIFEN Paraguay |

---

## 🏗️ Arquitectura General

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
│                     HUB CENTRAL DE IA (Servidor 192.168.100.160)                     │
│                                                                                      │
│  ┌────────────────────────────────────────────────────────────────────────────────┐ │
│  │                         FRONTEND - Angular 17+                                  │ │
│  │  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐          │ │
│  │  │  Dashboard   │ │  Sistemas    │ │  Consultas   │ │  Reportes    │          │ │
│  │  │  Principal   │ │  y Fuentes   │ │  Historial   │ │  y Métricas  │          │ │
│  │  └──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘          │ │
│  │  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐          │ │
│  │  │  Clientes    │ │  Usuarios    │ │Conocimientos │ │ Configuración│          │ │
│  │  │  Empresas    │ │  y Roles     │ │  por Sistema │ │  IA/Límites  │          │ │
│  │  └──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘          │ │
│  └────────────────────────────────────────────────────────────────────────────────┘ │
│                                          │                                           │
│  ┌────────────────────────────────────────────────────────────────────────────────┐ │
│  │                         BACKEND - Node.js + NestJS                              │ │
│  │                                                                                  │ │
│  │   POST /api/auth/login              → Autenticación JWT                         │ │
│  │   POST /api/auth/refresh            → Renovar token                             │ │
│  │   GET  /api/sistemas                → Listar sistemas registrados               │ │
│  │   POST /api/consultas               → Consulta a la IA                          │ │
│  │   GET  /api/consultas/historial     → Historial de consultas                    │ │
│  │   CRUD /api/conocimientos           → Gestión de conocimientos                  │ │
│  │   CRUD /api/clientes                → Gestión de clientes                       │ │
│  │   CRUD /api/usuarios                → Gestión de usuarios                       │ │
│  │   POST /api/fuentes/indexar         → Indexar código fuente                     │ │
│  │   GET  /api/reportes/*              → Reportes y métricas                       │ │
│  │                                                                                  │ │
│  └────────────────────────────────────────────────────────────────────────────────┘ │
│                                          │                                           │
│         ┌────────────────────────────────┼────────────────────────────┐             │
│         ▼                                ▼                            ▼             │
│  ┌──────────────┐            ┌────────────────────┐          ┌──────────────┐       │
│  │ Claude API   │            │  PostgreSQL 16     │          │ Carpetas     │       │
│  │ (Anthropic)  │            │  + pgvector        │          │ Fuentes      │       │
│  │              │            │                    │          │              │       │
│  │ claude-3.5   │            │  - sistemas        │          │/fuentes/     │       │
│  │ sonnet       │            │  - conocimientos   │          │ ├─gasparini/ │       │
│  │              │            │  - clientes        │          │ ├─angular/   │       │
│  │              │            │  - usuarios        │          │ └─sistemia/  │       │
│  │              │            │  - consultas_log   │          │              │       │
│  │              │            │  - fuentes_codigo  │          │              │       │
│  └──────────────┘            └────────────────────┘          └──────────────┘       │
│                                                                                      │
└─────────────────────────────────────────────────────────────────────────────────────┘
                                          ▲
                                          │ HTTPS + JWT Token
          ┌───────────────────────────────┼───────────────────────────────┐
          │                               │                               │
          ▼                               ▼                               ▼
   ┌──────────────┐              ┌──────────────┐               ┌──────────────┐
   │ GASPARINI    │              │ SISTEMA      │               │ SISTEMIA     │
   │ NEXTYS       │              │ ANGULAR      │               │ (Blazor)     │
   │              │              │              │               │              │
   │ X-Sistema-Id:│              │ X-Sistema-Id:│               │ X-Sistema-Id:│
   │ "gasparini"  │              │ "angular"    │               │ "sistemia"   │
   └──────────────┘              └──────────────┘               └──────────────┘
```

---

## 📊 Modelo de Base de Datos (PostgreSQL)

### Diagrama ER

```
┌─────────────────┐       ┌─────────────────┐       ┌─────────────────┐
│    sistemas     │       │  conocimientos  │       │ fuentes_codigo  │
├─────────────────┤       ├─────────────────┤       ├─────────────────┤
│ id (PK)         │──┐    │ id (PK)         │       │ id (PK)         │
│ codigo          │  │    │ sistema_id (FK) │◄──────│ sistema_id (FK) │
│ nombre          │  │    │ categoria       │       │ ruta            │
│ descripcion     │  └───►│ titulo          │       │ nombre_archivo  │
│ api_key         │       │ contenido       │       │ extension       │
│ api_secret      │       │ palabras_clave  │       │ contenido       │
│ ruta_fuentes    │       │ embedding       │       │ resumen_ia      │
│ activo          │       │ prioridad       │       │ hash_contenido  │
│ config (JSONB)  │       │ veces_usado     │       │ updated_at      │
│ created_at      │       │ created_at      │       └─────────────────┘
└─────────────────┘       └─────────────────┘
         │
         │        ┌─────────────────┐       ┌─────────────────┐
         │        │    clientes     │       │    usuarios     │
         │        ├─────────────────┤       ├─────────────────┤
         │        │ id (PK)         │──┐    │ id (PK)         │
         │        │ nombre          │  │    │ cliente_id (FK) │◄─┐
         │        │ ruc             │  │    │ email           │  │
         │        │ email_contacto  │  │    │ password_hash   │  │
         │        │ telefono        │  │    │ nombre          │  │
         │        │ sistemas_ids[]  │  │    │ rol             │  │
         │        │ limite_diario   │  │    │ activo          │  │
         │        │ activo          │  │    │ ultimo_acceso   │  │
         │        │ created_at      │  │    │ created_at      │  │
         │        └─────────────────┘  │    └─────────────────┘  │
         │                             │                         │
         │        ┌────────────────────┴─────────────────────────┘
         │        │
         │        ▼
         │  ┌─────────────────┐
         │  │  consultas_log  │
         │  ├─────────────────┤
         └─►│ id (PK)         │
            │ usuario_id (FK) │
            │ sistema_id (FK) │
            │ pregunta        │
            │ respuesta       │
            │ tokens_entrada  │
            │ tokens_salida   │
            │ costo_estimado  │
            │ tiempo_ms       │
            │ fuentes (JSONB) │
            │ exitosa         │
            │ error           │
            │ ip_cliente      │
            │ created_at      │
            └─────────────────┘
```

### Scripts SQL

```sql
-- =============================================
-- CREAR BASE DE DATOS
-- =============================================
CREATE DATABASE hub_ia_central;
\c hub_ia_central;

-- Extensión para búsqueda semántica (opcional pero recomendada)
CREATE EXTENSION IF NOT EXISTS vector;
CREATE EXTENSION IF NOT EXISTS pg_trgm;  -- Para búsqueda de texto

-- =============================================
-- TABLA: sistemas
-- Sistemas registrados que pueden consultar la IA
-- =============================================
CREATE TABLE sistemas (
    id SERIAL PRIMARY KEY,
    codigo VARCHAR(50) UNIQUE NOT NULL,
    nombre VARCHAR(200) NOT NULL,
    descripcion TEXT,
    api_key VARCHAR(64) UNIQUE NOT NULL,
    api_secret VARCHAR(64) NOT NULL,
    ruta_fuentes VARCHAR(500),           -- Ruta en el servidor donde están las fuentes
    activo BOOLEAN DEFAULT true,
    config JSONB DEFAULT '{}',           -- Configuración específica del sistema
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Índices
CREATE INDEX idx_sistemas_codigo ON sistemas(codigo);
CREATE INDEX idx_sistemas_api_key ON sistemas(api_key);

-- =============================================
-- TABLA: conocimientos
-- Base de conocimiento por sistema
-- =============================================
CREATE TABLE conocimientos (
    id SERIAL PRIMARY KEY,
    sistema_id INT NOT NULL REFERENCES sistemas(id) ON DELETE CASCADE,
    categoria VARCHAR(100) NOT NULL,
    subcategoria VARCHAR(100),
    titulo VARCHAR(300) NOT NULL,
    contenido TEXT NOT NULL,
    palabras_clave TEXT[],
    -- embedding VECTOR(1536),           -- Descomentar si usas pgvector
    prioridad INT DEFAULT 5,
    veces_usado INT DEFAULT 0,
    activo BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Índices
CREATE INDEX idx_conocimientos_sistema ON conocimientos(sistema_id);
CREATE INDEX idx_conocimientos_categoria ON conocimientos(categoria);
CREATE INDEX idx_conocimientos_titulo ON conocimientos USING gin(titulo gin_trgm_ops);
CREATE INDEX idx_conocimientos_contenido ON conocimientos USING gin(contenido gin_trgm_ops);
CREATE INDEX idx_conocimientos_palabras ON conocimientos USING gin(palabras_clave);

-- =============================================
-- TABLA: fuentes_codigo
-- Código fuente indexado de cada sistema
-- =============================================
CREATE TABLE fuentes_codigo (
    id SERIAL PRIMARY KEY,
    sistema_id INT NOT NULL REFERENCES sistemas(id) ON DELETE CASCADE,
    ruta_relativa VARCHAR(500) NOT NULL,
    nombre_archivo VARCHAR(200) NOT NULL,
    extension VARCHAR(20),
    categoria VARCHAR(100),              -- Models, Services, Pages, Controllers, etc.
    contenido TEXT,
    resumen_ia TEXT,                     -- Resumen generado por Claude
    hash_contenido VARCHAR(64),          -- SHA256 para detectar cambios
    tamano_bytes BIGINT,
    lineas INT,
    fecha_archivo TIMESTAMP,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    UNIQUE(sistema_id, ruta_relativa)
);

-- Índices
CREATE INDEX idx_fuentes_sistema ON fuentes_codigo(sistema_id);
CREATE INDEX idx_fuentes_extension ON fuentes_codigo(extension);
CREATE INDEX idx_fuentes_categoria ON fuentes_codigo(categoria);
CREATE INDEX idx_fuentes_contenido ON fuentes_codigo USING gin(contenido gin_trgm_ops);

-- =============================================
-- TABLA: clientes
-- Empresas/Clientes que usan los sistemas
-- =============================================
CREATE TABLE clientes (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(200) NOT NULL,
    ruc VARCHAR(20),
    email_contacto VARCHAR(200),
    telefono VARCHAR(50),
    direccion TEXT,
    sistemas_permitidos INT[],           -- Array de IDs de sistemas que puede consultar
    limite_consultas_dia INT DEFAULT 100,
    limite_consultas_mes INT DEFAULT 3000,
    plan VARCHAR(50) DEFAULT 'basico',   -- basico, profesional, enterprise
    activo BOOLEAN DEFAULT true,
    notas TEXT,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Índices
CREATE INDEX idx_clientes_nombre ON clientes(nombre);
CREATE INDEX idx_clientes_ruc ON clientes(ruc);

-- =============================================
-- TABLA: usuarios
-- Usuarios que pueden hacer consultas
-- =============================================
CREATE TABLE usuarios (
    id SERIAL PRIMARY KEY,
    cliente_id INT NOT NULL REFERENCES clientes(id) ON DELETE CASCADE,
    email VARCHAR(200) UNIQUE NOT NULL,
    password_hash VARCHAR(500) NOT NULL,
    nombre VARCHAR(200) NOT NULL,
    rol VARCHAR(50) DEFAULT 'usuario',   -- admin, supervisor, usuario
    activo BOOLEAN DEFAULT true,
    email_verificado BOOLEAN DEFAULT false,
    ultimo_acceso TIMESTAMP,
    intentos_fallidos INT DEFAULT 0,
    bloqueado_hasta TIMESTAMP,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Índices
CREATE INDEX idx_usuarios_cliente ON usuarios(cliente_id);
CREATE INDEX idx_usuarios_email ON usuarios(email);

-- =============================================
-- TABLA: consultas_log
-- Historial de todas las consultas (auditoría)
-- =============================================
CREATE TABLE consultas_log (
    id BIGSERIAL PRIMARY KEY,
    usuario_id INT REFERENCES usuarios(id) ON DELETE SET NULL,
    sistema_id INT NOT NULL REFERENCES sistemas(id),
    cliente_id INT REFERENCES clientes(id) ON DELETE SET NULL,
    
    -- Consulta
    pregunta TEXT NOT NULL,
    contexto_adicional TEXT,
    
    -- Respuesta
    respuesta TEXT,
    fuentes_usadas JSONB,                -- [{tipo: "conocimiento", id: 1}, {tipo: "codigo", ruta: "..."}]
    confianza DECIMAL(3,2),              -- 0.00 a 1.00
    
    -- Métricas
    tokens_entrada INT,
    tokens_salida INT,
    costo_estimado DECIMAL(10,6),        -- En USD
    tiempo_respuesta_ms INT,
    modelo_usado VARCHAR(50),
    
    -- Estado
    exitosa BOOLEAN DEFAULT true,
    error TEXT,
    
    -- Auditoría
    ip_cliente VARCHAR(50),
    user_agent TEXT,
    created_at TIMESTAMP DEFAULT NOW()
);

-- Índices
CREATE INDEX idx_consultas_usuario ON consultas_log(usuario_id);
CREATE INDEX idx_consultas_sistema ON consultas_log(sistema_id);
CREATE INDEX idx_consultas_cliente ON consultas_log(cliente_id);
CREATE INDEX idx_consultas_fecha ON consultas_log(created_at);
CREATE INDEX idx_consultas_exitosa ON consultas_log(exitosa);

-- Particionado por mes (opcional para alto volumen)
-- CREATE TABLE consultas_log_2026_01 PARTITION OF consultas_log
--     FOR VALUES FROM ('2026-01-01') TO ('2026-02-01');

-- =============================================
-- TABLA: tokens_refresh
-- Para manejo de refresh tokens
-- =============================================
CREATE TABLE tokens_refresh (
    id SERIAL PRIMARY KEY,
    usuario_id INT NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE,
    token VARCHAR(500) UNIQUE NOT NULL,
    expira_en TIMESTAMP NOT NULL,
    revocado BOOLEAN DEFAULT false,
    ip_creacion VARCHAR(50),
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_tokens_usuario ON tokens_refresh(usuario_id);
CREATE INDEX idx_tokens_token ON tokens_refresh(token);

-- =============================================
-- TABLA: configuracion_global
-- Configuración del sistema
-- =============================================
CREATE TABLE configuracion_global (
    clave VARCHAR(100) PRIMARY KEY,
    valor TEXT,
    descripcion TEXT,
    tipo VARCHAR(20) DEFAULT 'string',   -- string, number, boolean, json
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Insertar configuración inicial
INSERT INTO configuracion_global (clave, valor, descripcion, tipo) VALUES
('claude_api_key', '', 'API Key de Anthropic Claude', 'string'),
('claude_model', 'claude-3-5-sonnet-20241022', 'Modelo de Claude a usar', 'string'),
('claude_max_tokens', '4096', 'Máximo de tokens por respuesta', 'number'),
('costo_por_1k_tokens_entrada', '0.003', 'Costo en USD por 1K tokens entrada', 'number'),
('costo_por_1k_tokens_salida', '0.015', 'Costo en USD por 1K tokens salida', 'number'),
('limite_global_diario', '10000', 'Límite global de consultas por día', 'number'),
('mantenimiento', 'false', 'Sistema en mantenimiento', 'boolean');

-- =============================================
-- DATOS INICIALES
-- =============================================

-- Sistemas
INSERT INTO sistemas (codigo, nombre, descripcion, api_key, api_secret, ruta_fuentes) VALUES
('gasparini', 'Gasparini/Nextys', 'Sistema de gestión basado en PowerBuilder', 
 'gas_' || encode(gen_random_bytes(24), 'hex'), encode(gen_random_bytes(24), 'hex'),
 '/fuentes/gasparini'),
('angular', 'Sistema Angular', 'Sistema moderno de gestión en desarrollo',
 'ang_' || encode(gen_random_bytes(24), 'hex'), encode(gen_random_bytes(24), 'hex'),
 '/fuentes/angular'),
('sistemia', 'SistemIA', 'Sistema de gestión con SIFEN Paraguay (Blazor)',
 'sia_' || encode(gen_random_bytes(24), 'hex'), encode(gen_random_bytes(24), 'hex'),
 '/fuentes/sistemia');

-- Cliente de prueba
INSERT INTO clientes (nombre, ruc, email_contacto, sistemas_permitidos, limite_consultas_dia) VALUES
('Admin Central', '00000000-0', 'admin@empresa.com', ARRAY[1,2,3], 1000);

-- Usuario admin
INSERT INTO usuarios (cliente_id, email, password_hash, nombre, rol) VALUES
(1, 'admin@empresa.com', 
 '$2b$10$PLACEHOLDER_HASH_CAMBIAR_EN_PRODUCCION',  -- Cambiar por hash real de bcrypt
 'Administrador', 'admin');

-- =============================================
-- VISTAS ÚTILES
-- =============================================

-- Resumen de consultas por día
CREATE VIEW v_consultas_diarias AS
SELECT 
    DATE(created_at) as fecha,
    sistema_id,
    COUNT(*) as total_consultas,
    SUM(CASE WHEN exitosa THEN 1 ELSE 0 END) as exitosas,
    SUM(tokens_entrada + tokens_salida) as total_tokens,
    SUM(costo_estimado) as costo_total,
    AVG(tiempo_respuesta_ms) as tiempo_promedio_ms
FROM consultas_log
GROUP BY DATE(created_at), sistema_id
ORDER BY fecha DESC;

-- Resumen de consultas por cliente
CREATE VIEW v_consultas_cliente AS
SELECT 
    c.id as cliente_id,
    c.nombre as cliente,
    COUNT(cl.id) as total_consultas,
    SUM(cl.costo_estimado) as costo_total,
    MAX(cl.created_at) as ultima_consulta
FROM clientes c
LEFT JOIN consultas_log cl ON cl.cliente_id = c.id
GROUP BY c.id, c.nombre;

-- =============================================
-- FUNCIONES
-- =============================================

-- Función para buscar conocimientos relevantes
CREATE OR REPLACE FUNCTION buscar_conocimientos(
    p_sistema_id INT,
    p_query TEXT,
    p_limite INT DEFAULT 5
) RETURNS TABLE (
    id INT,
    titulo VARCHAR,
    contenido TEXT,
    categoria VARCHAR,
    relevancia REAL
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        k.id,
        k.titulo,
        k.contenido,
        k.categoria,
        similarity(k.titulo || ' ' || k.contenido, p_query) as relevancia
    FROM conocimientos k
    WHERE k.sistema_id = p_sistema_id
      AND k.activo = true
      AND (
          k.titulo ILIKE '%' || p_query || '%'
          OR k.contenido ILIKE '%' || p_query || '%'
          OR p_query = ANY(k.palabras_clave)
      )
    ORDER BY relevancia DESC, k.prioridad DESC, k.veces_usado DESC
    LIMIT p_limite;
END;
$$ LANGUAGE plpgsql;

-- Función para verificar límite de consultas
CREATE OR REPLACE FUNCTION verificar_limite_consultas(
    p_cliente_id INT
) RETURNS BOOLEAN AS $$
DECLARE
    v_limite INT;
    v_usadas INT;
BEGIN
    SELECT limite_consultas_dia INTO v_limite FROM clientes WHERE id = p_cliente_id;
    
    SELECT COUNT(*) INTO v_usadas 
    FROM consultas_log 
    WHERE cliente_id = p_cliente_id 
      AND DATE(created_at) = CURRENT_DATE;
    
    RETURN v_usadas < v_limite;
END;
$$ LANGUAGE plpgsql;
```

---

## 🔐 API Endpoints

### Autenticación

```typescript
// POST /api/auth/login
// Body: { email: string, password: string }
// Response: { 
//   accessToken: string, 
//   refreshToken: string, 
//   user: { id, nombre, email, rol, cliente },
//   sistemas: [{ id, codigo, nombre }]  // Sistemas permitidos
// }

// POST /api/auth/refresh
// Body: { refreshToken: string }
// Response: { accessToken: string, refreshToken: string }

// POST /api/auth/logout
// Headers: Authorization: Bearer {token}
// Response: { success: true }
```

### Consultas IA

```typescript
// POST /api/consultas
// Headers: 
//   Authorization: Bearer {token}
//   X-Sistema-Id: sistemia  (opcional, por defecto usa el primero permitido)
// Body: {
//   pregunta: string,
//   contexto?: string,        // Contexto adicional (página actual, datos seleccionados)
//   incluir_codigo?: boolean  // Si debe buscar en código fuente
// }
// Response: {
//   success: true,
//   respuesta: string,
//   fuentes: [
//     { tipo: 'conocimiento', titulo: '...', id: 1 },
//     { tipo: 'codigo', archivo: 'Services/DEXmlBuilder.cs', lineas: '45-120' }
//   ],
//   tokens: { entrada: 1500, salida: 800 },
//   tiempo_ms: 2340
// }

// GET /api/consultas/historial
// Headers: Authorization: Bearer {token}
// Query: ?desde=2026-01-01&hasta=2026-01-31&sistema=sistemia&limite=50
// Response: { consultas: [...], total: 150, pagina: 1 }

// GET /api/consultas/:id
// Response: { consulta completa con fuentes }
```

### Conocimientos (CRUD)

```typescript
// GET /api/conocimientos
// Query: ?sistema=sistemia&categoria=SIFEN&buscar=nota%20credito
// Response: { conocimientos: [...], total: 25 }

// POST /api/conocimientos
// Body: {
//   sistema_id: 3,
//   categoria: 'SIFEN',
//   titulo: 'Cómo crear una Nota de Crédito',
//   contenido: '...',
//   palabras_clave: ['nota credito', 'NC', 'devolución']
// }

// PUT /api/conocimientos/:id
// Body: { ...campos a actualizar }

// DELETE /api/conocimientos/:id

// POST /api/conocimientos/importar
// Body: { sistema_id: 3, conocimientos: [...] }  // Importación masiva
```

### Fuentes de Código

```typescript
// POST /api/fuentes/indexar
// Body: { sistema_id: 3 }  // Reindexar todas las fuentes del sistema
// Response: { 
//   archivos_procesados: 150,
//   archivos_nuevos: 5,
//   archivos_actualizados: 12,
//   errores: []
// }

// GET /api/fuentes
// Query: ?sistema=sistemia&extension=.cs&categoria=Services
// Response: { fuentes: [...] }

// GET /api/fuentes/:id
// Response: { fuente con contenido completo }
```

### Administración

```typescript
// CRUD /api/clientes
// CRUD /api/usuarios
// CRUD /api/sistemas

// GET /api/reportes/dashboard
// Response: {
//   consultas_hoy: 150,
//   consultas_mes: 4500,
//   costo_mes: 45.32,
//   sistemas_activos: 3,
//   top_preguntas: [...],
//   tendencia_semanal: [...]
// }

// GET /api/reportes/uso-por-cliente
// Query: ?desde=2026-01-01&hasta=2026-01-31

// GET /api/reportes/costos
// Query: ?periodo=mensual
```

---

## 🔧 Backend Node.js (NestJS)

### Estructura del Proyecto

```
hub-ia-central/
├── src/
│   ├── auth/
│   │   ├── auth.module.ts
│   │   ├── auth.controller.ts
│   │   ├── auth.service.ts
│   │   ├── jwt.strategy.ts
│   │   └── dto/
│   │       ├── login.dto.ts
│   │       └── register.dto.ts
│   │
│   ├── consultas/
│   │   ├── consultas.module.ts
│   │   ├── consultas.controller.ts
│   │   ├── consultas.service.ts
│   │   └── dto/
│   │       └── consulta.dto.ts
│   │
│   ├── conocimientos/
│   │   ├── conocimientos.module.ts
│   │   ├── conocimientos.controller.ts
│   │   └── conocimientos.service.ts
│   │
│   ├── fuentes/
│   │   ├── fuentes.module.ts
│   │   ├── fuentes.controller.ts
│   │   └── fuentes.service.ts
│   │
│   ├── claude/
│   │   ├── claude.module.ts
│   │   └── claude.service.ts          # Integración con Anthropic API
│   │
│   ├── clientes/
│   │   ├── clientes.module.ts
│   │   ├── clientes.controller.ts
│   │   └── clientes.service.ts
│   │
│   ├── usuarios/
│   │   ├── usuarios.module.ts
│   │   ├── usuarios.controller.ts
│   │   └── usuarios.service.ts
│   │
│   ├── reportes/
│   │   ├── reportes.module.ts
│   │   ├── reportes.controller.ts
│   │   └── reportes.service.ts
│   │
│   ├── common/
│   │   ├── guards/
│   │   │   ├── jwt-auth.guard.ts
│   │   │   └── roles.guard.ts
│   │   ├── decorators/
│   │   │   └── roles.decorator.ts
│   │   ├── interceptors/
│   │   │   └── logging.interceptor.ts
│   │   └── pipes/
│   │       └── validation.pipe.ts
│   │
│   ├── database/
│   │   ├── database.module.ts
│   │   └── entities/
│   │       ├── sistema.entity.ts
│   │       ├── conocimiento.entity.ts
│   │       ├── cliente.entity.ts
│   │       ├── usuario.entity.ts
│   │       ├── consulta-log.entity.ts
│   │       └── fuente-codigo.entity.ts
│   │
│   ├── app.module.ts
│   └── main.ts
│
├── test/
├── .env
├── .env.example
├── nest-cli.json
├── package.json
└── tsconfig.json
```

### Servicio de Claude (claude.service.ts)

```typescript
import { Injectable, Logger } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import Anthropic from '@anthropic-ai/sdk';

@Injectable()
export class ClaudeService {
  private readonly logger = new Logger(ClaudeService.name);
  private client: Anthropic;
  private model: string;
  private maxTokens: number;

  constructor(private configService: ConfigService) {
    this.client = new Anthropic({
      apiKey: this.configService.get<string>('CLAUDE_API_KEY'),
    });
    this.model = this.configService.get<string>('CLAUDE_MODEL') || 'claude-3-5-sonnet-20241022';
    this.maxTokens = this.configService.get<number>('CLAUDE_MAX_TOKENS') || 4096;
  }

  async consultar(params: {
    pregunta: string;
    sistemaId: string;
    sistemaNombre: string;
    conocimientosRelevantes: string[];
    codigoRelevante?: string[];
    contextoAdicional?: string;
  }): Promise<{
    respuesta: string;
    tokensEntrada: number;
    tokensSalida: number;
  }> {
    const systemPrompt = this.construirSystemPrompt(params);
    const userMessage = this.construirUserMessage(params);

    try {
      const startTime = Date.now();
      
      const response = await this.client.messages.create({
        model: this.model,
        max_tokens: this.maxTokens,
        system: systemPrompt,
        messages: [{ role: 'user', content: userMessage }],
      });

      const elapsed = Date.now() - startTime;
      this.logger.log(`Consulta procesada en ${elapsed}ms`);

      return {
        respuesta: response.content[0].type === 'text' ? response.content[0].text : '',
        tokensEntrada: response.usage.input_tokens,
        tokensSalida: response.usage.output_tokens,
      };
    } catch (error) {
      this.logger.error('Error consultando Claude:', error);
      throw error;
    }
  }

  private construirSystemPrompt(params: {
    sistemaId: string;
    sistemaNombre: string;
  }): string {
    return `Eres un asistente experto en el sistema "${params.sistemaNombre}" (código: ${params.sistemaId}).

Tu rol es ayudar a los usuarios con preguntas sobre el uso del sistema, resolver dudas técnicas, 
y guiarlos en los procesos.

REGLAS IMPORTANTES:
1. Responde SOLO basándote en la información proporcionada en el contexto
2. Si no tienes información suficiente, indícalo claramente
3. Usa un lenguaje claro y profesional en español
4. Si la pregunta requiere pasos, enuméralos claramente
5. Si hay código relevante, puedes referenciarlo pero no lo copies completo
6. Indica siempre las fuentes de tu respuesta

FORMATO DE RESPUESTA:
- Usa markdown para formatear
- Incluye ejemplos cuando sea útil
- Si hay advertencias importantes, resáltalas`;
  }

  private construirUserMessage(params: {
    pregunta: string;
    conocimientosRelevantes: string[];
    codigoRelevante?: string[];
    contextoAdicional?: string;
  }): string {
    let message = `PREGUNTA DEL USUARIO:\n${params.pregunta}\n\n`;

    if (params.contextoAdicional) {
      message += `CONTEXTO ADICIONAL:\n${params.contextoAdicional}\n\n`;
    }

    if (params.conocimientosRelevantes.length > 0) {
      message += `CONOCIMIENTOS RELEVANTES:\n`;
      params.conocimientosRelevantes.forEach((k, i) => {
        message += `\n--- Conocimiento ${i + 1} ---\n${k}\n`;
      });
      message += '\n';
    }

    if (params.codigoRelevante && params.codigoRelevante.length > 0) {
      message += `CÓDIGO FUENTE RELEVANTE:\n`;
      params.codigoRelevante.forEach((c, i) => {
        message += `\n--- Código ${i + 1} ---\n${c}\n`;
      });
    }

    message += `\nResponde la pregunta del usuario basándote en la información proporcionada.`;

    return message;
  }
}
```

### Servicio de Consultas (consultas.service.ts)

```typescript
import { Injectable, Logger } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { ConsultaLog } from '../database/entities/consulta-log.entity';
import { ClaudeService } from '../claude/claude.service';
import { ConocimientosService } from '../conocimientos/conocimientos.service';
import { FuentesService } from '../fuentes/fuentes.service';
import { ConsultaDto, ConsultaResponseDto } from './dto/consulta.dto';

@Injectable()
export class ConsultasService {
  private readonly logger = new Logger(ConsultasService.name);

  constructor(
    @InjectRepository(ConsultaLog)
    private consultaRepo: Repository<ConsultaLog>,
    private claudeService: ClaudeService,
    private conocimientosService: ConocimientosService,
    private fuentesService: FuentesService,
  ) {}

  async procesar(
    dto: ConsultaDto,
    usuarioId: number,
    clienteId: number,
    sistemaId: number,
    sistemaCodigo: string,
    sistemaNombre: string,
    ip: string,
  ): Promise<ConsultaResponseDto> {
    const startTime = Date.now();
    const fuentes: any[] = [];

    try {
      // 1. Buscar conocimientos relevantes
      const conocimientos = await this.conocimientosService.buscarRelevantes(
        sistemaId,
        dto.pregunta,
        5,
      );
      
      conocimientos.forEach(k => {
        fuentes.push({ tipo: 'conocimiento', id: k.id, titulo: k.titulo });
      });

      // 2. Buscar código relevante (si se solicita)
      let codigoRelevante: string[] = [];
      if (dto.incluir_codigo) {
        const archivos = await this.fuentesService.buscarRelevantes(
          sistemaId,
          dto.pregunta,
          3,
        );
        codigoRelevante = archivos.map(a => {
          fuentes.push({ 
            tipo: 'codigo', 
            archivo: a.ruta_relativa, 
            id: a.id 
          });
          return `Archivo: ${a.ruta_relativa}\n\n${a.contenido}`;
        });
      }

      // 3. Consultar a Claude
      const resultado = await this.claudeService.consultar({
        pregunta: dto.pregunta,
        sistemaId: sistemaCodigo,
        sistemaNombre: sistemaNombre,
        conocimientosRelevantes: conocimientos.map(k => 
          `Título: ${k.titulo}\nCategoría: ${k.categoria}\n\n${k.contenido}`
        ),
        codigoRelevante,
        contextoAdicional: dto.contexto,
      });

      const tiempoMs = Date.now() - startTime;

      // 4. Guardar en log
      const log = this.consultaRepo.create({
        usuario_id: usuarioId,
        cliente_id: clienteId,
        sistema_id: sistemaId,
        pregunta: dto.pregunta,
        respuesta: resultado.respuesta,
        fuentes_usadas: fuentes,
        tokens_entrada: resultado.tokensEntrada,
        tokens_salida: resultado.tokensSalida,
        costo_estimado: this.calcularCosto(resultado.tokensEntrada, resultado.tokensSalida),
        tiempo_respuesta_ms: tiempoMs,
        modelo_usado: 'claude-3-5-sonnet',
        exitosa: true,
        ip_cliente: ip,
      });
      await this.consultaRepo.save(log);

      // 5. Incrementar contador de uso de conocimientos
      await this.conocimientosService.incrementarUso(conocimientos.map(k => k.id));

      return {
        success: true,
        respuesta: resultado.respuesta,
        fuentes,
        tokens: {
          entrada: resultado.tokensEntrada,
          salida: resultado.tokensSalida,
        },
        tiempo_ms: tiempoMs,
      };
    } catch (error) {
      // Guardar error en log
      await this.consultaRepo.save({
        usuario_id: usuarioId,
        cliente_id: clienteId,
        sistema_id: sistemaId,
        pregunta: dto.pregunta,
        exitosa: false,
        error: error.message,
        ip_cliente: ip,
        tiempo_respuesta_ms: Date.now() - startTime,
      });

      throw error;
    }
  }

  private calcularCosto(tokensEntrada: number, tokensSalida: number): number {
    // Precios de Claude 3.5 Sonnet (enero 2026)
    const costoPorMilEntrada = 0.003;
    const costoPorMilSalida = 0.015;
    
    return (tokensEntrada / 1000 * costoPorMilEntrada) + 
           (tokensSalida / 1000 * costoPorMilSalida);
  }
}
```

---

## 🎨 Frontend Angular

### Estructura del Proyecto

```
hub-ia-frontend/
├── src/
│   ├── app/
│   │   ├── core/
│   │   │   ├── services/
│   │   │   │   ├── auth.service.ts
│   │   │   │   ├── api.service.ts
│   │   │   │   └── storage.service.ts
│   │   │   ├── guards/
│   │   │   │   ├── auth.guard.ts
│   │   │   │   └── role.guard.ts
│   │   │   ├── interceptors/
│   │   │   │   ├── auth.interceptor.ts
│   │   │   │   └── error.interceptor.ts
│   │   │   └── models/
│   │   │       ├── usuario.model.ts
│   │   │       ├── cliente.model.ts
│   │   │       ├── sistema.model.ts
│   │   │       └── consulta.model.ts
│   │   │
│   │   ├── features/
│   │   │   ├── auth/
│   │   │   │   ├── login/
│   │   │   │   └── auth.module.ts
│   │   │   │
│   │   │   ├── dashboard/
│   │   │   │   ├── dashboard.component.ts
│   │   │   │   └── dashboard.module.ts
│   │   │   │
│   │   │   ├── consultas/
│   │   │   │   ├── nueva-consulta/
│   │   │   │   ├── historial/
│   │   │   │   └── consultas.module.ts
│   │   │   │
│   │   │   ├── conocimientos/
│   │   │   │   ├── lista/
│   │   │   │   ├── editor/
│   │   │   │   └── conocimientos.module.ts
│   │   │   │
│   │   │   ├── clientes/
│   │   │   │   ├── lista/
│   │   │   │   ├── detalle/
│   │   │   │   └── clientes.module.ts
│   │   │   │
│   │   │   ├── usuarios/
│   │   │   │   └── usuarios.module.ts
│   │   │   │
│   │   │   ├── sistemas/
│   │   │   │   └── sistemas.module.ts
│   │   │   │
│   │   │   └── reportes/
│   │   │       ├── uso/
│   │   │       ├── costos/
│   │   │       └── reportes.module.ts
│   │   │
│   │   ├── shared/
│   │   │   ├── components/
│   │   │   │   ├── header/
│   │   │   │   ├── sidebar/
│   │   │   │   ├── loading/
│   │   │   │   └── confirm-dialog/
│   │   │   └── shared.module.ts
│   │   │
│   │   ├── layout/
│   │   │   ├── main-layout/
│   │   │   └── layout.module.ts
│   │   │
│   │   ├── app.component.ts
│   │   ├── app.module.ts
│   │   └── app-routing.module.ts
│   │
│   ├── assets/
│   ├── environments/
│   └── styles/
│
├── angular.json
├── package.json
└── tsconfig.json
```

### Pantallas Principales

#### 1. Dashboard
- Consultas hoy/semana/mes
- Costo acumulado
- Gráfico de tendencia
- Top 5 preguntas frecuentes
- Sistemas activos

#### 2. Consultas → Nueva Consulta
- Selector de sistema
- Campo de pregunta
- Checkbox "Incluir código fuente"
- Campo de contexto adicional (opcional)
- Botón Consultar
- Área de respuesta (markdown renderizado)
- Listado de fuentes usadas

#### 3. Consultas → Historial
- Filtros: fecha, sistema, usuario, cliente
- Tabla paginada con consultas
- Click para ver detalle completo
- Exportar a Excel/CSV

#### 4. Conocimientos
- Filtros: sistema, categoría
- Tabla CRUD
- Editor con preview markdown
- Importar/Exportar JSON

#### 5. Clientes
- Lista de empresas
- Crear/Editar cliente
- Asignar sistemas permitidos
- Configurar límites

#### 6. Usuarios
- Lista por cliente
- Crear/Editar usuario
- Asignar rol
- Ver actividad

#### 7. Sistemas
- Lista de sistemas registrados
- Regenerar API Key
- Configurar ruta de fuentes
- Botón "Reindexar fuentes"

#### 8. Reportes
- Uso por cliente/sistema
- Costos detallados
- Preguntas más frecuentes
- Tiempos de respuesta

---

## 🔌 Integración desde SistemIA (Blazor)

### Configuración en appsettings.json

```json
{
  "HubIACentral": {
    "Enabled": true,
    "BaseUrl": "https://192.168.100.160:3000/api",
    "SistemaId": "sistemia",
    "ApiKey": "sia_xxxxxxxxxxxx",
    "ApiSecret": "xxxxxxxxxxxx",
    "TimeoutSeconds": 60
  }
}
```

### Servicio Cliente (HubIAService.cs)

```csharp
public interface IHubIAService
{
    Task<ConsultaResponse?> ConsultarAsync(string pregunta, string? contexto = null);
    Task<bool> SincronizarConocimientosAsync(List<ArticuloConocimiento> articulos);
}

public class HubIAService : IHubIAService
{
    private readonly HttpClient _http;
    private readonly ILogger<HubIAService> _logger;
    private readonly HubIASettings _settings;

    public async Task<ConsultaResponse?> ConsultarAsync(string pregunta, string? contexto = null)
    {
        var request = new {
            pregunta,
            contexto,
            incluir_codigo = true
        };

        _http.DefaultRequestHeaders.Clear();
        _http.DefaultRequestHeaders.Add("X-Sistema-Id", _settings.SistemaId);
        _http.DefaultRequestHeaders.Add("X-API-Key", _settings.ApiKey);
        _http.DefaultRequestHeaders.Add("X-API-Secret", _settings.ApiSecret);

        var response = await _http.PostAsJsonAsync($"{_settings.BaseUrl}/consultas", request);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ConsultaResponse>();
        }
        
        return null;
    }
}
```

### Uso en AsistenteIAService

```csharp
// Si no encuentra respuesta local, consulta al Hub Central
if (respuestaLocal == null && _hubIAService.Enabled)
{
    var respuestaCentral = await _hubIAService.ConsultarAsync(mensaje, paginaActual);
    if (respuestaCentral != null)
    {
        return new RespuestaAsistente
        {
            Mensaje = respuestaCentral.Respuesta,
            TipoRespuesta = "central",
            Fuentes = respuestaCentral.Fuentes
        };
    }
}
```

---

## 📦 Despliegue

### Requisitos del Servidor (192.168.100.160)

- **OS:** Ubuntu 22.04 o Windows Server
- **Node.js:** 18 LTS o superior
- **PostgreSQL:** 16 con pgvector
- **RAM:** 4GB mínimo
- **Disco:** 50GB SSD

### Variables de Entorno (.env)

```env
# Database
DATABASE_URL=postgresql://usuario:password@localhost:5432/hub_ia_central

# JWT
JWT_SECRET=tu_secreto_super_seguro_aqui
JWT_EXPIRES_IN=24h
JWT_REFRESH_EXPIRES_IN=7d

# Claude API
CLAUDE_API_KEY=sk-ant-api03-xxxxxxx
CLAUDE_MODEL=claude-3-5-sonnet-20241022
CLAUDE_MAX_TOKENS=4096

# Server
PORT=3000
NODE_ENV=production

# Fuentes
FUENTES_BASE_PATH=/var/fuentes
```

### Docker Compose (opcional)

```yaml
version: '3.8'
services:
  api:
    build: ./backend
    ports:
      - "3000:3000"
    environment:
      - DATABASE_URL=postgresql://postgres:password@db:5432/hub_ia_central
    depends_on:
      - db
    volumes:
      - ./fuentes:/var/fuentes

  frontend:
    build: ./frontend
    ports:
      - "80:80"
    depends_on:
      - api

  db:
    image: postgres:16
    environment:
      - POSTGRES_PASSWORD=password
      - POSTGRES_DB=hub_ia_central
    volumes:
      - pgdata:/var/lib/postgresql/data
      - ./init.sql:/docker-entrypoint-initdb.d/init.sql

volumes:
  pgdata:
```

---

## 📅 Plan de Implementación

### Fase 1: Backend Base (1-2 semanas)
- [ ] Configurar proyecto NestJS
- [ ] Crear base de datos PostgreSQL
- [ ] Implementar autenticación JWT
- [ ] CRUD de Sistemas, Clientes, Usuarios

### Fase 2: Integración Claude (1 semana)
- [ ] Configurar API de Anthropic
- [ ] Implementar ClaudeService
- [ ] Crear endpoint de consultas
- [ ] Implementar logging de consultas

### Fase 3: Conocimientos y Fuentes (1 semana)
- [ ] CRUD de Conocimientos
- [ ] Servicio de indexación de código
- [ ] Búsqueda relevante

### Fase 4: Frontend Angular (2 semanas)
- [ ] Estructura base Angular
- [ ] Autenticación
- [ ] Dashboard
- [ ] Módulo de Consultas
- [ ] Módulo de Administración

### Fase 5: Integración Clientes (1 semana)
- [ ] Integrar SistemIA
- [ ] Integrar Sistema Angular
- [ ] Integrar Gasparini (si aplica)

### Fase 6: Testing y Producción (1 semana)
- [ ] Testing completo
- [ ] Documentación
- [ ] Despliegue en servidor

---

## 📞 Contacto

Para consultas sobre esta especificación, contactar al equipo de desarrollo.

**Última actualización:** 24 de enero de 2026
