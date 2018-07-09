<template>
  <div class="account-box">
    <div class="card text-center">
      <div class="logo">
        <img src="../assets/goro.jpg" alt="">
      </div>

      <form>
        <div class="form-group" title="首次登录，需要输入手机号码绑定账户">
          <input class="form-control" type="text" v-model.trim="mobilePhone" :maxlength="11" placeholder="请输入手机号码">
          <small>首次登录，需要输入手机号码绑定账户</small>
        </div>

        <button type="button" class="btn btn-primary btn-block" @click="onSubmit">确定绑定</button>
      </form>
    </div>

  </div>
</template>

<script>
  import {setStore, removeStore} from '../utils'
  import {setUserinfo} from '../identityUser'

  export default {
    data() {
      return {
        mobilePhone: '',
        isSubmit: false,
        openId: '',
      }
    },
    methods: {
      async autoAuth() {
        this.clearState();
        if (!this.openId) {
          // this.openId = 'oxz6qw-riVMn6jdrFp0tHWDl6Hh8';
          // this.openId = 'cyccess'; //企业微信UserID
          // console.log('跳转获取微信授权')
          window.location.href = "/Authorize";
          return;
        }
        console.log("openid:" + this.openId);
        let res = await this.$http.post('/api/Login', {openId: this.openId});
        if (res.data) {
          this.setState(res.data);
        }
        else {
          // this.$vux.toast.text('登录失败！');
        }
      },
      async onSubmit() {
        if (this.isSubmit) return;

        let valid = this.validate();
        if (valid) {
          this.isSubmit = true;
          let res = await this.$http.post('/api/UserBinding', {phoneNumber: this.mobilePhone, openId: this.openId});
          if (res.message === "OK") {
            this.setState(res.data);
          }
          else {
            this.$vux.toast.text(res.message);
            this.isSubmit = false;
          }
        }
      },
      validate() {
        let reg = /^1[0-9]{10}$/;
        if (!this.mobilePhone) {
          this.$vux.toast.text('请输入手机号');
          return false;
        }

        if (!reg.test(this.mobilePhone)) {
          this.$vux.toast.text('请输入正确的手机号');
          return false;
        }
        return true;
      },
      setState(model) {
        this.$cookies.set("openid", model.fUserOpenID, '0');
        setUserinfo(model);
        let groupNo = model.fUserGroupNumber; //用户分组编号
        if (groupNo === "001" || groupNo === "009") {
          this.$router.push('/salesReturnNotice');
        }
        else {
          this.$router.push('/salesOrder');
        }
      },
      clearState() {
        removeStore("userinfo");
        this.$cookies.remove("openid");
      }
    },
    beforeRouteEnter(to, from, next) {
      next(vm => {
        vm.openId = vm.$route.query.openId;
        vm.autoAuth();
      });
    }
  }
</script>

<style scoped>
  .account-box {
    position: absolute;
    top: 0;
    left: 0;
    bottom: 0;
    width: 100%;
  }

  .logo {
    padding: 5rem;
  }

  .card {
    border: 0;
    padding: 1rem;
  }

  .btn {
    border-radius: 0;
  }

  .form-control {
    border-radius: 0;
  }
</style>
