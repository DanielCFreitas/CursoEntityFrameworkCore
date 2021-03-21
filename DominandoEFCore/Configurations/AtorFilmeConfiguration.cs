using DominandoEFCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;

namespace DominandoEFCore.Configurations
{
    public class AtorFilmeConfiguration : IEntityTypeConfiguration<Ator>
    {
        public void Configure(EntityTypeBuilder<Ator> builder)
        {
            /*builder
                .HasMany(p => p.Filmes)
                .WithMany(p => p.Atores)
                .UsingEntity(p => p.ToTable("AtoresFilmes"));*/

            builder
                .HasMany(p => p.Filmes)
                .WithMany(p => p.Atores)
                .UsingEntity<Dictionary<string, object>>(
                    "FilmesAtores",
                    p => p.HasOne<Filme>().WithMany().HasForeignKey("FilmeId"),
                    p => p.HasOne<Ator>().WithMany().HasForeignKey("AtorId"),
                    p =>
                    {
                        p.Property<DateTime>("CadastradoEm").HasDefaultValueSql("CURRENT_DATE");
                    }
                );
        }
    }
}
