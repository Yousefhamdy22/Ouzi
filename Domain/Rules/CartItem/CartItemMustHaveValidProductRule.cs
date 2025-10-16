using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Rules.CartItem
{
    // Domain/Rules/CartItemMustHaveValidProductRule.cs
    public class CartItemMustHaveValidProductRule : IBusinessRule
    {
        private readonly int _productId;
        private readonly string _productName;

        public CartItemMustHaveValidProductRule(int productId, string productName)
        {
            _productId = productId;
            _productName = productName;
        }

        public bool IsBroken() => _productId <= 0 || string.IsNullOrWhiteSpace(_productName);
        public string Message => "Cart item must have a valid product ID and name.";
    }

    // Domain/Rules/CartItemMustHavePositiveQuantityRule.cs
    public class CartItemMustHavePositiveQuantityRule : IBusinessRule
    {
        private readonly int _quantity;

        public CartItemMustHavePositiveQuantityRule(int quantity) => _quantity = quantity;

        public bool IsBroken() => _quantity <= 0;
        public string Message => "Cart item quantity must be greater than zero.";
    }

    // Domain/Rules/CartItemMustHavePositivePriceRule.cs
    public class CartItemMustHavePositivePriceRule : IBusinessRule
    {
        private readonly decimal _price;

        public CartItemMustHavePositivePriceRule(decimal price) => _price = price;

        public bool IsBroken() => _price < 0;
        public string Message => "Cart item price cannot be negative.";
    }

    // Domain/Rules/CartItemMaximumQuantityRule.cs
    public class CartItemMaximumQuantityRule : IBusinessRule
    {
        private readonly int _quantity;
        private const int MaxQuantityPerProduct = 10; // Configurable limit

        public CartItemMaximumQuantityRule(int quantity) => _quantity = quantity;

        public bool IsBroken() => _quantity > MaxQuantityPerProduct;
        public string Message => $"Cannot add more than {MaxQuantityPerProduct} units of the same product.";
    }

    // Domain/Rules/CartItemPriceChangeRule.cs
    public class CartItemPriceChangeRule : IBusinessRule
    {
        private readonly decimal _currentPrice;
        private readonly decimal _newPrice;
        private const decimal MaxPriceChangePercent = 20m; // 20% price change threshold

        public CartItemPriceChangeRule(decimal currentPrice, decimal newPrice)
        {
            _currentPrice = currentPrice;
            _newPrice = newPrice;
        }

        public bool IsBroken()
        {
            if (_currentPrice == 0) return false; // First time adding
            var percentChange = Math.Abs((_newPrice - _currentPrice) / _currentPrice * 100);
            return percentChange > MaxPriceChangePercent;
        }

        public string Message => $"Product price has changed significantly. Please review before adding to cart.";
    }

    // Domain/Rules/CartItemStockValidationRule.cs
    public class CartItemStockValidationRule : IBusinessRule
    {
        private readonly int _requestedQuantity;
        private readonly int _availableStock;

        public CartItemStockValidationRule(int requestedQuantity, int availableStock)
        {
            _requestedQuantity = requestedQuantity;
            _availableStock = availableStock;
        }

        public bool IsBroken() => _requestedQuantity > _availableStock;
        public string Message => $"Only {_availableStock} units available in stock. Requested: {_requestedQuantity}.";
    }
}
