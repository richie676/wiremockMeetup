using Microsoft.VisualStudio.TestTools.UnitTesting;
using WireMock.Matchers;
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using System.Threading;
using Flurl.Http;
using wiremockMeetup.Dto;
using Serilog;
using Serilog.Core;

namespace wiremockMeetup
{
    [TestClass]
    public class WiremockTests
    {
        ILogger log;

        [TestInitialize]
        public void Setup() 
        {
            log = Log.Logger = new LoggerConfiguration()
                        .WriteTo.Console()
                        .CreateLogger();
        }

        [TestMethod]
        public void WiremockSetup()
        {
            var wiremockServer = FluentMockServer.Start(5000);

            wiremockServer
              .Given(
              Request.Create().WithPath("/api/user")
              .UsingGet()
              )
              .AtPriority(100)
              .RespondWith(
                Response.Create()
                  .WithStatusCode(200)
                  .WithBodyAsJson(new UserDto() { firstName = "Walter", lastName = "Wiremock", userName = "WireWalter"})
              );

            string urlLocal = "http://localhost:5000/api/user";
            UserDto resp = urlLocal.GetJsonAsync<UserDto>().Result;
            log.Information("User: {@resp}", resp);

            wiremockServer.Stop();
        }

        [TestMethod]
        public void GetUserShoudReturnUserMax()
        {
            string urlLocal = "http://localhost:5000/api/user";
            string urlAzure = "http://playgroundapimeetup.azurewebsites.net/api/user";
            UserDto resp = urlLocal.GetJsonAsync<UserDto>().Result;

            log.Information("User: {@resp}", resp);
        }
    }
}
