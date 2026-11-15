using AlertaCidadao.Application.DTOs;
using AlertaCidadao.Application.Services;
using AlertaCidadao.UnitTests.Fakes;

namespace AlertaCidadao.UnitTests.Services;

public sealed class UsuarioServiceTests
{
    private readonly FakeUsuarioRepository _repository = new();
    private readonly Pbkdf2PasswordHasher _hasher = new();

    private UsuarioService CriarServico() => new(_repository, _hasher);

    private static CriarUsuarioRequest RequestValido(
        string email = "cidadao@example.com",
        string perfil = "CIDADAO") => new()
    {
        Nome = "  Maria da Silva  ",
        Email = email,
        Senha = "SenhaSegura123!",
        Telefone = "(11) 99999-9999",
        Perfil = perfil
    };

    [Fact]
    public async Task CriarAsync_ComDadosValidos_DeveCadastrarUsuarioComEmailNormalizado()
    {
        var service = CriarServico();

        var resultado = await service.CriarAsync(RequestValido("  MARIA@EXAMPLE.COM "));

        Assert.True(resultado.Sucesso);
        Assert.NotNull(resultado.Dados);
        Assert.Equal("Maria da Silva", resultado.Dados.Nome);
        Assert.Equal("maria@example.com", resultado.Dados.Email);
        Assert.Equal("CIDADAO", resultado.Dados.Perfil);
        Assert.True(resultado.Dados.Ativo);
        Assert.Single(_repository.Usuarios);
        Assert.Equal(1, _repository.Adicoes);
        Assert.NotEqual("SenhaSegura123!", _repository.Usuarios[0].SenhaHash);
        Assert.StartsWith("PBKDF2-SHA256$", _repository.Usuarios[0].SenhaHash);
    }

    [Fact]
    public async Task CriarAsync_ComEmailDuplicado_DeveRetornarConflitoENaoCadastrar()
    {
        _repository.Inserir(new()
        {
            Nome = "Usuário existente",
            Email = "cidadao@example.com",
            SenhaHash = "hash",
            Perfil = "CIDADAO",
            Ativo = true
        });

        var resultado = await CriarServico().CriarAsync(RequestValido());

        Assert.False(resultado.Sucesso);
        Assert.Equal(TipoErroOperacao.Conflito, resultado.TipoErro);
        Assert.Equal(1, _repository.Usuarios.Count);
        Assert.Equal(0, _repository.Adicoes);
    }

    [Fact]
    public async Task CriarAsync_ComPerfilGestor_DeveRejeitarEscalonamentoPublico()
    {
        var resultado = await CriarServico().CriarAsync(RequestValido(perfil: "GESTOR"));

        Assert.False(resultado.Sucesso);
        Assert.Equal(TipoErroOperacao.Validacao, resultado.TipoErro);
        Assert.Empty(_repository.Usuarios);
    }

    [Fact]
    public async Task CriarAsync_ComPerfilDesconhecido_DeveRetornarErroDeValidacao()
    {
        var resultado = await CriarServico().CriarAsync(RequestValido(perfil: "ADMIN"));

        Assert.False(resultado.Sucesso);
        Assert.Equal(TipoErroOperacao.Validacao, resultado.TipoErro);
        Assert.Empty(_repository.Usuarios);
    }

    [Fact]
    public async Task ObterPorIdAsync_ComIdZero_DeveRetornarValidacao()
    {
        var resultado = await CriarServico().ObterPorIdAsync(0);

        Assert.False(resultado.Sucesso);
        Assert.Equal(TipoErroOperacao.Validacao, resultado.TipoErro);
    }

    [Fact]
    public async Task ObterPorIdAsync_QuandoUsuarioNaoExiste_DeveRetornarNaoEncontrado()
    {
        var resultado = await CriarServico().ObterPorIdAsync(999);

        Assert.False(resultado.Sucesso);
        Assert.Equal(TipoErroOperacao.NaoEncontrado, resultado.TipoErro);
    }

    [Fact]
    public async Task ExcluirAsync_DeveDesativarUsuarioSemApagarRegistro()
    {
        var cadastro = await CriarServico().CriarAsync(RequestValido());
        var id = cadastro.Dados!.IdUsuario;

        var resultado = await CriarServico().ExcluirAsync(id);

        Assert.True(resultado.Sucesso);
        Assert.Single(_repository.Usuarios);
        Assert.False(_repository.Usuarios[0].Ativo);
        Assert.Equal(1, _repository.Salvamentos);
    }

    [Fact]
    public async Task AtualizarAsync_ComUsuarioInexistente_DeveRetornarNaoEncontrado()
    {
        var request = new AtualizarUsuarioRequest
        {
            Nome = "Nome atualizado",
            Email = "novo@example.com",
            Telefone = null,
            Perfil = "CIDADAO",
            Ativo = true,
            Senha = null
        };

        var resultado = await CriarServico().AtualizarAsync(999, request);

        Assert.False(resultado.Sucesso);
        Assert.Equal(TipoErroOperacao.NaoEncontrado, resultado.TipoErro);
    }
}
