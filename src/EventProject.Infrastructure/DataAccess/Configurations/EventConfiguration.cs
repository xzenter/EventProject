using EventProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventProject.Infrastructure.DataAccess.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        // Указание таблицы
        builder.ToTable("events");

        // Указание PK
        builder.HasKey(e => e.Id);

        // Указание, что первичный ключ обязателен и генерируется в коде
        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .IsRequired();

        // Указание обязательного поля названия события с ограничением по кол-ву символов
        builder.Property(e => e.Title)
            .HasMaxLength(100)
            .IsRequired();

        // Указание необязательного поля описания события с ограничением по кол-ву символов
        builder.Property(e => e.Description)
            .HasMaxLength(1000)
            .IsRequired(false);

        // Указание обязательного поля даты начала события
        builder.Property(e => e.StartAt)
            .IsRequired();

        // Указание обязательного поля даты окончания события
        builder.Property(e => e.EndAt)
            .IsRequired();

        // Указание обязательного поля общего количества мест на событие
        builder.Property(e => e.TotalSeats)
            .IsRequired();

        // Указание необязательного поля текущего количества мест на событие
        builder.Property(e => e.AvailableSeats)
            .IsRequired();
    }
}