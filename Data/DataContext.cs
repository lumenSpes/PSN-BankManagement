using BankManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace BankManagement.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
        public DbSet<UsersTbl> UsersTbls { get; set; }
        public DbSet<UsersBankAccTbl> UsersBankAccTbls { get; set; }
        public DbSet<BankAccTypeTbl> BankAccTypeTbls { get; set; }
        public DbSet<StatusTbl> StatusTbls { get; set; }
        public DbSet<TransTbl> TransTbls { get; set; }
        public DbSet<TransTypeTbl> TransTypeTbls { get; set; }
        public DbSet<UserTypeTbl> UserTypeTbls { get; set; }
        public DbSet<ElectricityBillTbl> ElectricityBillTbls { get; set; }
        public DbSet<DepositeTbl> DepositeTbls { get; set; }
        public DbSet<UserProfileTbl> UserProfileTbls { get; set; }
        public DbSet<FixedDeposit> FixedDeposits { get; set; }
        public DbSet<Loan> Loans { get; set; }
    }
}
