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
        public DbSet<Network> Networks { get; set; }
        public DbSet<NetworkCalculationTask> NetworkCalculationTasks { get; set; }
        public FxContext() : base("DefaultConnection")
        { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Quant>().HasKey(e => e.Id, cfg => cfg.IsClustered(false));
            modelBuilder.Entity<Tick>().HasKey(e => e.Id, cfg => cfg.IsClustered(false));
            modelBuilder.Entity<Quant>().HasRequired(q=>q.Symbol).WithMany(s=>s.Quants).WillCascadeOnDelete(false);
            modelBuilder.Entity<Tick>().HasRequired(q => q.Symbol).WithMany(s => s.Ticks).WillCascadeOnDelete(false);
            modelBuilder.Entity<Quant>().HasRequired(q => q.Tick).WithMany(s => s.Quants).WillCascadeOnDelete(false);
            modelBuilder.Entity<Network>().HasRequired(q => q.Symbol).WithMany(s => s.Networks).WillCascadeOnDelete(false);
            modelBuilder.Entity<NetworkCalculationTask>().HasRequired(q => q.Symbol).WithMany(s => s.NetworkCalculationTasks).WillCascadeOnDelete(false);
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
        public virtual ICollection<Quant> Quants { get; set; }
    }
    public enum Recommendation : byte
    {
        CloseAll = 0,
        Buy = 1,
        Sell = 2
    }
    public class Quant
    {
        public Guid Id { get; set; }
        [Index("IX_Quant", 2, IsClustered = true, IsUnique = true)]
        public int SymbolId { get; set; }
        public virtual Symbol Symbol { get; set; }
        public double Close { get; set; }
        [Index("IX_Quant", 1, IsClustered = true, IsUnique = true)]
        public int Ind { get; set; }
        public Guid TickId { get; set; }
        public virtual Tick Tick { get; set; }
        public Recommendation? Recommendation { get; set; }
    }
    public class Symbol
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double QuantSize { get; set; }
        public int InputWindow { get; set; }
        public int PredictWindow { get; set; }
        public virtual ICollection<Quant> Quants { get; set; }
        public virtual ICollection<Tick> Ticks { get; set; }
        public virtual ICollection<Network> Networks { get; set; }
        public virtual ICollection<NetworkCalculationTask> NetworkCalculationTasks { get; set; }
    }

    public enum TaskStatus:byte
    {
        New = 1,
        InProgress = 2,
        Complited = 3
    }
    public class NetworkCalculationTask
    {
        public int Id { get; set; }
        public DateTime ShouldStartAt { get; set; }
        public TaskStatus Status { get; set; }
        public int SymbolId { get; set; }
        public virtual Symbol Symbol { get; set; }
    }
    public class Network
    {
        public Guid Id { get; set; }
        public int SymbolId { get; set; }
        public virtual Symbol Symbol { get; set; }
        public string FileName { get; set; }
        public DateTime LastRecalculation { get; set; }
        public int QuantsProcessed { get; set; }
        public double Quality { get; set; }
    }
}