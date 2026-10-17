namespace eventual.Exceptions;

public class LoteSoldOutException(int loteId, int available)
     : DomainException($"O lote {loteId} tem apenas {available} ingresso(s) disponível(is).");