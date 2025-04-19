using Microsoft.AspNetCore.Mvc;
using MyCryptoAPI.Services;

namespace MyCryptoAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            await _userService.CreateUserAsync(user);
            return Ok("User created.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest login)
        {
            var user = await _userService.AuthenticateAsync(login.Email, login.Password);
            if (user == null) return Unauthorized("Invalid credentials");

            return Ok(user);
        }
        [HttpGet("{username}")]
         public async Task<IActionResult> GetUserByUsername(string username)
       {
          var user = await _userService.GetUserByUsernameAsync(username);
          if (user == null)
        {
        return NotFound($"User with username '{username}' not found.");
        }
           return Ok(user);
         }
        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
        [HttpPost("{username}/buy")]
         public async Task<IActionResult> BuyCrypto(string username, [FromBody] BuyRequest request)
        {    
          var user = await _userService.GetUserByUsernameAsync(username);
          if (user == null) return NotFound("User not found.");

          if (user.Balance < request.TotalCost)
          return BadRequest("Insufficient balance.");

   
          user.Balance -= request.TotalCost;


          var holding = user.Holdings.FirstOrDefault(h => h.Symbol == request.Symbol);
        if (holding != null)
        {
            holding.Amount += request.Amount;
        }
        else
        {
        user.Holdings.Add(new Holding { Symbol = request.Symbol, Amount = request.Amount });
        }

         await _userService.UpdateUserAsync(user);
         return Ok(user);
        }

         public class BuyRequest
        {
          public string Symbol { get; set; } = string.Empty;
          public decimal Amount { get; set; }
          public decimal TotalCost { get; set; }
        }
        [HttpGet("profile/{username}")]
        public async Task<IActionResult> GetUserProfile(string username)
        {
          var user = await _userService.GetUserByUsernameAsync(username);
          if(user != null) return Ok(new{ 
             id = user.Id,
             username = user.Username,
             email = user.Email,
             balance = user.Balance,
             paymentMethods = user.PaymentMethods,
             holdings = user.Holdings
          });
          return Ok(user);

        }
        [HttpPost("add/{id}")]
        public async Task<IActionResult> AddPaymentMethod([FromBody] PaymentMethodRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId) ||
                string.IsNullOrEmpty(request.Method) ||
                string.IsNullOrEmpty(request.CardNumber) ||
                string.IsNullOrEmpty(request.ExpiryDate) ||
                string.IsNullOrEmpty(request.CVV))
            {
                return BadRequest("All fields are required.");
            }

            var paymentMethod = new PaymentMethod
            {
                Method = request.Method,
                CardNumber = request.CardNumber,
                ExpiryDate = request.ExpiryDate,
                CVV = request.CVV
            };

            try
            {
                await _userService.SavePaymentMethodAsync(request.UserId, paymentMethod);
                return Ok(new { message = "Payment method added successfully ." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        [HttpPost("{username}/deposit")]
public async Task<IActionResult> DepositFunds(string username, [FromBody] DepositRequest request)
{
    
    if (request.Amount <= 0)
        return BadRequest("Amount must be greater than zero.");

    var user = await _userService.GetUserByUsernameAsync(username);
    if (user == null)
        return NotFound("User not found.");

    if (user.PaymentMethods == null || user.PaymentMethods.Count == 0)
        return BadRequest("No payment method on file. Please add a card first.");

    user.Balance += request.Amount;

    await _userService.UpdateUserAsync(user);

    return Ok(new { balance = user.Balance });
}


public class DepositRequest
{
    public decimal Amount { get; set; }
}

    public class PaymentMethodRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;  
        public string CardNumber { get; set; } = string.Empty;
        public string ExpiryDate { get; set; } = string.Empty; 
        public string CVV { get; set; } = string.Empty;
    }

    }
}
