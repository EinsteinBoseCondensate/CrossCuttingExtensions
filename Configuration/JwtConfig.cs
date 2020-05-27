using System;
using System.Collections.Generic;
using System.Text;

namespace CrossCuttingExtensions.Configuration
{
    public class JwtConfig
    {
        public JwtSection JwtSection { get; set; }
    }
    public class JwtSection
    {
        public string Secret { get; set; }
    }
}
