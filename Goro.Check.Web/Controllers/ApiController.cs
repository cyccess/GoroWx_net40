using Goro.Check.Service;
using Goro.Check.Service.Model;
using System.Web.Mvc;

namespace Goro.Check.Web.Controllers
{
    public class ApiController : Controller
    {
        private readonly IApiService apiService;

        public ApiController()
        {
            apiService = new ApiService();
        }


        public ActionResult Login(string openId)
        {
            var userInfo = apiService.GetUserInfo(openId);

            var model = new ReturnModel();
            model.Code = 100;
            model.Data = userInfo;
            return Json(model);
        }

        public ActionResult UserBinding(string phoneNumber, string openId)
        {
            var model = new ReturnModel();

            var res = apiService.UserOpenIdBinding(phoneNumber, openId);

            if (res == "OK")
            {
                var userInfo = apiService.GetUserInfo(openId);
                model.Data = userInfo;
            }

            model.Code = 100;
            model.Message = res;

            return Json(model);
        }

        /// <summary>
        /// 退货通知单
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult SalesReturnNotice(string phoneNumber, int page = 1)
        {
            var res = apiService.GetSalesReturnNotice(phoneNumber, page);

            var model = new ReturnModel();
            if (res.Count == 0) model.Code = 0;

            model.Data = res;

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SalesReturnNoticeDetail(string phoneNumber, string fBillNo)
        {
            var res = apiService.GetSalesReturnNoticeDetail(phoneNumber, fBillNo);

            var model = new ReturnModel();
            model.Code = 100;
            model.Data = res;

            return Json(model);
        }

        /// <summary>
        /// 退货单审核
        /// </summary>
        /// <param name="userGroupNumber"></param>
        /// <param name="phoneNumber"></param>
        /// <param name="billNo"></param>
        /// <param name="result">审核结果 Y/N</param>
        /// <param name="reason">未通过原因</param>
        /// <returns></returns>
        public ActionResult UpdateSalesReturn(string userGroupNumber, string phoneNumber, string billNo, string result, string reason)
        {
            reason = reason ?? "";
            var res = apiService.UpdateSalesReturn(userGroupNumber, phoneNumber, billNo, result, reason);

            var model = new ReturnModel();
            model.Code = 100;
            model.Message = res;

            return Json(model);
        }

        /// <summary>
        /// 销售订单
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult SalesOrders(string phoneNumber, int page = 1)
        {
            var res = apiService.GetSalesOrderList(phoneNumber, page);

            var model = new ReturnModel();
            if (res.Count == 0) model.Code = 0;
            model.Data = res;

            return Json(model);
        }

        public ActionResult SalesOrderDetail(string phoneNumber, string fBillNo)
        {
            var res = apiService.GetSalesOrderDetail(phoneNumber, fBillNo);

            var model = new ReturnModel();
            model.Code = 100;
            model.Data = res;

            return Json(model);
        }

        /// <summary>
        /// 销售订单审核
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult UpdateSalesOrder(SalesOrderViewModel model)
        {
            model.reason = model.reason ?? "";
            var res = apiService.UpdateSalesOrder(model);

            var data = new ReturnModel();
            data.Code = 100;
            data.Message = res;

            return Json(data);
        }

        /// <summary>
        /// 订单查询
        /// </summary>
        /// <param name="fBillNo"></param>
        /// <param name="fEmpName">业务员</param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult QueryOrderList(string fBillNo, string fEmpName, string userGroupNumber, int page = 1)
        {
            var model = new ReturnModel();

            if (!apiService.IsExistsUserGroupFieldDisplayed(userGroupNumber))
            {
                model.Code = -5;
                model.Message = "用户所在分组，未设置显示字段";
                return Json(model);
            }

            var list = apiService.QueryOrderList(fBillNo, fEmpName, page);

            if (list == null || list.Count == 0) model.Code = 0;

            model.Data = list;
            return Json(model);
        }

        // 订单详情
        public ActionResult OrderDetail(string phoneNumber, string fBillNo)
        {
            var res = apiService.QueryOrderDetail(phoneNumber, fBillNo);

            var model = new ReturnModel();
            model.Code = 100;
            model.Data = res;

            return Json(model);
        }

        /// <summary>
        /// 库存查询
        /// </summary>
        /// <param name="itemName"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult QueryStockList(string itemName, int page = 1)
        {
            var list = apiService.QueryStockList(itemName, page);

            var model = new ReturnModel();

            if (list == null || list.Count == 0) model.Code = 0;

            model.Data = list;
            return Json(model);
        }

        /// <summary>
        /// 信用额度查询
        /// </summary>
        /// <param name="custName"></param>
        /// <param name="fEmpName">业务员</param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult QueryCreditList(string custName, string fEmpName, int page = 1)
        {
            var list = apiService.QueryCreditList(custName, fEmpName, page);

            var model = new ReturnModel();

            if (list == null || list.Count == 0) model.Code = 0;

            model.Data = list;
            return Json(model);
        }
    }
}
