export default [
  {
    path: '/',
    name: 'Authorize',
    component: resolve => require(['@/components/Authorize.vue'], resolve),
    meta: {title: '加载中...'}
  },
  {
    path: '/account',
    name: 'Account',
    component: resolve => require(['@/components/Account.vue'], resolve),
    meta: {title: '绑定手机号'}
  }, {
    path: '/salesReturnNotice',
    name: 'SalesReturnNotice',
    component: resolve => require(['@/components/SalesReturnNotice.vue'], resolve),
    meta: {title: '退货通知单', requiresAuth: true}
  }, {
    path: '/salesReturnNoticeDetail',
    name: 'SalesReturnNoticeDetail',
    component: resolve => require(['@/components/SalesReturnNoticeDetail.vue'], resolve),
    meta: {title: '退货通知单详情', requiresAuth: true}
  }
  , {
    path: '/salesOrder',
    name: 'salesOrder',
    component: resolve => require(['@/components/SalesOrder.vue'], resolve),
    meta: {title: '销售订单', requiresAuth: true}
  }, {
    path: '/salesOrderDetail',
    name: 'salesOrder',
    component: resolve => require(['@/components/SalesOrderDetail.vue'], resolve),
    meta: {title: '销售订单详情', requiresAuth: true}
  }
]

