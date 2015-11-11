using System;
using System.Collections.Generic;
using System.Linq;
using Ascend15.Models.Pages;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;

namespace Ascend15.Models.ViewModels
{
    public class CheckoutViewModel
    {
        public CheckoutViewModel(CheckoutPage currentPage, Cart cart)
        {
            CurrentPage = currentPage;
            Cart = cart;

            if (cart.OrderForms != null && cart.OrderForms.Any())
            {
                var orderForm = cart.OrderForms.First();
                if (orderForm.Shipments != null && orderForm.Shipments.Any())
                {
                    SelectedShippingMethod = orderForm.Shipments.First().ShippingMethodId;
                }

                if (orderForm.Payments != null && orderForm.Payments.Any())
                {
                    SelectedPaymentMethod = orderForm.Payments.First().PaymentMethodId;
                }

            }
        }

        public CheckoutPage CurrentPage { get; set; }
        public Cart Cart { get; set; }
        public IEnumerable<PaymentMethodDto.PaymentMethodRow> PaymentMethods { get; set; }
        public IEnumerable<ShippingRate> ShipmentMethods { get; set; }
        public Guid SelectedShippingMethod { get; set; }
        public Guid SelectedPaymentMethod { get; set; }
    }
}
