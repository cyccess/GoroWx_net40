using Goro.Check.Service;
using Goro.Check.Service.Model;
using System.Web.Mvc;

namespace Goro.Check.Web.Controllers
{
    public class ApiController : Controller
    {
        private ApiService apiService;

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

        public ActionResult SalesReturnNotice(int page = 1)
        {
            var res = apiService.GetSalesReturnNotice("13011111111", page);

            var model = new ReturnModel();
            model.Code = 100;
            model.Data = res;

            return Json(model, JsonRequestBehavior.AllowGet);
        }


        public ActionResult SalesReturnNoticeDetail(string phoneNumber, string fBillNo)
        {
            var res = apiService.GetSalesReturnNoticeDetail("13011111111", fBillNo);

            var model = new ReturnModel();
            model.Code = 100;
            model.Data = res;

            return Json(model);
        }

        /// <summary>
        /// 
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
            var res = apiService.UpdateSalesReturn("001", "13022222222", billNo, result, reason);

            var model = new ReturnModel();
            model.Code = 100;
            model.Message = res;

            return Json(model);
        }


        public ActionResult SalesOrders(int page = 1)
        {
            var res = apiService.GetSalesOrderList("13044444444", page);

            var model = new ReturnModel();
            model.Code = 100;
            model.Data = res;

            return Json(model);
        }

        public ActionResult SalesOrderDetail(string phoneNumber, string billTypeNumber, string fBillNo)
        {
            var res = apiService.GetSalesOrderDetail("13033333333", fBillNo);

            var model = new ReturnModel();
            model.Code = 100;
            model.Data = res;

            return Json(model);
        }

        public ActionResult UpdateSalesOrder(SalesOrderViewModel model)
        {
            model.reason = model.reason ?? "";
            var res = apiService.UpdateSalesOrder(model);

            var data = new ReturnModel();
            data.Code = 100;
            data.Message = res;

            return Json(data);
        }
    }
}
