<template>
  <div class="sales-box">

    <div class="search-bar">
      <search
        placeholder="订单号、业务员或客户名称"
        v-model="fBillNo"
        :autoFixed="false"
        @on-submit="onSubmit"
        @on-cancel="onCancel"
        ref="search"
      >
      </search>
      <div class="filter-box vux-1px-b">
        <div class="search-filter" @click="showFilter=!showFilter"><span>筛选</span><i class="icon-filter"></i></div>

        <div :class="[showFilter?'open':'']" class="filter-con">
          <div class="filter-item vux-1px-b">
            <div class="item-label">是否确认</div>
            <checker
              v-model="isConfirm"
              radio-required
              default-item-class="demo5-item"
              selected-item-class="demo5-item-selected">
              <checker-item value="-1">全部</checker-item>
              <checker-item value="1">已确认</checker-item>
              <checker-item value="0">未确认</checker-item>
              <checker-item value="2">特批未通过</checker-item>
            </checker>
          </div>
          <div class="filter-item vux-1px-b">
            <div class="item-label">是否发货</div>
            <checker
              v-model="isStock"
              radio-required
              default-item-class="demo5-item"
              selected-item-class="demo5-item-selected">
              <checker-item value="-1">全部</checker-item>
              <checker-item value="1">已发货</checker-item>
              <checker-item value="0">未发货</checker-item>
            </checker>
          </div>
          <div class="filter-item vux-1px-b">
            <div class="item-label">是否开票</div>
            <checker
              v-model="isInvoice"
              radio-required
              default-item-class="demo5-item"
              selected-item-class="demo5-item-selected">
              <checker-item value="-1">全部</checker-item>
              <checker-item value="1">已开票</checker-item>
              <checker-item value="0">未开票</checker-item>
            </checker>
          </div>
          <div class="filter-item vux-1px-b">
            <div class="item-label">是否收款</div>
            <checker
              v-model="isReceive"
              radio-required
              default-item-class="demo5-item"
              selected-item-class="demo5-item-selected">
              <checker-item value="-1">全部</checker-item>
              <checker-item value="1">已收款</checker-item>
              <checker-item value="0">未收款</checker-item>
            </checker>
          </div>

          <div class="filter-btn-group">
            <button @click="showFilter=false" class="btn btn-secondary btn-sm">取消</button>
            <button @click="onFilter" class="btn btn-primary btn-sm">确定</button>
          </div>
        </div>

      </div>
    </div>

    <scroller :on-refresh="refresh" :on-infinite="infinite" ref="myscroller" :no-data-text="noData" style="top:5.66rem">
      <div class="sales-list">
        <div @click="detail(index)" class="sales-item" v-for="(item,index) in list" :key="index">
          <div class="bill-no vux-1px-b">订单编号：{{item.fBillNo}}</div>
          <div class="custom">
            <div>部门:{{item.fDeptName}}</div>
            <div>物料:{{item.fName}}</div>
          </div>
        </div>
      </div>
    </scroller>
  </div>
</template>
<script>
  import {Search, Checker, CheckerItem, Popup } from 'vux'
  import {mapState} from 'vuex'

  export default {
    components: {
      Search, Checker, CheckerItem, Popup
    },
    data() {
      return {
        page: 0,
        noData: '',
        list: [],
        fBillNo: "",
        showFilter:false,
        isConfirm:"-1",  //是否确认
        isStock:"-1", //是否发货
        isInvoice:"-1",   //是否开票
        isReceive:"-1"   //是否收款
      }
    },
    computed: {
      ...mapState([
        'userInfo'
      ])
    },
    created() {

    },
    methods: {
      refresh(done) {
        this.list = [];
        this.page = 0;
        this.noData = '';
        this.fBillNo = '';
        done();
      },
      async infinite(done) {
        // 质检组不能查询订单
        if (this.userInfo.fUserGroupNumber === "009") {
          this.noData = "您不能进行销售订单查询！";
          done(true);
          return;
        }

        if (this.noData) {
          done();
          return;
        }
        this.page += 1;

        let fEmpName = ''; //业务员名称
        //业务人员只能查看自己的订单
        if (this.userInfo.fUserGroupNumber === "007") {
          fEmpName = this.userInfo.fEmpName;
        }

        let res = await this.$http.post('/api/QueryOrderList', {
          fBillNo: this.fBillNo,
          fEmpName: fEmpName,
          userGroupNumber: this.userInfo.fUserGroupNumber,
          page: this.page,
          isConfirm: this.isConfirm,
          isStock: this.isStock,
          isInvoice: this.isInvoice,
          isReceive: this.isReceive
        });

        if (res.code === 100) {
          if (res.data.length < 10) {
            this.noData = '没有更多了';
          }

          this.list = [...this.list, ...res.data];
          done()
        }
        else {
          if (this.list.length === 0) {
            this.noData = "暂无数据";
          }
          done(true);
        }
      },
      detail(index) {
        let fBillNo = this.list[index].fBillNo;
        this.$router.push({path: '/orderDetail', query: {billNo: fBillNo}});
      },

      getResult() {
        this.list = [];
        this.page = 0;
        this.noData = '';
        this.$refs.myscroller.finishInfinite(false);
      },
      onSubmit() {
        this.$refs.search.setBlur();
        this.getResult();
      },
      onCancel(){
        this.fBillNo = '';
        this.getResult();
      },
      onFilter(){
        this.list = [];
        this.page = 0;
        this.noData = '';
        this.$refs.myscroller.finishInfinite(false);

        this.showFilter = false;
      }
    }
  }
</script>
<style scoped lang="less">

  .sales-box {
    position: absolute;
    top: 0;
    left: 0;
    bottom: 0;
    width: 100%;
    background-color: #f1f1f1;
  }

  .sales-list {

  }

  .sales-list > div:nth-of-type(odd) {
    background-color: #f9f9f9;
  }

  .sales-item {
    margin-top: .56667rem;
    padding-left: .75rem;
    background-color: #fff;
    font-size: .87333rem;
    .bill-no {
      height: 3.06667rem;
      line-height: 3.06667rem;
      margin-right: .4rem;
      color: #666;
    }
    .custom {
      padding: .93333rem 0;
    }
  }

  .search-bar {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    background-color: #f1f1f1;
    z-index: 111;
  }

  .filter-box{
    background-color: #fff;
    padding: .5rem;
  }

  .search-filter{
    display: flex;
    justify-content: flex-end;
    align-items: center;
    color:#1296db;
    font-size: .875rem;
  }

  .icon-filter{
    display: inline-block;
    width: 32px;
    height: 32px;
    background: url("../../src/assets/icon-filter.png") no-repeat;
  }

  .filter-con{
    display: none;
    position: absolute;
    top: 48px;
    left: 0;
    width: 100%;
    background-color: #fff;
  }
  .filter-con.open{
    display: block;
  }

  .filter-item{
    padding: .875rem;
    font-size: .875rem;
    .item-label{
      line-height: 2rem;
    }
  }
  .filter-btn-group{
    display: flex;
    .btn{
      flex:1;
      border-radius: 0;
    }
  }

  .vux-checker-box{
    display: flex;
  }
  .demo5-item {
    flex: 1;
    line-height: 1.65rem;
    text-align: center;
    border-radius: 3px;
    border: 1px solid #ccc;
    background-color: #fff;
    margin-right:.5rem;
    color:#666;
  }
  .demo5-item-selected {
    border-color: #1296db;
    background-color: #1296db;
    color:#fff;
  }
</style>
