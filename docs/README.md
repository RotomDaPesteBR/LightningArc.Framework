# LightningArc Documentation

Bem-vindo à documentação oficial do ecossistema **LightningArc**. Esta biblioteca foi projetada para fornecer padrões arquiteturais robustos e utilitários de alta performance para aplicações .NET modernas.

## 🚀 Guia Rápido
- **[Introdução](getting-started/introduction.md)**: Visão geral e filosofia do projeto.
- **[Padrão Result](core-features/result-pattern.md)**: Como tratar erros de forma funcional e elegante.
- **[Integração Web](web-integration/results-mapping.md)**: Automatizando respostas HTTP no ASP.NET Core.

---

## 📚 Seções

### 1. [Core Features](core-features/result-pattern.md)
O coração do ecossistema.
- [Result Pattern](core-features/result-pattern.md): Sucessos, Falhas e Agregação de Erros.
- [Value Objects](core-features/value-objects.md): Tipos primitivos com regras de negócio (Email, etc).
- [JSON Serialization](core-features/json-serialization.md): Conversores e extensões para `System.Text.Json`.

### 2. [Data Access](data-access/abstractions.md)
Padronização da camada de persistência.
- [Abstractions](data-access/abstractions.md): Repositórios e Unit of Work.
- [ADO.NET & Dapper](data-access/ado-dapper.md): Implementações leves para SQL Server e Oracle.
- [Entity Framework Core](data-access/entity-framework.md): Repositório base para EF.
- [Mappers](data-access/mappers.md): Adaptadores para AutoMapper e Mapster.

### 3. [Web Integration](web-integration/results-mapping.md)
Extensões específicas para ASP.NET Core.
- [HTTP Mapping](web-integration/results-mapping.md): Conversão automática de `Result` para RFC 7807 (Problem Details).
- [CORS Policies](web-integration/cors.md): Configurações simplificadas de segurança.
- [OpenAPI](web-integration/openapi.md): Suporte aprimorado para Swagger e documentação de API.

### 3. [Analyzers](analyzers/README.md)
Análise estática em tempo de compilação para o ecossistema.
- [Regras Result](analyzers/result-rules.md): LARC001–LARC003 — Uso seguro de `.Value`, `.Error` e descarte de `Result`.
- [Regras ValueObject](analyzers/value-object-rules.md): LARC010–LARC012 — Conversão implícita e criação descartada.
- [Regras Data & Infra](analyzers/infra-rules.md): LARC020–LARC030 — Conexões síncronas, HostedServices vazios e `ReleaseConnection(null)`.

### 4. [Advanced Topics](advanced/metalama.md)
Recursos para cenários complexos.
- [Metalama](advanced/metalama.md): Programação orientada a aspectos (AOP) em tempo de compilação.
- [Internals](advanced/internals/README.md): Detalhes técnicos de implementação.

---

## 🛠️ Referência Técnica
- **[API Reference](api/README.md)**: Documentação detalhada de namespaces e classes.
- **[AI Skill](skill/SKILL.md)**: Contexto otimizado para assistentes de IA (Claude Code/Gemini).
