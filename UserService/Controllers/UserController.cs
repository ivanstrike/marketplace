using UserMicroservice.Data;
using UserMicroservice.Model;
using UserMicroservice.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace UserMicroservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("list")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<User>>> GetUserListAsync()
        {
            _logger.LogInformation("Fetching the list of users.");
            var users = await _userService.GetUserListAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<User>> GetUserByIdAsync(Guid id)
        {
            _logger.LogInformation("Fetching user with ID {Id}", id);
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {Id} not found.", id);
                return NotFound($"User with ID {id} not found.");
            }

            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<User>> AddUserAsync([FromBody] CreateUserDTO user)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid user model: {ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Creating a new user with email {Email}", user.Email);
            var createdUser = await _userService.CreateUserAsync(user);

            _logger.LogDebug("Created user with ID {Id}", createdUser.Id);

            var url = Url.Action(nameof(GetUserByIdAsync), "User", new { id = createdUser.Id }, Request.Scheme);
            return Created(url, createdUser);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<User>> UpdateUserAsync(Guid id, [FromBody] UpdateUserDTO updatedUser)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var editedUser = await _userService.UpdateUserAsync(id, updatedUser);
            if (editedUser == null)
                return NotFound($"User with ID {id} not found.");

            return Ok(editedUser);
        }

        // Удаление пользователя
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteUserAsync(Guid id)
        {
            _logger.LogInformation("Deleting user with ID {Id}", id);
            var deleted = await _userService.DeleteUserAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("User with ID {Id} not found.", id);
                throw new KeyNotFoundException($"User with ID {id} not found.");
            }

            return NoContent();
        }
    }
}
