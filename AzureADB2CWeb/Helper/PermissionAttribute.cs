using AzureADB2CWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace AzureADB2CWeb.Helper
{
    public class PermissionAttribute : AuthorizeAttribute, IAsyncAuthorizationFilter
    {
        public string Role { get; set; }
        private IUserService userService;

        public PermissionAttribute(string role)
        {
            Role = role;
        }


        //Notice here we have no "await" in the method, but we need Async here, as it returns a Task
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Notice here we need userService, but it cannot be DJ from the Contractor
            // As our Contructor will get only the "Role" string
            // So that we use this approach !!!!
            userService = context.HttpContext.RequestServices.GetService<IUserService>();
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                var user = userService.GetUserFromSession();
                var newClaim = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, user.UserRole) });

                context.HttpContext.User.AddIdentity(newClaim);
                if (user.UserRole == Role)
                {
                    return;
                }
                context.Result = new StatusCodeResult(403);
                context.Result = new RedirectResult("/error/403");
                return;
            }
            context.Result = new StatusCodeResult(401);
            context.Result = new RedirectResult("/error/401");
            return;
        }
    }
}
