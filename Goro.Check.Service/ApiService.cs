using Goro.Check.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Data;
using Goro.Check.Service.Model;

namespace Goro.Check.Service
{
    public class ApiService : IApiService
    {
        public UserInfo GetUserInfo(string openId)
        {
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[]
                {
                    new SqlParameter{ ParameterName = "@OpenId", Value = openId, SqlDbType = SqlDbType.NVarChar }
                };

                string sql = "select * from tm_v_UserInfo where FUserOpenID=@OpenId";
                var res = SqlHelper.ExecuteDataTable(CommandType.Text, sql, sqlParameter);

                var entity = res.AsEnumerable().Select(u => new UserInfo
                {
                    FEmpName = u.Field<string>(0),
                    FPhoneNumber = u.Field<string>(1),
                    FEmpID = u.Field<int?>(2),
                    FEmpNumber = u.Field<string>(3),
                    FUserGroupID = u.Field<int?>(4),
                    FUserGroupNumber = u.Field<string>(5),
                    FUserGroupName = u.Field<string>(6),
                    FUserOpenID = u.Field<string>(7)
                }).FirstOrDefault();

                //entity.FEmpName = "总经理组";
                //entity.FUserGroupNumber = "002";
                //entity.FPhoneNumber = "13044444444";

                return entity;
            }
            catch (Exception e)
            {
                LoggerHelper.Info("获取用户信息错误：" + e);
                return null;
            }
        }


