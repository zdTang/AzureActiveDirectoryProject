using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

namespace AzureADB2CApi
{
    public class Program
    {


        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            /*
             options.Authority = $"https://azureadb2cmikedomain.b2clogin.com/AzureADB2CmikeDomain.onmicrosoft.com/{policy}/v2.0/";
            options.ClientId = "570c99e9-8b2e-45c9-97d9-ebebf5f141de";
            options.ResponseType = "code";    // when authenticate with "secret" other than "token"
            options.SaveTokens = true;
            options.Scope.Add(options.ClientId); // same as ClientId
            options.ClientSecret = "Sl18Q~PhmN6gu~ChyFpySKLYQetcvkjQCt-4Jc~P";
            options.CallbackPath = "/signin-oidc-" + policy;
            options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname" };
             */




            // Here add services and setup it.
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(
                options =>
                {
                    builder.Configuration.Bind("AzureADB2C", options);
                    //options.TokenValidationParameters.NameClaimType = "name";
                },
                options =>
                {
                    builder.Configuration.Bind("AzureADB2C", options);
                }
                );
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();// Here to setup pipeline to use it.
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}