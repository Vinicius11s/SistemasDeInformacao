using Agile360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agile360.Infrastructure.Data.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("cliente");
        builder.HasKey(e => e.Id);

        // FK columns: banco usa id_ prefix, convenção geraria _id suffix
        builder.Property(e => e.AdvogadoId).HasColumnName("id_advogado");

        // Constraints — nomes gerados automaticamente pelo UseSnakeCaseNamingConvention
        builder.Property(e => e.TipoCliente)      .IsRequired().HasMaxLength(20);
        builder.Property(e => e.NomeCompleto)      .HasMaxLength(300);
        builder.Property(e => e.CPF)               .HasMaxLength(14);
        builder.Property(e => e.RG)                .HasMaxLength(20);
        builder.Property(e => e.OrgaoExpedidor)    .HasMaxLength(20);
        builder.Property(e => e.RazaoSocial)       .HasMaxLength(300);
        builder.Property(e => e.CNPJ)              .HasMaxLength(18);
        builder.Property(e => e.InscricaoEstadual) .HasMaxLength(20);
        builder.Property(e => e.Telefone)          .HasMaxLength(20);
        builder.Property(e => e.CEP)               .HasMaxLength(9);
        builder.Property(e => e.Estado)            .HasMaxLength(2);
        builder.Property(e => e.Cidade)            .HasMaxLength(100);
        builder.Property(e => e.Endereco)          .HasMaxLength(300);
        builder.Property(e => e.Numero)            .HasMaxLength(20);
        builder.Property(e => e.Bairro)            .HasMaxLength(100);
        builder.Property(e => e.Complemento)       .HasMaxLength(100);
        builder.Property(e => e.EstadoCivil)       .HasMaxLength(20);
        builder.Property(e => e.AreaAtuacao)       .HasMaxLength(200);
        builder.Property(e => e.NumeroConta)       .HasMaxLength(50);
        builder.Property(e => e.Pix)               .HasMaxLength(100);
        // Campos de BaseEntity sem coluna no banco
        builder.Ignore(e => e.CreatedAt);
        builder.Ignore(e => e.UpdatedAt);

        // Colunas que NÃO existem na tabela cliente do Supabase
        builder.Ignore(e => e.Observacoes); // existe em processo/compromisso/prazo, não em cliente
        builder.Ignore(e => e.Origem);      // coluna origem não existe na tabela cliente

        // Propriedades calculadas — sem coluna
        builder.Ignore(e => e.TipoPessoa);
        builder.Ignore(e => e.NomeExibicao);
        builder.Ignore(e => e.Documento);

        builder.HasIndex(e => e.AdvogadoId);
        builder.HasIndex(e => e.CPF);
        builder.HasIndex(e => e.CNPJ);
    }
}
