using LibraryDataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace LibraryDataAccess
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions options) : base(options) { }

        public DbSet<Patron> Patrons { get; set; }
        //run PM> add-migration "Initial Migration"
        //run PM> update-database
        public DbSet<Book> Books { get; set; }
        public DbSet<BranchHours> BranchHours { get; set; }
        public DbSet<Checkout> Checkouts { get; set; }
        public DbSet<CheckoutHistory> CheckoutHistories { get; set; }
        public DbSet<LibraryBranch> LibraryBranches { get; set; }
        public DbSet<LibraryCard> LibraryCards { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<LibraryAsset> LibraryAssets { get; set; }
        public DbSet<Hold> Holds { get; set; }
        public DbSet<Video> Videos { get; set; }
        //run PM> add-migration "Add Initial entity models"
        //run PM> update-database

    }
}
