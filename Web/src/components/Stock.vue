<template>
  <div class="sales-box">
    <div class="search-bar">
      <search
        placeholder="物料名称"
        @on-change="getResult"
        v-model="itemName"
        @on-focus="onFocus"
        @on-cancel="onCancel"
        @on-submit="onSubmit"
        ref="search"></search>
    </div>

    <scroller :on-refresh="refresh" :on-infinite="infinite" ref="myscroller" :no-data-text="noData">
      <div class="sales-list">
        <div class="sales-item" v-for="(item,index) in list" :key="index">
          <div class="bill-no vux-1px-b">物料编号：{{item.fItemNumber}}</div>
          <div class="custom">
            <div>{{item.fItemName}}</div>
            <div>{{item.fItemModel}}</div>
            <div>{{item.fQty}}</div>
          </div>
        </div>
      </div>
    </scroller>
  </div>
</template>
<script>
  import { Search } from 'vux'
  export default {
    components: {
      Search
    },
    data() {
      return {
        page: 0,
        noData: '',
        list: [],
        itemName:""
      }
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
        if (this.noData) {
          done();
          return;
        }
        this.page += 1;
        let res = await this.$http.post('/api/QueryStockList', {itemName: this.itemName, page: this.page});

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
      getResult (val) {
        console.log('on-change', val)
        this.list =[];
        this.page = 0;
        this.noData = '';
        this.$refs.myscroller.finishInfinite(false);
      },
      onSubmit () {
        this.$refs.search.setBlur();
        this.getResult();
      },
      onFocus () {
        console.log('on focus')
      },
      onCancel () {
        console.log('on cancel')
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

  .sales-item {
    margin-top: .56667rem;
    padding-left: .75rem;
    background-color: #fff;
    font-size: .87333rem;
    .bill-no {
      height: 3.06667rem;
      line-height: 3.06667rem;
      margin-right: .4rem;
      color:#666;
    }
    .custom {
      padding: .93333rem 0;
    }
  }

  .search-bar{
    position: fixed;
    top:0;
    left: 0;
    width: 100%;
    background-color: #f1f1f1;
    z-index: 111;
  }
</style>
