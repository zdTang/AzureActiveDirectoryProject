using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Application Client Id: 85f87650-66e3-4b1d-a3ad-1aeabbaf3000
// OAuth2.0 Authentication Endpoint: https://login.microsoftonline.com/5950d41d-9b21-4de2-bd66-bd8e54f0bd86/oauth2/v2.0/authorize
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; // This is using Cookie under the hood
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;//OpenId Connect
}).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.Authority = "https://login.microsoftonline.com/5950d41d-9b21-4de2-bd66-bd8e54f0bd86/v2.0";
    options.ClientId = "85f87650-66e3-4b1d-a3ad-1aeabbaf3000";
    //options.ResponseType = "id_token";   // This value must match AD's configuration, another option is "Access Token"?

    //https://sazzer.github.io/blog/2016/09/03/OpenID-Connect-Response-Types/
    /*  The actual set of response types are:
        code - The requester would like an Authorization Code to be returned to them
        token - The requester would like an Access Token to be returned to them
        id_token - The requester would like an ID Token to be returned to them
        none - The requester doesn’t want any of the above to be returned to them
     */

    options.ResponseType = "code";    // when authenticate with "secret" other than "token"
    options.SaveTokens = true;
    options.ClientSecret = "sKR8Q~dxuWtrqom7BNcS7JB9DNl6iwrgR.X-Ednd";// the "value" of the "Secret" created in AD
});

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
