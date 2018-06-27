import {getStore, setStore} from './utils';

const USER_INFO = 'userinfo';

export const getUserinfo = () => {
  let userInfo = null;
  let user = getStore(USER_INFO);
  if (user) {
    userInfo = JSON.parse(user);
  }
  console.log(userInfo)
  return userInfo;
};

export const setUserinfo = (model) => {
  setStore(USER_INFO, JSON.stringify(model));
};
