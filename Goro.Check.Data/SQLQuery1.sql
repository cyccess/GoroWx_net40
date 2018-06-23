

select * from tm_v_UserInfo


select * from tm_v_UserGroupFieldDisplayed

exec tm_p_GetFieldDisplayed '13033333333','001'


select * from tm_v_SalesReturnHead


select fbillno,fdate,FCustName,fnumber,fname,fmodel,fprice,fqty,fempname,famount,fbillername,FUnitName 
from tm_v_SalesReturnNotice



Exec tm_p_GetSalesReturnNotice @PhoneNumber= N'13011111111'

declare @msg nvarchar(100);
Exec tm_p_UpdateSalesReturnCSO '13022222222','SEIN215', 'Y', ' ',@msg output
print(@msg)




Exec tm_p_GetSalesOrderList '13044444444'

select * from tm_v_SeOrderList

--tm_p_UpdateSalesOrderGM
--tm_p_UpdateSalesOrderPDC
--tm_p_UpdateSalesOrderPDD
--tm_p_UpdateSalesOrderME
--tm_p_UpdateSalesOrderPO


select FNumber ,FName  from t_Department

declare @msg nvarchar=''
exec tm_p_UpdateUserOpenID '13011111111' ,'oxz6qw-riVMn6jdrFp0tHWDl6Hh8', @msg output


--=============================


ALTER view [dbo].[tm_v_UserInfo]
as
select t1.FName as FEmpName,
		t1.F_102 as FPhoneNumber,
		t2.FItemID as FEmpID,
		t2.FNumber as FEmpNumber,
		t1.F_105 as FUserGroupID,
		t3.FNumber as FUserGroupNumber,
		t3.FName as FUserGroupName,
		t1.F_106 as FUserOpenID
from t_Item_3035 t1 left join
		t_Emp t2 on (t1.FName = t2.FName) left join
		t_Department t3 on (t1.F_105 = t3.FItemID)


GO



create proc [dbo].[tm_p_UpdateUserOpenID] @PhoneNumber varchar(50),@OpenID varchar(100),@Msg nvarchar(100) output
as
if exists (select FItemID from t_Item_3035 where F_102 = @PhoneNumber)
begin
	update t_Item_3035 set F_106 = @OpenID where F_102 = @PhoneNumber
	set @Msg = 'OK'
end
else
begin
	set @Msg = '手机号在用户信息中不存在'
end

