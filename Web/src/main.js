// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.base.conf with an alias.
import qs from 'qs';
import Vue from 'vue'
import FastClick from 'fastclick'
import VueRouter from 'vue-router'
import App from './App'
import routes from './router/index'
import store from './store'
import VueScroller from 'vue-scroller'
import VueCookies from 'vue-cookies'
import Moment from 'vue-moment'

import {AjaxPlugin, AlertPlugin, ConfirmPlugin, ToastPlugin} from 'vux'

Vue.use(VueRouter);
Vue.use(VueScroller);
Vue.use(VueCookies);
Vue.use(AjaxPlugin);
Vue.use(AlertPlugin);
Vue.use(ConfirmPlugin);
Vue.use(ToastPlugin);
Vue.use(Moment);

const router = new VueRouter({
  routes
});


FastClick.attach(document.body);

Vue.config.productionTip = false;

// 请求时的拦截
AjaxPlugin.$http.interceptors.request.use((request) => {
  // 发送请求之前做一些处理
  // console.log(request)
  if (request.data) {
    let contentType = request.headers["Content-Type"]
    // contentType参数不是application/json，转换参数格式
    if (!(contentType && contentType.indexOf("application/json") > -1))
      request.data = qs.stringify(request.data);
  }

  return request;
}, error => {
  // 当请求异常时做一些处理
  return Promise.reject(error);
});

// 响应时拦截
AjaxPlugin.$http.interceptors.response.use(response => {
  if (response.status === 200) {
    console.log(response.data)
    return response.data;
  } else {
    return Promise.reject(response);
  }
}, error => {
  // 当响应异常时做一些处理
  return Promise.reject(error);
});

Vue.filter('money', function (value) {
  if (!value) return '0.00';
  value = parseFloat(value);
  return value.toFixed(2)
});

Vue.filter("hide", (customName) => {
  if (!customName) return '';
  var reg = /^(.{2}).*(.{4})$/;
  return customName.replace(reg, '$1****$2');
});

router.beforeEach((to, from, next) => {
  let openId = store.state.openId;
  if (to.meta.requiresAuth && !openId) {
    next({
      path: '/',
      query: {redirect: to.fullPath}
    });
  }
  else {
     window.document.title = to.meta.title;
     next();
  }
});

/* eslint-disable no-new */
new Vue({
  router,
  store,
  render: h => h(App)
}).$mount('#app-box');
