# Introdução ao LightningArc

O **LightningArc** é um ecossistema de bibliotecas C# projetado para acelerar o desenvolvimento de aplicações modernas, focando em robustez, tipagem forte e padrões arquiteturais limpos.

O projeto nasceu com o objetivo de fornecer ferramentas que faltam ou que são excessivamente complexas no dia a dia do desenvolvedor .NET, como um gerenciamento funcional de erros e abstrações simplificadas para acesso a dados.

---

## 🏛️ Arquitetura do Ecossistema

O ecossistema é dividido em duas grandes categorias:

### 1. Core (Fundação)
Bibliotecas agnósticas de framework que definem os padrões base do sistema.
- **`LightningArc.Results`**: Implementação completa do padrão Result.
- **`LightningArc.Primitives`**: Contratos, Value Objects e tipos base.
- **`LightningArc.Json`**: Utilitários para serialização moderna.
- **`LightningArc.Core`**: O "canivete suíço" com helpers generalistas.

### 2. Integrations (Extensões)
Pontes de integração com frameworks populares da indústria.
- **`LightningArc.*.AspNetCore`**: Mapeamentos HTTP, CORS, OpenAPI.
- **`LightningArc.Data.*`**: Implementações para ADO.NET, Dapper e Entity Framework Core.
- **`LightningArc.Mappers.*`**: Adaptadores para AutoMapper e Mapster.
- **`LightningArc.Metalama`**: Integração com Programação Orientada a Aspectos.

---

## 💡 Filosofia de Design

1.  **Fail-Fast e Funcional**: Preferimos o uso de `Result` em vez de exceções para fluxo normal de negócio.
2.  **Extensibilidade**: A maioria das classes é projetada para ser herdada ou estendida (protegendo a API interna com `internal` e expondo o necessário com `protected`).
3.  **Modernidade**: Utilizamos os recursos mais recentes do C# (12/13+) e focamos em .NET 8.0 e 9.0/10.0.
4.  **Ergonomia**: Operadores implícitos, desconstrução e lógicos para que a biblioteca pareça nativa da linguagem.

---

## 🛣️ Próximos Passos
- Entenda como tratar erros com o **[Padrão Result](../core-features/result-pattern.md)**.
- Veja como simplificar seus controllers com a **[Integração AspNetCore](../web-integration/results-mapping.md)**.
- Explore as facilidades de **[Acesso a Dados](../data-access/abstractions.md)**.
