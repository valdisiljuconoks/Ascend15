using Mediachase.Commerce.Orders;

namespace Ascend15.Services
{
    public interface ICartService
    {
        Cart Cart { get; }
        bool AddToCart(string code);
        void RemoveFromCart(string code);
        Shipment CreateShipment();
        void DeleteCart();
    }
}
