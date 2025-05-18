using Microsoft.EntityFrameworkCore;

namespace WinFormsApp1
{
    public class AppDbContext : DbContext
    {
        public DbSet<Сотрудник> Сотрудники { get; set; }
        public DbSet<Статус_дня> Статусы_дней { get; set; }
        public DbSet<Табель> Табели { get; set; }
        public DbSet<Отклонение> Отклонения { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=LABA4;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Явное указание имен таблиц
            modelBuilder.Entity<Сотрудник>().ToTable("Сотрудник");
            modelBuilder.Entity<Статус_дня>().ToTable("Статус_дня");
            modelBuilder.Entity<Табель>().ToTable("Табель");
            modelBuilder.Entity<Отклонение>().ToTable("Отклонение");

            modelBuilder.Entity<Сотрудник>().HasKey(e => e.Сотрудник_id);
            modelBuilder.Entity<Статус_дня>().HasKey(e => e.Статус_id);
            modelBuilder.Entity<Табель>().HasKey(e => new { e.Табель_id, e.Сотрудник_id, e.Статус_id });
            modelBuilder.Entity<Отклонение>().HasKey(e => new { e.Отклонение_id, e.Табель_id, e.Сотрудник_id, e.Статус_id });

            modelBuilder.Entity<Табель>()
                .HasOne(t => t.Сотрудник)
                .WithMany(s => s.Табели)
                .HasForeignKey(t => t.Сотрудник_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Табель>()
                .HasOne(t => t.Статус)
                .WithMany(s => s.Табели)
                .HasForeignKey(t => t.Статус_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Отклонение>()
                .HasOne(o => o.Табель)
                .WithMany(t => t.Отклонения)
                .HasForeignKey(o => new { o.Табель_id, o.Сотрудник_id, o.Статус_id })
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
