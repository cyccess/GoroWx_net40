using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goro.Check.Service.Model
{
    public class CreditViewModel
    {
        public int Id { get; set; }
        public string FCustName { get; set; }
        public decimal FAmount { get; set; }
        public string FEmpName { get; set; }
    }
}
