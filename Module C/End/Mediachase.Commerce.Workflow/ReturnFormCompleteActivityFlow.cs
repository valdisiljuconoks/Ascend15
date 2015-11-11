﻿using EPiServer.ServiceLocation;
using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Workflow.Activities.ReturnFormActivities;
using Mediachase.Commerce.WorkflowCompatibility;

namespace Mediachase.Commerce.Workflow
{
    /// <summary>
    /// This class represents the ReturnFormComplete workflow
    /// </summary>
    [ActivityFlowConfiguration(Name = OrderGroupWorkflowManager.ReturnFormCompleteWorkflowName)]
    public class ReturnFormCompleteActivityFlow : ActivityFlow
    {
        /// <inheritdoc />
        public override ActivityFlowRunner Configure(ActivityFlowRunner activityFlow)
        {
            return activityFlow.Do<CreateExchangePaymentActivity>();
        }
    }
}
