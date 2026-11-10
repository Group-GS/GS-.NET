using AlertaCidadao.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlertaCidadao.Infrastructure.Persistence;

public class AlertaCidadaoDbContext : DbContext
{
    public AlertaCidadaoDbContext(
        DbContextOptions<AlertaCidadaoDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<PontoMonitoramento> PontosMonitoramento =>
        Set<PontoMonitoramento>();

    public DbSet<LeituraEnchente> LeiturasEnchente =>
        Set<LeituraEnchente>();

    public DbSet<Alerta> Alertas => Set<Alerta>();

    public DbSet<RequisicaoIdempotencia> RequisicoesIdempotencia =>
        Set<RequisicaoIdempotencia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigurarUsuario(modelBuilder);
        ConfigurarPontoMonitoramento(modelBuilder);
        ConfigurarLeituraEnchente(modelBuilder);
        ConfigurarAlerta(modelBuilder);
        ConfigurarIdempotencia(modelBuilder);
    }

    private static void ConfigurarUsuario(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("USUARIO");

            entity.HasKey(x => x.IdUsuario)
                .HasName("PK_USUARIO");

            entity.Property(x => x.IdUsuario)
                .HasColumnName("ID_USUARIO")
                .HasColumnType("NUMBER")
                .ValueGeneratedOnAdd();

            entity.Property(x => x.Nome)
                .HasColumnName("NOME")
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(x => x.Email)
                .HasColumnName("EMAIL")
                .HasMaxLength(180)
                .IsRequired();

            entity.HasIndex(x => x.Email)
                .IsUnique()
                .HasDatabaseName("UQ_USUARIO_EMAIL");

            entity.Property(x => x.SenhaHash)
                .HasColumnName("SENHA_HASH")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.Telefone)
                .HasColumnName("TELEFONE")
                .HasMaxLength(20);

            entity.Property(x => x.Perfil)
                .HasColumnName("PERFIL")
                .HasMaxLength(20)
                .HasDefaultValue("CIDADAO")
                .IsRequired();

            entity.Property(x => x.Ativo)
                .HasColumnName("ATIVO")
                .HasColumnType("NUMBER(1)")
                .HasDefaultValue(true);

