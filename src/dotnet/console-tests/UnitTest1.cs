using System;
using console;
using Xunit;

namespace console_tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var hoge = new Hoge()
            {
                Name = "hoge",
            };
            Assert.Equal("hoge", hoge.Name);
        }

        [Fact]
        public void Test2() => Assert.True(1 == 2, "Let's Fail!!");
        private void ThrowException() => throw new InvalidOperationException("test fail");
        [Fact]
        public void Test3() => ThrowException();
        [Fact]
        public void Test4() => Assert.Contains(new[] { 1, 2, 3 }, i => i == 4);
    }
}
