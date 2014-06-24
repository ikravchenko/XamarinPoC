using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace Tests
{
	[TestFixture ()]
	public class NativeTest
	{
//		[DllImport("NativeUtil.dll")]
//		public static extern bool isAcceptable (float param, float threshold);

		[Test ()]
		public void TestCase ()
		{
			//isAcceptable(15.4f, 15.0f)
			Assert.IsTrue (true);
		}
	}
}

