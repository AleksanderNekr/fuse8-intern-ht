using Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Data.EntitiesConfigurations;

/// <inheritdoc />
public class CacheTaskConfig : IEntityTypeConfiguration<CacheTaskEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CacheTaskEntity> builder)
    {
        builder.Property(static x => x.Id)
               .HasColumnName("id");

        builder.Property(static x => x.Status)
               .HasColumnName("status")
               .HasConversion(static status => status.ToString(),
                              static s => Enum.Parse<Status>(s, true));

        builder.HasKey(static x => x.Id)
               .HasName("cache_task_pk");

        builder.ToTable("cache_tasks",
                        static tableBuilder =>
                        {
                            tableBuilder.HasCheckConstraint("status_enum_range_ch",
                                                            $"status IN ('{Status.Created}', '{Status.Running}', '{Status.RanToCompletion}', '{Status.Canceled}', '{Status.Faulted}')");
                        });
    }
}
