using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Faulty
{
	public class NotRewrittenException : Exception
	{
		public NotRewrittenException()
			: base("This method should never be called. Make sure the assembly is correctly post-processed by Fody.Faulty")
		{

		}
	}

    public static class Block
    {
        public static void ConvertFinallyToFault()
        {
			throw new NotRewrittenException();
		}
    }
}
