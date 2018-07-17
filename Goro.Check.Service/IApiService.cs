﻿using Goro.Check.Service.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Goro.Check.Service
{
    public interface IApiService
    {
        UserInfo GetUserInfo(string openId);

        string UserOpenIdBinding(string phoneNumber, string openId);

        List<SalesOrder> QueryOrderList(string fBillNo, int page);

        List<StockViewModel> QueryStockList(string itemName, int page);

        List<CreditViewModel> QueryCreditList(string custName, int page);

        List<SalesReturnNotice> GetSalesReturnNotice(string phoneNumber, int page);

        SalesReturnNoticeDetail GetSalesReturnNoticeDetail(string phoneNumber, string fBillNo);

        string UpdateSalesReturn(string userGroupNumber, string phoneNumber, string billNo, string result, string reason);

        List<SalesOrder> GetSalesOrderList(string phoneNumber, int page);

        SalesOrderDetail GetSalesOrderDetail(string phoneNumber, string fBillNo, string billTypeNumber = "001");

        string UpdateSalesOrder(SalesOrderViewModel model);
    }
}