        public string UserOpenIdBinding(string phoneNumber, string openId)
        {
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[]
                {
                    new SqlParameter{ ParameterName = "@PhoneNumber", Value = phoneNumber, SqlDbType = SqlDbType.NVarChar },
                    new SqlParameter{ ParameterName = "@OpenID", Value = openId, SqlDbType = SqlDbType.NVarChar },
                    new SqlParameter{ ParameterName = "@Msg", Value="", Direction = ParameterDirection.Output, Size=100, SqlDbType = SqlDbType.NVarChar }
                };

                var res = SqlHelper.ExecuteNonQuery(CommandType.StoredProcedure, "tm_p_UpdateUserOpenID", sqlParameter);
                LoggerHelper.Info("绑定用户[" + phoneNumber + "]，openid:" + openId);

                return sqlParameter[2].Value.ToString();
            }
            catch (Exception e)
            {
                LoggerHelper.Info("绑定用户错误：[" + phoneNumber + "] " + e);
                return "绑定用户失败";
            }
        }

        /// <summary>
        /// 查询订单
        /// </summary>
        /// <param name="fBillNo"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public List<SalesOrder> QueryOrderList(QueryOrderViewModel model)
        {
            try
            {
                int pageSize = 10;
                string queryValue = "";// 是否
                var sqlParameter = new List<SqlParameter>()
                {
                    new SqlParameter("@PageIndex",(model.page - 1) * pageSize),
                    new SqlParameter("@PageSize",model.page * pageSize)
                };

                string sql = "select ROW_NUMBER() over(order by FDate desc) rownumber, FBillNo,FCustName,FName,FDeptName from tm_v_SEOrderQuery where 1=1";

                if (!string.IsNullOrEmpty(model.fBillNo))
                {
                    sql += " and (FBillNo like @FBillNo or FEmpName like @FBillNo or FCustName like @FBillNo)";
                    sqlParameter.Add(new SqlParameter { ParameterName = "@FBillNo", Value = "%" + model.fBillNo + "%", SqlDbType = SqlDbType.NVarChar });
                }

                if (!string.IsNullOrEmpty(model.fEmpName)) //根据业务人员名称查询
                {
                    sql += " and FEmpName like @FEmpName";
                    sqlParameter.Add(new SqlParameter { ParameterName = "@FEmpName", Value = "%" + model.fEmpName + "%", SqlDbType = SqlDbType.NVarChar });
                }

                if (!model.isConfirm.Equals("-1")) // 是否确认
                {
                    sql += "and FConfirmStatus=@FConfirmStatus";

                    //总经理未审核通过的为特批未通过
                    if (model.isConfirm.Equals("2"))
                    {
                        queryValue = "特批未通过";
                    }
                    else if (model.isConfirm.Equals("0"))
                    {
                        queryValue = "否";
                    }
                    else if (model.isConfirm.Equals("1"))
                    {
                        queryValue = "是";
                    }
                    sqlParameter.Add(new SqlParameter { ParameterName = "@FConfirmStatus", Value = queryValue, SqlDbType = SqlDbType.NVarChar });
                }

                if (!model.isStock.Equals("-1")) //是否发货
                {
                    sql += " and FStockStatus=@FStockStatus";
                    queryValue = model.isStock == "1" ? "是" : "否";
                    sqlParameter.Add(new SqlParameter { ParameterName = "@FStockStatus", Value = queryValue, SqlDbType = SqlDbType.NVarChar });
                }

                if (!model.isInvoice.Equals("-1")) //是否开票
                {
                    sql += " and FInvoiceStatus=@FInvoiceStatus";
                    queryValue = model.isInvoice == "1" ? "是" : "否";
                    sqlParameter.Add(new SqlParameter { ParameterName = "@FInvoiceStatus", Value = queryValue, SqlDbType = SqlDbType.NVarChar });
                }

                if (!model.isReceive.Equals("-1")) //是否收款
                {
                    sql += " and FReceiveStatus=@FReceiveStatus";
                    queryValue = model.isReceive == "1" ? "是" : "否";
                    sqlParameter.Add(new SqlParameter { ParameterName = "@FReceiveStatus", Value = queryValue, SqlDbType = SqlDbType.NVarChar });
                }

                string cmdText = "select* from(" + sql + ") as t"
                           + " where rownumber>@PageIndex and rownumber<=@PageSize";

                var res = SqlHelper.ExecuteDataTable(CommandType.Text, cmdText, sqlParameter.ToArray());

                var list = res.AsEnumerable().Select(row => new SalesOrder
                {
                    Id = Convert.ToInt32(row["rownumber"]),
                    FBillNo = row["FBillNo"].ToString(),
                    FCustName = row["FCustName"].ToString(),
                    FName = row["FName"].ToString(),
                    FDeptName = row["FDeptName"].ToString()
                }).ToList();

                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 查询订单详情
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="fBillNo"></param>
        /// <returns></returns>
        public SalesOrderDetail QueryOrderDetail(string phoneNumber, string fBillNo)
        {
            try
            {
                SalesOrderDetail salesOrder = new SalesOrderDetail();
                var fields = GetUserGroupFieldDisplayed(phoneNumber, "003");

                salesOrder.Field = fields;
                var field = string.Join(",", fields.Select(f => f.FFieldName).ToArray());

                SqlParameter[] sqlParameter = new SqlParameter[]
                {
                    new SqlParameter("@FBillNo",fBillNo)
                };

                string sql = "select " + field + " from tm_v_SEOrderQuery where FBillNo=@FBillNo";

                LoggerHelper.Info("sql:" + sql);

                var res = SqlHelper.ExecuteDataTable(CommandType.Text, sql, sqlParameter);
                salesOrder.Order = res;

                LoggerHelper.Info("查询订单详情");
                return salesOrder;
            }
            catch (Exception e)
            {
                LoggerHelper.Info("查询订单详情错误：" + e);
                return null;
            }
        }


        public List<StockViewModel> QueryStockList(string itemName, int page)
        {
            try
            {
                int pageSize = 10;
                var sqlParameter = new List<SqlParameter>()
                {
                    new SqlParameter("@PageIndex",(page - 1) * pageSize),
                    new SqlParameter("@PageSize",page * pageSize)
                };

                string sql = "select ROW_NUMBER() over(order by FItemNumber) rownumber, FItemNumber,FItemName,FItemModel,FStockName,FQty from tm_v_icinventory where 1=1";

                if (!string.IsNullOrEmpty(itemName))
                {
                    sql += " and FItemName like @FItemName";
                    sqlParameter.Add(new SqlParameter { ParameterName = "@FItemName", Value = "%" + itemName + "%", SqlDbType = SqlDbType.NVarChar });
                }

                string cmdText = "select* from(" + sql + ") as t"
                           + " where rownumber>@PageIndex and rownumber<=@PageSize";

                var res = SqlHelper.ExecuteDataTable(CommandType.Text, cmdText, sqlParameter.ToArray());

                var list = res.AsEnumerable().Select(row => new StockViewModel
                {
                    Id = Convert.ToInt32(row["rownumber"]),
                    FItemNumber = row["FItemNumber"].ToString(),
                    FItemName = row["FItemName"].ToString(),
                    FItemModel = row["FItemModel"].ToString(),
                    FStockName = row["FStockName"].ToString(),
                    FQty = Convert.ToDecimal(row["FQty"])
                }).ToList();

                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<CreditViewModel> QueryCreditList(string custName, string fEmpName, int page)
        {
            try
            {
                int pageSize = 10;
                var sqlParameter = new List<SqlParameter>()
                {
                    new SqlParameter("@PageIndex",(page - 1) * pageSize),
                    new SqlParameter("@PageSize",page * pageSize)
                };

                string sql = "select ROW_NUMBER() over(order by FCustName) rownumber, FCustName,FAmount,FEmpName from tm_v_CreditObject where 1=1";

                if (!string.IsNullOrEmpty(custName))
                {
                    sql += " and(FCustName like @FCustName or FEmpName like @FCustName)";
                    sqlParameter.Add(new SqlParameter { ParameterName = "@FCustName", Value = "%" + custName + "%", SqlDbType = SqlDbType.NVarChar });
                }

                if (!string.IsNullOrEmpty(fEmpName)) //根据业务人员名称查询
                {
                    sql += " and FEmpName=@FEmpName";
                    sqlParameter.Add(new SqlParameter { ParameterName = "@FEmpName", Value = fEmpName, SqlDbType = SqlDbType.NVarChar });
                }

                string cmdText = "select* from(" + sql + ") as t"
                           + " where rownumber>@PageIndex and rownumber<=@PageSize";

                var res = SqlHelper.ExecuteDataTable(CommandType.Text, cmdText, sqlParameter.ToArray());

                var list = res.AsEnumerable().Select(row => new CreditViewModel
                {
                    Id = Convert.ToInt32(row["rownumber"]),
                    FCustName = row["FCustName"].ToString(),
                    FAmount = Convert.ToDecimal(row["FAmount"]),
                    FEmpName = row["FEmpName"].ToString(),

                }).ToList();

                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// 退货通知单
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public List<SalesReturnNotice> GetSalesReturnNotice(string phoneNumber, int page)
        {
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[]
                {
                    new SqlParameter{ ParameterName = "@PhoneNumber", Value = phoneNumber, SqlDbType = SqlDbType.NVarChar }
                };

                //退货通知单-根据手机号获取单号和客户
                var dt = SqlHelper.ExecuteDataTable(CommandType.StoredProcedure, "tm_p_GetSalesReturnNotice", sqlParameter);

                int count = (page - 1) * 10;
                var salesReturnNotices = dt.AsEnumerable().Select(r => new SalesReturnNotice
                {
                    FBillNo = r[0].ToString(),
                    FCustName = r[1].ToString()
                }).Skip(count).Take(20).ToList();

                int sn = count;
                foreach (var item in salesReturnNotices)
                {
                    item.Id = sn += 1;
                }

                LoggerHelper.Info("退货通知单：根据手机号获取单号和客户");

                return salesReturnNotices;
            }
            catch (Exception e)
            {
                LoggerHelper.Info("退货通知单：[" + phoneNumber + "]" + e);
                return null;
            }
        }

        /// <summary>
        /// 退货通知单详情
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="groupId"></param>
        /// <param name="fBillNo"></param>
        /// <returns></returns>
        public SalesReturnNoticeDetail GetSalesReturnNoticeDetail(string phoneNumber, string fBillNo)
        {
            try
            {
                SalesReturnNoticeDetail returnNoticeDetail = new SalesReturnNoticeDetail();

                var fields = GetUserGroupFieldDisplayed(phoneNumber, "002");
                returnNoticeDetail.Field = fields;

                var field = string.Join(",", fields.Select(f => f.FFieldName).ToArray());

                SqlParameter[] sqlParameter = new SqlParameter[]
                {
                    new SqlParameter{ ParameterName = "@FBillNo", Value = fBillNo, SqlDbType = SqlDbType.NVarChar }
                };

                string sql = "select " + field + " from tm_v_SalesReturnNotice where FBillNo=@FBillNo";
                var res = SqlHelper.ExecuteDataTable(CommandType.Text, sql, sqlParameter);

                returnNoticeDetail.Order = res;

                LoggerHelper.Info("退货通知单详情");
                return returnNoticeDetail;
            }
            catch (Exception e)
            {
                LoggerHelper.Info("退货通知单详情，[" + phoneNumber + "],fBillNo：" + fBillNo + e);
                return null;
            }
        }

        /// <summary>
        /// 根据手机号获得可查看字段
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="billTypeNumber">单据类型，001：销售订单；002：销售退货通知单</param>
        /// <returns></returns>
        public List<FieldDisplayed> GetUserGroupFieldDisplayed(string phoneNumber, string billTypeNumber)
        {
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[]
                {
                   new SqlParameter{ ParameterName = "@PhoneNumber", Value = phoneNumber, SqlDbType = SqlDbType.NVarChar },
                   new SqlParameter{ ParameterName = "@BillTypeNumber", Value = billTypeNumber, SqlDbType = SqlDbType.NVarChar },
                };

                var dt = SqlHelper.ExecuteDataTable(CommandType.StoredProcedure, "tm_p_GetFieldDisplayed", sqlParameter);
                var fields = dt.AsEnumerable().Select(r => new FieldDisplayed
                {
                    FFieldDataType = r["FFieldDataType"].ToString(),
                    FFieldName = GetFirstLowerStr(r["FFieldName"].ToString()),
                    FFieldDescription = r["FFieldDescription"].ToString()
                }).ToList();

                return fields;
            }
            catch (Exception e)
            {
                LoggerHelper.Info("根据手机号获得可查看字段，[" + phoneNumber + "]" + e);
                return null;
            }
        }

        /// <summary>
        /// 是否存在用户组字段显示
        /// </summary>
        /// <param name="userGroupNumber"></param>
        /// <returns></returns>
        public bool IsExistsUserGroupFieldDisplayed(string userGroupNumber)
        {
            string cmdText = "select count(1) from tm_v_UserGroupFieldDisplayed where FUserGroupNumber=@UserGroupNumber";
            var res = SqlHelper.ExecuteScalar(CommandType.Text, cmdText, new SqlParameter("@UserGroupNumber", userGroupNumber));
            bool check = false;
            if (res != null)
            {
                int.TryParse(res.ToString(), out int count);
                if (count > 0)
                {
                    check = true;
                }
            }
            return check;
        }

        /// <summary>
        /// 首字母转小写
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private string GetFirstLowerStr(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                if (s.Length > 1)
                {
                    return char.ToLower(s[0]) + s.Substring(1);
                }
                return char.ToLower(s[0]).ToString();
            }
            return null;
        }

        /// <summary>
        /// 退货通知单审核
        /// </summary>
        /// <param name="userGroupNumber"></param>
        /// <param name="phoneNumber"></param>
        /// <param name="billNo"></param>
        /// <param name="result"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public string UpdateSalesReturn(string userGroupNumber, string phoneNumber, string billNo, string result, string reason)
        {
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[]
                {
                    new SqlParameter("@PhoneNumber",phoneNumber),
                    new SqlParameter("@BillNo",billNo),
                    new SqlParameter("@Result",result),
                    new SqlParameter("@Reason",reason),
                    new SqlParameter{ ParameterName = "@Msg", Value="", Direction = ParameterDirection.Output, Size=100, SqlDbType = SqlDbType.NVarChar }
                };

                string cmdText = "";
                string toUser = "";
                string title = "";
                string noticeDetailUrl = WebConfig.WebHost + "/#/salesReturnNoticeDetail?billNo=" + billNo;

                if (userGroupNumber == "001") //销售总监组审核更新
                {
                    cmdText = "tm_p_UpdateSalesReturnCSO";
                }
                if (userGroupNumber == "009") //质检组审核更新
                {
                    cmdText = "tm_p_UpdateSalesReturnQC";
                }

                if (cmdText == "")
                    return "用户分组未找到！";

                SqlHelper.ExecuteNonQuery(CommandType.StoredProcedure, cmdText, sqlParameter);
                var msg = sqlParameter.Last().Value.ToString();
                if (msg == "OK")
                {
                    if (userGroupNumber == "001")
                    {
                        toUser = GetUserIdByUserGroup("002", "008"); // 总经理，制单人
                        title = "退货通知单[" + billNo + "]" + (result == "Y" ? " 销售总监已审核通过" : " 销售总监未审核通过");
                        WechatService.Send(toUser, title, reason, noticeDetailUrl);
                        // 如果同意，发送微信通知给质检组
                        if (result == "Y")
                        {
                            toUser = GetUserIdByUserGroup("009");
                            title = "您有退通知单[" + billNo + "]需要审核";
                            WechatService.Send(toUser, title, reason, noticeDetailUrl);
                        }
                    }
                    if (userGroupNumber == "009")
                    {
                        toUser = GetUserIdByUserGroup("001", "002", "008");//销售总监、总经理和制单人
                        title = "退货通知单[" + billNo + "]" + (result == "Y" ? " 已通过质检审核" : " 未通过质检审核");
                        WechatService.Send(toUser, title, reason, noticeDetailUrl);
                    }
                }
                LoggerHelper.Info("退货通知单[" + billNo + "]:" + msg);
                return msg;
            }
            catch (Exception e)
            {
                LoggerHelper.Info("退货通知单审核[" + userGroupNumber + "]:" + phoneNumber + e);
                return "退货通知单审核失败";
            }
        }


        /// <summary>
        /// 销售订单-根据手机号获取单号、客户、类型
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="page"></param>
        public List<SalesOrder> GetSalesOrderList(string phoneNumber, int page)
        {
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[]
                {
                    new SqlParameter{ ParameterName = "@PhoneNumber", Value = phoneNumber, SqlDbType = SqlDbType.NVarChar }
                };

                //销售订单-根据手机号获取单号和客户
                var dt = SqlHelper.ExecuteDataTable(CommandType.StoredProcedure, "tm_p_GetSalesOrderList", sqlParameter);

                int count = (page - 1) * 20;
                var salesOrders = dt.AsEnumerable().Select(r => new SalesOrder
                {
                    FBillNo = r[0].ToString(),
                    FCustName = r[1].ToString(),
                    FName = r[2].ToString(),
                    FDeptName = r[3].ToString(),
                    FType = r[4].ToString()
                }).Skip(count).Take(20).ToList();

                int sn = count;
                foreach (var item in salesOrders)
                {
                    item.Id = sn += 1;
                }

                LoggerHelper.Info("销售订单-根据手机号获取单号、客户、类型");

                return salesOrders;
            }
            catch (Exception e)
            {
                LoggerHelper.Info("销售订单 " + e);
                return null;
            }
        }

        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="fBillNo"></param>
        /// <returns></returns>
        public SalesOrderDetail GetSalesOrderDetail(string phoneNumber, string fBillNo)
        {
            try
            {
                SalesOrderDetail salesOrder = new SalesOrderDetail();
                var fields = GetUserGroupFieldDisplayed(phoneNumber, "001");
                salesOrder.Field = fields;
                var field = string.Join(",", fields.Select(f => f.FFieldName).ToArray());

                SqlParameter[] sqlParameter = new SqlParameter[]
                {
                    new SqlParameter("@FBillNo",fBillNo)
                };

                string sql = "select " + field + " from tm_v_SeOrderList where FBillNo=@FBillNo";

                LoggerHelper.Info("sql:" + sql);

                var res = SqlHelper.ExecuteDataTable(CommandType.Text, sql, sqlParameter);
                salesOrder.Order = res;

                LoggerHelper.Info("销售订单详情");
                return salesOrder;
            }
            catch (Exception e)
            {
                LoggerHelper.Info("销售订单详情错误：" + e);
                return null;
            }
        }

        /// <summary>
        /// 销售单审核
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string UpdateSalesOrder(SalesOrderViewModel model)
        {
            try
            {
                switch (model.userGroupNumber)
                {
                    case "002":
                        return UpdateSalesOrderGM(model);
                    case "004":
                        return UpdateSalesOrderPD(model);
                    case "005":
                        return UpdateSalesOrderME(model);
                    case "006":
                        return UpdateSalesOrderPO(model);
                    default:
                        LoggerHelper.Info("销售单审核：用户分组未找到" + model.userGroupNumber);
                        return "用户分组未找到！";
                }
            }
            catch (Exception e)
            {
                LoggerHelper.Info("销售单审核【" + model.billNo + "：" + model.phoneNumber + "】" + e);
                return "销售单审核失败";
            }
        }

        /// <summary>
        /// 总经理审核
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private static string UpdateSalesOrderGM(SalesOrderViewModel model)
        {
            var sqlParameter = new List<SqlParameter>()
            {
                new SqlParameter("@PhoneNumber",model.phoneNumber),
                new SqlParameter("@BillNo",model.billNo),
                new SqlParameter("@Reason", model.reason),
                new SqlParameter("@Result", model.result),
                new SqlParameter { ParameterName = "@Msg", Value = "", Direction = ParameterDirection.Output, Size = 100, SqlDbType = SqlDbType.NVarChar }
            };

            SqlHelper.ExecuteNonQuery(CommandType.StoredProcedure, "tm_p_UpdateSalesOrderGM", sqlParameter.ToArray());
            var msg = sqlParameter.Last().Value.ToString();

            if (msg == "OK")
            {
                string toUser = GetUserIdByUserGroup("008"); //制单人组
                toUser += GetSeOrderUserId(model.billNo); //业务员消息只发当前订单
                string title = "销售订单[" + model.billNo + "]已特批";
                string noticeDetailUrl = WebConfig.WebHost + "/#/salesOrderDetail?billNo=" + model.billNo;

                model.reason = ComposeMessageContent(model.billNo, model.reason);

                WechatService.Send(toUser, title, model.reason, noticeDetailUrl);

                if (model.result == "Y")
                {
                    toUser = GetUserIdByUserGroup("004"); //通知生产组
                    title = "您有销售订单[" + model.billNo + "]需要确认";
                    WechatService.Send(toUser, title, model.reason, noticeDetailUrl);
                }
            }

            LoggerHelper.Info("销售单审核,总经理审核更新【" + model.billNo + "：" + model.phoneNumber + "】" + msg);
            return msg;
        }

        private static string UpdateSalesOrderPD(SalesOrderViewModel model)
        {
            var sqlParameter = new List<SqlParameter>()
            {
                new SqlParameter("@PhoneNumber",model.phoneNumber),
                new SqlParameter("@BillNo",model.billNo)
            };

            string cmdText = "";
            if (model.result == "Y") //生产确认通过更新
            {
                cmdText = "tm_p_UpdateSalesOrderPDC";
                sqlParameter.Add(new SqlParameter("@DeliveryDate", model.deliveryDate));
                sqlParameter.Add(new SqlParameter("@Result", model.result));
            }

            if (model.result == "N") //生产不通过更新
            {
                cmdText = "tm_p_UpdateSalesOrderPDD";
                sqlParameter.Add(new SqlParameter("@Result", model.result));
                sqlParameter.Add(new SqlParameter("@Reason", model.reason));
                sqlParameter.Add(new SqlParameter("@ISME", model.isMe));
                sqlParameter.Add(new SqlParameter("@ISPO", model.isPo));
            }

            sqlParameter.Add(new SqlParameter { ParameterName = "@Msg", Value = "", Direction = ParameterDirection.Output, Size = 100, SqlDbType = SqlDbType.NVarChar });

            var res = SqlHelper.ExecuteNonQuery(CommandType.StoredProcedure, cmdText, sqlParameter.ToArray());
            var msg = sqlParameter.Last().Value.ToString();

            if (msg == "OK")
            {
                string toUser = "";
                string title = "";
                string noticeDetailUrl = WebConfig.WebHost + "/#/salesOrderDetail?billNo=" + model.billNo;

                model.reason = ComposeMessageContent(model.billNo, model.reason);

                if (model.result == "Y")
                {
                    toUser = GetUserIdByUserGroup("008");//制单人组
                    toUser += GetSeOrderUserId(model.billNo); //业务员消息只发当前订单
                    title = "销售订单[" + model.billNo + "]已通过生产确认";
                    WechatService.Send(toUser, title, model.reason, noticeDetailUrl);
                }
                else
                {
                    toUser = GetUserIdByUserGroup("008");//制单人组
                    toUser += GetSeOrderUserId(model.billNo); //业务员消息只发当前订单
                    title = "销售订单[" + model.billNo + "]生产未确认";
                    WechatService.Send(toUser, title, model.reason, noticeDetailUrl);

                    if (model.isMe == "1")
                    {
                        toUser = GetUserIdByUserGroup("005");//工艺通知
                        title = "您有销售订单[" + model.billNo + "]需要回复";
                        WechatService.Send(toUser, title, model.reason, noticeDetailUrl);
                    }

                    if (model.isPo == "1")
                    {
                        toUser = GetUserIdByUserGroup("006"); //供应通知
                        title = "您有销售订单[" + model.billNo + "]需要回复";
                        WechatService.Send(toUser, title, model.reason, noticeDetailUrl);
                    }
                }
            }
            LoggerHelper.Info("销售生产审核更新【" + model.billNo + "：" + model.phoneNumber + "】" + msg);

            return msg;
        }

        private static string UpdateSalesOrderME(SalesOrderViewModel model)
        {
            var sqlParameter = new List<SqlParameter>()
            {
                new SqlParameter("@PhoneNumber",model.phoneNumber),
                new SqlParameter("@BillNo",model.billNo),
                new SqlParameter("@Reason", model.reason),
                new SqlParameter { ParameterName = "@Msg", Value = "", Direction = ParameterDirection.Output, Size = 100, SqlDbType = SqlDbType.NVarChar }
            };

            string cmdText = "tm_p_UpdateSalesOrderME";

            var res = SqlHelper.ExecuteNonQuery(CommandType.StoredProcedure, cmdText, sqlParameter.ToArray());
            var msg = sqlParameter.Last().Value.ToString();

            if (msg == "OK")
            {
                string toUser = GetUserIdByUserGroup("004", "008"); ;//生产组,制单人组
                toUser += GetSeOrderUserId(model.billNo); //业务员消息只发当前订单
                string noticeDetailUrl = WebConfig.WebHost + "/#/salesOrderDetail?billNo=" + model.billNo;
                model.reason = ComposeMessageContent(model.billNo, model.reason);

                WechatService.Send(toUser, "销售订单[" + model.billNo + "]工艺已回复", model.reason, noticeDetailUrl);
            }

            LoggerHelper.Info("销售订单[" + model.billNo + "]工艺已回复：" + msg);

            return msg;
        }

        private static string UpdateSalesOrderPO(SalesOrderViewModel model)
        {
            var sqlParameter = new List<SqlParameter>()
            {
                new SqlParameter("@PhoneNumber",model.phoneNumber),
                new SqlParameter("@BillNo",model.billNo),
                new SqlParameter("@Reason", model.reason),
                new SqlParameter { ParameterName = "@Msg", Value = "", Direction = ParameterDirection.Output, Size = 100, SqlDbType = SqlDbType.NVarChar }
            };

            string cmdText = "tm_p_UpdateSalesOrderPO";

            var res = SqlHelper.ExecuteNonQuery(CommandType.StoredProcedure, cmdText, sqlParameter.ToArray());
            var msg = sqlParameter.Last().Value.ToString();

            if (msg == "OK")
            {
                string toUser = GetUserIdByUserGroup("004", "008"); ;//生产组,制单人组
                toUser += GetSeOrderUserId(model.billNo); //业务员消息只发当前订单
                string noticeDetailUrl = WebConfig.WebHost + "/#/salesOrderDetail?billNo=" + model.billNo;
                model.reason = ComposeMessageContent(model.billNo, model.reason);

                WechatService.Send(toUser, "销售订单[" + model.billNo + "]供应已回复", model.reason, noticeDetailUrl);
            }
            LoggerHelper.Info("销售订单[" + model.billNo + "]供应已回复：" + msg);

            return msg;
        }

        public static void SendNotice()
        {
            try
            {
                SendNoticeToGM();
                SendNoticeToPD();
                SendNoticeToCSO();

                LoggerHelper.Info("定时消息发送");
            }
            catch (Exception e)
            {
                LoggerHelper.Info("消息发送错误：" + e);
            }
        }

        /// <summary>
        /// 销售订单 - 通知总经理
        /// </summary>
        private static void SendNoticeToGM()
        {
            string cmdText = "select top 1 FBillNo,FGMCheckCause from tm_v_SeOrderList where FISGM=1 and FISGMSendMessage=0";
            var dt = SqlHelper.ExecuteDataTable(CommandType.Text, cmdText);
            var res = dt.AsEnumerable()
                .Select(o => new { FBillNo = o["FBillNo"], FGMCheckCause = o["FGMCheckCause"] })
                .SingleOrDefault();

            if (res != null)
            {
                // 更新消息状态
                string sql = "update seorder set FHeadSelfs0165=1 where FBillNo=@FBillNo";
                SqlHelper.ExecuteNonQuery(CommandType.Text, sql, new SqlParameter("@FBillNo", res.FBillNo));

                string toUser = GetUserIdByUserGroup("002"); // 总经理组
                string title = "您有销售订单[" + res.FBillNo + "]需要审核";
                string desc = "由于" + res.FGMCheckCause + "需要审核";
                string noticeDetailUrl = WebConfig.WebHost + "/#/salesOrderDetail?billNo=" + res.FBillNo;
                string content = ComposeMessageContent(res.FBillNo.ToString(), desc);

                WechatService.Send(toUser, title, content, noticeDetailUrl);
            }
        }

        /// <summary>
        /// 销售单 - 不需要总经理审核的订单，通知生产
        /// </summary>
        private static void SendNoticeToPD()
        {
            string cmdText = "select top 1 FBillNo from tm_v_SeOrderList where FISGM=0 and FISPDSendMessage=0";
            var fBillNo = SqlHelper.ExecuteScalar(CommandType.Text, cmdText);

            if (fBillNo != null)
            {
                // 更新消息状态
                string sql = "update seorder set FHeadSelfs0166=1 where FBillNo=@FBillNo";
                SqlHelper.ExecuteNonQuery(CommandType.Text, sql, new SqlParameter("@FBillNo", fBillNo));

                string toUser = GetUserIdByUserGroup("004"); // 生产组
                string title = "您有销售订单[" + fBillNo + "]需要确认";
                string noticeDetailUrl = WebConfig.WebHost + "/#/salesOrderDetail?billNo=" + fBillNo;
                string content = ComposeMessageContent(fBillNo.ToString(), null);

                WechatService.Send(toUser, title, content, noticeDetailUrl);
            }
        }

        /// <summary>
        /// 退货通知单，销售总监消息通知
        /// </summary>
        private static void SendNoticeToCSO()
        {
            string cmdText = "select top 1 FBillNo from tm_v_SalesReturnNotice where FISCSOSendMessage=0";
            var fBillNo = SqlHelper.ExecuteScalar(CommandType.Text, cmdText);

            if (fBillNo != null)
            {
                // 更新消息状态
                string sql = "update SEOutStock set FHeadSelfs1256=1 where FBillNo=@FBillNo";
                SqlHelper.ExecuteNonQuery(CommandType.Text, sql, new SqlParameter("@FBillNo", fBillNo));

                string toUser = GetUserIdByUserGroup("001"); // 销售总监组
                string title = "您有退货通知单[" + fBillNo + "]需要审核";
                string noticeDetailUrl = WebConfig.WebHost + "/#/salesReturnNoticeDetail?billNo=" + fBillNo;
                WechatService.Send(toUser, title, "", noticeDetailUrl);
            }
        }


        /// <summary>
        /// 根据用户分组获取用户绑定的微信openid
        /// </summary>
        /// <param name="userGroupNumber">分组编号</param>
        /// <returns></returns>
        private static string GetUserIdByUserGroup(params string[] userGroupNumber)
        {
            var sqlParameter = new List<SqlParameter>();

            List<string> parameter = new List<string>();
            for (int i = 0; i < userGroupNumber.Length; i++)
            {
                sqlParameter.Add(new SqlParameter("@Group" + i, userGroupNumber[i]));
                parameter.Add("@Group" + i);
            }

            string sql = "select FUserOpenID from tm_v_UserInfo where FUserGroupNumber in(" + string.Join(",", parameter) + ")";

            var res = SqlHelper.ExecuteDataTable(CommandType.Text, sql, sqlParameter.ToArray());

            var rowCollection = res.AsEnumerable()
                .Where(u => u["FUserOpenID"] != DBNull.Value)
                .Select(u => u["FUserOpenID"]).ToArray();

            string idStr = string.Join("|", rowCollection);

            LoggerHelper.Info("根据用户分组获取用户绑定的微信openid:" + idStr);

            return idStr;
        }

        /// <summary>
        /// 获取销售单制单人员的用户ID
        /// </summary>
        /// <param name="fBillNo"></param>
        /// <returns></returns>
        private static string GetSeOrderUserId(string fBillNo)
        {
            string sql = "select top 1 b.FUserOpenID from tm_v_SeOrderList a inner join tm_v_UserInfo b on a.FEmpName=b.FEmpName where a.FBillNo=@FBillNo";
            var userId = SqlHelper.ExecuteScalar(CommandType.Text, sql, new SqlParameter("@FBillNo", fBillNo));

            LoggerHelper.Info("获取销售单制单人员的用户ID:" + userId);

            if (userId != null)
                return "|" + userId.ToString();

            return "";
        }

        //发送通知消息，增加部门，物料名称内容
        public static string ComposeMessageContent(string fBillNo, string reason)
        {
            SqlParameter[] sqlParameter = new SqlParameter[]
            {
                new SqlParameter("@FBillNo",fBillNo)
            };

            string sql = "select FName,FDeptName from tm_v_SeOrderList where FBillNo=@FBillNo";

            var res = SqlHelper.ExecuteDataTable(CommandType.Text, sql, sqlParameter);

            var orderInfo = res.AsEnumerable()
                .Select(row => new SalesOrderInfo
                {
                    FName = row.Field<string>("FName"),
                    FDeptName = row.Field<string>("FDeptName")
                }).FirstOrDefault();

            if (orderInfo == null) return reason;

            string conent = "<div>部门：" + orderInfo.FDeptName + "</div>";
            conent += "<div>物料：" + orderInfo.FName + "</div>";

            if (!string.IsNullOrEmpty(reason))
            {
                conent += "<div>原因：" + reason + "</div>";
            }

            return conent;
        }
    }
}
