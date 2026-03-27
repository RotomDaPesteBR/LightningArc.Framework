# Documentação do Módulo Results.AspNetCore

O módulo **`LightningArc.Results.AspNetCore`** fornece uma ponte robusta entre o domínio da aplicação (que utiliza `Result<T>`) e a camada Web (ASP.NET Core), automatizando a conversão de resultados em respostas HTTP padronizadas.

-----

## 1. Adaptadores de Endpoint (`EndpointResult`)

As classes `EndpointResult` e `EndpointResult<TValue>` eliminam o código repetitivo de conversão manual. Elas implementam `IResult` e suportam conversão implícita.

```csharp
[HttpGet("{id}")]
public async Task<EndpointResult<Product>> Get(int id)
{
    // Retorna Result<Product> diretamente.
    // O adaptador converte para 200 OK ou 4xx/5xx baseado no erro.
    return await _service.GetProduct(id);
}
```

### Comportamento Automático:
- **Sucesso**: Mapeia para o código HTTP correspondente ao tipo de `Success` (ex: `Ok` -> 200, `Created` -> 201).
- **Falha**: Converte o objeto `Error` em um **Problem Details (RFC 7807)** com o Status Code apropriado.

-----

## 2. Tratamento Global de Exceções

A biblioteca inclui o **`ResultExceptionHandler`**, que captura exceções não tratadas e as transforma em objetos `Error` padronizados, garantindo que sua API nunca retorne um stack trace bruto ou respostas inconsistentes.

### Configuração:
No `Program.cs` (requer .NET 8.0+):

```csharp
builder.Services.AddEndpointResults(); // Já registra o ExceptionHandler
// ...
app.UseExceptionHandler(); // Ativa o pipeline de exceções do ASP.NET Core
```

### Mapeamentos de Exceção Padrão:
| Exceção | Erro Result | Status HTTP |
| :--- | :--- | :--- |
| `ValidationException` | `Validation.InvalidParameter` | 400 Bad Request |
| `UnauthorizedAccessException` | `Authentication.Forbidden` | 403 Forbidden |
| `DbException` | `Database.ConnectionFailed` | 500 Internal Server Error |
| `NotImplementedException` | `Application.NotImplemented` | 501 Not Implemented |

-----

## 3. Configuração de Mapeamento

Você pode centralizar a tradução entre erros de domínio e contratos HTTP:

```csharp
builder.Services.AddEndpointResults(configureMappings: (successes, errors) =>
{
    errors.Map<MyDomainError>(HttpStatusCode.Conflict, "Título do Erro", "urn:api:error-type");
});
```

-----

## 4. Documentação Automática de Erros

Mantenha a documentação da sua API sempre atualizada com o vocabulário de erros do domínio:

```csharp
if (app.Environment.IsDevelopment())
{
    // Gera um arquivo Markdown com todos os erros conhecidos e seus mapeamentos HTTP
    app.OutputErrorsList("Docs/Errors.md");
}
```
