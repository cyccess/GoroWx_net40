﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Goro.Check.Service.Model
{
    public class SalesOrder : SalesReturnNotice
    {
        public string FType { get; set; }


        public string FName { get; set; }

        public string FDeptName { get; set; }
    }
}
