using Domain.Common;


namespace Domain.Rules.Cart
{
   
    public class CartMustHaveUserIdRule : IBusinessRule
    {
        private readonly Guid _userId;

        public CartMustHaveUserIdRule(Guid userId) => _userId = userId;

        public bool IsBroken() => string.IsNullOrWhiteSpace(_userId.ToString());
        public string Message => "Cart must have a valid user ID.";
    }

    public class CartMustHaveItemsRule : IBusinessRule
    {
        private readonly IEnumerable<Domain.Entities.CartItem> _items;

        public CartMustHaveItemsRule(IEnumerable<Domain.Entities.CartItem> items) => _items = items;

        public bool IsBroken() => !_items.Any();
        public string Message => "Cart must have at least one item to perform this operation.";
    }

    public class CartCannotBeModifiedWhenEmptyRule : IBusinessRule
    {
        private readonly bool _isEmpty;

        public CartCannotBeModifiedWhenEmptyRule(bool isEmpty) => _isEmpty = isEmpty;

        public bool IsBroken() => _isEmpty;
        public string Message => "Cannot modify an empty cart.";
    }

    public class CartItemLimitRule : IBusinessRule
    {
        private readonly int _currentItemCount;
        private readonly int _newItemQuantity;
        private const int MaxTotalItems = 50; 

        public CartItemLimitRule(int currentItemCount, int newItemQuantity)
        {
            _currentItemCount = currentItemCount;
            _newItemQuantity = newItemQuantity;
        }

        public bool IsBroken() => (_currentItemCount + _newItemQuantity) > MaxTotalItems;
        public string Message => $"Cart cannot exceed {MaxTotalItems} total items. Current: {_currentItemCount}";
    }

  
    public class CartUniqueProductLimitRule : IBusinessRule
    {
        private readonly int _currentUniqueProducts;
        private const int MaxUniqueProducts = 20;

        public CartUniqueProductLimitRule(int currentUniqueProducts)
        {
            _currentUniqueProducts = currentUniqueProducts;
        }

        public bool IsBroken() => _currentUniqueProducts >= MaxUniqueProducts;
        public string Message => $"Cart cannot exceed {MaxUniqueProducts} different products.";
    }

 
    public class CartTotalAmountLimitRule : IBusinessRule
    {
        private readonly decimal _currentTotal;
        private readonly decimal _newItemTotal;
        private const decimal MaxCartTotal = 10000m;

        public CartTotalAmountLimitRule(decimal currentTotal, decimal newItemTotal)
        {
            _currentTotal = currentTotal;
            _newItemTotal = newItemTotal;
        }

        public bool IsBroken() => (_currentTotal + _newItemTotal) > MaxCartTotal;
        public string Message => $"Cart total cannot exceed ${MaxCartTotal}.";
    }
}
