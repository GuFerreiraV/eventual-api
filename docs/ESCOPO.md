Uma API completa em que organizadores criam eventos com lotes de ingressos (com preço e quantidade), participantes compram ingressos, e a entrada é validada pela leitura de um QR code único.

Por que se encaixa no seu nível: em ASP.NET (intermediário), o projeto exige arquitetura em camadas, autenticação JWT com papéis (organizador e participante) e controle de concorrência para não vender acima do limite do lote. Também traz geração de QR code, testes de integração com WebApplicationFactory e deploy. No Swagger (básico para intermediário), você vai documentar o JWT na UI, rotas que devolvem imagem (image/png), upload do banner do evento com IFormFile e versionamento da API (v1). Com o deploy no Azure ou no Render, o Swagger público vira a vitrine do portfólio.

Escopo: mais de 1 mês

Ponto de partida:

POST /events e POST /events/{id}/batches para criar o evento e os lotes (organizador)
POST /orders compra ingressos e garante o estoque com controle de concorrência
GET /tickets/{id}/qrcode devolve o QR code em PNG
POST /checkin valida o código e impede que o mesmo ingresso entre duas vezes