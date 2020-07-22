using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication
{
    public class HelloService : IHelloService
    {
        public string Greeting => "Change class_id type to string (from HelloService)";
    }
}
