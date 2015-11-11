using System.Linq;
using Ascend15.Extensions;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Security;
using Mediachase.Commerce.Website.Helpers;
using Mediachase.MetaDataPlus;

namespace Ascend15.Services
{
    [ServiceConfiguration(typeof (ICartService), Lifecycle = ServiceInstanceScope.Unique)]
    public class CartService : ICartService
    {
        private readonly CartHelper _helper;

        public CartService()
        {
            _helper = new CartHelper(Cart.DefaultName, PrincipalInfo.CurrentPrincipal.GetContactId());
        }

        public Cart Cart => _helper.Cart;

        public bool AddToCart(string code)
        {
            var entry = CatalogContext.Current.GetCatalogEntry(code);
            _helper.AddEntry(entry);
            Cart.ProviderId = "frontend";

            // TODO: return back to viewmodel
            var warnings = Cart.Validate();
            Cart.AcceptChanges();

            return warnings != null;
        }

        public void RemoveFromCart(string code)
        {
            var line = Cart.GetLineItem(code);
            if (line != null)
            {
                PurchaseOrderManager.RemoveLineItemFromOrder(Cart, line.LineItemId);
                Cart.Validate();
                Cart.AcceptChanges();
            }
        }

        public Shipment CreateShipment()
        {
            if (Cart.ObjectState == MetaObjectState.Added)
            {
                Cart.AcceptChanges();
            }

            var orderForms = Cart.OrderForms;
            if (orderForms.Count == 0)
            {
                orderForms.AddNew().AcceptChanges();
                orderForms.Single().Name = Cart.Name;
            }

            var orderForm = orderForms.First();

            var shipments = orderForm.Shipments;
            if (shipments.Count != 0)
            {
                shipments.Clear();
            }

            var shipment = shipments.AddNew();
            for (var i = 0; i < orderForm.LineItems.Count; i++)
            {
                var item = orderForm.LineItems[i];
                shipment.AddLineItemIndex(i, item.Quantity);
            }

            shipment.AcceptChanges();

            return shipment;
        }

        public void DeleteCart()
        {
            var cart = Cart;
            foreach (OrderForm orderForm in cart.OrderForms)
            {
                foreach (Shipment shipment in orderForm.Shipments)
                {
                    shipment.Delete();
                }
                orderForm.Delete();
            }
            foreach (OrderAddress address in cart.OrderAddresses)
            {
                address.Delete();
            }

            _helper.Delete();
            cart.AcceptChanges();
        }
    }
}
