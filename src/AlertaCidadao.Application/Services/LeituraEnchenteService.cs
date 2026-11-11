using AlertaCidadao.Application.DTOs;
using AlertaCidadao.Application.Interfaces;
using AlertaCidadao.Domain.Entities;
namespace AlertaCidadao.Application.Services;
public sealed class LeituraEnchenteService(ILeituraEnchenteRepository repository) : ILeituraEnchenteService
{
    public async Task<IReadOnlyList<LeituraEnchenteDto>> ListarAsync(long? idPonto = null, CancellationToken ct = default) =>
        (await repository.ListarAsync(idPonto, ct)).Select(Mapear).ToList();

    public async Task<ResultadoOperacao<LeituraEnchenteDto>> ObterPorIdAsync(long id, CancellationToken ct = default)
    {
        if (id <= 0) return ResultadoOperacao<LeituraEnchenteDto>.FalhouValidacao("id", "O ID deve ser maior que zero.");
        var leitura = await repository.ObterPorIdAsync(id, ct);
        return leitura is null
            ? ResultadoOperacao<LeituraEnchenteDto>.Falhou(TipoErroOperacao.NaoEncontrado, "Leitura de enchente não encontrada.")
            : ResultadoOperacao<LeituraEnchenteDto>.Ok(Mapear(leitura));
    }

    public async Task<ResultadoOperacao<LeituraEnchenteDto>> CriarAsync(CriarLeituraEnchenteRequest request, CancellationToken ct = default)
    {
        if (request.DataHoraLeitura == default)
            return ResultadoOperacao<LeituraEnchenteDto>.FalhouValidacao("DataHoraLeitura", "Informe a data e hora em que a leitura foi realizada.");
        if (!await repository.PontoAtivoExisteAsync(request.IdPonto, ct))
            return ResultadoOperacao<LeituraEnchenteDto>.FalhouValidacao("IdPonto", "O ponto informado não existe ou está desativado.");

        var origem = request.Origem.Trim().ToUpperInvariant();
        if (origem is not ("SIMULACAO" or "SENSOR" or "MANUAL"))
            return ResultadoOperacao<LeituraEnchenteDto>.FalhouValidacao("Origem", "A origem deve ser SIMULACAO, SENSOR ou MANUAL.");

        var chaveEvento = TextoOuNull(request.ChaveEvento);
        if (chaveEvento is not null && await repository.ChaveEventoExisteAsync(chaveEvento, null, ct))
            return ResultadoOperacao<LeituraEnchenteDto>.FalhouComCampo(TipoErroOperacao.Conflito, "ChaveEvento", "Esta chave de evento já foi utilizada.");
        if (await repository.LeituraDoPontoNaDataExisteAsync(request.IdPonto, request.DataHoraLeitura, null, ct))
            return ResultadoOperacao<LeituraEnchenteDto>.FalhouComCampo(TipoErroOperacao.Conflito, "DataHoraLeitura", "Já existe uma leitura deste ponto com a mesma data e hora.");

        var leitura = new LeituraEnchente
        {
            IdPonto = request.IdPonto,
            NivelAguaCm = request.NivelAguaCm,
            ChuvaMm = request.ChuvaMm,
            ElevacaoCmH = request.ElevacaoCmH,
            DataHoraLeitura = request.DataHoraLeitura,
            Origem = origem,
            ChaveEvento = chaveEvento,
            DataRecebimento = DateTimeOffset.UtcNow
        };
        await repository.AdicionarAsync(leitura, ct);
        return ResultadoOperacao<LeituraEnchenteDto>.Ok(Mapear(leitura));
    }

    public async Task<ResultadoOperacao<LeituraEnchenteDto>> AtualizarAsync(long id, AtualizarLeituraEnchenteRequest request, CancellationToken ct = default)
    {
        if (id <= 0) return ResultadoOperacao<LeituraEnchenteDto>.FalhouValidacao("id", "O ID deve ser maior que zero.");
        var leitura = await repository.ObterPorIdAsync(id, ct);
        if (leitura is null) return ResultadoOperacao<LeituraEnchenteDto>.Falhou(TipoErroOperacao.NaoEncontrado, "Leitura de enchente não encontrada.");
        if (request.DataHoraLeitura == default)
            return ResultadoOperacao<LeituraEnchenteDto>.FalhouValidacao("DataHoraLeitura", "Informe a data e hora da leitura.");

        var origem = request.Origem.Trim().ToUpperInvariant();
        if (origem is not ("SIMULACAO" or "SENSOR" or "MANUAL"))
            return ResultadoOperacao<LeituraEnchenteDto>.FalhouValidacao("Origem", "A origem deve ser SIMULACAO, SENSOR ou MANUAL.");
        if (await repository.LeituraDoPontoNaDataExisteAsync(leitura.IdPonto, request.DataHoraLeitura, id, ct))
            return ResultadoOperacao<LeituraEnchenteDto>.FalhouComCampo(TipoErroOperacao.Conflito, "DataHoraLeitura", "Já existe outra leitura deste ponto com a mesma data e hora.");

        leitura.NivelAguaCm = request.NivelAguaCm;
        leitura.ChuvaMm = request.ChuvaMm;
        leitura.ElevacaoCmH = request.ElevacaoCmH;
        leitura.DataHoraLeitura = request.DataHoraLeitura;
        leitura.Origem = origem;
        await repository.SalvarAlteracoesAsync(ct);
        return ResultadoOperacao<LeituraEnchenteDto>.Ok(Mapear(leitura));
    }

    public async Task<ResultadoOperacao<bool>> ExcluirAsync(long id, CancellationToken ct = default)
    {
        if (id <= 0) return ResultadoOperacao<bool>.FalhouValidacao("id", "O ID deve ser maior que zero.");
        var leitura = await repository.ObterPorIdAsync(id, ct);
        if (leitura is null) return ResultadoOperacao<bool>.Falhou(TipoErroOperacao.NaoEncontrado, "Leitura de enchente não encontrada.");
        if (await repository.PossuiAlertaAsync(id, ct))
            return ResultadoOperacao<bool>.Falhou(TipoErroOperacao.Conflito, "Não é possível excluir esta leitura porque existe um alerta associado a ela.");
        await repository.RemoverAsync(leitura, ct);
        return ResultadoOperacao<bool>.Ok(true);
    }

    private static string? TextoOuNull(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
    private static LeituraEnchenteDto Mapear(LeituraEnchente x) => new(x.IdLeitura, x.IdPonto, x.NivelAguaCm, x.ChuvaMm, x.ElevacaoCmH, x.DataHoraLeitura, x.Origem, x.ChaveEvento, x.DataRecebimento);
}
