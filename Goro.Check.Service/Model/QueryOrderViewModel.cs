using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goro.Check.Service.Model
{
    public class QueryOrderViewModel
    {
        public string fBillNo { get; set; }
        public string fEmpName { get; set; }
        public string userGroupNumber { get; set; }
        public int page { get; set; } = 1;
        public string isConfirm { get; set; }
        public string isStock { get; set; }
        public string isInvoice { get; set; }
        public string isReceive { get; set; }
    }
}
