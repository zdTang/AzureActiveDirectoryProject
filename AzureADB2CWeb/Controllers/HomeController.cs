using AzureADB2CWeb.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Claims;
using AzureADB2CWeb.Services;
using AzureADB2CWeb.Helper;

namespace AzureADB2CWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IUserService _userService;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, IUserService userService)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _userService = userService;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var b2cObjectId = ((ClaimsIdentity)(HttpContext.User.Identity)).FindFirst(ClaimTypes.NameIdentifier).Value;
                var user = _userService.GetById(b2cObjectId);
                if (user == null || string.IsNullOrWhiteSpace(user.B2CObjectId))
                {
                    var role = ((ClaimsIdentity)(HttpContext.User.Identity)).FindFirst("extension_UserRole")?.Value;
                    user = new()
                    {
                        B2CObjectId = b2cObjectId,
                        Email = ((ClaimsIdentity)HttpContext.User.Identity).FindFirst("emails")?.Value,
                        UserRole = role
                    };
                    _userService.Create(user);
                }

            }
            return View();
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }
        [Authorize(Roles = "homeowner")]
        //[Permission("homeowner")]
        public IActionResult HomeOwner()
        {
            return View();
        }
        [Authorize(Roles = "contractor")]
        //[Permission("contractor")]
        public IActionResult Contractor()
        {
            return View();
        }

        // must give it a Redirection URL, or after Challenge, it will be confused where to go and will encounter Dead Loop
        public IActionResult SignIn()
        {
            var schema = OpenIdConnectDefaults.AuthenticationScheme;
            var redirectUrl = Url.ActionContext.HttpContext.Request.Scheme + "://" + Url.ActionContext.HttpContext.Request.Host;
            return Challenge(new AuthenticationProperties { RedirectUri = redirectUrl }, schema);
        }

        public IActionResult SignOut()
        {
            var schema = OpenIdConnectDefaults.AuthenticationScheme;
            return SignOut(new AuthenticationProperties(), CookieAuthenticationDefaults.AuthenticationScheme, schema);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> APICall()
        {
            // This is good example how to call web api with Authentication

            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:7078/WeatherForecast");
            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessToken);
            var response = await client.SendAsync(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {

            }
            return Content(response.ToString());
        }

    }

}