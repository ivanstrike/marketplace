using UserMicroservice.Model;

namespace UserMicroservice.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetUserListAsync(); 
        Task<User?> GetUserByIdAsync(Guid id); 
        Task<User> CreateUserAsync(CreateUserDTO user);
        Task<User?> UpdateUserAsync(Guid id, UpdateUserDTO updatedUser);
        Task<User?> UpdateProductCartIdAsync (Guid id, Guid productcartId);
        Task<bool> DeleteUserAsync(Guid id);
        public Task<User> ValidateUser(UserLoginDTO loginDto);
    }
}
