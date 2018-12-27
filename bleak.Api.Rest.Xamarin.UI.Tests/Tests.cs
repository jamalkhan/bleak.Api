using System;
using System.IO;
using System.Linq;
using bleak.Api.Rest.Common;
using bleak.Api.Rest.Portable;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace bleak.Api.Rest.Xamarin.UI.Tests
{
    [TestFixture(Platform.Android)]
    [TestFixture(Platform.iOS)]
    public class Tests
    {
        IApp app;
        Platform platform;

        public Tests(Platform platform)
        {
            this.platform = platform;
        }

        [SetUp]
        public void BeforeEachTest()
        {
            app = AppInitializer.StartApp(platform);
        }

        [Test]
        public void WelcomeTextIsDisplayed()
        {
            var s = "https://google.com";
            var serializer = new PortableJsonSerializer();
            var restManager = new PortableRestManager(serializer, serializer, "PortableRestManagerTest");
            var results = restManager.ExecuteRestMethod<string, string>(uri: new Uri(s), verb: HttpVerbs.GET);
            Assert.IsTrue(results.Results.Contains("<body"));

            //AppResult[] results = app.WaitForElement(c => c.Marked("Welcome to Xamarin.Forms!"));
            //app.Screenshot("Welcome screen.");

            //Assert.IsTrue(results.Any());
        }
    }
}
