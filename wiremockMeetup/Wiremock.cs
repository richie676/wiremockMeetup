using System;
using System.Collections.Generic;
using System.Text;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;
using wiremockMeetup.Dto;

namespace wiremockMeetup
{
    public static class Wiremock
    {

        public static WireMockServer FirstnameFritz(int port)
        {
            var wiremockServer = FluentMockServer.Start(port);

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

            return wiremockServer;
        }

        public static WireMockServer WildcardMatcher(int port)
        {
            var wiremockServer = FluentMockServer.Start(port);

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

            return wiremockServer;
        }

        public static WireMockServer PriotityMapping(int port)
        {
            var wiremockServer = FluentMockServer.Start(port);

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

            return wiremockServer;
        }

        public static WireMockServer WildcardInPath(int port)
        {
            var wiremockServer = FluentMockServer.Start(port);

            wiremockServer
              .Given(
              Request.Create().WithPath("/api/*")
              .UsingGet()
              .WithParam("firstname", "fritz")
              )
              .AtPriority(100)
              .RespondWith(
                Response.Create()
                  .WithStatusCode(200)
                  .WithBodyAsJson(new UserDto() { firstName = "Walter", lastName = "Wiremock", userName = "WireWalter" })
              );

            return wiremockServer;
        }


        public static WireMockServer WithSettings(int port)
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

            return wiremockServer;
        }

        public static WireMockServer WithSettingsAzure()
        {
            var setting = new FluentMockServerSettings();

          //  setting.Urls = new string[] { "http://playgroundapimeetup.azurewebsites.net" };
            setting.Urls = new string[] { "http://myDomain.com" };
           // setting.AllowPartialMapping = true;

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

            return wiremockServer;
        }
    }
}
