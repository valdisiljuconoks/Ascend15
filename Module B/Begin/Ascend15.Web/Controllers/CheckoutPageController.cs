using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Ascend15.Extensions;
using Ascend15.Models.Pages;
using Ascend15.Models.ViewModels;
using Ascend15.Services;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Data.Provider;

namespace Ascend15.Controllers
{
    public class CheckoutPageController : BasePageController<CheckoutPage>
    {
        private readonly ICartService _cartService;
        private readonly string _currentLanguage;
        private readonly string _marketid;

        public CheckoutPageController(ICurrentMarket currentMarket, ICartService cartService)
        {
            _cartService = cartService;
            _marketid = currentMarket.GetCurrentMarket().MarketId.Value;
            _currentLanguage = currentMarket.GetCurrentMarket().DefaultLanguage.TwoLetterISOLanguageName;
        }

        public ActionResult Index(CheckoutPage currentPage)
        {
            return View(new CheckoutViewModel(currentPage, _cartService.Cart)
            {
                PaymentMethods = GetPaymentMethods(),
                ShipmentMethods = GetShipmentMethods()
            });
        }

        [HttpPost]
        public ActionResult UpdateShipping(Guid shippingMethodId)
        {
            var shipment = _cartService.CreateShipment();
            UpdateShipment(shipment, GetShipmentMethodById(shippingMethodId));
            UpdateShippingAddress(shipment);

            _cartService.Cart.Validate();
            _cartService.Cart.AcceptChanges();

            return Redirect(CheckoutPage.GetUrl());
        }

        [HttpPost]
        public ActionResult UpdatePayment(Guid paymentMethodId)
        {
            var method = GetPaymentMethodById(paymentMethodId);

            _cartService.Cart.OrderForms.First().Payments.Clear();
            _cartService.Cart.OrderForms.First().Payments.Add(new OtherPayment
            {
                Amount = _cartService.Cart.SubTotal + _cartService.Cart.ShippingTotal,
                BillingAddressId = "DefaultShippingAddress",
                PaymentMethodId = paymentMethodId,
                PaymentMethodName = method.Name
            });

            _cartService.Cart.OrderForms.First().Payments.AcceptChanges();

            _cartService.Cart.Prepare();
            _cartService.Cart.AcceptChanges();

            return Redirect(CheckoutPage.GetUrl());
        }

        [HttpPost]
        public ActionResult PlaceOrder()
        {
            using (var scope = new TransactionScope())
            {
                _cartService.Cart.Checkout();
                var purchaseOrder = _cartService.Cart.SaveAsPurchaseOrder();

                TempData["PONumber"] = purchaseOrder.TrackingNumber;

                _cartService.DeleteCart();
                scope.Complete();
            }

            return Redirect(CheckoutPage.ConfirmationPage.GetUrl());
        }

        private IEnumerable<ShippingRate> GetShipmentMethods()
        {
            return ShippingManager.GetShippingMethodsByMarket(_marketid, false)
                                  .ShippingMethod
                                  .Where(m => m.IsActive
                                              && m.LanguageId.Equals(_currentLanguage, StringComparison.OrdinalIgnoreCase)
                                              && m.Currency.Equals(_cartService.Cart.BillingCurrency, StringComparison.OrdinalIgnoreCase))
                                  .OrderBy(m => m.Ordering)
                                  .Select(m => new ShippingRate(m.ShippingMethodId, m.DisplayName, new Money(m.BasePrice, m.Currency)));
        }

        private ShippingRate GetShipmentMethodById(Guid id)
        {
            return GetShipmentMethods().FirstOrDefault(r => r.Id == id);
        }

        private void UpdateShipment(Shipment shipment, ShippingRate shippingCost)
        {
            if (shipment == null)
            {
                throw new ArgumentNullException(nameof(shipment));
            }

            if (shippingCost == null)
            {
                throw new ArgumentNullException(nameof(shippingCost));
            }

            shipment.ShippingMethodId = shippingCost.Id;
            shipment.ShippingMethodName = shippingCost.Name;
            shipment.SubTotal = shippingCost.Money.Amount;
            shipment.ShipmentTotal = shippingCost.Money.Amount;
            shipment.AcceptChanges();
        }

        private void UpdateShippingAddress(Shipment shipment)
        {
            _cartService.Cart.OrderAddresses.Clear();
            _cartService.Cart.OrderAddresses.Add(new OrderAddress
            {
                CountryCode = "USA",
                CountryName = "United States",
                Name = "DefaultShippingAddress",
                FirstName = "John",
                LastName = "Doe",
                DaytimePhoneNumber = "+1 123 456 789"
            });

            _cartService.Cart.OrderAddresses.AcceptChanges();

            shipment.ShippingAddressId = "DefaultShippingAddress";
            shipment.AcceptChanges();
        }

        private IEnumerable<PaymentMethodDto.PaymentMethodRow> GetPaymentMethods()
        {
            return PaymentManager.GetPaymentMethodsByMarket(_marketid)
                                 .PaymentMethod
                                 .Where(m => m.IsActive
                                             && m.LanguageId.Equals(_currentLanguage, StringComparison.OrdinalIgnoreCase))
                                 .OrderBy(m => m.Ordering);
        }

        private PaymentMethodDto.PaymentMethodRow GetPaymentMethodById(Guid id)
        {
            return GetPaymentMethods().FirstOrDefault(m => m.PaymentMethodId == id);
        }
    }
}
