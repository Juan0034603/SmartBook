using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmartBook.Domain.Entities;

namespace SmartBook.Persistence.DbContexts;

public class SmartBookDbContext(DbContextOptions<SmartBookDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Cliente> Clientes { get; set; }

    public DbSet<Libro> Libros { get; set; }

    public DbSet<VentaLibro> VentasLibros { get; set; }
    public DbSet<DetalleVenta> DetalleVentas { get; set; }

    public DbSet<Ingreso> Ingresos { get; set; }
    public DbSet<DetalleIngresos> DetalleIngresos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
            d => d.ToDateTime(TimeOnly.MinValue),
            d => DateOnly.FromDateTime(d)
        );

        modelBuilder.Entity<VentaLibro>(entity =>
        {
            entity.ToTable("ventas");
            entity.HasKey(e => e.Id);

            // Relación con DetalleVenta (uno a muchos)
            entity.HasMany(v => v.Detalles)
                  .WithOne()  // ← SIN especificar FK aquí
                  .HasForeignKey(d => d.VentaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.Property(e => e.FechaNacimiento)
                  .HasConversion(dateOnlyConverter);  // ✅ APLICAR AQUÍ
        });

        // ✅ NO necesitas configurar DetalleVenta si ya lo hiciste arriba
        // ELIMINA esta parte:
        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            // ... ELIMINAR
        });

        // ✅ AGREGAR: Configuración de DetalleIngresos
        modelBuilder.Entity<DetalleIngresos>(entity =>
        {
            entity.ToTable("DetalleIngresos");
            entity.HasKey(e => e.IdDetalleIngreso);

            // Relación con Ingreso
            entity.HasOne(d => d.Ingreso)
                  .WithMany(i => i.Detalles)
                  .HasForeignKey(d => d.IdIngreso);

            // ✅ CRÍTICO: Relación con Libro (evita LibroIdLibro)
            entity.HasOne(d => d.Libro)
                  .WithMany()
                  .HasForeignKey(d => d.IdLibro)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ✅ AGREGAR: Configuración de Ingreso
        modelBuilder.Entity<Ingreso>(entity =>
        {
            entity.ToTable("Ingresos");
            entity.HasKey(e => e.IdIngreso);

            entity.HasMany(e => e.Detalles)
                  .WithOne(d => d.Ingreso)
                  .HasForeignKey(d => d.IdIngreso)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }



}



