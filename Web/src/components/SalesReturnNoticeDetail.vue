<template>
  <div class="sales-box" v-if="field.length>0">
    <div class="orderInfo">
      <div class="info-title">退货通知单详情</div>
      <div class="info-text" v-for="(item,index) in field" :key="index" v-if="model[item.fFieldName]" :class="[index===2 ? 'vux-1px-b line' : '']">
        <span>{{item.fFieldDescription}}：</span>
        <span v-if="item.fFieldDataType==='datetime'">{{model[item.fFieldName]|moment('YYYY-MM-DD HH:mm:ss')}}</span>
        <span v-else>{{model[item.fFieldName]}}</span>
      </div>
    </div>

    <div class="btnGroup">
      <div class="btn-wrapper" v-if="userInfo.fUserGroupNumber==='001'||userInfo.fUserGroupNumber==='009'">
        <button @click="modalShow=true" class="btn btn-secondary" type="submit">不同意</button>
        <button @click="agree" class="btn btn-primary" type="submit">同意</button>
      </div>
    </div>
    <x-dialog v-model="modalShow" class="dialog-disagree">
      <div class="card">
        <div class="dialog-close" @click="modalShow=false">
          <span class="vux-close"></span>
        </div>
        <div class="card-body">
          <h6 class="card-subtitle mb-2 text-muted">不同意原因</h6>
          <textarea class="form-control" v-model="reason"></textarea>
          <div class="btn-box">
            <button @click="disagree" type="button" class="btn btn-sm btn-primary">确定</button>
            <button @click="cancel" type="button" class="btn btn-sm btn-secondary">取消</button>
          </div>
        </div>
      </div>
    </x-dialog>
  </div>

  <div class="order-tip" v-else>订单信息不存在！</div>
</template>

<script>
  import {XDialog} from 'vux'
  import {mapState} from 'vuex'

  export default {
    components: {
      XDialog
    },
    data() {
      return {
        modalShow: false,
        reason: '',
        billNo: '',
        model: {},
        field: []
      }
    },
    computed:{
      ...mapState([
        'userInfo'
      ])
    },
    created() {
      this.billNo = this.$route.query.billNo;
      this.getData();
    },
    methods: {
      async getData() {
        let res = await this.$http.post('/api/SalesReturnNoticeDetail', {
          phoneNumber: this.userInfo.fPhoneNumber,
          fBillNo: this.billNo
        });
        if (res.data.order.length > 0) {
          this.model = res.data.order[0];
          this.field = res.data.field;
        }
      },
      async agree() {
        this.update("Y");
      },
      async disagree() {
        this.modalShow = false;
        this.update("N");
      },
      async update(result) {
        let args = {
          phoneNumber: this.userInfo.fPhoneNumber,
          userGroupNumber: this.userInfo.fUserGroupNumber,
          billNo: this.billNo,
          result: result,
          reason: this.reason
        };

        let res = await this.$http.post('/api/UpdateSalesReturn', args);
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
        let vm = this;
        this.$vux.alert.show({
          title: '提示',
          content: message,
          onHide() {
            vm.$router.push({path: '/salesReturnNotice'});
          }
        });
      },
      cancel() {
        this.modalShow = false;
        this.reason = '';
      }
    }
  }
</script>

<style lang="less" scoped>
  @import '~vux/src/styles/close';

  .sales-box {
    padding: .875rem;
    font-size: .875rem;
  }

  .btnGroup{
    position:relative;
    z-index: 5;
    height: 1rem;
  }
  .btn-wrapper {
    display: flex;
    position: fixed;
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
    font-weight: 600;
    margin-bottom: .5rem;
  }

  .info-text {
    line-height: 1.5rem;
  }

  .line {
    padding-bottom: .85rem;
    margin-bottom: .85rem;
  }

  .order-tip{
    position: absolute;
    top: -.66667rem;
    right: 0;
    bottom: 0;
    left: 0;
    height: 4rem;
    margin: auto;
    color:#999;
    text-align: center;
  }


  .weui-dialog {
    text-align: left;
    .card-subtitle {
      text-align: left;
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
