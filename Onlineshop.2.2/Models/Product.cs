using Microsoft.EntityFrameworkCore;

namespace Productec.Models
{
    public class Product
    {
        public Guid Id { set; get; }
        public string Name { get; set; }
        public double Price { get; set; }
        public int Count { get; set; }
        public string Description { get; set; }

        public Product(string name, double price, int count, string description)
        {
            Name = name;
            Price = price;
            Count = count;
            Description = description;
            Guid Id = Guid.NewGuid();
        }
        public void Buy(int count)
        {
            Count = Count - count;
        }
    }
    class ProductDTO
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public int Count { get; set; }

        public string Description { get; set; }
    }
    public class ProductRepository : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public string Name { get; set; }

        public ProductRepository(string name)
        {
            Name = name;
            Products = Set<Product>();
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={Name}");
        }
        #region CRUD
        public void Add(Product product)
        { Products.Add(product); }
        public List<Product> ReadAll()
        {
            return Products.ToList();
        }
        public Product Read(Guid id)
        {
            return Products.ToList().Find(x => x.Id == id);
        }
        public void UpdateName(Guid id, string name)
        {
            Products.ToList().Find(x => x.Id == id).Name = name;
        }
        public void UpdatePrice(Guid id, double price)
        {
            Products.ToList().Find(x => x.Id == id).Price = price;
        }
        public void UpdateCount(Guid id, int count)
        {
            Products.ToList().Find(x => x.Id == id).Count = count;
        }
        public void UpdateDescription(Guid id, string description)
        {
            Products.ToList().Find(x => x.Id == id).Description = description;
        }
        public void Buy(Guid id, int count)
        {
            Products.ToList().Find(x => x.Id == id).Buy(count);
        }
        public void Delete(Guid id)
        {
            Products.Remove(Read(id));
        }
        
        #endregion
    }
}
