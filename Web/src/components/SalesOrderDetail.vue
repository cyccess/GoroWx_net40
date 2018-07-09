<template>
  <div class="sales-box">

    <div class="orderInfo">
      <div class="info-title">
        <span>销售订单审核 - {{userName}}</span>
        <div v-if="userGroupNumber==='002'&&model['fGMResult']" class="orderState">
          <div>状态：{{model['fGMResult']}}</div>
        </div>
        <div v-if="userGroupNumber!=='002'&&model['fPDCResult']" class="orderState">
          <div>状态：{{model['fPDCResult']}}</div>
        </div>
      </div>

      <div class="info-text" v-for="(item,index) in field" :key="index" :class="[index===2 ? 'vux-1px-b line' : '']">
        <span>{{item.fFieldDescription}}：{{model[item.fFieldName]}}</span>
      </div>

      <!--生产显示回复-->
      <div class="info-text" v-if="userGroupNumber==='004'">
        <span>工艺回复：{{model['fMEContent']}}</span><br>
        <span>供应回复：{{model['fPOContent']}}</span>
      </div>

      <!--工艺/供应显示生产不确认原因-->
      <div class="info-text" v-if="userGroupNumber==='005'||userGroupNumber==='006'">生产不确认原因：{{model['fPDDeason']}}</div>
    </div>

    <!--总经理组审核按钮-->
    <div class="btn-wrapper" v-if="userGroupNumber==='002'">
      <button @click="modalShow=true" class="btn btn-secondary" type="submit">不同意</button>
      <button @click="agree" class="btn btn-primary" type="submit">同意</button>
    </div>

    <!--生产确认/不确认按钮-->
    <div class="btn-wrapper" v-else-if="userGroupNumber==='004'">
      <button @click="modalShow=true" class="btn btn-secondary" type="submit">不确认</button>
      <button @click="agree" class="btn btn-primary" type="submit">确认</button>
    </div>

    <!--工艺/供应回复按钮-->
    <div class="btn-wrapper" v-if="userGroupNumber==='005'||userGroupNumber==='006'">
      <button @click="modelReply=true" class="btn btn-primary" type="submit">回复</button>
    </div>

    <!--总经理组、生产审核对话框-->
    <x-dialog v-model="modalShow" class="dialog-disagree">
      <div class="card">
        <div class="dialog-close" @click="cancel">
          <span class="vux-close"></span>
        </div>
        <div class="card-body">
          <div v-if="userGroupNumber==='004'">
            <h6 class="card-subtitle mb-2 text-muted">选择组</h6>
            <div class="check-group">
              <check-icon :value.sync="isMe">工艺组</check-icon>
              <check-icon :value.sync="isPo">供应组</check-icon>
            </div>
          </div>

          <h6 class="card-subtitle mb-2 text-muted">不同意原因</h6>
          <textarea class="form-control" v-model="reason"></textarea>
          <div class="btn-box">
            <button @click="disagree" type="button" class="btn btn-sm btn-primary">确定</button>
            <button @click="cancel" type="button" class="btn btn-sm btn-secondary">取消</button>
          </div>
        </div>
      </div>
    </x-dialog>

    <!--回复对话框-->
    <x-dialog v-model="modelReply" class="dialog-reply">
      <div class="card">
        <div class="dialog-close" @click="cancel">
          <span class="vux-close"></span>
        </div>
        <div class="card-body">
          <h6 class="card-subtitle mb-2 text-muted">回复内容</h6>
          <textarea class="form-control" v-model="reason"></textarea>
          <div class="btn-box">
            <button @click="reply" type="button" class="btn btn-sm btn-primary">确定</button>
            <button @click="cancel" type="button" class="btn btn-sm btn-secondary">取消</button>
          </div>
        </div>
      </div>
    </x-dialog>

    <!--交期选择对话框-->
    <x-dialog v-model="modelDelivery">
      <div class="card">
        <div class="dialog-close" @click="cancel">
          <span class="vux-close"></span>
        </div>
        <div class="card-body">
          <h6 class="card-subtitle mb-2 text-muted">交期时间</h6>
          <datetime-view v-model="deliveryDate" ref="datetime"></datetime-view>
          <div class="btn-box">
            <button @click="proAgree" type="button" class="btn btn-sm btn-primary">确定</button>
            <button @click="cancel" type="button" class="btn btn-sm btn-secondary">取消</button>
          </div>
        </div>
      </div>
    </x-dialog>
  </div>
