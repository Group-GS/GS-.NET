namespace AlertaCidadao.Application.DTOs;

public sealed record LeituraEnchenteDto(
    long IdLeitura,
    long IdPonto,
    decimal NivelAguaCm,
    decimal ChuvaMm,
    decimal ElevacaoCmH,
    DateTimeOffset DataHoraLeitura,
    string Origem,
    string? ChaveEvento,
    DateTimeOffset DataRecebimento);
