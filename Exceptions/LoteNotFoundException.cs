namespace eventual.Exceptions;

public class LoteNotFoundException(int loteId)
    : NotFoundException($"Lote {loteId} não encontrado.");
