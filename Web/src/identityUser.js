import {getStore} from  './utils';

let userInfo = null;
let user = getStore("userinfo");
if(user){
  userInfo = JSON.parse(user);
}

console.log(userInfo)

export default userInfo;
