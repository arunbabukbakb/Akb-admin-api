using Models;
using Models.DtoModels;
namespace Data.Repository.IRepository
{
    public interface IAuthRepository
    {
        public Users? Authenticate(LoginModel login);
        public Customer? CustomerAuthenticate(LoginModel login);
        LoggedInUserDto GetLoggedInUser();
    }
}
