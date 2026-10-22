## Pacotes & Dependências
```shell
dotnet add package Microsoft.AspNetCore.OpenApi NSwag.AspNetCore
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design   // ferramenta de migrations

/* output
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.13" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.3" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.13" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Relational" Version="10.0.13" />
<PackageReference Include="NSwag.AspNetCore" Version="14.7.1" />
*/
```
## Desenvolvimento local

O banco é **PostgreSQL 17** em container. A senha nunca vai para o git: crie `secrets/db_password.txt` (na raiz, copiando `secrets/db_password.txt.example`) com a senha que quiser.

### Modo 1: tudo no compose (API + Postgres)
```shell
docker compose up -d --build   // API em http://localhost:8080/swagger
docker compose down            // para os containers; os dados ficam no volume eventual-data
docker compose down -v         // para e APAGA o volume (banco zerado)
```
A senha chega aos dois containers como **Docker secret** (arquivo em `/run/secrets/Database__Password`). O Postgres lê via `POSTGRES_PASSWORD_FILE`; a API lê via `AddKeyPerFile`, que vira a chave `Database:Password`. Dentro do compose a API acessa o banco pelo nome do serviço (`Host=postgres`).

### Modo 2: só o Postgres no container, API com `dotnet run`
```shell
cd Eventual.Api
dotnet user-secrets set "Database:Password" "<mesma senha do arquivo>"   // uma vez
cd ..
docker compose up -d postgres
cd Eventual.Api
dotnet dev-certs https --trust
dotnet run --launch-profile https
```
Aqui a API roda fora do container (não enxerga `/run/secrets`), então a senha vem do **user-secrets** do .NET e o banco é `localhost:5432`.

Nos dois modos, em Development a API aplica as **migrations** pendentes ao iniciar. Nova mudança no modelo: `dotnet ef migrations add <Nome> --project Eventual.Api`.

### Testes
```shell
dotnet test
```
Exige o **Docker ligado**. O Testcontainers sobe um Postgres descartável (separado do banco de dev), cria um banco novo por teste e remove o container no fim.

## Testando pelo Swagger

Com a aplicação rodando, abra **https://localhost:7201/swagger** (modo 2) ou **http://localhost:8080/swagger** (modo 1).

Em cada rota: clique nela → **Try it out** → preencha os parâmetros/corpo → **Execute**. A resposta aparece logo abaixo, com o status code e o corpo.

> O banco é **PostgreSQL** com volume: os dados sobrevivem a reinícios. Para começar do zero, `docker compose down -v`. Se a aplicação já estava aberta antes de uma mudança no código, pare-a (`Ctrl+C`) e rode `dotnet run` de novo (ou `docker compose up -d --build` no modo 1).

### Roteiro: do evento à reserva

**1. Criar um evento** — `POST /events`

```json
{
  "name": "Show de Rock",
  "description": "Noite de bandas locais",
  "local": "Arena Central",
  "start": "2026-01-01T20:00:00Z",
  "end": "2027-12-31T23:59:00Z"
}
```

Esperado: **201**, com o `id` do evento (ex.: `1`) e `lotes: []`. As datas estão em UTC (o `Z` no final). O exemplo usa um evento já iniciado e com `end` no futuro, para que todo o roteiro (compra e check-in) funcione hoje.

**2. Criar lotes no evento** — `POST /events/{eventId}/lotes` com `eventId = 1`

```json
{ "name": "Pista", "price": 80, "quantity": 10 }
```

Esperado: **201**, com `available: 10`. Crie um segundo lote pequeno para testar o esgotamento:

```json
{ "name": "Camarote", "price": 200, "quantity": 2 }
```

**3. Consultar**

| Rota | O que conferir |
|---|---|
| `GET /events` | lista resumida, sem lotes |
| `GET /events/1` | o evento com os dois lotes e o `available` de cada um |
| `GET /events/1/lotes` | só os lotes do evento |
| `GET /lotes/1` | um lote isolado |

**4. Reservar ingressos** — `POST /lotes/{loteId}/orders` com `loteId = 1`

```json
{ "quantity": 2, "buyerName": "Ana Souza", "buyerEmail": "ana@exemplo.com" }
```

