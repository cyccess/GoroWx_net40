import Vue from 'vue';

import store from './store'

export const validate = () => {
  let msg = '';
  const now = Vue.moment(new Date(), 'YYYY-MM-DD HH:mm:ss');
  if (now.isBetween('2018-10-15 09:00:00', '2018-10-19 09:00:00')) {
    store.state.permission = false;
    msg = '系统试用即将到期，请联系管理员'
  }
  else {
    store.state.permission = true;
    msg = '系统试用已到期，请联系管理员';
  }
  Vue.$vux.toast.show({
    type: 'text',
    text: msg,
    time: 5000,
    position: 'top',
    width: '20rem'
  });
};



