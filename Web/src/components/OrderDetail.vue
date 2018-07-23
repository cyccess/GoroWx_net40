<template>
  <div class="sales-box">
    <div class="orderInfo">
      <div class="info-text" v-for="(item,index) in field" :key="index" :class="[index===2 ? 'vux-1px-b line' : '']">
        <span v-if="item.fFieldDataType==='datetime'">{{item.fFieldDescription}}：{{model[item.fFieldName]|moment('YYYY-MM-DD HH:mm:ss')}}</span>
        <div v-else-if="item.fFieldName==='fLog'" class="vux-1px-t" v-html="$options.filters.log(model[item.fFieldName])"></div>
        <span v-else>{{item.fFieldDescription}}：{{model[item.fFieldName]}}</span>
      </div>
    </div>
  </div>
</template>

<script>
  import {mapState} from 'vuex'

  export default {
    data() {
      return {
        fBillNo: '',
        model: {},
        field: []
      }
    },
    computed: {
      ...mapState([
        'userInfo'
      ]),
      showLog(){

      }
    },
    created() {
      this.billNo = this.$route.query.billNo;
      this.getData();
    },
    methods: {
      async getData() {
        let args = {
          billTypeNumber: '',
          phoneNumber: this.userInfo.fPhoneNumber,
          fBillNo: this.billNo
        };

        if (this.userInfo.fUserGroupNumber === '001' || this.userInfo.fUserGroupNumber === '009') {
          args.billTypeNumber = '002'; // 退货通知单
        }
        else {
          args.billTypeNumber = '001'; // 销售单
        }

        let res = await this.$http.post('/api/OrderDetail', args);

        if (res.data) {
          this.model = res.data.order[0];
          this.field = res.data.field;
        }
      }
    },
    filters:{
      log(value){
        let item = [];
        if(value){
          item = value.split(';');
        }
        return item.join("<br>");
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

  .orderInfo {

    overflow-x: auto;;

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
</style>
