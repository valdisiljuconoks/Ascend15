﻿using EPiServer.ServiceLocation;
using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Workflow.Activities;
using Mediachase.Commerce.Workflow.Activities.Cart;
using Mediachase.Commerce.Workflow.Activities.OrderGroupActivities;
using Mediachase.Commerce.Workflow.Activities.PurchaseOrderActivities;
using Mediachase.Commerce.WorkflowCompatibility;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Mediachase.Commerce.Workflow
{
    /// <summary>
    /// This class represents the Cart Prepare workflow
    /// </summary>
    [ActivityFlowConfiguration(Name = OrderGroupWorkflowManager.CartPrepareWorkflowName)]
    public class CartPrepareActivityFlow : ActivityFlow
    {
        /// <inheritdoc />
        public override ActivityFlowRunner Configure(ActivityFlowRunner activityFlow)
        {
            return activityFlow.Do<ValidateLineItemsActivity>()
                            .Do<GetFulfillmentWarehouseActivity>()
                            .If(() => ShouldCheckInstoreInventory())
                                .Do<CheckInstoreInventoryActivity>()
                            .Else()
                                .Do<CheckInventoryActivity>()
                            .EndIf()
                            .Do<ProcessShipmentsActivity>()
                            .Do<CalculateTotalsActivity>()
                            .Do<RemoveDiscountsActivity>()
                            .Do<CalculateTotalsActivity>()
                            .Do<CalculateDiscountsActivity>()
                            .Do<CalculateTotalsActivity>()
                            .Do<CalculateTaxActivity>()
                            .Do<CalculateTotalsActivity>();
        }
    }
}
