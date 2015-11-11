<%@ Control Language="c#" Inherits="Mediachase.Commerce.Manager.Order.TaxEdit" Codebehind="TaxEdit.ascx.cs" %>
<%@ Register Src="~/Apps/Core/SaveControl.ascx" TagName="SaveControl" TagPrefix="ecf" %>
<%@ Register Src="~/Apps/Core/Controls/EditViewControl.ascx" TagName="EditViewControl" TagPrefix="ecf" %>
<div class="editDiv">
<ecf:EditViewControl AppId="Order" ViewId="Tax-Edit" id="ViewControl" runat="server" MDContext="<%# Mediachase.Commerce.Orders.OrderContext.MetaDataContext %>"></ecf:EditViewControl>
<ecf:SaveControl id="EditSaveControl" ShowDeleteButton="false" runat="server"></ecf:SaveControl>
</div>