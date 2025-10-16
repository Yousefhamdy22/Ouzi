
using Domain.Common;


namespace Domain.Entities
{
    public class OrderItem : Entity , IAggregateRoot
    {

        #region Properites

      
        public int ProductId { get; private set; }
        public string ProductName { get; private set; }
        public decimal Price { get; private set; }
        public int Quantity { get; private set; }
        public string ImageUrl { get; private set; }

        private OrderItem() { }
        public  OrderItem(int ProductId, int  Quantity, decimal Price ) { 
           this.ProductId = ProductId;
            this.Quantity = Quantity;
            this.Price = Price;

        }

        #endregion


        #region Logic

        public static OrderItem Create(int productId, string productName, decimal price, int quantity, string imageUrl = null)
        {
            //CheckRule(new OrderItemMustHaveValidProductRule(productId, productName));
            //CheckRule(new OrderItemMustHavePositiveQuantityRule(quantity));
            //CheckRule(new OrderItemMustHavePositivePriceRule(price));

            return new OrderItem
            {
                ProductId = productId,
                ProductName = productName,
                Price = price,
                Quantity = quantity,
                ImageUrl = imageUrl
            };
        }

      

        public void IncreaseQuantity(int amount)
        {
            //CheckRule(new QuantityMustBePositiveRule(amount));
            Quantity += amount;
        }

        public decimal GetTotal() => Price * Quantity;

        #endregion
    }

}

