using MongoDB.Bson.Serialization.Attributes;
using System;

namespace DShop.Services.Customers.Domain
{
    public class CartItem
    {
        [BsonElement(elementName: "productId")]
        public Guid ProductId { get; private set; }
        [BsonElement(elementName: "productName")]
        public string ProductName { get; private set; }
        [BsonElement(elementName: "unitPrice")]
        public decimal UnitPrice { get; private set; }
        [BsonElement(elementName: "quantity")]
        public int Quantity { get; private set; }
        public decimal TotalPrice => Quantity * UnitPrice;

        protected CartItem()
        {
        }

        public CartItem(Product product, int quantity)
        {
            ProductId = product.Id;
            ProductName = product.Name;
            UnitPrice = product.Price;
            Quantity = quantity;
        }

        public void IncreaseQuantity(int quantity)
        {
            Quantity += quantity;
        }

        public void Update(Product product)
        {
            ProductName = product.Name;
            UnitPrice = product.Price;
        }
    }
}