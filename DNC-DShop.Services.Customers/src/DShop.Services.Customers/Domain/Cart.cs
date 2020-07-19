using System;
using System.Collections.Generic;
using System.Linq;
using DShop.Common.Types;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace DShop.Services.Customers.Domain
{
    public class Cart : IIdentifiable
    {
        private ISet<CartItem> _items = new HashSet<CartItem>();
        public Guid Id { get; private set; }
        [BsonElement(elementName: "CreatedAt")]
        public DateTime CreatedAt { get; private set; }
        [BsonElement(elementName: "Items")]
        public IEnumerable<CartItem> Items
        {
            get => _items;
            set => _items = new HashSet<CartItem>(value);
        }

        protected Cart()
        {
        }

        public Cart(Guid userId)
        {
            Id = userId;
            CreatedAt = DateTime.UtcNow;
        }

        //[JsonConstructor]
        //public Cart(Guid id, DateTime createdAt, IEnumerable<CartItem> items)
        //{
        //    Id = id;
        //    CreatedAt = createdAt;
        //    Items = items;
        //}

        public void Clear()
            => _items.Clear();

        public void AddProduct(Product product, int quantity)
        {
            var item = GetCartItem(product.Id);
            if (item != null)
            {
                item.IncreaseQuantity(quantity);

                return;
            }
            item = new CartItem(product, quantity);
            _items.Add(item);
        }

        public void DeleteProduct(Guid productId)
        {
            var item = GetCartItem(productId);
            if (item == null)
            {
                throw new DShopException("product_not_found",
                    $"Product with id: '{productId}' was not found.");
            }
            _items.Remove(item);
        }

        public void UpdateProduct(Product product)
        {
            var item = GetCartItem(product.Id);
            if (item == null)
            {
                throw new DShopException("product_not_found",
                    $"Product with id: '{product.Id}' was not found.");
            }
            item.Update(product);
        }

        private CartItem GetCartItem(Guid productId)
            => _items.SingleOrDefault(x => x.ProductId == productId);
    }
}