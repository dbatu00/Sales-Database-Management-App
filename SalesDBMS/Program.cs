public class Store
{
    public int Id { get; set; }
    public string StoreName { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Cost { get; set; }
    public decimal Price { get; set; }
}

public class Sale
{
    public int StoreId { get; set; }
    public int ProductId { get; set; }
    public DateTime Date { get; set; }
    public int SaleQuantity { get; set; }
    public decimal Stock { get; set; }
}


public class SalesManager
{
    private List<Store> stores;
    private List<Product> products;
    private List<Sale> sales;

    private string storesFilePath = "Data/stores.csv";
    private string productsFilePath = "Data/products.csv";
    private string salesFilePath = "Data/inventory-sales.csv";

    public SalesManager()
    {
        stores = LoadStores();
        products = LoadProducts();
        sales = LoadSales();
    }

    private List<Store> LoadStores()
    {
        List<Store> stores = new List<Store>();
        using (StreamReader reader = new StreamReader(storesFilePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] data = line.Split(',');
                if (int.TryParse(data[0], out int id))
                {
                    string storeName = data[1];
                    stores.Add(new Store { Id = id, StoreName = storeName });
                }
                else
                {
                    Console.WriteLine($"Stores loaded: {line}");
                }
            }
        }
        return stores;
    }

    private List<Product> LoadProducts()
    {
        List<Product> products = new List<Product>();
        using (StreamReader reader = new StreamReader(productsFilePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] data = line.Split(',');
                if (int.TryParse(data[0], out int id) && decimal.TryParse(data[2], out decimal cost) && decimal.TryParse(data[3], out decimal price))
                {
                    string name = data[1];
                    products.Add(new Product { Id = id, Name = name, Cost = cost, Price = price });
                }
                else
                {
                    Console.WriteLine($"Products loaded: {line}");
                }
            }
        }
        return products;
    }

    private List<Sale> LoadSales()
    {
       
        List<Sale> sales = new List<Sale>();
        using (StreamReader reader = new StreamReader(salesFilePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] data = line.Split(',');
                if (int.TryParse(data[1], out int storeId) && int.TryParse(data[0], out int productId) && DateTime.TryParse(data[2], out DateTime date)
                    && int.TryParse(data[3], out int SaleQuantity) && decimal.TryParse(data[4], out decimal Stock))
                {
                    sales.Add(new Sale { StoreId = storeId, ProductId = productId, Date = date, SaleQuantity = SaleQuantity, Stock = Stock });
                }
                else
                {
                    Console.WriteLine($"Inventory loaded: {line}\n");
                }
            }
        }
       
        return sales;
    }


    public List<Sale> GetSalesHistory()
    {
        return sales;
    }

    public void PrintSalesHistory()
    {
        // Get sales history
        Console.WriteLine("Sales History:");
        foreach (var sale in GetSalesHistory())
        {
            Console.WriteLine($" Product ID: {sale.ProductId}, Store ID: {sale.StoreId}, Date: {sale.Date}, Sale Quantity: {sale.SaleQuantity}, Stock: {sale.Stock}");
        }
        Console.WriteLine("\n");

    }

    public void AddSale(Sale newSale)
    {
        sales.Add(newSale);
        string newSaleData = $"{newSale.ProductId},{newSale.StoreId},{newSale.Date.ToString("yyyy-MM-dd")},{newSale.SaleQuantity},{newSale.Stock}";
        using (StreamWriter writer = new StreamWriter(salesFilePath, true))
        {
            writer.WriteLine(newSaleData);
        }
        Console.WriteLine($"New Sale Added: {newSaleData}");
    }

    public void DeleteSale(Sale sale)
    {
        string saleData = $"{sale.ProductId},{sale.StoreId},{sale.Date.ToString("yyyy-MM-dd")},{sale.SaleQuantity},{sale.Stock}";
        sales.Remove(sale);
        SaveSalesToFile();
        Console.WriteLine($"Sale deleted: {saleData}");
    }

    /*public void UpdateSale(Sale updatedSale)
    {
        string saleData = $"{updatedSale.StoreId},{updatedSale.ProductId},{updatedSale.Date.ToString("yyyy-MM-dd")},{updatedSale.SaleQuantity},{updatedSale.Stock}";
        Sale existingSale = sales.FirstOrDefault(s => s.StoreId == updatedSale.StoreId && s.ProductId == updatedSale.ProductId && s.Date == updatedSale.Date);
        if (existingSale != null)
        {
            existingSale.SaleQuantity = updatedSale.SaleQuantity;
            existingSale.Stock = updatedSale.Stock;
            SaveSalesToFile();
            Console.WriteLine($"Sale updated: {saleData}");
        }
    }
    */

    public void UpdateSale(int index, Sale updatedSale)
    {
        string saleData = $"{updatedSale.ProductId},{updatedSale.StoreId},{updatedSale.Date.ToString("yyyy-MM-dd")},{updatedSale.SaleQuantity},{updatedSale.Stock}";
        if (index >= 0 && index < sales.Count)
        {
            Sale existingSale = sales[index];
            existingSale.SaleQuantity = updatedSale.SaleQuantity;
            existingSale.Stock = updatedSale.Stock;
           
            SaveSalesToFile();
            Console.WriteLine($"Sale at index {index} updated: {saleData}");
        }
        else
        {
            Console.WriteLine($"Invalid index: {index}");
        }
    }


    public decimal GetProfitForStore(int storeId)
    {
        decimal totalProfit = 0;
        foreach (var sale in sales)
        {
            if (sale.StoreId == storeId)
            {
                Product product = products.FirstOrDefault(p => p.Id == sale.ProductId);
                if (product != null)
                {
                    decimal cost = product.Cost;
                    decimal price = product.Price;
                    decimal profit = (price - cost) * sale.SaleQuantity;
                    totalProfit += profit;
                }
            }
        }
        return totalProfit;
    }

    public Store GetMostProfitableStore()
    {
        decimal maxProfit = 0;
        Store mostProfitableStore = null;
        foreach (var store in stores)
        {
            decimal profit = GetProfitForStore(store.Id);
            if (profit > maxProfit)
            {
                maxProfit = profit;
                mostProfitableStore = store;
            }
        }
        return mostProfitableStore;
    }

    public Product GetBestSellerBySaleQuantity()
    {
        var groupedSales = sales.GroupBy(s => s.ProductId);
        var bestSeller = groupedSales.OrderByDescending(g => g.Sum(s => s.SaleQuantity)).FirstOrDefault();
        if (bestSeller != null)
        {
            int productId = bestSeller.Key;
            return products.FirstOrDefault(p => p.Id == productId);
        }
        return null;
    }

    private void SaveSalesToFile()
    {
        using (StreamWriter writer = new StreamWriter(salesFilePath))
        {
            writer.WriteLine("ProductId,StoreId,Date,SalesQuantity,Stock");
            foreach (var sale in sales)
            {
                string saleData = $"{sale.StoreId},{sale.ProductId},{sale.Date.ToString("yyyy-MM-dd")},{sale.SaleQuantity},{sale.Stock}";
                writer.WriteLine(saleData);
            }
        }
    }  
}

