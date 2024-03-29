using AzureADB2CWeb.Data;
using AzureADB2CWeb.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AzureADB2CWeb
{
    public class Program
    {

        public static string Tenant = "AzureADB2CmikeDomain.onmicrosoft.com";
        public static string AzureADB2CHostname = "azureadb2cmikedomain.b2clogin.com";
        public static string ClientID = "570c99e9-8b2e-45c9-97d9-ebebf5f141de";
        public static string PolicySignUpSignIn = "B2C_1_SignIn_Up";
        public static string PolicyEditProfile = "B2C_1_Edit";
        public static string ClientSecret = "Sl18Q~PhmN6gu~ChyFpySKLYQetcvkjQCt-4Jc~P";
        // this scope copied from WEB API project-"Expose an API" Menu
        public static string Scope = "https://AzureADB2CmikeDomain.onmicrosoft.com/da920a6c-ecdb-4863-92a4-d580dc8883f3/fullAccess"; // Now we are going to access the API so that we need use the new scope

        public static string AuthorityBase = $"https://{AzureADB2CHostname}/{Tenant}";
        public static string AuthoritySignInUp = $"{AuthorityBase}/{PolicySignUpSignIn}/v2.0";
        public static string AuthorityEditProfile = $"{AuthorityBase}/{PolicyEditProfile}/v2.0";
        //options.Authority = "https://azureadb2cmikedomain.b2clogin.com/AzureADB2CmikeDomain.onmicrosoft.com/B2C_1_SignIn_Up/v2.0/";
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddHttpContextAccessor();
            // Should understand how to setup Session
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromDays(1);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConection")));
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; // This is using Cookie under the hood
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;//OpenId Connect
            }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                // After adding the policy, copy the part before ".well_known" from the URL
                //https://AzureADB2CmikeDomain.b2clogin.com/AzureADB2CmikeDomain.onmicrosoft.com/<policy-name>/v2.0/.well-known/openid-configuration
                options.Authority = AuthoritySignInUp;
                options.ClientId = ClientID;
                //options.ResponseType = "id_token";   // This value must match AD's configuration, another option is "Access Token"?

                //https://sazzer.github.io/blog/2016/09/03/OpenID-Connect-Response-Types/
                /*  The actual set of response types are:
                    code - The requester would like an Authorization Code to be returned to them
                    token - The requester would like an Access Token to be returned to them
                    id_token - The requester would like an ID Token to be returned to them
                    none - The requester doesn�t want any of the above to be returned to them
                 */
                options.ResponseType = "code";    // when authenticate with "secret" other than "token"
                options.SaveTokens = true;
                //see tutorial why we need add this key.
                options.Scope.Add(Scope); // same as ClientId
                options.ClientSecret = ClientSecret;
                //options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = "name" }; 
                options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname" };
                //options.Events = new OpenIdConnectEvents
                //{
                //    OnTokenValidated = async opt =>
                //    {
                //        string role = opt.Principal.FindFirstValue("extension_UserRole");
                //        var claims = new List<Claim>
                //        {
                //            new Claim(ClaimTypes.Role, role),
                //        };
                //        var appIdentity = new ClaimsIdentity(claims);
                //        opt.Principal.AddIdentity(appIdentity);
                //    }
                //};
            }).AddOpenIdConnect("B2C_1_Edit", GetOpenIdConnectOptions("B2C_1_Edit"));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

        private static Action<OpenIdConnectOptions> GetOpenIdConnectOptions(string policy) => options =>
        {
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            // After adding the policy, copy the part before ".well_known" from the URL
            //https://AzureADB2CmikeDomain.b2clogin.com/AzureADB2CmikeDomain.onmicrosoft.com/<policy-name>/v2.0/.well-known/openid-configuration
            options.Authority = AuthorityEditProfile;
            options.ClientId = ClientID;
            options.ResponseType = "code";    // when authenticate with "secret" other than "token"
            options.SaveTokens = true;
            options.Scope.Add(Scope); // same as ClientId
            options.ClientSecret = ClientSecret;
            options.CallbackPath = "/signin-oidc-" + policy;
            options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname" };
            //This can handel when user click "Cancel" error, will direct to Home page
            options.Events = new OpenIdConnectEvents
            {
                OnMessageReceived = context =>
                {
                    if (!string.IsNullOrEmpty(context.ProtocolMessage.Error) && !string.IsNullOrEmpty(context.ProtocolMessage.ErrorDescription))
                    {
                        if (context.ProtocolMessage.Error.Contains("access_denied"))
                        {
                            context.HandleResponse();
                            context.Response.Redirect("/");
                        }
                    }
                    return Task.FromResult(0);
                }
            };
        };
    }


}