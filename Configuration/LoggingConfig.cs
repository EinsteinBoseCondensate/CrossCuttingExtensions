using System;
using System.Collections.Generic;
using System.Text;

namespace CrossCuttingExtensions.Configuration
{
    public class LoggingSection
    {
        public bool IsEnabled { get; set; } = new bool();
    }
    public class LoggingConfig
    {
        public LoggingSection LoggingSection { get; set; }
    }
}
