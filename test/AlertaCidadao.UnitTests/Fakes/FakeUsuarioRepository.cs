using AlertaCidadao.Application.Interfaces;
using AlertaCidadao.Domain.Entities;

namespace AlertaCidadao.UnitTests.Fakes;

internal sealed class FakeUsuarioRepository : IUsuarioRepository
{
    private readonly List<Usuario> _usuarios = [];
    private long _nextId = 1;

    public IReadOnlyList<Usuario> Usuarios => _usuarios;
    public int Adicoes { get; private set; }
    public int Salvamentos { get; private set; }

    public Task<IReadOnlyList<Usuario>> ListarAsync(bool incluirInativos, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Usuario>>(
            _usuarios.Where(x => incluirInativos || x.Ativo).ToList());

    public Task<Usuario?> ObterPorIdAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(_usuarios.FirstOrDefault(x => x.IdUsuario == id));

    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken ct = default) =>
        Task.FromResult(_usuarios.FirstOrDefault(x =>
            x.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));

    public Task<bool> EmailExisteAsync(string email, long? ignorarId = null, CancellationToken ct = default) =>
        Task.FromResult(_usuarios.Any(x =>
            x.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
            (!ignorarId.HasValue || x.IdUsuario != ignorarId.Value)));

    public Task AdicionarAsync(Usuario entidade, CancellationToken ct = default)
    {
        entidade.IdUsuario = _nextId++;
        _usuarios.Add(entidade);
        Adicoes++;
        return Task.CompletedTask;
    }

    public Task SalvarAlteracoesAsync(CancellationToken ct = default)
    {
        Salvamentos++;
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Usuario entidade, CancellationToken ct = default)
    {
        _usuarios.Remove(entidade);
        return Task.CompletedTask;
    }

    public void Inserir(Usuario usuario)
    {
        if (usuario.IdUsuario == 0)
            usuario.IdUsuario = _nextId++;
        _usuarios.Add(usuario);
    }
}
