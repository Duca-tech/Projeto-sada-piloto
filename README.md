Utilizei Claude Code para realizar a documentação.

# SADA – Gestão de Tarefas (API)


API RESTful em **.NET 9** para gestão de tarefas. Ela permite cadastrar, consultar, listar com filtros, editar e excluir tarefas. Os dados ficam em memória com **Entity Framework Core InMemory**, então não é preciso instalar banco de dados.

---

## Sumário

- [Tecnologias](#tecnologias)
- [Arquitetura](#arquitetura)
- [Estrutura do projeto](#estrutura-do-projeto)
- [Como rodar](#como-rodar)
- [Endpoints](#endpoints)
- [Como testar](#como-testar)
- [Validações e códigos HTTP](#validações-e-códigos-http)
- [Logging](#logging)

---

## Tecnologias

| Item | Uso |
|---|---|
| .NET 9 / ASP.NET Core | API Web com controllers |
| Entity Framework Core InMemory | Persistência em memória |
| Swashbuckle (Swagger) | Documentação interativa da API |
| Data Annotations | Validação dos dados de entrada |
| ILogger + HTTP Logging | Log da aplicação e das requisições |

---

## Arquitetura

O projeto segue uma **arquitetura em camadas inspirada na Clean Architecture**. Cada camada tem uma responsabilidade e as dependências apontam sempre **para dentro**, em direção ao domínio:

```
API_SADA  ──►  Application  ──►  Domain
   │                ▲
   └──►  Infraestructure ┘
```

| Camada | Responsabilidade |
|---|---|
| **Domain** | Entidades (`TarefaInfo`, `LogInfo`), enums de status e rotinas utilitárias (`Rotina`). Não depende de nenhuma outra camada. |
| **Application** | Casos de uso (`TarefaCasoUso`), validações (`TarefaService`), DTOs de entrada e saída e a **interface** do repositório (`ITarefaRespositorio`). |
| **Infraestructure** | Implementação do repositório com EF Core (`TarefaRepository`), o `DbContext` e a gravação do log de auditoria (`LogBase`). |
| **API_SADA** | Controllers HTTP, configuração da injeção de dependência, Swagger e logging. |

### Por que este modelo?

- **Separação de responsabilidades:** o controller só traduz HTTP, as regras ficam no caso de uso e a persistência fica isolada no repositório. Cada parte pode mudar sem afetar as outras.
- **Inversão de dependência (o "D" do SOLID):** a camada Application define a interface `ITarefaRespositorio` e a Infraestructure a implementa. Trocar o InMemory por SQL Server, por exemplo, exige mudar só a Infraestructure.
- **Testabilidade:** como o caso de uso depende de uma interface, o repositório pode ser substituído por um falso nos testes.
- **Injeção de dependência:** cada camada registra seus serviços (`AddDependenciaApplication` e `AddDependenciaRepository`), e o `Program.cs` só os conecta.

---

## Estrutura do projeto

```
API Project SADA/
├── API_SADA/                       # Camada de apresentação (Web API)
│   ├── Controllers/TarefaController.cs
│   ├── Program.cs                  # DI, Swagger e logging
│   └── API_SADA.http               # Requisições prontas para testar
├── Application/                    # Casos de uso e contratos
│   └── Tarefa/
│       ├── Dto/                    # TarefaRequest, TarefaResponse, TarefaBuscaResponse, TarefaListRequest, TarefaListResponse
│       ├── ITarefaRespositorio.cs  # Interface do repositório
│       ├── TarefaCasoUso.cs        # Regras e orquestração
│       └── TarefaService.cs        # Validações de status e paginação
├── Domain/                         # Entidades e regras puras
│   ├── Tarefa/                     # TarefaInfo, EnumTarefa
│   ├── Log/                        # LogInfo, EnumLog
│   └── Rotina.cs                   # Conversão de datas, normalização de texto, validação de GUID
└── Infraestructure/                # Persistência
    ├── DbContext.cs
    ├── LogBase/LogBase.cs          # Log de auditoria no banco
    └── TarefaInfra/TarefaRepository.cs
```

---

## Como rodar

### Pré-requisitos

- [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0) ou superior

### Pela linha de comando

Dentro da pasta `API Project SADA`:

```bash
dotnet restore
dotnet run --project API_SADA --launch-profile http
```

A API sobe em **http://localhost:5232**. Para abrir o Swagger, acesse:

> **http://localhost:5232/swagger**

### Pelo Visual Studio

1. Abra `API Project SADA.sln`.
2. Defina **API_SADA** como projeto de inicialização.
3. Pressione **F5**. O navegador abre direto no Swagger.

> Como os dados ficam em memória, eles **são perdidos ao reiniciar** a aplicação.

---

## Endpoints

URL base: `/api/tarefa`

| Método | Rota | Descrição | Sucesso |
|---|---|---|---|
| `POST` | `/api/tarefa` | Cadastra uma tarefa e devolve o código único (GUID) | `201 Created` |
| `GET` | `/api/tarefa` | Lista as tarefas, com filtros opcionais e paginação | `200 OK` |
| `GET` | `/api/tarefa/{id}` | Busca uma tarefa pelo código | `200 OK` |
| `PUT` | `/api/tarefa/{id}` | Edita título, descrição, data de vencimento e status | `200 OK` |
| `DELETE` | `/api/tarefa/{id}` | Exclui uma tarefa | `204 No Content` |

### Corpo da requisição (POST e PUT)

```json
{
  "titulo": "Preparar apresentação",
  "descricao": "Montar os slides",
  "dataVencimento": "31/12/2026",
  "status": "Pendente"
}
```

| Campo | Obrigatório | Regras |
|---|---|---|
| `titulo` | Sim | De 3 a 100 caracteres |
| `descricao` | Não | Texto livre |
| `dataVencimento` | Sim | Formato `dd/MM/yyyy` |
| `status` | Sim | `Pendente`, `EmProgresso` ou `Concluida` (aceita acentos, espaços e maiúsculas/minúsculas, como `Em progresso` e `Concluída`) |

No `PUT`, o código da tarefa vem **na rota**, e um eventual `id` no corpo é ignorado.

### Filtros da listagem (query string)

| Parâmetro | Exemplo | Descrição |
|---|---|---|
| `status` | `Pendente` | Filtra pelo status |
| `dataVencimento` | `31/12/2026` | Filtra pela data de vencimento |
| `pagina` | `1` | Página (padrão 1, mínimo 1) |
| `tamanho` | `10` | Itens por página (padrão 10, de 1 a 100) |

Os filtros podem ser combinados, por exemplo `GET /api/tarefa?status=Pendente&dataVencimento=31/12/2026`.

Formato da resposta:

```json
{
  "total": 1,
  "dados": [ { "id": "…", "titulo": "…", "descricao": "…", "dataVencimento": "2026-12-31T00:00:00", "status": 1 } ],
  "pagina": 1,
  "totalPaginas": 1,
  "erro": null,
  "status": 200
}
```

> No retorno, o `status` da tarefa é numérico: `1` = Pendente, `2` = EmProgresso, `3` = Concluida.

---

## Como testar

### 1. Swagger (recomendado)

Com a API rodando, abra **http://localhost:5232/swagger**. Cada endpoint tem descrição, parâmetros e códigos de resposta documentados, e pode ser executado ali mesmo em **Try it out**.

Roteiro sugerido:

1. `POST /api/tarefa` com o corpo de exemplo e copie o `id` retornado.
2. `GET /api/tarefa/{id}` com esse id.
3. `GET /api/tarefa?status=Pendente` para ver o filtro.
4. `PUT /api/tarefa/{id}` mudando o status para `Concluida`.
5. `DELETE /api/tarefa/{id}`; depois o `GET` desse id deve retornar **404**.

### 2. Arquivo `.http` (Visual Studio / VS Code)

O arquivo [`API_SADA/API_SADA.http`](API_SADA/API_SADA.http) já tem todas as requisições. O id criado no `POST` é reaproveitado automaticamente nas requisições seguintes.

### 3. curl

```bash
# Criar
curl -i -X POST http://localhost:5232/api/tarefa \
  -H "Content-Type: application/json" \
  -d '{"titulo":"Estudar EF Core","dataVencimento":"31/12/2026","status":"Pendente"}'

# Listar com filtro
curl "http://localhost:5232/api/tarefa?status=Pendente"

# Buscar / editar / excluir (troque <id> pelo código retornado)
curl http://localhost:5232/api/tarefa/<id>
curl -X PUT http://localhost:5232/api/tarefa/<id> -H "Content-Type: application/json" \
  -d '{"titulo":"Estudar EF Core","dataVencimento":"31/12/2026","status":"Concluida"}'
curl -i -X DELETE http://localhost:5232/api/tarefa/<id>
```

---

## Validações e códigos HTTP

| Código | Quando |
|---|---|
| `200 OK` | Busca, listagem ou edição com sucesso |
| `201 Created` | Tarefa criada. O header `Location` aponta para `/api/tarefa/{id}` |
| `204 No Content` | Tarefa excluída |
| `400 Bad Request` | Título ausente ou fora do tamanho, status inválido, data fora do formato `dd/MM/yyyy`, id que não é GUID, paginação inválida ou JSON malformado |
| `404 Not Found` | Tarefa não encontrada |
| `409 Conflict` | Tentativa de criar uma tarefa com um `id` que já existe |
| `500 Internal Server Error` | Erro inesperado (registrado no log) |

As validações acontecem em duas etapas:

1. **Na entrada (Data Annotations):** o ASP.NET valida o DTO automaticamente e devolve `400` com a lista de erros (`ValidationProblemDetails`) antes de chegar ao caso de uso.
2. **No caso de uso (`TarefaCasoUso`):** valida status, formato da data, GUID, existência da tarefa e paginação. Responde com uma mensagem clara e o código HTTP correspondente.

---

## Logging

A API tem três níveis de log:

1. **Log de requisições (HTTP Logging):** cada chamada registra no console o método, a rota, a query, o status da resposta e a duração.
2. **Log da aplicação (`ILogger`):** o `TarefaCasoUso` registra
   - `Information` em criação, edição e exclusão com sucesso;
   - `Warning` em entradas inválidas ou tarefas não encontradas;
   - `Error` com a exceção completa em falhas inesperadas.
3. **Log de auditoria no banco (`LogBase`):** cada cadastro, alteração e exclusão grava um registro `LogInfo` com o tipo da operação e o código da tarefa.

Os níveis podem ser ajustados no `appsettings.json`, na seção `Logging:LogLevel`.

Exemplo de saída no console:

```
info: Application.Tarefa.TarefaCasoUso[0]
      Tarefa 697afb7d-8968-41e4-929c-d20b817b7f77 criada com sucesso
info: Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware[9]
      Request and Response:
      Method: POST
      PathBase: 
      Path: /api/tarefa
      QueryString: 
      StatusCode: 201
      Duration: 41,3
```
