using Microsoft.EntityFrameworkCore;

namespace EfConstantObjectBug
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using (var db = new Context())
            {
                await db.Database.EnsureCreatedAsync();
            }
            using (var db = new Context())
            {
                if (db.Dealers.Any() == false)
                {
                    var data = new List<Dealer>
                        {
                            new Dealer { DealerName = "Andrew's Records"},
                            new Dealer { DealerName = "Nik's Nik Naks" },
                            new Dealer { DealerName = "Darren's Hardware" },
                            new Dealer { DealerName = "Matt's Mats" }
                        };
                    db.Dealers.AddRange(data);
                    db.SaveChanges();
                }
            }
            using (var db = new Context())
            {
                int ccount = 5;

                Console.WriteLine("This query works, each Compound gets a new instance of Data");
                
                var dealers = db.Dealers
                    .Select(d => new Compound
                    {
                        Dealer = d,
                        Data = new Data
                        {
                            CustomerCount = 5 // assign a constant
                        }
                    }
                    )
                    .ToList();
                foreach (var dealer in dealers)
                {
                    dealer.Data.CustomerCount = dealer.Dealer.DealerName.Length;
                    dealer.Data.Comment = $"{dealer.Dealer.DealerName}: Hello";
                }

                foreach (var dealer in dealers)
                {
                    Console.WriteLine(dealer.Data.Comment);
                }

                Console.WriteLine("This query is incorrect, each Compound gets the same instance of Data");

                dealers = db.Dealers
                    .Select(d => new Compound
                    {
                        Dealer = d,
                        Data = new Data
                        {
                            CustomerCount = ccount // assign a variable
                        }
                    }
                    )
                    .ToList();
                foreach (var dealer in dealers)
                {
                    // This should create a different Data Comment for each compound object,
                    // but in this case all the Data objects are the same instance
                    dealer.Data.CustomerCount = dealer.Dealer.DealerName.Length;
                    dealer.Data.Comment = $"{dealer.Dealer.DealerName}: Hello";
                }

                foreach (var dealer in dealers)
                {
                    Console.WriteLine(dealer.Data.Comment);
                }

            }
        }
    }

    public class Dealer
    {
        public int DealerId { get; set; }
        public string DealerName { get; set; }
    }

    public class Compound
    {
        public Dealer Dealer { get; set; }
        public Data Data { get; set; }
    }

    public class Context : DbContext
    {
        public DbSet<Dealer> Dealers { get; set; }
        public Context()
        {

        }

        public Context(DbContextOptions<Context> options) : base(options)
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=.\\;Database=EfConstantBug;Trusted_Connection=True;MultipleActiveResultSets=true");
        }
    }

    public class Data
    {
        public int CustomerCount { get; set; }
        public string Comment { get; set; }
    }
}
