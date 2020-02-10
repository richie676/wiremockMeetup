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
using System.Threading.Tasks;
using System.Net.Http;

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

            var respPost = "http://localhost:5000/api/user"
                            .PostAsync(null).ReceiveJson<UserDto>().Result;
            log.Information("User: {@resp}", respPost);

            UserDto respGetParam = "http://localhost:5000/api/user"
                .SetQueryParams("firstname=Walter")
                .SetQueryParams("lastname=Walter")
                .SetQueryParams("username=Walter")
                .GetJsonAsync<UserDto>().Result;
            log.Information("User: {@resp}", respGetParam);

            var respPostParam = "http://localhost:5000/api/user"
                .SetQueryParams("firstname=Wolfgang")
                .SetQueryParams("lastname=Wolfgang")
                .SetQueryParams("username=Wolfgang")
                .PostAsync(null).ReceiveJson<UserDto>().Result;
            log.Information("User: {@resp}", respPostParam);
        }







        [TestMethod]
        public void SimpleMockingGet()
        {
            var wiremockServer = FluentMockServer.Start(5000);

            wiremockServer
              .Given(
              Request.Create().WithPath("/api/*")
              .UsingGet()
              )
              .AtPriority(100)
              .RespondWith(
                Response.Create()
                  .WithStatusCode(200)
                  .WithBodyAsJson(new UserDto() { firstName = "Walter", lastName = "Wiremock", userName = "WalterWire" })
              );

            UserDto resp = "http://localhost:5000/api/user"
                            .GetJsonAsync<UserDto>().Result;

            log.Information("User: {@resp}", resp);

            wiremockServer.Stop();
        }





        [TestMethod]
        public void SimpleMockingGetResponseWithOptions()
        {
            var wiremockServer = FluentMockServer.Start(5000);

            wiremockServer
              .Given(
              Request.Create().WithPath("/api/*")
              .UsingGet()
              )
              .AtPriority(100)
              .RespondWith(
                Response.Create()
                  .WithSuccess() //<-- Code 200
                  .WithHeader("access-control-allow-origin", "*") //<-- Headers
                  .WithHeader("hello", "world") //<-- Headers
                  .WithDelay(5000) //<-- Delay
                  .WithBodyAsJson(new UserDto() { firstName = "Walter", lastName = "Wiremock", userName = "WalterWire" })
              );

            var resp = "http://localhost:5000/api/user"
                            .GetAsync().Result;

            log.Information("Headers: {@header}", resp.Headers);
            log.Information("User: {@body}", resp.Content.ReadAsStringAsync().Result);

            wiremockServer.Stop();
        }





        [TestMethod]
        public void SimpleMockingGetReturn500()
        {
            var wiremockServer = FluentMockServer.Start(5000);

            wiremockServer
              .Given(
              Request.Create().WithPath("/api/*")
              .UsingGet()
              )
              .AtPriority(100)
              .RespondWith(
                Response.Create()
                  .WithStatusCode(500) //<-- Code 500
              );

            UserDto resp = "http://localhost:5000/api/user"
                            .GetJsonAsync<UserDto>().Result;

            log.Information("User: {@resp}", resp);

            wiremockServer.Stop();
        }





        [TestMethod]
        public void SimpleMockingPost()
        {
            var wiremockServer = FluentMockServer.Start(5000);

            wiremockServer
              .Given(
              Request.Create().WithPath("/api/*")
              .UsingPost() //<-- Post
              .WithParam("firstname", "Walter")
              )
              .AtPriority(100)
              .RespondWith(
                Response.Create()
                  .WithStatusCode(200)
                  .WithBodyAsJson(new UserDto() { firstName = "Walter", lastName = "Wiremock", userName = "WalterWire" })
              );

            var data = new UserDto() { firstName = "Walter", lastName = "Wiremock", userName = "WalterWire" };

            var resp = "http://localhost:5000/api/user"
                            .SetQueryParams("firstname=Walter")
                            .PostAsync(null).ReceiveJson<UserDto>().Result;

            log.Information("User: {@resp}", resp);

            wiremockServer.Stop();
        }







        [TestMethod]
        public void SimpleMockingGetWithParameter()
        {
            var wiremockServer = FluentMockServer.Start(5000);

            wiremockServer
            .Given(
            Request.Create().WithPath("/api/*")
            .UsingGet()
            )
            .AtPriority(100)
            .RespondWith(
              Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new UserDto() { firstName = "Walter", lastName = "Wiremock", userName = "WalterWire" })
            );

            wiremockServer
              .Given(
              Request.Create().WithPath("/api/*")
              .UsingGet()
              .WithParam("firstname", "Wolfgang")
              )
              .AtPriority(100)
              .RespondWith(
                Response.Create()
                  .WithStatusCode(200)
                  .WithBodyAsJson(new UserDto() { firstName = "Wolfgang", lastName = "Wiremock", userName = "Wolfwire" })
              );


            UserDto resp = "http://localhost:5000/api/user"
                            .SetQueryParams("firstname=Wolfgang")
                            .GetJsonAsync<UserDto>().Result;

            log.Information("User: {@resp}", resp);

            wiremockServer.Stop();
        }



        [TestMethod]
        public void SimpleMockingGetWithParameterSatictWillFail()
        {
            var wiremockServer = FluentMockServer.Start(5000);

            wiremockServer
              .Given(
              Request.Create().WithPath("/api/*")
              .UsingGet()
              .WithParam("firstname", "Wolfgang") //<-- Parameter mandatory
              )
              .AtPriority(100)
              .RespondWith(
                Response.Create()
                  .WithStatusCode(200)
                  .WithBodyAsJson(new UserDto() { firstName = "Wolfgang", lastName = "Wiremock", userName = "Wolfwire" })
              );

            UserDto resp = "http://localhost:5000/api/user"
                            .GetJsonAsync<UserDto>().Result; //<-- Parameter missing, not mapping found

            log.Information("User: {@resp}", resp);

            wiremockServer.Stop();
        }


        [TestMethod]
        public void SimpleMockingGetWithParameterWithPartialMapping()
        {
            var setting = new FluentMockServerSettings();

            setting.Port = 5000;
            setting.AllowPartialMapping = true; //<-- PartialMapping

            var wiremockServer = FluentMockServer.Start(setting);

            wiremockServer
              .Given(
              Request.Create().WithPath("/api/*")
              .UsingGet()
              .WithParam("firstname", "Wolfgang") //<-- Parameter
              )
              .AtPriority(100)
              .RespondWith(
                Response.Create()
                  .WithStatusCode(200)
                  .WithBodyAsJson(new UserDto() { firstName = "Wolfgang", lastName = "Wiremock", userName = "Wolfwire" })
              );

            UserDto resp = "http://localhost:5000/api/user"
                            .GetJsonAsync<UserDto>().Result;  //<-- Parameter missing, but mapping will be found

            log.Information("User: {@resp}", resp);

            wiremockServer.Stop();
        }







        [TestMethod]
        public void SimpleMockingWildcardInPath()
        {
            var wiremockServer = FluentMockServer.Start(5000);

            wiremockServer
              .Given(
              Request.Create().WithPath("/api/*")  //<-- Wildcard in Path
              .UsingGet()
              .WithParam("firstname", "Walter")
              )
              .AtPriority(100)
              .RespondWith(
                Response.Create()
                  .WithStatusCode(200)
                  .WithBodyAsJson(new UserDto() { firstName = "Walter", lastName = "Wiremock", userName = "WireWalter" })
              );

            UserDto resp = "http://localhost:5000/api/someotherPath"
                        .SetQueryParams("firstname=Walter")
                        .GetJsonAsync<UserDto>().Result;

            log.Information("User: {@resp}", resp);

            wiremockServer.Stop();
        }




        [TestMethod]
        public void SimpleMockingWildcardInParameter()
        {
            //https://github.com/WireMock-Net/WireMock.Net/wiki/Request-Matching

            var wiremockServer = FluentMockServer.Start(5000);

            wiremockServer
             .Given(
             Request.Create().WithPath("/api/user")
             .UsingGet()
             .WithParam("firstname", new WildcardMatcher("Wa*")) //<-- Wildcard in Parameter
             )
             .AtPriority(100)
             .RespondWith(
               Response.Create()
                 .WithStatusCode(200)
                 .WithBodyAsJson(new UserDto() { firstName = "Walter", lastName = "Wildcard", userName = "WalterWild" })
             );

            UserDto resp = "http://localhost:5000/api/user"
                        .SetQueryParams("firstname=Walter")
                        .GetJsonAsync<UserDto>().Result;

            log.Information("User: {@resp}", resp);

            wiremockServer.Stop();
        }



        [TestMethod]
        public void PriotityMapping()
        {
            var setting = new FluentMockServerSettings();

            setting.Port = 5000;
            setting.AllowPartialMapping = true;

            var wiremockServer = FluentMockServer.Start(setting);

            wiremockServer
             .Given(
             Request.Create().WithPath("/api/*")
             .UsingGet()
             .WithParam("firstname", "Walter")
             )
             .AtPriority(50) // <-- Priority 50
             .RespondWith(
               Response.Create()
                 .WithStatusCode(200)
                 .WithBodyAsJson(new UserDto() { firstName = "Walter", lastName = "Wiremock", userName = "PriorityA" })
             );

            wiremockServer
              .Given(
              Request.Create().WithPath("/api/user")
              .UsingGet()
              .WithParam("firstname", "Walter")
              )
              .AtPriority(100) // <-- Priority 100
              .RespondWith(
                Response.Create()
                  .WithStatusCode(200)
                  .WithBodyAsJson(new UserDto() { firstName = "Walter", lastName = "Wildcard", userName = "PriorityB" })
              );

            UserDto resp = "http://localhost:5000/api/user"
                            .SetQueryParams("firstname=Walter")
                            .GetJsonAsync<UserDto>().Result;

            log.Information("User: {@resp}", resp);

            wiremockServer.Stop();
        }




        [TestMethod]
        public void ResetMapping()
        {
            var setting = new FluentMockServerSettings();

            setting.Port = 5000;
            setting.AllowPartialMapping = true;

            var wiremockServer = FluentMockServer.Start(setting);

            wiremockServer
             .Given(
             Request.Create().WithPath("/api/*")
             .UsingGet()
             .WithParam("firstname", "Walter")
             )
             .AtPriority(50)
             .RespondWith(
               Response.Create()
                 .WithStatusCode(200)
                 .WithBodyAsJson(new UserDto() { firstName = "Walter", lastName = "Wiremock", userName = "PriorityA" })
             );

            wiremockServer.ResetMappings(); // <-- reset Mapping

            wiremockServer
              .Given(
              Request.Create().WithPath("/api/user")
              .UsingGet()
              .WithParam("firstname", "Walter")
              )
              .AtPriority(100)
              .RespondWith(
                Response.Create()
                  .WithStatusCode(200)
                  .WithBodyAsJson(new UserDto() { firstName = "Walter", lastName = "Wildcard", userName = "PriorityB" })
              );

            UserDto resp = "http://localhost:5000/api/user"
                            .SetQueryParams("firstname=Walter")
                            .GetJsonAsync<UserDto>().Result;

            log.Information("User: {@resp}", resp);

            wiremockServer.Stop();
        }





        [TestMethod]
        public void LogEntries()
        {
            var setting = new FluentMockServerSettings();

            setting.Port = 5000;
            setting.AllowPartialMapping = true;

            var wiremockServer = FluentMockServer.Start(setting);

            wiremockServer
             .Given(
             Request.Create().WithPath("/api/*")
             .UsingGet()
             .WithParam("firstname", "Walter")
             )
             .AtPriority(50)
             .RespondWith(
               Response.Create()
                 .WithStatusCode(200)
                 .WithBodyAsJson(new UserDto() { firstName = "Walter", lastName = "Wiremock", userName = "PriorityA" })
             );

            wiremockServer
              .Given(
              Request.Create().WithPath("/api/user")
              .UsingGet()
              .WithParam("firstname", "Walter")
              .WithParam("lastname", "Wildcard")
              )
              .AtPriority(100)
              .RespondWith(
                Response.Create()
                  .WithStatusCode(200)
                  .WithBodyAsJson(new UserDto() { firstName = "Walter", lastName = "Wildcard", userName = "PriorityB" })
              );

            UserDto respA = "http://localhost:5000/api/user"
                .SetQueryParams("firstname=Wa")
                .GetJsonAsync<UserDto>().Result;
            log.Information("User: {@resp}", respA);

            UserDto respB = "http://localhost:5000/api/user"
                            .SetQueryParams("firstname=Walter")
                            .GetJsonAsync<UserDto>().Result;
            log.Information("User: {@resp}", respB);

            UserDto respC = "http://localhost:5000/api/user"
                .SetQueryParams("firstname=Walter")
                .SetQueryParams("lastname=Wildcard")
                .GetJsonAsync<UserDto>().Result;
            log.Information("User: {@resp}", respC);

            log.Information("Logs: {@LogEntries}", wiremockServer.LogEntries); //<-- LogEntries

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
        public void WiremockOptionsCall()
        {
            var setting = new FluentMockServerSettings();

            setting.Port = 5000;
            setting.AllowPartialMapping = false;

            var wiremockServer = FluentMockServer.Start(setting);

            wiremockServer
                   .Given(
                   Request.Create().WithPath("/api/user")
                   .UsingMethod("OPTIONS") //<-- OPTIONS
                   //.UsingAnyMethod()
                   )
                   .AtPriority(1000)
                   .RespondWith(
                     Response.Create()
                       .WithStatusCode(200)
                       .WithHeader("access-control-allow-headers", "*")
                       .WithHeader("access-control-allow-methods", "GET, HEAD, POST, PUT, DELETE, CONNECT, OPTIONS, TRACE, PATCH")
                       .WithHeader("access-control-allow-origin", "*")
                   );

            var resp = "http://localhost:5000/api/user"
                            .OptionsAsync()
                            .ReceiveJson().Result;

            log.Information("User: {@resp}", resp);

            wiremockServer.Stop();
        }
    }
}
