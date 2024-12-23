using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserMicroservice.Model;
using UserMicroservice.Data;
using UserMicroservice.RabbitMQ;
using UserMicroservice.Model;
using Microsoft.AspNetCore.Identity;
using BCrypt.Net;

namespace UserMicroservice.Services
{
    public class UserService : IUserService
    {
        private readonly DbContextClass _context;
        private readonly RabbitMqPublisher _publisher;

        public UserService(DbContextClass context, RabbitMqPublisher publisher)
        {
            _context = context;
            _publisher = publisher;
        }

        public async Task<IEnumerable<User>> GetUserListAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User> CreateUserAsync(CreateUserDTO userdto)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userdto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Пользователь с таким email уже существует.");
            }

            Guid _id = Guid.NewGuid();
            string _passwordHasher = BCrypt.Net.BCrypt.HashPassword(userdto.Password);
            User user = new User(_id, userdto.Name, userdto.Email);
            user.PasswordHash = _passwordHasher;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Отправить сообщение о создании пользователя
            await _publisher.PublishMessageAsync("user.created", new { UserId = user.Id } );
            

            return user;
        }

        public async Task<User?> UpdateUserAsync(Guid id, UpdateUserDTO updatedUser)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return null;

            user.Name = updatedUser.Name;
            user.Email = updatedUser.Email;

            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> UpdateProductCartIdAsync(Guid userId, Guid productcartId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            user.ProductCartId = productcartId;
          
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            // Отправить сообщение о удалении пользователя
            await _publisher.PublishMessageAsync("user.deleted", new { UserId = id, CartID = user.ProductCartId });

            return true;
        }

        public async Task<User> ValidateUser(UserLoginDTO loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            if (user == null)
            {
                return null;
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
            return isPasswordValid ? user : null;
        }

        public async Task<User?> CreateProduct(Guid creatortId, Guid productId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == creatortId);
            if (user != null)
            {
                user.CreatedProductIds.Add(productId);
                await _context.SaveChangesAsync();
            }
            return user;
        }
    }
}
