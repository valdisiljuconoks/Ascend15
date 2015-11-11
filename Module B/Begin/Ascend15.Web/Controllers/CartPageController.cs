using System.Web.Mvc;
using Ascend15.Extensions;
using Ascend15.Models.Pages;
using Ascend15.Models.ViewModels;
using Ascend15.Services;

namespace Ascend15.Controllers
{
    public class CartPageController : BasePageController<CartPage>
    {
        private readonly ICartService _cartService;

        public CartPageController(ICartService cartService)
        {
            _cartService = cartService;
        }

        public ActionResult Index(CartPage currentPage)
        {
            return View(new CartPageViewModel(_cartService.Cart, currentPage));
        }

        [HttpPost]
        public ActionResult Add(string code, string returnUrl = null)
        {
            _cartService.AddToCart(code);
            return Redirect(CartPage.GetUrl());
        }

        [HttpPost]
        public ActionResult Remove(string code)
        {
            _cartService.RemoveFromCart(code);
            return Redirect(CartPage.GetUrl());
        }
    }
}
