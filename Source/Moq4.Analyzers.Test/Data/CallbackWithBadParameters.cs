﻿using Moq;

namespace CallbackWithBadParameters
{
    interface IMyService
    {
        int Do(string s);
    }

    class MyUnitTests
    {
        void MyTest()
        {
            var mock = new Mock<IMyService>();
            mock.Setup(x => x.Do(It.IsAny<string>())).Callback((string s1, string s2) => { });
        }
    }
}