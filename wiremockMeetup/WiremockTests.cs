using Microsoft.VisualStudio.TestTools.UnitTesting;
using WireMock.Matchers;
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using System.Threading;

namespace wiremockMeetup
{
    [TestClass]
    public class WiremockTests
    {
        [TestMethod]
        public void WiremockSetup()
        {
            var wiremockServer = FluentMockServer.Start(80);

            var wildcard = new WildcardMatcher("*");

            wiremockServer
              .Given(
              Request.Create().WithPath("/example/api")
              .UsingPost()
              .WithParam("user", wildcard)
              .UsingPost()
              )
              .AtPriority(1)
              .RespondWith(
                Response.Create()
                  .WithStatusCode(200)
                  .WithHeader("access-control-allow-origin", "*")
                  .WithHeader("Content-Type", "text/json")
                  .WithHeader("Hallo", "World")
              );

            Thread.Sleep(15_000);
            wiremockServer.Stop();
        }
    }
}
