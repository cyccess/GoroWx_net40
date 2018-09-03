<template>
  <div class="sales-box">
    <div class="search-bar">
      <search
        placeholder="输入客户或业务员名称"
        v-model="custName"
        @on-cancel="onCancel"
        @on-submit="onSubmit"
        ref="search"></search>
    </div>

    <scroller :on-refresh="refresh" :on-infinite="infinite" ref="myscroller" :no-data-text="noData">
      <div class="sales-list">
        <div class="sales-item" v-for="(item,index) in list" :key="index">
          <div class="bill-no vux-1px-b">{{item.fCustName}}</div>
          <div class="custom">
            <div>金额:{{item.fAmount}}</div>
            <div>业务员:{{item.fEmpName}}</div>
          </div>
        </div>
      </div>
    </scroller>
  </div>
</template>
<script>
  import {Search} from 'vux'
  import {mapState} from 'vuex'

  export default {
    components: {
      Search
    },
    data() {
      return {
        page: 0,
        noData: '',
        list: [],
        custName: ""
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
        this.itemName = '';
        done();
      },
      async infinite(done) {
        // 只能总经理、销售总监、财务业务员、制单人员查询信用额度
        if (!(['001', '002', '003', '007', '008'].indexOf(this.userInfo.fUserGroupNumber) > -1)) {
          this.noData = "您不能进行信用额度查询！";
          done(true);
          return;
        }

        if (this.noData) {
          done();
          return;
        }
        this.page += 1;

        let fEmpName = ''; //业务员名称
        //业务人员只能查看自己的客户
        if (this.userInfo.fUserGroupNumber === "007") {
          fEmpName = this.userInfo.fEmpName;
        }

        let res = await this.$http.post('/api/QueryCreditList', {custName: this.custName, fEmpName: fEmpName, page: this.page});

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
      onCancel() {
        this.itemName = '';
        this.getResult();
      }
    }
  }
</script>

<style scoped lang="less">
  .sales-box {
    position: fixed;
    top: 0;
    left: 0;
    bottom: 0;
    width: 100%;
    background-color: #f1f1f1;
  }

  .sales-list {
    margin-top: 3.333rem;
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
      overflow: hidden;
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
</style>
