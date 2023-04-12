using AzureADB2CWeb.Models;

namespace AzureADB2CWeb.Services
{
    public interface IUserService
    {
        User Create(User user); // How come we have such a wired method?
        User GetById(string b2cObjectId);
        Task<string> GetB2CTokenAsync();
        User GetUserFromSession();
    }
}
