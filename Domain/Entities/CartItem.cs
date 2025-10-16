
using Domain.Common;
using Domain.Rules.CartItem;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace Domain.Entities
{
    public class CartItem : Entity
    {

        #region Properites
        public int Id { get; set; }
        public int ProductId { get; private set; }

        [ForeignKey("Cart")]
        public int CartId { get; internal set; }
        public string ProductName { get; private set; }
        public decimal UnitPrice { get; private set; }
        public int Quantity { get; private set; }
        public string ImageUrl { get; private set; }

        public decimal Total => UnitPrice * Quantity;

        [JsonIgnore]
        public Cart Cart { get; set; }
        public Product Product { get; set; }
        private CartItem() { }



        public CartItem(int cartId, int productId, string productName, decimal unitPrice, int quantity, string imageUrl)
        {
            CartId = cartId; 
            ProductId = productId;
            ProductName = productName;
            UnitPrice = unitPrice;
            Quantity = quantity;
            ImageUrl = imageUrl;
        }
        #endregion


        #region Logic
        public static CartItem Create(int cartId, int productId, string productName, decimal unitPrice, int quantity, string imageUrl = null)
        {
            CheckRule(new CartItemMustHaveValidProductRule(productId, productName));
            CheckRule(new CartItemMustHavePositiveQuantityRule(quantity));
            CheckRule(new CartItemMustHavePositivePriceRule(unitPrice));

            return new CartItem
            {
                CartId = cartId,
                ProductId = productId,
                ProductName = productName,
                UnitPrice = unitPrice,
                Quantity = quantity,
                ImageUrl = imageUrl
            };
        }

    
        //public static CartItem Create(int cartId , int productId, string productName, decimal unitPrice, int quantity, string imageUrl = null)
        //{
        //    CheckRule(new CartItemMustHaveValidProductRule(productId, productName));
        //    CheckRule(new CartItemMustHavePositiveQuantityRule(quantity));
        //    CheckRule(new CartItemMustHavePositivePriceRule(unitPrice));

        //    return new CartItem
        //    {
        //        CartId = cartId,
        //        ProductId = productId,
        //        ProductName = productName,
        //        UnitPrice = unitPrice,
        //        Quantity = quantity,
        //        ImageUrl = imageUrl
        //    };
           
        //}

        public void IncreaseQuantity(int amount)
        {
            //CheckRule(new QuantityMustBePositiveRule(amount));
            Quantity += amount;
        }

        public void UpdateQuantity(int newQuantity)
        {
            CheckRule(new CartItemMustHavePositiveQuantityRule(newQuantity));
            Quantity = newQuantity;
        }

        public void DecreaseQuantity(int amount)
        {
            //CheckRule(new QuantityMustBePositiveRule(amount));
            //CheckRule(new QuantityCannotBeNegativeRule(Quantity, amount));
            Quantity -= amount;
        }

        #endregion
    }
}