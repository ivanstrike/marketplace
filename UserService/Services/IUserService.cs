using MicroService.Model;
using UserMicroservice.Model;

namespace MicroService.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetUserListAsync(); 
        Task<User?> GetUserByIdAsync(Guid id); 
        Task<User> CreateUserAsync(CreateUserDTO user);
        Task<User?> UpdateUserAsync(Guid id, UpdateUserDTO updatedUser);
        Task<bool> DeleteUserAsync(Guid id);
    }
}