Esperado: **201**, com `status: "Pending"`, `totalAmount: 160` (80 × 2), `expiresAt` 15 minutos depois de `createdAt` e `tickets: []` (os ingressos só são gerados após o pagamento).

**5. Conferir o efeito da reserva**

- `GET /orders/1` → o pedido criado.
- `GET /lotes/1` → `available` caiu de 10 para **8**. A vaga fica reservada por 15 minutos; depois disso, se o pedido não for pago, ela volta a contar como disponível e o pedido aparece como `Expired`.

**6. Pagar o pedido** — `POST /orders/{id}/payment` com `id = 1`

O gateway de pagamento do MVP é falso: o resultado é escolhido pelo token enviado.

| Token | Resultado |
|---|---|
| `tok_approved` | aprova |
| `tok_declined` | recusa ("recusado pelo emissor do cartão") |
| qualquer outro | recusa ("token de pagamento inválido") |

Primeiro, simule uma recusa:

```json
{ "paymentToken": "tok_declined" }
```

Esperado: **402**. O pedido **continua `Pending`** e segura a vaga: dá para tentar de novo até ele expirar. Agora pague de verdade:

```json
{ "paymentToken": "tok_approved" }
```

Esperado: **200**, com `status: "Paid"` e `tickets` com 2 ingressos, cada um com um `code` (o conteúdo do QR code) e `checkedInAt: null`.

**7. Pagar de novo o mesmo pedido** — repita o passo anterior com `tok_approved`

Esperado: **200**, com **os mesmos** ingressos. Pagar um pedido já pago não gera erro nem ingressos novos (cliques duplos são seguros).

**8. Fazer o check-in na portaria** — `POST /events/{eventId}/check-in` com `eventId = 1`

Copie o `code` de um dos ingressos da resposta do pagamento (é o conteúdo do QR code):

```json
{ "code": "cole-aqui-o-code-do-ingresso" }
```

Esperado: **200**, com `code`, `buyerName`, `loteName` e `checkedInAt`. A portaria abre **30 minutos antes** do `start` do evento e fecha no `end`. O evento do roteiro já começou, então o check-in é aceito. Para ver a portaria fechada, crie um evento com `start` daqui a algumas horas.

**9. Ler o mesmo QR de novo** — repita o passo anterior com o mesmo `code`

Esperado: **409** — "já foi utilizado às HH:mm (UTC)". Um ingresso só entra uma vez, mesmo que dois leitores o leiam ao mesmo tempo.

### Roteiro: provocando os erros

| O que fazer | Rota e corpo | Esperado |
|---|---|---|
| Evento inexistente | `GET /events/99` | **404** — "Evento 99 não encontrado." |
| Lote inexistente | `GET /lotes/99` | **404** |
| Pedido inexistente | `GET /orders/99` | **404** |
| Datas invertidas | `POST /events` com `end` antes de `start` | **400** — erro no campo `End` |
| Lote inválido | `POST /events/1/lotes` com `{ "name": "", "quantity": 0 }` | **400** — erros em `Name`, `Price` e `Quantity` |
| Mais de 5 ingressos | `POST /lotes/1/orders` com `"quantity": 6` | **400** — "máximo 5 ingresso(s)" |
| Esgotar o lote | `POST /lotes/2/orders` com `"quantity": 2` (passa) e de novo com `"quantity": 1` | **201** e depois **409** — "apenas 0 ingresso(s) disponível(is)" |
| Evento já terminado | crie um evento com `end` no passado, um lote nele, e tente reservar | **409** — "o evento já terminou" |
| Dados do comprador inválidos | `POST /lotes/1/orders` com `"buyerEmail": "nao-e-email"` | **400** — erro em `BuyerEmail` |
| Pagamento recusado | `POST /orders/1/payment` com `{ "paymentToken": "tok_declined" }` | **402** — o pedido continua `Pending` |
| Pagamento sem token | `POST /orders/1/payment` com `{}` | **400** — erro em `PaymentToken` |
| Pagar pedido inexistente | `POST /orders/99/payment` com `tok_approved` | **404** — "Pedido 99 não encontrado." |
| Pagar pedido vencido | espere 15 min após reservar e pague | **409** — "o pedido expirou"; o pedido passa a `Expired` |
| Check-in repetido | `POST /events/1/check-in` duas vezes com o mesmo `code` | **200** e depois **409** — "já foi utilizado às HH:mm" |
| Ingresso de outro evento | `POST /events/2/check-in` com o `code` de um ingresso do evento 1 | **400** — "não pertence ao evento 2" |
| Ingresso inexistente | `POST /events/1/check-in` com `{ "code": "11111111-1111-1111-1111-111111111111" }` | **404** — "Ingresso … não encontrado." |
| Portaria ainda fechada | check-in em evento cujo `start` está a mais de 30 min | **409** — "o check-in abre às HH:mm" |
| Evento encerrado | check-in depois do `end` do evento | **409** — "o check-in está encerrado" |
| Check-in sem código | `POST /events/1/check-in` com `{}` | **400** — erro em `Code` |

