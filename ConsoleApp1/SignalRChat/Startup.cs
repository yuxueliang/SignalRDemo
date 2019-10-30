using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.AspNetCore.OAuth2Introspection;
using IdentityServer4.AccessTokenValidation;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SignalRChat.Hubs;

namespace SignalRChat
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddCors(options => options.AddPolicy("CorsPolicy",
                builder =>
                {
                    builder.AllowAnyMethod().AllowAnyHeader()
                        .WithOrigins("http://localhost:5000")
                        .AllowCredentials();
                }));



            services.AddIdentityServer()
                 .AddDeveloperSigningCredential()
                 .AddInMemoryClients(new[] {
                    new Client {
                        ClientId = "my-app",
                        ClientName = "my-app",
                        ClientSecrets = { new Secret("secret".Sha256()) },
                        AllowedScopes = { "my-api" },
                        AllowedGrantTypes = GrantTypes.ResourceOwnerPassword
                    }
                 })
                 .AddInMemoryApiResources(new[] {
                    new ApiResource("my-api", "SignalR Test API")
                 })
                 .AddInMemoryIdentityResources(new List<IdentityResource> {
                    new IdentityResources.OpenId(),
                    new IdentityResources.Profile(),
                    new IdentityResources.Email()
                 })
                 .AddInMemoryPersistedGrants()
                 .AddTestUsers(new List<TestUser>{
                    new TestUser {
                        SubjectId = "alice",
                        Username = "alice",
                        Password = "password"
                    },
                    new TestUser {
                        SubjectId = "bob",
                        Username = "bob",
                        Password = "password"
                    }
                 });

            services
                .AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "http://localhost:5000";
                    options.RequireHttpsMetadata = false;
                    options.ApiName = "my-api";
                    options.NameClaimType = "sub";
                    options.TokenRetriever = new Func<HttpRequest, string>(req =>
                    {
                        var fromHeader = TokenRetrieval.FromAuthorizationHeader();
                        var fromQuery = TokenRetrieval.FromQueryString();
                        return fromHeader(req) ?? fromQuery(req);
                    });
                });




            services.AddSignalR(hubOptions =>
            {
                hubOptions.EnableDetailedErrors = true;//客户端能够获取到错误信息，默认值为 false，因为这些异常消息可能包含敏感信息。
                hubOptions.KeepAliveInterval = TimeSpan.FromMinutes(1);//一分钟没有发送消息，服务器会ping消息，保持打开状态
            })
                .AddMessagePackProtocol()//添加 MessagePack 可支持 JSON 和 MessagePack 客户端。
                .AddStackExchangeRedis("192.168.189.128:6379");//使用redis发布订阅的特性，客户端发送消息到redis 底板，redis将消息发送给服务端，反之也一样，redis底板知道所有的客户端连接和服务器
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();//对连接到 SignalR 中心的用户进行身份验证
            app.UseCors("CorsPolicy");
            app.UseSignalR(routes =>
            {
                var desiredTransports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;

                routes.MapHub<ChatHub>("/chatHub", options => { options.Transports = desiredTransports; });
            });
            app.UseIdentityServer();

            app.UseMvc();
        }
    }
}
