using System;
using System.Collections.Generic;
using Moq;

namespace CallbackSignatureShouldMatchMockedMethod.TestGoodSetupAndCallback;

internal interface IFoo
{
    int Do(string s);

    int Do(int i, string s, DateTime dt);

    int Do(List<string> l);
}

internal class MyUnitTests
{
    private void TestGoodSetupAndCallback()
    {
        var mock = new Mock<IFoo>();
        mock.Setup(x => x.Do(It.IsAny<string>())).Callback((string s) => { });
        mock.Setup(x => x.Do(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>())).Callback((int i, string s, DateTime dt) => { });
        mock.Setup(x => x.Do(It.IsAny<List<string>>())).Callback((List<string> l) => { });
    }
}
