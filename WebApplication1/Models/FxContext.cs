using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class FxContext: DbContext
    {
        public DbSet<Tick> Ticks { get; set; }
        public DbSet<Quant> Quants { get; set; }
        public DbSet<Symbol> Symbols { get; set; }
        public DbSet<SymbolSettings> SymbolSettings { get; set; }
        public FxContext() : base("DefaultConnection")
        { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Quant>().HasKey(e => e.Id, cfg => cfg.IsClustered(false));
            modelBuilder.Entity<Tick>().HasKey(e => e.Id, cfg => cfg.IsClustered(false));
        }
    }

    public class Tick
    {
        public Guid Id { get; set; }
        [Index("IX_Tick", 2, IsClustered = true)]
        public int SymbolId { get; set; }
        public virtual Symbol Symbol { get; set; }
        public double Close { get; set; }
        [Index("IX_Tick", 1, IsClustered = true)]
        public DateTime Time { get; set; }
    }
    public class Quant
    {
        public Guid Id { get; set; }
        [Index("IX_Quant", 2, IsClustered = true)]
        public int SymbolId { get; set; }
        public virtual Symbol Symbol { get; set; }
        public double Close { get; set; }
        [Index("IX_Quant", 1, IsClustered = true)]
        public int Ind { get; set; }
    }
    public class SymbolSettings
    {
        public int Id { get; set; }
        [Index("IX_SymbolSettings", 1, IsUnique = true)]
        public int SymbolId { get; set; }
        public virtual Symbol Symbol { get; set; }
        public double QuantSize { get; set; }
    }
    public class Symbol
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}