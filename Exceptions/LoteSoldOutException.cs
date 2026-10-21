namespace eventual.Exceptions;

public class LoteSoldOutException(int loteId, int available)
     : ConflictException($"O lote {loteId} tem apenas {available} ingresso(s) disponível(is).");