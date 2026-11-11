using AlertaCidadao.Application.DTOs;
using AlertaCidadao.Application.Interfaces;
using AlertaCidadao.Domain.Entities;

namespace AlertaCidadao.Application.Services;

public sealed class PontoMonitoramentoService
    : IPontoMonitoramentoService
{
    private readonly IPontoMonitoramentoRepository _repository;

    public PontoMonitoramentoService(
        IPontoMonitoramentoRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<PontoMonitoramentoDto>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        var pontos = await _repository.ListarAtivosAsync(
            cancellationToken
        );

        return pontos.Select(MapearDto).ToList();
    }

    public async Task<IReadOnlyList<PontoMonitoramentoDto>> ListarTodosAsync(
        CancellationToken cancellationToken = default)
    {
        var pontos = await _repository.ListarTodosAsync(cancellationToken);
        return pontos.Select(MapearDto).ToList();
    }

    public async Task<ResultadoOperacao<PontoMonitoramentoDto>>
        ObterPorIdAsync(
            long id,
            CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return ResultadoOperacao<PontoMonitoramentoDto>.FalhouValidacao(
                "id", "O identificador deve ser maior que zero."
            );
        }

        var ponto = await _repository.ObterPorIdAsync(
            id,
            cancellationToken
        );

        if (ponto is null || !ponto.Ativo)
        {
            return ResultadoOperacao<PontoMonitoramentoDto>.Falhou(
                TipoErroOperacao.NaoEncontrado,
                "Ponto de monitoramento não encontrado."
            );
        }

        return ResultadoOperacao<PontoMonitoramentoDto>.Ok(
            MapearDto(ponto)
        );
    }

    public async Task<ResultadoOperacao<PontoMonitoramentoDto>>
        CriarAsync(
            CriarPontoMonitoramentoRequest request,
            CancellationToken cancellationToken = default)
    {
        var erro = Validar(
            request.Codigo,
            request.Nome,
            request.Descricao,
            request.Logradouro,
            request.Bairro,
            request.Cidade,
            request.Uf,
            request.Latitude,
            request.Longitude
        );

        if (erro is not null)
        {
            if (erro.Contains("latitude e longitude juntas", StringComparison.OrdinalIgnoreCase))
            {
                return ResultadoOperacao<PontoMonitoramentoDto>.FalhouComErros(
                    TipoErroOperacao.Validacao,
                    "Informe os dois campos de coordenadas.",
                    new Dictionary<string, string[]>
                    {
                        ["Latitude"] = [erro],
                        ["Longitude"] = [erro]
                    });
            }

            return ResultadoOperacao<PontoMonitoramentoDto>.FalhouValidacao(
                CampoDaMensagem(erro), erro
            );
        }

        var codigo = NormalizarCodigo(request.Codigo);

        if (await _repository.CodigoExisteAsync(
            codigo,
            null,
            cancellationToken))
        {
            return ResultadoOperacao<PontoMonitoramentoDto>.FalhouComCampo(
                TipoErroOperacao.Conflito,
                "Codigo",
                "Já existe um ponto de monitoramento com esse código."
            );
        }

        var ponto = new PontoMonitoramento
        {
            Codigo = codigo,
            Nome = request.Nome.Trim(),
            Descricao = NormalizarTexto(request.Descricao),
            Logradouro = NormalizarTexto(request.Logradouro),
            Bairro = NormalizarTexto(request.Bairro),
            Cidade = NormalizarTexto(request.Cidade),
            Uf = NormalizarUf(request.Uf),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Ativo = true,
            DataCadastro = DateTimeOffset.UtcNow
        };

        await _repository.AdicionarAsync(
            ponto,
            cancellationToken
        );

        return ResultadoOperacao<PontoMonitoramentoDto>.Ok(
            MapearDto(ponto)
        );
    }

    public async Task<ResultadoOperacao<PontoMonitoramentoDto>>
        AtualizarAsync(
            long id,
            AtualizarPontoMonitoramentoRequest request,
            CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return ResultadoOperacao<PontoMonitoramentoDto>.FalhouValidacao(
                "id", "O identificador deve ser maior que zero."
            );
        }

        var erro = Validar(
            request.Codigo,
            request.Nome,
            request.Descricao,
            request.Logradouro,
            request.Bairro,
            request.Cidade,
            request.Uf,
            request.Latitude,
            request.Longitude
        );

        if (erro is not null)
        {
            if (erro.Contains("latitude e longitude juntas", StringComparison.OrdinalIgnoreCase))
            {
                return ResultadoOperacao<PontoMonitoramentoDto>.FalhouComErros(
                    TipoErroOperacao.Validacao,
                    "Informe os dois campos de coordenadas.",
                    new Dictionary<string, string[]>
                    {
                        ["Latitude"] = [erro],
                        ["Longitude"] = [erro]
                    });
            }

            return ResultadoOperacao<PontoMonitoramentoDto>.FalhouValidacao(
                CampoDaMensagem(erro), erro
            );
        }

        var ponto = await _repository.ObterPorIdAsync(
            id,
            cancellationToken
        );

        if (ponto is null || !ponto.Ativo)
        {
            return ResultadoOperacao<PontoMonitoramentoDto>.Falhou(
                TipoErroOperacao.NaoEncontrado,
                "Ponto de monitoramento não encontrado."
            );
        }

        var codigo = NormalizarCodigo(request.Codigo);

        if (await _repository.CodigoExisteAsync(
            codigo,
            id,
            cancellationToken))
        {
            return ResultadoOperacao<PontoMonitoramentoDto>.FalhouComCampo(
                TipoErroOperacao.Conflito,
                "Codigo",
                "Já existe outro ponto com esse código."
            );
        }

        ponto.Codigo = codigo;
        ponto.Nome = request.Nome.Trim();
        ponto.Descricao = NormalizarTexto(request.Descricao);
        ponto.Logradouro = NormalizarTexto(request.Logradouro);
        ponto.Bairro = NormalizarTexto(request.Bairro);
        ponto.Cidade = NormalizarTexto(request.Cidade);
        ponto.Uf = NormalizarUf(request.Uf);
        ponto.Latitude = request.Latitude;
        ponto.Longitude = request.Longitude;
        ponto.Ativo = request.Ativo;

        await _repository.SalvarAlteracoesAsync(
            cancellationToken
        );

        return ResultadoOperacao<PontoMonitoramentoDto>.Ok(
            MapearDto(ponto)
        );
    }

    public async Task<ResultadoOperacao<bool>> ExcluirAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return ResultadoOperacao<bool>.FalhouValidacao(
                "id", "O identificador deve ser maior que zero."
            );
        }

        var ponto = await _repository.ObterPorIdAsync(
            id,
            cancellationToken
        );

        if (ponto is null || !ponto.Ativo)
        {
            return ResultadoOperacao<bool>.Falhou(
                TipoErroOperacao.NaoEncontrado,
                "Ponto de monitoramento não encontrado."
            );
        }

        ponto.Ativo = false;

        await _repository.SalvarAlteracoesAsync(
            cancellationToken
        );

        return ResultadoOperacao<bool>.Ok(true);
    }

    private static string? Validar(
        string codigo,
        string nome,
        string? descricao,
        string? logradouro,
        string? bairro,
        string? cidade,
        string? uf,
        decimal? latitude,
        decimal? longitude)
    {
        if (string.IsNullOrWhiteSpace(codigo) || codigo.Trim().Length > 50)
            return "O código é obrigatório e deve ter até 50 caracteres.";

        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length > 120)
            return "O nome é obrigatório e deve ter até 120 caracteres.";

        if (descricao?.Length > 500)
            return "A descrição deve ter até 500 caracteres.";

        if (logradouro?.Length > 180)
            return "O logradouro deve ter até 180 caracteres.";

        if (bairro?.Length > 100)
            return "O bairro deve ter até 100 caracteres.";

        if (cidade?.Length > 100)
            return "A cidade deve ter até 100 caracteres.";

        if (!string.IsNullOrWhiteSpace(uf) && uf.Trim().Length != 2)
            return "A UF deve conter exatamente 2 caracteres.";

        if (latitude is < -90 or > 90)
            return "A latitude deve estar entre -90 e 90.";

        if (longitude is < -180 or > 180)
            return "A longitude deve estar entre -180 e 180.";

        if (latitude.HasValue != longitude.HasValue)
            return "Informe latitude e longitude juntas.";

        return null;
    }

    private static string NormalizarCodigo(string codigo)
    {
        return codigo.Trim().ToLowerInvariant();
    }

    private static string? NormalizarTexto(string? texto)
    {
        return string.IsNullOrWhiteSpace(texto)
            ? null
            : texto.Trim();
    }

    private static string? NormalizarUf(string? uf)
    {
        return string.IsNullOrWhiteSpace(uf)
            ? null
            : uf.Trim().ToUpperInvariant();
    }

    private static string CampoDaMensagem(string mensagem)
    {
        if (mensagem.Contains("código", StringComparison.OrdinalIgnoreCase)) return "Codigo";
        if (mensagem.Contains("nome", StringComparison.OrdinalIgnoreCase)) return "Nome";
        if (mensagem.Contains("descrição", StringComparison.OrdinalIgnoreCase)) return "Descricao";
        if (mensagem.Contains("logradouro", StringComparison.OrdinalIgnoreCase)) return "Logradouro";
        if (mensagem.Contains("bairro", StringComparison.OrdinalIgnoreCase)) return "Bairro";
        if (mensagem.Contains("cidade", StringComparison.OrdinalIgnoreCase)) return "Cidade";
        if (mensagem.Contains("UF", StringComparison.OrdinalIgnoreCase)) return "Uf";
        if (mensagem.Contains("latitude", StringComparison.OrdinalIgnoreCase)) return "Latitude";
        if (mensagem.Contains("longitude", StringComparison.OrdinalIgnoreCase)) return "Longitude";
        return "request";
    }

    private static PontoMonitoramentoDto MapearDto(
        PontoMonitoramento ponto)
    {
        return new PontoMonitoramentoDto(
            ponto.IdPonto,
            ponto.Codigo,
            ponto.Nome,
            ponto.Descricao,
            ponto.Logradouro,
            ponto.Bairro,
            ponto.Cidade,
            ponto.Uf,
            ponto.Latitude,
            ponto.Longitude,
            ponto.Ativo,
            ponto.DataCadastro
        );
    }
}