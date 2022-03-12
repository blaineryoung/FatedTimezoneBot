using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Information.Exceptions
{
    public class GearNotFoundException: Exception
    {
        public GearNotFoundException(string message) : base(message) { }
    }
}
