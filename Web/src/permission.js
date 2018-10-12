
import moment from 'vue-moment'

export const validate = () => {
  const end = moment("2018-10-19 17:00:00");

  const now = new moment();

  if(now.isAfter(end)){

  }
};



