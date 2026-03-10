using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SocietyVaccinations.Models;

public partial class SVContext : DbContext
{
    public SVContext()
    {
    }

    public SVContext(DbContextOptions<SVContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Consultation> Consultations { get; set; }

    public virtual DbSet<Medical> Medicals { get; set; }

    public virtual DbSet<Regional> Regionals { get; set; }

    public virtual DbSet<Society> Societies { get; set; }

    public virtual DbSet<Spot> Spots { get; set; }

    public virtual DbSet<SpotVaccine> SpotVaccines { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vaccination> Vaccinations { get; set; }

    public virtual DbSet<Vaccine> Vaccines { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Data Source=(localdb)\\mssqllocaldb;Integrated Security=true;Database=SocietyVaccination");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Consultation>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Doctor).WithMany(p => p.Consultations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_consultations_doctor");

            entity.HasOne(d => d.Society).WithMany(p => p.Consultations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_consultations_society");
        });

        modelBuilder.Entity<Medical>(entity =>
        {
            entity.HasOne(d => d.Spot).WithMany(p => p.Medicals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_medicals_spots");

            entity.HasOne(d => d.User).WithMany(p => p.Medicals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_medicals_users");
        });

        modelBuilder.Entity<Society>(entity =>
        {
            entity.Property(e => e.IdCardNumber).IsFixedLength();
            entity.Property(e => e.Name).IsFixedLength();

            entity.HasOne(d => d.Regional).WithMany(p => p.Societies)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_societies_regionals");
        });

        modelBuilder.Entity<Spot>(entity =>
        {
            entity.HasOne(d => d.Regional).WithMany(p => p.Spots)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_spots_regionals");
        });

        modelBuilder.Entity<SpotVaccine>(entity =>
        {
            entity.HasOne(d => d.Spot).WithMany(p => p.SpotVaccines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_spot_vaccines_spots");

            entity.HasOne(d => d.Vaccine).WithMany(p => p.SpotVaccines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_spot_vaccines_vaccines");
        });

        modelBuilder.Entity<Vaccination>(entity =>
        {
            entity.HasOne(d => d.Doctor).WithMany(p => p.VaccinationDoctors)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_vaccinations_doctors");

            entity.HasOne(d => d.Officer).WithMany(p => p.VaccinationOfficers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_vaccinations_officers");

            entity.HasOne(d => d.Society).WithMany(p => p.Vaccinations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_vaccinations_societies");

            entity.HasOne(d => d.Spot).WithMany(p => p.Vaccinations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_vaccinations_spots");

            entity.HasOne(d => d.Vaccine).WithMany(p => p.Vaccinations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_vaccinations_vaccines");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
