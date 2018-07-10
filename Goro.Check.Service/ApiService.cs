﻿using Goro.Check.Data;
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

                int count = (page - 1) * 20;
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
                if (userGroupNumber == "001") //销售总监组审核更新
                {
                    cmdText = "tm_p_UpdateSalesReturnCSO";

                    string toUser = GetUserIdByUserGroup("002,008");
                    WechatService.Send(toUser, "退货通知单", reason, WebConfig.WebHost + "/#/salesReturnNoticeDetail?billNo=" + billNo);
                }
                if (userGroupNumber == "009") //质检组审核更新
                {
                    cmdText = "tm_p_UpdateSalesReturnQC";
                }

                LoggerHelper.Info("退货通知单审核[" + userGroupNumber + "]:" + phoneNumber);

                var res = SqlHelper.ExecuteNonQuery(CommandType.StoredProcedure, cmdText, sqlParameter);
                var msg = sqlParameter[4].Value;

                return msg.ToString();
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

                //退货通知单-根据手机号获取单号和客户
                var dt = SqlHelper.ExecuteDataTable(CommandType.StoredProcedure, "tm_p_GetSalesOrderList", sqlParameter);

                int count = (page - 1) * 20;
                var salesOrders = dt.AsEnumerable().Select(r => new SalesOrder
                {
                    FBillNo = r[0].ToString(),
                    FCustName = r[1].ToString(),
                    FType = r[2].ToString(),
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

                //field += ",fMEContent,fPOContent,fPDDeason,fGMResult,fPDCResult";

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

                #region 

                var sqlParameter = new List<SqlParameter>()
                {
                    new SqlParameter("@PhoneNumber",model.phoneNumber),
                    new SqlParameter("@BillNo",model.billNo),
                };

                string cmdText = "";
                if (model.userGroupNumber == "002") //总经理审核更新
                {
                    cmdText = "tm_p_UpdateSalesOrderGM";
                    sqlParameter.Add(new SqlParameter("@Reason", model.reason));
                    sqlParameter.Add(new SqlParameter("@Result", model.result));
                }

                if (model.userGroupNumber == "004")
                {
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
                }

                if (model.userGroupNumber == "005") //工艺回复更新
                {
                    cmdText = "tm_p_UpdateSalesOrderME";
                    sqlParameter.Add(new SqlParameter("@Reason", model.reason));
                }

                if (model.userGroupNumber == "006") //供应回复更新
                {
                    cmdText = "tm_p_UpdateSalesOrderPO";
                    sqlParameter.Add(new SqlParameter("@Reason", model.reason));
                }

                if (cmdText == "")
                {
                    LoggerHelper.Info("销售单审核：用户分组未找到");
                    return "用户分组未找到！";
                }

                LoggerHelper.Info("销售单审核【" + model.userGroupNumber + "：" + model.phoneNumber + "】");

                sqlParameter.Add(new SqlParameter { ParameterName = "@Msg", Value = "", Direction = ParameterDirection.Output, Size = 100, SqlDbType = SqlDbType.NVarChar });

                var res = SqlHelper.ExecuteNonQuery(CommandType.StoredProcedure, cmdText, sqlParameter.ToArray());
                var msg = sqlParameter.Last().Value;

                return msg.ToString();

                #endregion
            }
            catch (Exception e)
            {
                LoggerHelper.Info("销售单审核【" + model.userGroupNumber + "：" + model.phoneNumber + "】" + e);
                return "销售单审核失败";
            }
        }

        private string UpdateSalesOrderGM(SalesOrderViewModel model)
        {
            var sqlParameter = new List<SqlParameter>()
            {
                new SqlParameter("@PhoneNumber",model.phoneNumber),
                new SqlParameter("@BillNo",model.billNo),
                new SqlParameter("@Reason", model.reason),
                new SqlParameter("@Result", model.result),
                new SqlParameter { ParameterName = "@Msg", Value = "", Direction = ParameterDirection.Output, Size = 100, SqlDbType = SqlDbType.NVarChar }
            };

            string cmdText = "tm_p_UpdateSalesOrderGM";

            var res = SqlHelper.ExecuteNonQuery(CommandType.StoredProcedure, cmdText, sqlParameter.ToArray());
            var msg = sqlParameter.Last().Value;
            LoggerHelper.Info("销售单审核,总经理审核更新【" + model.userGroupNumber + "：" + model.phoneNumber + "】" + msg);

            return msg.ToString();
        }

        private string UpdateSalesOrderPD(SalesOrderViewModel model)
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
            var msg = sqlParameter.Last().Value;
            LoggerHelper.Info("销售单审核,生产审核更新【" + model.userGroupNumber + "：" + model.phoneNumber + "】" + msg);

            return msg.ToString();
        }

        private string UpdateSalesOrderME(SalesOrderViewModel model)
        {
            var sqlParameter = new List<SqlParameter>()
            {
                new SqlParameter("@PhoneNumber",model.phoneNumber),
                new SqlParameter("@BillNo",model.billNo),
                new SqlParameter("@Reason", model.reason)
            };

            string cmdText = "tm_p_UpdateSalesOrderME";

            var res = SqlHelper.ExecuteNonQuery(CommandType.StoredProcedure, cmdText, sqlParameter.ToArray());
            var msg = sqlParameter.Last().Value;
            LoggerHelper.Info("销售单审核,工艺审核更新【" + model.userGroupNumber + "：" + model.phoneNumber + "】" + msg);

            return msg.ToString();
        }

        private string UpdateSalesOrderPO(SalesOrderViewModel model)
        {
            var sqlParameter = new List<SqlParameter>()
            {
                new SqlParameter("@PhoneNumber",model.phoneNumber),
                new SqlParameter("@BillNo",model.billNo),
                new SqlParameter("@Reason", model.reason)
            };

            string cmdText = "tm_p_UpdateSalesOrderPO";

            var res = SqlHelper.ExecuteNonQuery(CommandType.StoredProcedure, cmdText, sqlParameter.ToArray());
            var msg = sqlParameter.Last().Value;
            LoggerHelper.Info("销售单审核,供应审核更新【" + model.userGroupNumber + "：" + model.phoneNumber + "】" + msg);

            return msg.ToString();
        }


        public string GetUserIdByUserGroup(string userGroupNumber)
        {
            SqlParameter[] sqlParameter = new SqlParameter[]
            {
                new SqlParameter{ ParameterName = "@UserGroupNumber", Value = userGroupNumber, SqlDbType = SqlDbType.NVarChar }
            };

            string sql = "select * from tm_v_UserInfo where FUserGroupNumber in(@UserGroupNumber)";
            var res = SqlHelper.ExecuteDataTable(CommandType.Text, sql, sqlParameter);

            var rowCollection = res.AsEnumerable().Select(u => u["FUserOpenID"]);

            return string.Join("|", rowCollection);
        }
    }
}
