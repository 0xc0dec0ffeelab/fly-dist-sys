using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fly_dist_sys
{
    public class RPCError : Exception
    {
        public int Code { get; }
        public RPCError(string message) : base(message) { }
        public RPCError(int code, string message) : base(message) { Code = code; }
    }
}
