<template>

</template>

<script>
  import {mapMutations} from 'vuex'
  import {setStore, getStore} from "../utils"

  export default {
    data() {
      return {
        openId: '',
        redirect: ''
      }
    },
    methods: {
      ...mapMutations([
        'setUserinfo', 'setOpenid'
      ]),
      async autoAuth() {

        if (this.redirect) {
          setStore("redirect", this.redirect)
        }

        // 如果本地没有授权信息，并且不是获取授权后跳转到此页面的，将跳转进行授权操作
        if (!window.localStorage.getItem('openid') && !this.openId) {
          // window.location.href = "http://localhost:8002/Authorize";
          window.location.href = "/Authorize";
          return;
        }

        let openid = this.openId || window.localStorage.getItem('openid');
        console.log("正在登录...:" + openid);
        let res = await this.$http.post('/api/Login', {openId: openid});
        if (res.data) {
          this.setState(res.data);
        }
        else {
          this.$router.replace({path: '/account', query: {openId: openid}});
        }
      },
      setState(model) {
        this.setUserinfo(model);
        this.setOpenid(model.fUserOpenID);

        let path = getStore("redirect");
        console.log("path:" + path);
        if (path) {
          this.$router.replace(path);
        }
      }
    },
    beforeRouteEnter(to, from, next) {
      next(vm => {
        vm.openId = vm.$route.query.openId;
        vm.redirect = vm.$route.query.redirect;
        vm.autoAuth();
      });
    }
  }
</script>

<style scoped>

</style>