class Program
{
    static void Main(string[] args)
    {
        SalesManager salesManager = new SalesManager();
        salesManager.PrintSalesHistory();

        // Add new sale
        Sale newSale = new Sale
        {
            StoreId = 1,
            ProductId = 2,
            Date = DateTime.Now,
            SaleQuantity = 3,
            Stock = 75
        };
        salesManager.AddSale(newSale);
        salesManager.PrintSalesHistory();



        // Delete sale
        Sale saleToDelete = salesManager.GetSalesHistory().FirstOrDefault();
        if (saleToDelete != null)
        {
            salesManager.DeleteSale(saleToDelete);
        }

        salesManager.PrintSalesHistory();


        // Update sale
        // Not: update sale fonksiyonunda update edilecek sale'i seçmenin yanında
        // hangi değerlerinin değiştirileceğinin update sale'i overload ederek belirtilebil-
        // eceğinin farkındayım ancak 6! adet farklı overload yazmak istemedim
        // sale'in indexi seçilerek update edilebiliyor
        // öte yandan bir date'in bütün değerlerini update edebilmek ne kadar sağlıklı 
        // bilmiyorum ancak herhangi bir kural belirtilmediği için stock ve sale quantity'nin update
        // edilebilmesine izin verdim okunulabilirlik için

        Sale newerSale = new Sale
        {
            StoreId = 5,
            ProductId = 7,
            Date = DateTime.Now,
            SaleQuantity = 20,
            Stock = 3000
        };        
        salesManager.UpdateSale(2, newerSale);
        salesManager.PrintSalesHistory();


        // Get profit for given store
        int storeId = 1;
        decimal storeProfit = salesManager.GetProfitForStore(storeId);
        Console.WriteLine($"Profit for Store ID {storeId}: {storeProfit}");

        // Get the most profitable store
        Store mostProfitableStore = salesManager.GetMostProfitableStore();
        if (mostProfitableStore != null)
        {
            Console.WriteLine($"Most Profitable Store: Store ID {mostProfitableStore.Id}, Store Name: {mostProfitableStore.StoreName}");
        }

        // Get the best-selling product by SaleQuantity
        Product bestSellerBySaleQuantity = salesManager.GetBestSellerBySaleQuantity();
        if (bestSellerBySaleQuantity != null)
        {
            Console.WriteLine($"Best Seller by Sales SaleQuantity: Product ID {bestSellerBySaleQuantity.Id}, Name: {bestSellerBySaleQuantity.Name}");
        }

        Console.ReadLine();
    }
}





