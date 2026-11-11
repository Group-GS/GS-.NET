using AlertaCidadao.Application.DTOs;
using AlertaCidadao.Application.Interfaces;
using AlertaCidadao.Domain.Entities;
namespace AlertaCidadao.Application.Services;
public sealed class UsuarioService(IUsuarioRepository repository, IPasswordHasher passwordHasher) : IUsuarioService
{
    public async Task<IReadOnlyList<UsuarioDto>> ListarAsync(bool incluirInativos = false, CancellationToken ct = default) =>
        (await repository.ListarAsync(incluirInativos, ct)).Select(Mapear).ToList();

    public async Task<ResultadoOperacao<UsuarioDto>> ObterPorIdAsync(long id, CancellationToken ct = default)
    {
        if (id <= 0) return ResultadoOperacao<UsuarioDto>.FalhouValidacao("id", "O ID deve ser maior que zero.");
        var usuario = await repository.ObterPorIdAsync(id, ct);
        return usuario is null
            ? ResultadoOperacao<UsuarioDto>.Falhou(TipoErroOperacao.NaoEncontrado, "Usuário não encontrado.")
            : ResultadoOperacao<UsuarioDto>.Ok(Mapear(usuario));
    }

    public async Task<ResultadoOperacao<UsuarioDto>> CriarAsync(CriarUsuarioRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await repository.EmailExisteAsync(email, null, ct))
            return ResultadoOperacao<UsuarioDto>.FalhouComCampo(TipoErroOperacao.Conflito, "Email", "Este e-mail já está cadastrado.");

        var perfil = request.Perfil.Trim().ToUpperInvariant();
        if (perfil is not ("CIDADAO" or "GESTOR"))
            return ResultadoOperacao<UsuarioDto>.FalhouValidacao("Perfil", "O perfil deve ser CIDADAO ou GESTOR.");
        if (perfil == "GESTOR")
            return ResultadoOperacao<UsuarioDto>.FalhouValidacao("Perfil", "O perfil GESTOR só pode ser concedido por uma operação administrativa autenticada.");

        var usuario = new Usuario
        {
            Nome = request.Nome.Trim(), Email = email,
            SenhaHash = passwordHasher.GerarHash(request.Senha),
            Telefone = TextoOuNull(request.Telefone), Perfil = perfil,
            Ativo = true, DataCadastro = DateTimeOffset.UtcNow
        };
        await repository.AdicionarAsync(usuario, ct);
        return ResultadoOperacao<UsuarioDto>.Ok(Mapear(usuario));
    }

    public async Task<ResultadoOperacao<UsuarioDto>> AtualizarAsync(long id, AtualizarUsuarioRequest request, CancellationToken ct = default)
    {
        if (id <= 0) return ResultadoOperacao<UsuarioDto>.FalhouValidacao("id", "O ID deve ser maior que zero.");
        var usuario = await repository.ObterPorIdAsync(id, ct);
        if (usuario is null) return ResultadoOperacao<UsuarioDto>.Falhou(TipoErroOperacao.NaoEncontrado, "Usuário não encontrado.");

        var email = request.Email.Trim().ToLowerInvariant();
        if (await repository.EmailExisteAsync(email, id, ct))
            return ResultadoOperacao<UsuarioDto>.FalhouComCampo(TipoErroOperacao.Conflito, "Email", "Este e-mail já pertence a outro usuário.");

        var perfil = request.Perfil.Trim().ToUpperInvariant();
        if (perfil is not ("CIDADAO" or "GESTOR"))
            return ResultadoOperacao<UsuarioDto>.FalhouValidacao("Perfil", "O perfil deve ser CIDADAO ou GESTOR.");
        if (perfil == "GESTOR")
            return ResultadoOperacao<UsuarioDto>.FalhouValidacao("Perfil", "O perfil GESTOR só pode ser concedido por uma operação administrativa autenticada.");

        usuario.Nome = request.Nome.Trim();
        usuario.Email = email;
        usuario.Telefone = TextoOuNull(request.Telefone);
        usuario.Perfil = perfil;
        usuario.Ativo = request.Ativo;
        if (!string.IsNullOrWhiteSpace(request.Senha))
            usuario.SenhaHash = passwordHasher.GerarHash(request.Senha);

        await repository.SalvarAlteracoesAsync(ct);
        return ResultadoOperacao<UsuarioDto>.Ok(Mapear(usuario));
    }

    public async Task<ResultadoOperacao<bool>> ExcluirAsync(long id, CancellationToken ct = default)
    {
        if (id <= 0) return ResultadoOperacao<bool>.FalhouValidacao("id", "O ID deve ser maior que zero.");
        var usuario = await repository.ObterPorIdAsync(id, ct);
        if (usuario is null) return ResultadoOperacao<bool>.Falhou(TipoErroOperacao.NaoEncontrado, "Usuário não encontrado.");
        usuario.Ativo = false;
        await repository.SalvarAlteracoesAsync(ct);
        return ResultadoOperacao<bool>.Ok(true);
    }

    private static string? TextoOuNull(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
    private static UsuarioDto Mapear(Usuario x) => new(x.IdUsuario, x.Nome, x.Email, x.Telefone, x.Perfil, x.Ativo, x.DataCadastro);
}
