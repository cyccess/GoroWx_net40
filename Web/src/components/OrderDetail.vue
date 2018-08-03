<template>
  <div class="sales-box">
    <div class="orderInfo">
      <div class="info-title">
        <span>订单详情</span>
        <div class="orderState">{{model.FStatus}}</div>
      </div>

      <div class="info-text" v-for="(item,index) in field" :key="index" v-if="model[item.fFieldName]" :class="[index===2 ? 'vux-1px-b line' : '']">
        <span v-if="item.fFieldDataType==='datetime'">{{item.fFieldDescription}}：{{model[item.fFieldName]|moment('YYYY-MM-DD HH:mm:ss')}}</span>
        <div v-else-if="item.fFieldName==='fLog'" class="line vux-1px-t" v-html="$options.filters.logs(model[item.fFieldName])"></div>
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
      ])
    },
    created() {
      this.billNo = this.$route.query.billNo;
      this.getData();
    },
    methods: {
      async getData() {
        let args = {
          phoneNumber: this.userInfo.fPhoneNumber,
          fBillNo: this.billNo
        };

        let res = await this.$http.post('/api/OrderDetail', args);

        if (res.data) {
          this.model = res.data.order[0];
          this.field = res.data.field;
        }
      }
    },
    filters:{
      logs(value){
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
