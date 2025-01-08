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

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<ActionResult<User>> GetUserByIdAsync(Guid id)
        {
            _logger.LogInformation("Fetching user with ID {Id}", id);

            var user = await _userService.GetUserByIdAsync(id);
            return Ok(user); 
        }

        [HttpPost]
        public async Task<ActionResult<User>> AddUserAsync([FromForm] CreateUserDTO user)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid user model: {ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Creating a new user with email {Email}", user.Email);
            var createdUser = await _userService.CreateUserAsync(user);

            _logger.LogInformation("Created user with ID {Id}", createdUser.Id);

            var url = Url.Action(nameof(GetUserByIdAsync), "User", new { id = createdUser.Id }, Request.Scheme);
            return Created(url, createdUser);
        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public async Task<ActionResult<User>> UpdateUserAsync(Guid id, [FromBody] UpdateUserDTO updatedUser)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("Updating user with ID {Id}", id);
            var editedUser = await _userService.UpdateUserAsync(id, updatedUser);
            return Ok(editedUser);
        }

        [HttpDelete("{id:guid}")]
        [Authorize]
        public async Task<ActionResult> DeleteUserAsync(Guid id)
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Token is required.");
            }
            _logger.LogInformation("Deleting user with ID {Id}", id);
            await _userService.DeleteUserAsync(id, token);
            return NoContent();
        }
    }
}
