using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goro.Check.Service.Model
{
    public class StockViewModel
    {
        public int Id { get; set; }
        public string FItemNumber { get; set; }
        public string FItemName { get; set; }
        public string FItemModel { get; set; }
        public string FStockName { get; set; }
        public decimal FQty { get; set; }
    }
}
