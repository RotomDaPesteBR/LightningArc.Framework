# Documentação da Biblioteca Results

A biblioteca **`LightningArc.Results`** implementa o **Padrão Result** para o gerenciamento explícito e estruturado de sucessos e falhas operacionais. Seu objetivo é promover a criação de código **limpo, expressivo e robusto**, eliminando a dependência excessiva de exceções para o controle de fluxo normal.

-----

## 1. Tipos de Resultados

| Tipo | Uso | Descrição |
| :--- | :--- | :--- |
| **`Result<TValue>`** | Retorno com valor. | Representa o resultado de uma operação que contém um valor de sucesso (`TValue`) ou uma informação de falha (`Error`). |
| **`Result`** | Retorno sem valor. | Destinado a operações que apenas retornam um status (sucesso/falha). |
| **`Error`** | Falha. | Classe base tipada para estruturar o motivo da falha, incluindo códigos numéricos e detalhes (`ErrorDetail`). |
| **`Success`** | Sucesso. | Base para resultados de sucesso padronizados. |

-----

## 2. Ergonomia e Açúcar Sintático

A biblioteca utiliza recursos modernos do C# para tornar o uso de resultados o mais natural possível.

### 2.1. Operadores Booleanos
Você pode usar objetos `Result` diretamente em expressões condicionais:

```csharp
Result result = DoWork();

if (result) { /* Sucesso */ }
if (!result) { /* Falha */ }
```

### 2.2. Desconstrução (Deconstruction)
Extraia os valores de um resultado ou erro de forma posicional:

```csharp
// Para Result<T>
var (isSuccess, value, error) = result;

// Para Error
var (code, message, details) = error;
```

### 2.3. Acesso Seguro (TryGetValue / TryGetError)
Evite exceções ao acessar valores ou erros de forma segura:

```csharp
Result<int> result = DoWork();

if (result.TryGetValue(out int value))
{
    // Sucesso, 'value' contém o resultado
}

Result failureResult = DoSomethingElse();
if (failureResult.TryGetError(out Error error))
{
    // Falha, 'error' contém o motivo
}
```

### 2.4. Conversões Explícitas
Acesse o valor ou o erro de um `Result<T>` através de casting (gera exceção se o estado for inválido):

```csharp
int val = (int)result;   // Extrai o valor
Error err = (Error)result; // Extrai o erro
```

-----

## 3. Composição de Fluxo (LINQ-style)

Utilize métodos de extensão para encadear operações sem blocos `if` aninhados:

| Método | Finalidade |
| :--- | :--- |
| **`Bind`** | Encadeia operações que também retornam `Result`. Interrompe no primeiro erro. |
| **`Map`** | Transforma o valor de sucesso em outro tipo. |
| **`Tap`** | Executa um efeito colateral (ex: Log) apenas em caso de sucesso. |
| **`OnFailure`** | Executa uma ação apenas em caso de falha. |
| **`Match`** | Reduz o resultado para um valor único tratando ambos os casos (Sucesso/Falha). |

-----

## 4. Gerenciamento de Erros (`Error`)

### 4.1. Estrutura de Código
O código é composto por um **Prefixo** (Módulo) e um **Sufixo** (Erro específico).
`Code = (Prefix * 1000) + Suffix`.

### 4.2. Agregação de Erros (`AggregateError`)
Você pode combinar múltiplos erros usando o operador `+`. Isso é útil para acumular falhas de validação:

```csharp
Error finalError = error1 + error2 + error3;

if (finalError is AggregateError agg)
{
    var allErrors = agg.Errors; // Lista individual de erros
    var flat = agg.Flatten();   // Achata hierarquias de erros agregados
}
```

### 4.3. Detalhes de Erro (`ErrorDetail`)
Adicione contexto específico (como nome do campo) aos erros:

```csharp
return Error.Validation.InvalidParameter("Dados inválidos", [
    ("Nome", "Obrigatório"),
    ("Idade", "Deve ser maior que 18")
]);
```

-----

## 5. Integração com ASP.NET Core

Para converter automaticamente `Result` em respostas HTTP padronizadas (RFC 7807), utilize o pacote **`LightningArc.Results.AspNetCore`**:

```csharp
[HttpGet]
public EndpointResult<User> Get() => _service.GetUser(); 
// Retorna 200 OK com o User ou 4xx/5xx baseado no Error.
```
