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
using Flurl;
using WireMock.Settings;

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
        public void WithoutMock()
        {   
            UserDto respGet = "http://localhost:5000/api/user"
                            .GetJsonAsync<UserDto>().Result;

            log.Information("User: {@resp}", respGet);

            //UserDto respPost = "http://localhost:5000/api/user".PostJsonAsync(new { firstname = "walter"}).Result;

            //log.Information("User: {@resp}", respPost);

            UserDto respGetParam = "http://localhost:5000/api/user"
                .SetQueryParams("firstname=Walter")
                .SetQueryParams("lastname=Walter")
                .SetQueryParams("username=Walter")
                .GetJsonAsync<UserDto>().Result;

            log.Information("User: {@resp}", respGetParam);

            //UserDto respPostParam = "http://localhost:5000/api/user"
            //             .SetQueryParams("firstname=walter")
            //             .GetJsonAsync<UserDto>().Result;

            //log.Information("User: {@resp}", respPostParam);
        }


        [TestMethod]
        public void SimpleMockingGet()
        {
            var wiremockServer = FluentMockServer.Start(5000);

            wiremockServer
              .Given(
              Request.Create().WithPath("/api/*")
              .UsingGet()
              .WithParam("firstname", "walter")
              )
              .AtPriority(100)
              .RespondWith(
                Response.Create()
                  .WithStatusCode(200)
                  .WithBodyAsJson(new UserDto() { firstName = "Walter", lastName = "Wiremock", userName = "WalterWire" })
              );

            UserDto resp = "http://localhost:5000/api/user"
                            .SetQueryParams("firstname=walter")
                            .GetJsonAsync<UserDto>().Result;

            log.Information("User: {@resp}", resp);

            wiremockServer.Stop();
        }





        //[TestMethod]
        //public void SimpleMockingPost()
        //{
        //    var wiremockServer = FluentMockServer.Start(5000);

        //    wiremockServer
        //      .Given(
        //      Request.Create().WithPath("/api/*")
        //      .UsingPost()
        //      .WithParam("firstname", "walter")
        //      )
        //      .AtPriority(100)
        //      .RespondWith(
        //        Response.Create()
        //          .WithStatusCode(200)
        //          .WithBodyAsJson(new UserDto() { firstName = "Walter", lastName = "Wiremock", userName = "WalterWire" })
        //      );

        //    UserDto resp = "http://localhost:5000/api/user"
        //                    .PostJsonAsync<UserDto>(new {"firstname" = "walter"}).Result;

        //    log.Information("User: {@resp}", resp);

        //    wiremockServer.Stop();
        //}






        [TestMethod]
        public void SimpleMockingWildcardInPath()
        {
            var wiremockServer = FluentMockServer.Start(5000);

            wiremockServer
              .Given(
              Request.Create().WithPath("/api/*")
              .UsingGet()
              .WithParam("firstname", "walter")
              )
              .AtPriority(100)
              .RespondWith(
                Response.Create()
                  .WithStatusCode(200)
                  .WithBodyAsJson(new UserDto() { firstName = "Walter", lastName = "Wiremock", userName = "WireWalter" })
              );

            UserDto resp = "http://localhost:5000/api/someotherPath"
                        .SetQueryParams("firstname=walter")
                        .GetJsonAsync<UserDto>().Result;

            log.Information("User: {@resp}", resp);

            wiremockServer.Stop();
        }




        [TestMethod]
        public void SimpleMockingWildcardInParameter()
        {
            var wiremockServer = FluentMockServer.Start(5000);

            wiremockServer
             .Given(
             Request.Create().WithPath("/api/user")
             .UsingGet()
             .WithParam("firstname", new WildcardMatcher("wa*"))
             )
             .AtPriority(100)
             .RespondWith(
               Response.Create()
                 .WithStatusCode(200)
                 .WithBodyAsJson(new UserDto() { firstName = "Walter", lastName = "Wildcard", userName = "WalterWild" })
             );

            UserDto resp = "http://localhost:5000/api/someotherPath"
                        .SetQueryParams("firstname=walter")
                        .GetJsonAsync<UserDto>().Result;

            log.Information("User: {@resp}", resp);

            wiremockServer.Stop();
        }






        [TestMethod]
        public void PriotityMapping()
        {
            var wiremockServer = FluentMockServer.Start(5000);

            wiremockServer
             .Given(
             Request.Create().WithPath("/api/*")
             .UsingGet()
             .WithParam("firstname", "walter")
             )
             .AtPriority(100)
             .RespondWith(
               Response.Create()
                 .WithStatusCode(200)
                 .WithBodyAsJson(new UserDto() { firstName = "Walter", lastName = "Wiremock", userName = "WalterWire" })
             );


            wiremockServer
              .Given(
              Request.Create().WithPath("/api/user")
              .UsingGet()
              .WithParam("firstname", new WildcardMatcher("wa*"))
              .WithParam("lastname", new WildcardMatcher("wo*"))
              )
              .AtPriority(100)
              .RespondWith(
                Response.Create()
                  .WithStatusCode(200)
                  .WithBodyAsJson(new UserDto() { firstName = "Walter", lastName = "Wildcard", userName = "WalterWild" })
              );

            UserDto resp = "http://localhost:5000/api/user"
                            .SetQueryParams("firstname=walter")
                            .GetJsonAsync<UserDto>().Result;

            log.Information("User: {@resp}", resp);

            wiremockServer.Stop();
        }






        [TestMethod]
        public void WiremockWithSetting()
        {
            var setting = new FluentMockServerSettings();

            setting.Port = 5000;
            setting.AllowPartialMapping = true;

            var wiremockServer = FluentMockServer.Start(setting);

            wiremockServer
                  .Given(
                  Request.Create().WithPath("/api/user")
                  .UsingGet()
                  .WithParam("firstname", "fritz")
                  )
                  .AtPriority(100)
                  .RespondWith(
                    Response.Create()
                      .WithStatusCode(200)
                      .WithBodyAsJson(new UserDto() { firstName = "Walter", lastName = "Wiremock", userName = "WireWalter" })
                  );

            UserDto resp = "http://localhost:5000/api/user"
                            .SetQueryParams("firstname=walter")
                            .GetJsonAsync<UserDto>().Result;

            log.Information("User: {@resp}", resp);

            wiremockServer.Stop();
        }




        [TestMethod]
        public void WiremockWithExternUrl()
        {
            var setting = new FluentMockServerSettings();

            setting.Urls = new string[] { "http://playgroundapimeetup.azurewebsites.net" };

            var wiremockServer = FluentMockServer.Start(setting);

            wiremockServer
              .Given(
              Request.Create().WithPath("/api/user")
              .UsingGet()
              .WithParam("firstname", "fritz")
              )
              .AtPriority(100)
              .RespondWith(
                Response.Create()
                  .WithStatusCode(200)
                  .WithBodyAsJson(new UserDto() { firstName = "Walter", lastName = "Wiremock", userName = "WireWalter" })
              );

            UserDto resp = "http://localhost:5000/api/user"
                            .SetQueryParams("firstname=walter")
                            .GetJsonAsync<UserDto>().Result;

            log.Information("User: {@resp}", resp);

            wiremockServer.Stop();
        }
    }
}
