using Ascend15.Models.Pages;
using Mediachase.Commerce.Orders;

namespace Ascend15.Models.ViewModels
{
    public class CartPageViewModel
    {
        public CartPageViewModel(Cart cart, CartPage currentPage)
        {
            Cart = cart;
            CurrentPage = currentPage;
        }

        public Cart Cart { get; set; }
        public CartPage CurrentPage { get; set; }
    }
}
