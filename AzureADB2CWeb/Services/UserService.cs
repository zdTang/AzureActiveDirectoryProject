using AzureADB2CWeb.Data;
using AzureADB2CWeb.Extensions;
using AzureADB2CWeb.Models;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace AzureADB2CWeb.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserService(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public User Create(User user)
        {
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
            return user;
        }

        public async Task<string?> GetB2CTokenAsync()
        {
            try
            {
                return await _httpContextAccessor!.HttpContext!.GetTokenAsync("access_token");
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public User? GetById(string b2cObjectId)
        {
            try
            {
                return _dbContext!.Users!.FirstOrDefault(u => u.B2CObjectId == b2cObjectId);
            }
            catch (Exception ex)
            {
                return new User();
            }
        }

        public User GetUserFromSession()
        {
            // Here we want to put a "User" object into the Session,so that we can know all infromation about this User
            // The B2C cannot provide all fields( customer field need graph api to retrieve)
            var user = _httpContextAccessor.HttpContext.Session.GetComplexData<User>("UserSession");
            if (user == null || string.IsNullOrWhiteSpace(user.B2CObjectId))
            {
                var idClaim = ((ClaimsIdentity)_httpContextAccessor.HttpContext.User.Identity).FindFirst(ClaimTypes.NameIdentifier);
                string userId = idClaim.Value;
                user = GetById(userId);
                _httpContextAccessor.HttpContext.Session.SetComplexData("UserSession", user);
            }
            return user;
        }
    }
}
