using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Tests
{
	[TestFixture]
	public class AssemblyWithBlockingClassTests
	{
		[Test]
		public void TestClassIsNotBlocked()
		{
			var weaverHelper = new WeaverHelper(@"AssemblyToProcess\AssemblyToProcess.csproj");
			var type = weaverHelper.Assembly.GetType("");
			var instance = Activator.CreateInstance(type);
		}
	}
}
