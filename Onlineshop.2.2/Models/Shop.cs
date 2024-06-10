using Microsoft.EntityFrameworkCore;
using Productec.Models;


namespace Shopic.Models
{
    public class Shop
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public Guid Id { get; set; }
        public string Owner { get; set; }

        public Shop(string name, string address, string owner)
        {
            Id = Guid.NewGuid();
            Name = name;
            Address = address;
            Owner = owner;
        }

        #region CRUD
        public void AddProduct(Product product)
        {
            using ProductRepository Assortiment = new ProductRepository(Name);
            Assortiment.Add(product);
            Assortiment.SaveChanges();
        }

        public List<Product> ReadAllProduct()
        {
            using ProductRepository Assortiment = new ProductRepository(Name);
            return Assortiment.ReadAll();
        }

        public Product ReadProduct(Guid id)
        {
            using ProductRepository Assortiment = new ProductRepository(Name);
            return Assortiment.Read(id);
        }

        public void UpdateName(Guid id, string name)
        {
            using ProductRepository Assortiment = new ProductRepository(Name);
            Assortiment.UpdateName(id, name);
            Assortiment.SaveChanges();
        }

        public void UpdatePrice(Guid id, double price)
        {
            using ProductRepository Assortiment = new ProductRepository(Name);
            Assortiment.UpdatePrice(id, price);
            Assortiment.SaveChanges();
        }

        public void UpdateDescription(Guid id, string description)
        {
            using ProductRepository Assortiment = new ProductRepository(Name);
            Assortiment.UpdateDescription(id, description);
            Assortiment.SaveChanges();
        }

        public void UpdateCount(Guid id, int count)
        {
            using ProductRepository Assortiment = new ProductRepository(Name);
            Assortiment.UpdatePrice(id, count);
            Assortiment.SaveChanges();
        }

        public void UpdateProduct(Product product)
        {
            using var assortiment = new ProductRepository(Name);
            var existingProduct = assortiment.Read(product.Id);
            if (existingProduct != null)
            {
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Count = product.Count;
                existingProduct.Description = product.Description;
                assortiment.SaveChanges();
            }
        }

        public void Buy(Guid id, int count)
        {
            using ProductRepository Assortiment = new ProductRepository(Name);
            Assortiment.Buy(id, count);
            Assortiment.SaveChanges();
        }

        public void DeleteProduct(Guid id)
        {
            using ProductRepository Assortiment = new ProductRepository(Name);
            Assortiment.Delete(id);
            Assortiment.SaveChanges();
        }
        #endregion
    }

    public class ShopDTO
    {
        public string Name { get; set; }
        public string Address { get; set; }
    }

    public class ShopRepository : DbContext
    {
        public DbSet<Shop> Shops { get; set; }

        public ShopRepository()
        {
            Shops = Set<Shop>();
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source=shops.db");
        }
        public List<Shop> ReadAllByOwner(string owner)
        {
            return Shops.Where(shop => shop.Owner == owner).ToList();
        }
        #region CRUD
        public void Add(Shop shop)
        {
            Shops.Add(shop);
            SaveChanges();
        }

        public Shop Read(string name)
        {
            return Shops.ToList().Find(x => x.Name == name);
        }

        public Shop Read(Guid id)
        {
            return Shops.ToList().Find(x => x.Id == id);
        }

        public List<Shop> ReadAll()
        {
            return Shops.ToList();
        }

        public void Delete(Guid id)
        {
            var shop = Read(id);
            if (shop != null)
            {
                Entry(shop).State = EntityState.Deleted;
                SaveChanges();
            }
        }
        #endregion
    }
}
