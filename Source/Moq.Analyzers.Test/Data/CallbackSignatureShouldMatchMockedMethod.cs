using System;
using System.Collections.Generic;
using Moq;

namespace CallbackSignatureShouldMatchMockedMethod;

internal interface IFoo
{
    int Do(string s);

    int Do(int i, string s, DateTime dt);

    int Do(List<string> l);
}

internal class MyUnitTests
{
    private void TestBadCallbacks()
    {
        var mock = new Mock<IFoo>();
        mock.Setup(x => x.Do(It.IsAny<string>())).Callback((int i) => { });
        mock.Setup(x => x.Do(It.IsAny<string>())).Callback((string s1, string s2) => { });
        mock.Setup(x => x.Do(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>())).Callback((string s1, int i1) => { });
        mock.Setup(x => x.Do(It.IsAny<List<string>>())).Callback((int i) => { });
    }

    private void TestGoodSetupAndParameterlessCallback()
    {
        var mock = new Mock<IFoo>();
        mock.Setup(x => x.Do(It.IsAny<string>())).Callback(() => { });
        mock.Setup(x => x.Do(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>())).Callback(() => { });
        mock.Setup(x => x.Do(It.IsAny<List<string>>())).Callback(() => { });
    }

    private void TestGoodSetupAndCallback()
    {
        var mock = new Mock<IFoo>();
        mock.Setup(x => x.Do(It.IsAny<string>())).Callback((string s) => { });
        mock.Setup(x => x.Do(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>())).Callback((int i, string s, DateTime dt) => { });
        mock.Setup(x => x.Do(It.IsAny<List<string>>())).Callback((List<string> l) => { });
    }

    private void TestGoodSetupAndReturnsAndCallback()
    {
        var mock = new Mock<IFoo>();
        mock.Setup(x => x.Do(It.IsAny<string>())).Returns(0).Callback((string s) => { });
        mock.Setup(x => x.Do(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>())).Returns(0).Callback((int i, string s, DateTime dt) => { });
        mock.Setup(x => x.Do(It.IsAny<List<string>>())).Returns(0).Callback((List<string> l) => { });
    }

    private void MyGoodSetupAndReturns()
    {
        var mock = new Mock<IFoo>();
        mock.Setup(x => x.Do(It.IsAny<string>())).Returns((string s) => { return 0; });
        mock.Setup(x => x.Do(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>())).Returns((int i, string s, DateTime dt) => { return 0; });
        mock.Setup(x => x.Do(It.IsAny<List<string>>())).Returns((List<string> l) => { return 0; });
    }
}
