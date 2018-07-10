import {setUserinfo, setOpenid} from "../identityUser";

export default {
  setUserinfo(state, userInfo) {
    state.userInfo = userInfo;
    setUserinfo(userInfo);
  },
  setOpenid(state, openid) {
    state.openId = openid;
    setOpenid(openid);
  }
}