</template>

<script>
  import {XDialog, CheckIcon, DatetimeView} from 'vux'
  import {getUserinfo} from '../identityUser'

  export default {
    components: {
      XDialog, CheckIcon, DatetimeView
    },
    data() {
      return {
        userInfo: {},
        modalShow: false,
        modelReply: false,
        modelDelivery: false,
        reason: '',
        billNo: '',
        model: {},
        field: [],
        deliveryDate: '',//交期
        isMe: false, //工艺是否审核
        isPo: false,  //供应是否审核
        userGroupNumber: '',
        userName: ''
      }
    },

    created() {
      this.billNo = this.$route.query.billNo;
      let now = new Date();
      this.deliveryDate = now.getFullYear() + '-' + now.getMonth() + '-' + now.getDate();
      this.userInfo = getUserinfo();
      this.userGroupNumber = this.userInfo.fUserGroupNumber;
      this.userName = this.userInfo.fEmpName;
      this.getData();
    },
    methods: {
      async getData() {
        let res = await this.$http.post('/api/SalesOrderDetail', {phoneNumber: this.userInfo.fPhoneNumber, fBillNo: this.billNo});

        this.model = res.data.order[0];
        this.field = res.data.field
      },
      proAgree() {
        this.modelDelivery = false;
        // 生产确认
        this.update("Y");
      },
      async agree() {
        if (this.userGroupNumber === '004') {
          this.modelDelivery = true;
          return;
        }
        await this.update("Y");
      },
      async reply() {
        this.modelReply = false;
        await this.update("");
      },
      async disagree() {
        this.modalShow = false;
        await this.update("N");
      },
      async update(result) {
        let args = {
          billNo: this.billNo,
          phoneNumber: this.userInfo.fPhoneNumber,
          userGroupNumber: this.userGroupNumber,
          result: result,
          reason: this.reason,
          deliveryDate: this.deliveryDate
        };

        //生产不确认，工艺/供应是否审核
        if (this.userGroupNumber === "004") {
          args.isMe = this.isMe ? "1" : "0";
          args.isPo = this.isPo ? "1" : "0";
        }

        let res = await this.$http.post('/api/UpdateSalesOrder', args);
        let message = res.message;
        if (result === "Y") {
          if (res.message === "OK") {
            message = "审核通过！";
          }
        } else {
          if (res.message === "OK") {
            message = "操作成功！";
          }
        }

        if (this.userGroupNumber === '005' || this.userGroupNumber === '006') {
          message = `销售订单[${this.billNo}]<br>回复完成！`
        }

        let vm = this;
        this.$vux.alert.show({
          title: '提示',
          content: message,
          onHide() {
            // vm.$router.push({path: '/salesOrder'});
          }
        });
      },
      cancel() {
        this.reason = '';
        this.isMe = false;
        this.isPo = false;

        this.modalShow = false;
        this.modelReply = false;
        this.modelDelivery = false;
      }
    }
  }
</script>

<style lang="less" scoped>
  @import '~vux/src/styles/close';

  .sales-box {
    position: fixed;
    top: 0;
    left: 0;
    bottom: 0;
    width: 100%;
    padding: .875rem;
    font-size: .875rem;
  }

  .btn-wrapper {
    display: flex;
    position: absolute;
    left: 0;
    bottom: 0;
    width: 100%;
    .btn {
      flex: 1;
      border-radius: 0;
    }
  }

  .orderInfo {
    margin-bottom: 1rem;
  }

  .info-title {
    display: flex;
    font-weight: 600;
    margin-bottom: .5rem;
    .orderState {
      flex: 1;
      text-align: right;
    }
  }

  .info-text {
    line-height: 1.65rem;
  }

  .line {
    padding-bottom: .85rem;
    margin-bottom: .85rem;
  }

  .weui-dialog {
    text-align: left;
    .card-subtitle {
      text-align: left;
    }
    .check-group {
      text-align: left;
      margin: 1rem 0;
    }
    .dialog-close {
      position: absolute;
      top: .75rem;
      right: .75rem;
    }
    .btn-box {
      margin-top: .5rem;
      text-align: right;
    }
  }
</style>
