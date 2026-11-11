using AlertaCidadao.Application.DTOs;
using AlertaCidadao.Application.Interfaces;
using AlertaCidadao.Domain.Entities;
namespace AlertaCidadao.Application.Services;
public sealed class AlertaService(IAlertaRepository repository) : IAlertaService
{
    private static readonly string[] Classificacoes = ["BAIXO", "MODERADO", "ALTO", "CRITICO"];

    public async Task<IReadOnlyList<AlertaDto>> ListarAsync(string? status = null, CancellationToken ct = default)
    {
        var filtro = string.IsNullOrWhiteSpace(status) ? null : status.Trim().ToUpperInvariant();
        return (await repository.ListarAsync(filtro, ct)).Select(Mapear).ToList();
    }

    public async Task<ResultadoOperacao<AlertaDto>> ObterPorIdAsync(long id, CancellationToken ct = default)
    {
        if (id <= 0) return ResultadoOperacao<AlertaDto>.FalhouValidacao("id", "O ID deve ser maior que zero.");
        var alerta = await repository.ObterPorIdAsync(id, ct);
        return alerta is null
            ? ResultadoOperacao<AlertaDto>.Falhou(TipoErroOperacao.NaoEncontrado, "Alerta não encontrado.")
            : ResultadoOperacao<AlertaDto>.Ok(Mapear(alerta));
    }

    public async Task<ResultadoOperacao<AlertaDto>> CriarAsync(CriarAlertaRequest request, CancellationToken ct = default)
    {
        if (!await repository.LeituraExisteAsync(request.IdLeitura, ct))
            return ResultadoOperacao<AlertaDto>.FalhouValidacao("IdLeitura", "A leitura informada não existe.");
        var classificacao = request.Classificacao.Trim().ToUpperInvariant();
        if (!Classificacoes.Contains(classificacao))
            return ResultadoOperacao<AlertaDto>.FalhouValidacao("Classificacao", "A classificação deve ser BAIXO, MODERADO, ALTO ou CRITICO.");
        if (await repository.AlertaParaLeituraExisteAsync(request.IdLeitura, null, ct))
            return ResultadoOperacao<AlertaDto>.FalhouComCampo(TipoErroOperacao.Conflito, "IdLeitura", "Já existe um alerta associado a esta leitura.");

        var alerta = new Alerta
        {
            IdLeitura = request.IdLeitura,
            Classificacao = classificacao,
            Probabilidade = request.Probabilidade,
            MensagemExplicativa = TextoOuNull(request.MensagemExplicativa),
            TipoMensagem = TextoOuNull(request.TipoMensagem)?.ToUpperInvariant(),
            ModeloMl = TextoOuNull(request.ModeloMl),
            Status = "ATIVO",
            DataGeracao = DateTimeOffset.UtcNow
        };
        await repository.AdicionarAsync(alerta, ct);
        return ResultadoOperacao<AlertaDto>.Ok(Mapear(alerta));
    }

    public async Task<ResultadoOperacao<AlertaDto>> AtualizarAsync(long id, AtualizarAlertaRequest request, CancellationToken ct = default)
    {
        if (id <= 0) return ResultadoOperacao<AlertaDto>.FalhouValidacao("id", "O ID deve ser maior que zero.");
        var alerta = await repository.ObterPorIdAsync(id, ct);
        if (alerta is null) return ResultadoOperacao<AlertaDto>.Falhou(TipoErroOperacao.NaoEncontrado, "Alerta não encontrado.");
        var classificacao = request.Classificacao.Trim().ToUpperInvariant();
        if (!Classificacoes.Contains(classificacao))
            return ResultadoOperacao<AlertaDto>.FalhouValidacao("Classificacao", "A classificação deve ser BAIXO, MODERADO, ALTO ou CRITICO.");
        var status = request.Status.Trim().ToUpperInvariant();
        if (status is not ("ATIVO" or "RESOLVIDO"))
            return ResultadoOperacao<AlertaDto>.FalhouValidacao("Status", "O status deve ser ATIVO ou RESOLVIDO.");

        alerta.Classificacao = classificacao;
        alerta.Probabilidade = request.Probabilidade;
        alerta.MensagemExplicativa = TextoOuNull(request.MensagemExplicativa);
        alerta.TipoMensagem = TextoOuNull(request.TipoMensagem)?.ToUpperInvariant();
        alerta.ModeloMl = TextoOuNull(request.ModeloMl);
        alerta.Status = status;
        alerta.DataResolucao = status == "RESOLVIDO" ? alerta.DataResolucao ?? DateTimeOffset.UtcNow : null;
        await repository.SalvarAlteracoesAsync(ct);
        return ResultadoOperacao<AlertaDto>.Ok(Mapear(alerta));
    }

    public async Task<ResultadoOperacao<bool>> ExcluirAsync(long id, CancellationToken ct = default)
    {
        if (id <= 0) return ResultadoOperacao<bool>.FalhouValidacao("id", "O ID deve ser maior que zero.");
        var alerta = await repository.ObterPorIdAsync(id, ct);
        if (alerta is null) return ResultadoOperacao<bool>.Falhou(TipoErroOperacao.NaoEncontrado, "Alerta não encontrado.");
        await repository.RemoverAsync(alerta, ct);
        return ResultadoOperacao<bool>.Ok(true);
    }

    private static string? TextoOuNull(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
    private static AlertaDto Mapear(Alerta x) => new(x.IdAlerta, x.IdLeitura, x.Classificacao, x.Probabilidade, x.MensagemExplicativa, x.TipoMensagem, x.ModeloMl, x.Status, x.DataGeracao, x.DataResolucao);
}