Todas as respostas de erro seguem o formato **ProblemDetails** (`title`, `status`, `detail`, e `type` quando houver). Erros de validação trazem também o campo `errors`, com a mensagem de cada campo inválido.

## Referência das rotas

Comportamento esperado de cada rota. Status de erro vêm sempre como ProblemDetails.

### Eventos

| Rota | Corpo | Sucesso | Erros |
|---|---|---|---|
| `GET /events` | — | **200** lista resumida (`id`, `name`, `local`, `start`, `end`), ordenada por `start` | — |
| `GET /events/{eventId}` | — | **200** evento completo com `lotes` (cada um com `available`) | **404** evento inexistente |
| `POST /events` | `name`*, `description`, `local`*, `start`*, `end`* | **201** evento criado + header `Location` | **400** campo obrigatório ausente, texto longo demais ou `end` ≤ `start` |

### Lotes

| Rota | Corpo | Sucesso | Erros |
|---|---|---|---|
| `GET /events/{eventId}/lotes` | — | **200** lotes do evento | **404** evento inexistente |
| `POST /events/{eventId}/lotes` | `name`*, `price`* (≥ 0), `quantity`* (≥ 1) | **201** lote com `available = quantity` + `Location: /lotes/{id}` | **400** validação · **404** evento inexistente |
| `GET /lotes/{id}` | — | **200** lote com `available` | **404** lote inexistente |

`available` = `quantity` − ingressos de pedidos `Paid` − ingressos de pedidos `Pending` ainda dentro do prazo.

### Pedidos

| Rota | Corpo | Sucesso | Erros |
|---|---|---|---|
| `POST /lotes/{loteId}/orders` | `quantity`* (1 a 5), `buyerName`*, `buyerEmail`* | **201** pedido `Pending`, `expiresAt` = criação + 15 min, `tickets: []` | **400** validação ou mais de 5 ingressos · **404** lote inexistente · **409** evento já terminou, lote sem vagas suficientes ou disputa excessiva pela mesma vaga (tente de novo) |
| `GET /orders/{id}` | — | **200** pedido com `tickets`; um `Pending` vencido aparece como `Expired` | **404** pedido inexistente |
| `POST /orders/{id}/payment` | `paymentToken`* | **200** pedido `Paid` com um ingresso por unidade; se já estava pago, devolve o mesmo pedido | **400** token ausente · **402** pagamento recusado (pedido segue `Pending`) · **404** pedido inexistente · **409** pedido expirado |

### Check-in

| Rota | Corpo | Sucesso | Erros |
|---|---|---|---|
| `POST /events/{eventId}/check-in` | `code`* (Guid do QR) | **200** `code`, `buyerName`, `loteName`, `checkedInAt` | **400** código ausente ou ingresso de outro evento · **404** evento ou ingresso inexistente · **409** portaria ainda fechada (abre 30 min antes do `start`), evento encerrado ou ingresso já utilizado (com o horário da primeira entrada) |

A resposta mostra só o necessário para a portaria: sem e-mail, valor pago ou id do pedido. As checagens seguem esta ordem: evento → ingresso → pertence ao evento → horário → já utilizado.

`*` = obrigatório.

**Regras de concorrência:**
- Duas reservas simultâneas nunca vendem além do lote: a segunda é recalculada e, sem vagas, recebe 409.
- Dois pagamentos simultâneos do mesmo pedido geram ingressos **uma única vez**; as duas requisições recebem 200 com o mesmo pedido.
- Duas leituras simultâneas do mesmo QR registram **uma única entrada**; a outra recebe 409.

