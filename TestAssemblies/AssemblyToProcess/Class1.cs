using Faulty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AssemblyToProcess
{
    public class Class1
	{
		private void ThrowsWithFault(Action onFault)
		{
			try
			{
				throw new InvalidOperationException();
				Assert.True(false);
			}
			finally
			{
				Block.ConvertToTryFault();
				onFault();
			}
			Assert.True(false);
		}


		[Fact]
		public void NoOp()
		{
			try
			{

			}
			finally
			{
				Block.ConvertToTryFault();
			}
		}



		[Fact]
		public void ShouldEnterFault()
		{
			var result = 0;
			try
			{
				ThrowsWithFault(() => result += 1);
			}
			catch (Exception ex)
			{
				result += 1;
			}
			Assert.Equal(2, result);
		}
		 
		[Fact]
		public void ShouldNotEnterFault()
		{
			try
			{
			}
			finally
			{
				Block.ConvertToTryFault();
				Assert.True(false);
			}
		}
	}
}
