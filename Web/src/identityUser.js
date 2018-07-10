import {getStore, setStore} from './utils';

const USER_INFO = 'userinfo';
const OPEN_ID = 'openid';

export const getUserinfo = () => {
  let userInfo = null;
  let user = getStore(USER_INFO);
  if (user) {
    userInfo = JSON.parse(user);
  }
  console.log(userInfo);
  return userInfo;
};

export const setUserinfo = (model) => {
  setStore(USER_INFO, JSON.stringify(model));
};

export const setOpenid = (openid) => {
  setStore(OPEN_ID, openid);
};

export const getOpenid = () => {
  getStore(OPEN_ID);
};

