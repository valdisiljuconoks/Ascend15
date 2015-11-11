using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using System;

namespace Mediachase.Commerce.Workflow.Activities.PurchaseOrderActivities
{
    public abstract class PurchaseOrderBaseActivity : OrderGroupActivityBase
    {
        private const string EventsCategory = "Handlers";

        #region Public Events
        
        /// <summary>
        /// Occurs when [changing order status].
        /// </summary>
        public static string ChangingOrderStatusEvent = "ChangingOrderStatus";

        
        /// <summary>
        /// Occurs when [changing order status].
        /// </summary>
        public static string ChangedOrderStatusEvent = "ChangedOrderStatus";
        

        #endregion

        protected OrderStatus OrderStatus
        {
            get
            {
                if (this.OrderGroup is PurchaseOrder)
                    return OrderStatusManager.GetPurchaseOrderStatus(this.OrderGroup as Mediachase.Commerce.Orders.PurchaseOrder);
                else
                    return OrderStatusManager.GetPurchaseOrderStatus(this.OrderGroup as PaymentPlan);
            }
        }

        protected void ChangeOrderStatus(OrderStatus newStatus)
        {
            RaiseEvent(ChangingOrderStatusEvent, EventArgs.Empty);

            this.OrderGroup.Status = newStatus.ToString();

            RaiseEvent(ChangedOrderStatusEvent, EventArgs.Empty);
        }

        protected static OrderShipmentStatus GetShipmentStatus(Shipment shipment)
        {
            OrderShipmentStatus retVal = OrderShipmentStatus.InventoryAssigned;
            if (!string.IsNullOrEmpty(shipment.Status))
            {
                retVal = (OrderShipmentStatus)Enum.Parse(typeof(OrderShipmentStatus), shipment.Status);
            }
            return retVal;
        }
    }
}
