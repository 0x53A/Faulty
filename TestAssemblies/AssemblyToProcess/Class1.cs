using Faulty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyToProcess
{
    public class Class1
    {
		public void Method1()
		{
			try
			{

			}
			catch
			{
				Block.ConvertToTryFault();
			}
		}
    }
}
