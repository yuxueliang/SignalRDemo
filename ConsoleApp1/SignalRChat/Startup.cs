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
                hubOptions.EnableDetailedErrors = true;//�ͻ����ܹ���ȡ��������Ϣ��Ĭ��ֵΪ false����Ϊ��Щ�쳣��Ϣ���ܰ���������Ϣ��
                hubOptions.KeepAliveInterval = TimeSpan.FromMinutes(1);//һ����û�з�����Ϣ����������ping��Ϣ�����ִ�״̬
            })
                .AddMessagePackProtocol()//��� MessagePack ��֧�� JSON �� MessagePack �ͻ��ˡ�
                .AddStackExchangeRedis("192.168.189.128:6379");//ʹ��redis�������ĵ����ԣ��ͻ��˷�����Ϣ��redis �װ壬redis����Ϣ���͸�����ˣ���֮Ҳһ����redis�װ�֪�����еĿͻ������Ӻͷ�����
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

            app.UseAuthentication();//�����ӵ� SignalR ���ĵ��û����������֤
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
