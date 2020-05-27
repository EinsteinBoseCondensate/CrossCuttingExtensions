using System;
using System.Collections.Generic;
using System.Text;

namespace CrossCuttingExtensions.Configuration
{
    public class ConnectionConfig { 
        public ConnectionStrings ConnectionStrings { get; set; }
    }
    public class ConnectionStrings
    {
        public string LocalDB { get; set; }
    }
}
