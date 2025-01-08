
using Microsoft.EntityFrameworkCore;
using UserMicroservice.Model;
using UserMicroservice.Data;
using UserMicroservice.RabbitMQ;
using UserMicroservice.RabbitMQ.Events;
using Microsoft.Extensions.Caching.Distributed;


namespace UserMicroservice.Services
{
    public class UserService : IUserService
    {
        private readonly DbContextClass _context;
        private readonly RabbitMqPublisher _publisher;
        private readonly IDistributedCache _cache;

        public UserService(DbContextClass context, RabbitMqPublisher publisher, IDistributedCache cache)
        {
            _context = context;
            _publisher = publisher;
            _cache = cache;
        }

        public async Task<IEnumerable<User>> GetUserListAsync()
        {
            try
            {
                return await _context.Users.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to fetch the list of users.", ex);
            }
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {id} not found.");
                }
                return user;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving user with ID {id}.", ex);
            }
        }

        public async Task<User> CreateUserAsync(CreateUserDTO userdto)
        {
            try
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userdto.Email);
                if (existingUser != null)
                {
                    throw new InvalidOperationException("User with the specified email already exists.");
                }

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Name = userdto.Name,
                    Email = userdto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(userdto.Password)
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Publish a message about user creation
                await _publisher.PublishMessageAsync("user.created", new { UserId = user.Id });

                return user;
            }
            catch (InvalidOperationException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create a new user.", ex);
            }
        }

        public async Task<User?> UpdateUserAsync(Guid id, UpdateUserDTO updatedUser)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {id} not found.");
                }

                user.Name = updatedUser.Name;
                user.Email = updatedUser.Email;

                await _context.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update user with ID {id}.", ex);
            }
        }

        public async Task<User?> UpdateProductCartIdAsync(Guid userId, Guid productcartId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {userId} not found.");
                }

                user.ProductCartId = productcartId;
                await _context.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update product cart ID for user with ID {userId}.", ex);
            }
        }

        public async Task<bool> DeleteUserAsync(Guid id, string jwtToken)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {id} not found.");
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                if (string.IsNullOrEmpty(jwtToken))
                {
                    throw new ArgumentException("Token cannot be null or empty", nameof(jwtToken));
                }

                await _cache.SetStringAsync(jwtToken, "blacklisted", new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                });
                // Publish a message about user deletion
                await _publisher.PublishMessageAsync("user.deleted", new UserDeletedEvent 
                {
                    CartId = user.ProductCartId, 
                    JwtToken = jwtToken 
                });

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete user with ID {id}.", ex);
            }
        }

        public async Task<User> ValidateUser(UserLoginDTO loginDto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
                if (user == null)
                {
                    throw new UnauthorizedAccessException("Invalid email or password.");
                }

                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
                if (!isPasswordValid)
                {
                    throw new UnauthorizedAccessException("Invalid email or password.");
                }

                return user;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to validate user credentials.", ex);
            }
        }

        public async Task<User?> CreateProduct(Guid creatorId, Guid productId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == creatorId);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {creatorId} not found.");
                }

                user.CreatedProductIds.Add(productId);
                await _context.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to add product ID {productId} for user {creatorId}.", ex);
            }
        }

        public async Task<User?> DeleteProduct(Guid creatorId, Guid productId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == creatorId);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {creatorId} not found.");
                }

                user.CreatedProductIds.Remove(productId);
                await _context.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to remove product ID {productId} for user {creatorId}.", ex);
            }
        }
    }
}
