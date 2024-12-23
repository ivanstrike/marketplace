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
        Task<User?> CreateProduct(Guid creatortId, Guid productId);
        Task<bool> DeleteUserAsync(Guid id);
        Task<User> ValidateUser(UserLoginDTO loginDto);
    }
}
