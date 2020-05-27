using System;
using System.Collections.Generic;
using System.Text;

namespace CrossCuttingExtensions.Models
{
    public class Command
    {
        public string Body { get; set; }
        public List<string> Arguments { get; set; }
    }
}