            entity.Property(x => x.DataCadastro)
                .HasColumnName("DATA_CADASTRO")
                .HasColumnType("TIMESTAMP WITH TIME ZONE")
                .HasDefaultValueSql("SYSTIMESTAMP");
        });
    }

    private static void ConfigurarPontoMonitoramento(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PontoMonitoramento>(entity =>
        {
            entity.ToTable("PONTO_MONITORAMENTO");

            entity.HasKey(x => x.IdPonto)
                .HasName("PK_PONTO_MONITORAMENTO");

            entity.Property(x => x.IdPonto)
                .HasColumnName("ID_PONTO")
                .HasColumnType("NUMBER")
                .ValueGeneratedOnAdd();

            entity.Property(x => x.Codigo)
                .HasColumnName("CODIGO")
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(x => x.Codigo)
                .IsUnique()
                .HasDatabaseName("UQ_PONTO_CODIGO");

            entity.Property(x => x.Nome)
                .HasColumnName("NOME")
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(x => x.Descricao)
                .HasColumnName("DESCRICAO")
                .HasMaxLength(500);

            entity.Property(x => x.Logradouro)
                .HasColumnName("LOGRADOURO")
                .HasMaxLength(180);

            entity.Property(x => x.Bairro)
                .HasColumnName("BAIRRO")
                .HasMaxLength(100);

            entity.Property(x => x.Cidade)
                .HasColumnName("CIDADE")
                .HasMaxLength(100);

            entity.Property(x => x.Uf)
                .HasColumnName("UF")
                .HasColumnType("CHAR(2)");

            entity.Property(x => x.Latitude)
                .HasColumnName("LATITUDE")
                .HasColumnType("NUMBER(9,6)");

            entity.Property(x => x.Longitude)
                .HasColumnName("LONGITUDE")
                .HasColumnType("NUMBER(9,6)");

            entity.Property(x => x.Ativo)
                .HasColumnName("ATIVO")
                .HasColumnType("NUMBER(1)")
                .HasDefaultValue(true);

            entity.Property(x => x.DataCadastro)
                .HasColumnName("DATA_CADASTRO")
                .HasColumnType("TIMESTAMP WITH TIME ZONE")
                .HasDefaultValueSql("SYSTIMESTAMP");
        });
    }

    private static void ConfigurarLeituraEnchente(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LeituraEnchente>(entity =>
        {
            entity.ToTable("LEITURA_ENCHENTE");

            entity.HasKey(x => x.IdLeitura)
                .HasName("PK_LEITURA_ENCHENTE");

            entity.Property(x => x.IdLeitura)
                .HasColumnName("ID_LEITURA")
                .HasColumnType("NUMBER")
                .ValueGeneratedOnAdd();

            entity.Property(x => x.IdPonto)
                .HasColumnName("ID_PONTO")
                .HasColumnType("NUMBER");

            entity.Property(x => x.NivelAguaCm)
                .HasColumnName("NIVEL_AGUA_CM")
                .HasColumnType("NUMBER(10,2)")
                .IsRequired();

            entity.Property(x => x.ChuvaMm)
                .HasColumnName("CHUVA_MM")
                .HasColumnType("NUMBER(10,2)")
                .HasDefaultValue(0m);

            entity.Property(x => x.ElevacaoCmH)
                .HasColumnName("ELEVACAO_CM_H")
                .HasColumnType("NUMBER(10,2)")
                .HasDefaultValue(0m);

            entity.Property(x => x.DataHoraLeitura)
                .HasColumnName("DATA_HORA_LEITURA")
                .HasColumnType("TIMESTAMP WITH TIME ZONE")
                .IsRequired();

            entity.Property(x => x.Origem)
                .HasColumnName("ORIGEM")
                .HasMaxLength(20)
                .HasDefaultValue("SIMULACAO");

            entity.Property(x => x.ChaveEvento)
                .HasColumnName("CHAVE_EVENTO")
                .HasMaxLength(100);

            entity.HasIndex(x => x.ChaveEvento)
                .IsUnique()
                .HasDatabaseName("UQ_LEITURA_EVENTO")
                .HasFilter("\"CHAVE_EVENTO\" IS NOT NULL");

            entity.HasIndex(x => new { x.IdPonto, x.DataHoraLeitura })
                .IsUnique()
                .HasDatabaseName("UQ_LEITURA_PONTO_DATA");

            entity.Property(x => x.DataRecebimento)
                .HasColumnName("DATA_RECEBIMENTO")
                .HasColumnType("TIMESTAMP WITH TIME ZONE")
                .HasDefaultValueSql("SYSTIMESTAMP");

            entity.HasOne(x => x.Ponto)
                .WithMany(x => x.Leituras)
                .HasForeignKey(x => x.IdPonto)
                .HasConstraintName("FK_LEITURA_PONTO")
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurarAlerta(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alerta>(entity =>
        {
            entity.ToTable("ALERTA");

            entity.HasKey(x => x.IdAlerta)
                .HasName("PK_ALERTA");

            entity.Property(x => x.IdAlerta)
                .HasColumnName("ID_ALERTA")
                .HasColumnType("NUMBER")
                .ValueGeneratedOnAdd();

            entity.Property(x => x.IdLeitura)
                .HasColumnName("ID_LEITURA")
                .HasColumnType("NUMBER");

            entity.HasIndex(x => x.IdLeitura)
                .IsUnique()
                .HasDatabaseName("UQ_ALERTA_LEITURA");

            entity.Property(x => x.Classificacao)
                .HasColumnName("CLASSIFICACAO")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.Probabilidade)
                .HasColumnName("PROBABILIDADE")
                .HasColumnType("NUMBER(5,4)");

            entity.Property(x => x.MensagemExplicativa)
                .HasColumnName("MENSAGEM_EXPLICATIVA")
                .HasColumnType("CLOB");

            entity.Property(x => x.TipoMensagem)
                .HasColumnName("TIPO_MENSAGEM")
                .HasMaxLength(20);

            entity.Property(x => x.ModeloMl)
                .HasColumnName("MODELO_ML")
                .HasMaxLength(100);

            entity.Property(x => x.Status)
                .HasColumnName("STATUS")
                .HasMaxLength(20)
                .HasDefaultValue("ATIVO")
                .IsRequired();

            entity.Property(x => x.DataGeracao)
                .HasColumnName("DATA_GERACAO")
                .HasColumnType("TIMESTAMP WITH TIME ZONE")
                .HasDefaultValueSql("SYSTIMESTAMP");

            entity.Property(x => x.DataResolucao)
                .HasColumnName("DATA_RESOLUCAO")
                .HasColumnType("TIMESTAMP WITH TIME ZONE");

            entity.HasOne(x => x.Leitura)
                .WithOne(x => x.Alerta)
                .HasForeignKey<Alerta>(x => x.IdLeitura)
                .HasConstraintName("FK_ALERTA_LEITURA")
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurarIdempotencia(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RequisicaoIdempotencia>(entity =>
        {
            entity.ToTable("REQUISICAO_IDEMPOTENCIA");

            entity.HasKey(x => x.ChaveIdempotencia)
                .HasName("PK_REQUISICAO_IDEMPOTENCIA");

            entity.Property(x => x.ChaveIdempotencia)
                .HasColumnName("CHAVE_IDEMPOTENCIA")
                .HasMaxLength(100);

            entity.Property(x => x.MetodoHttp)
                .HasColumnName("METODO_HTTP")
                .HasMaxLength(10)
                .IsRequired();

            entity.Property(x => x.Rota)
                .HasColumnName("ROTA")
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(x => x.StatusHttp)
                .HasColumnName("STATUS_HTTP")
                .HasColumnType("NUMBER(3)");

            entity.Property(x => x.RespostaJson)
                .HasColumnName("RESPOSTA_JSON")
                .HasColumnType("CLOB");

            entity.Property(x => x.DataCriacao)
                .HasColumnName("DATA_CRIACAO")
                .HasColumnType("TIMESTAMP WITH TIME ZONE")
                .HasDefaultValueSql("SYSTIMESTAMP");

            entity.Property(x => x.DataExpiracao)
                .HasColumnName("DATA_EXPIRACAO")
                .HasColumnType("TIMESTAMP WITH TIME ZONE");

            entity.HasIndex(x => x.DataExpiracao)
                .HasDatabaseName("IX_IDEMPOTENCIA_EXPIRA");
        });
    }
}