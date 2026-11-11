using AlertaCidadao.Application.DTOs;
using AlertaCidadao.Application.Interfaces;
using AlertaCidadao.Domain.Entities;
namespace AlertaCidadao.Application.Services;
public sealed class RequisicaoIdempotenciaService(IRequisicaoIdempotenciaRepository repository) : IRequisicaoIdempotenciaService
{
    public async Task<IReadOnlyList<RequisicaoIdempotenciaDto>> ListarAsync(CancellationToken ct = default) =>
        (await repository.ListarAsync(ct)).Select(Mapear).ToList();

    public async Task<ResultadoOperacao<RequisicaoIdempotenciaDto>> ObterPorChaveAsync(string chave, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(chave) || chave.Trim().Length > 100)
            return ResultadoOperacao<RequisicaoIdempotenciaDto>.FalhouValidacao("chave", "A chave é obrigatória e deve ter até 100 caracteres.");
        var registro = await repository.ObterPorChaveAsync(chave.Trim(), ct);
        return registro is null
            ? ResultadoOperacao<RequisicaoIdempotenciaDto>.Falhou(TipoErroOperacao.NaoEncontrado, "Chave de idempotência não encontrada.")
            : ResultadoOperacao<RequisicaoIdempotenciaDto>.Ok(Mapear(registro));
    }

    public async Task<ResultadoOperacao<RequisicaoIdempotenciaDto>> CriarAsync(CriarRequisicaoIdempotenciaRequest request, CancellationToken ct = default)
    {
        var chave = request.ChaveIdempotencia.Trim();
        if (await repository.ObterPorChaveAsync(chave, ct) is not null)
            return ResultadoOperacao<RequisicaoIdempotenciaDto>.FalhouComCampo(TipoErroOperacao.Conflito, "ChaveIdempotencia", "Esta chave de idempotência já está cadastrada.");
        var agora = DateTimeOffset.UtcNow;
        if (request.DataExpiracao.HasValue && request.DataExpiracao.Value <= agora)
            return ResultadoOperacao<RequisicaoIdempotenciaDto>.FalhouValidacao("DataExpiracao", "A data de expiração deve ser posterior à data atual.");

        var registro = new RequisicaoIdempotencia
        {
            ChaveIdempotencia = chave,
            MetodoHttp = request.MetodoHttp.Trim().ToUpperInvariant(),
            Rota = request.Rota.Trim(),
            StatusHttp = request.StatusHttp,
            DataCriacao = agora,
            DataExpiracao = request.DataExpiracao
        };
        await repository.AdicionarAsync(registro, ct);
        return ResultadoOperacao<RequisicaoIdempotenciaDto>.Ok(Mapear(registro));
    }

    public async Task<ResultadoOperacao<RequisicaoIdempotenciaDto>> AtualizarAsync(string chave, AtualizarRequisicaoIdempotenciaRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(chave) || chave.Trim().Length > 100)
            return ResultadoOperacao<RequisicaoIdempotenciaDto>.FalhouValidacao("chave", "A chave é obrigatória e deve ter até 100 caracteres.");
        var registro = await repository.ObterPorChaveAsync(chave.Trim(), ct);
        if (registro is null) return ResultadoOperacao<RequisicaoIdempotenciaDto>.Falhou(TipoErroOperacao.NaoEncontrado, "Chave de idempotência não encontrada.");
        if (request.DataExpiracao.HasValue && request.DataExpiracao.Value <= registro.DataCriacao)
            return ResultadoOperacao<RequisicaoIdempotenciaDto>.FalhouValidacao("DataExpiracao", "A data de expiração deve ser posterior à data de criação.");
        registro.MetodoHttp = request.MetodoHttp.Trim().ToUpperInvariant();
        registro.Rota = request.Rota.Trim();
        registro.StatusHttp = request.StatusHttp;
        registro.DataExpiracao = request.DataExpiracao;
        await repository.SalvarAlteracoesAsync(ct);
        return ResultadoOperacao<RequisicaoIdempotenciaDto>.Ok(Mapear(registro));
    }

    public async Task<ResultadoOperacao<bool>> ExcluirAsync(string chave, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(chave) || chave.Trim().Length > 100)
            return ResultadoOperacao<bool>.FalhouValidacao("chave", "A chave é obrigatória e deve ter até 100 caracteres.");
        var registro = await repository.ObterPorChaveAsync(chave.Trim(), ct);
        if (registro is null) return ResultadoOperacao<bool>.Falhou(TipoErroOperacao.NaoEncontrado, "Chave de idempotência não encontrada.");
        await repository.RemoverAsync(registro, ct);
        return ResultadoOperacao<bool>.Ok(true);
    }

    private static RequisicaoIdempotenciaDto Mapear(RequisicaoIdempotencia x) => new(x.ChaveIdempotencia, x.MetodoHttp, x.Rota, x.StatusHttp, x.DataCriacao, x.DataExpiracao);
}
