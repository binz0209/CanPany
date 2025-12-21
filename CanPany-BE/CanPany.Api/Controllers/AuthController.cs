using CanPany.Api.Extensions;
using CanPany.Application.Common.Models;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Google.Apis.Auth;
using System.Net.Mail;
using System.Net;

namespace CanPany.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _users;
    private readonly IJwtTokenService _jwt;
    private readonly ILocalizationService _localization;
    private readonly IWalletService _wallet;

    public AuthController(IUserService users, IJwtTokenService jwt, ILocalizationService localization, IWalletService wallet)
    {
        _users = users;
        _jwt = jwt;
        _localization = localization;
        _wallet = wallet;
    }

    public record RegisterRequest(string FullName, string Email, string Password, string Role = "User"); // Role: "Candidate" or "Company"

    // ✅ Cập nhật LoginRequest để có RememberMe
    public record LoginRequest(string Email, string Password, bool RememberMe = false);

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        try
        {
            var user = await _users.RegisterAsync(req.FullName, req.Email, req.Password, req.Role);
            var (token, exp) = _jwt.GenerateToken(user.Id, user.Email, user.Role);
            var response = new { accessToken = token, expiresIn = exp };
            var message = _localization.GetSuccess("UserCreated");
            return Ok(ApiResponse<object>.CreateSuccess(response, message));
        }
        catch (Domain.Exceptions.BusinessRuleViolationException ex) when (ex.RuleName == "EmailExists")
        {
            var message = _localization.GetError("EmailExists");
            return BadRequest(ApiResponse.CreateError(message, "EmailExists"));
        }
        catch (Exception ex)
        {
            var message = _localization.GetError("ValidationFailed");
            return BadRequest(ApiResponse.CreateError(message, "ValidationFailed"));
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        try
        {
            var user = await _users.ValidateUserAsync(req.Email, req.Password);
            if (user is null)
            {
                var errorMsg = _localization.GetError("InvalidCredentials");
                return Unauthorized(ApiResponse.CreateError(errorMsg, "InvalidCredentials"));
            }

            // ✅ Gọi GenerateToken với RememberMe
            var (token, exp) = _jwt.GenerateToken(user.Id, user.Email, user.Role, req.RememberMe);
            var response = new { accessToken = token, expiresIn = exp };
            var successMsg = _localization.GetSuccess("Login");
            return Ok(ApiResponse<object>.CreateSuccess(response, successMsg));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [AuthController.Login] Error: {ex.Message}");
            Console.WriteLine($"❌ [AuthController.Login] StackTrace: {ex.StackTrace}");
            return StatusCode(500, ApiResponse.CreateError(_localization.GetError("InternalServer"), "InternalServer"));
        }
    }

    [AllowAnonymous]
    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest req)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(req.IdToken, new GoogleJsonWebSignature.ValidationSettings());
            var user = await _users.GetByEmailAsync(payload.Email);
            if (user == null)
            {
                user = await _users.RegisterAsync(
                    payload.Name ?? payload.Email.Split('@')[0],
                    payload.Email,
                    Guid.NewGuid().ToString(),
                    "Candidate" // Default to Candidate for Google login
                );
                // Wallet is automatically created in RegisterAsync
            }
            else
            {
                // Ensure wallet exists for existing users (backward compatibility)
                try
                {
                    await _wallet.EnsureAsync(user.Id);
                }
                catch (Exception ex)
                {
                    // Log but don't fail login
                    Console.WriteLine($"Warning: Failed to ensure wallet for user {user.Id}: {ex.Message}");
                }
            }

            // Google login không cần RememberMe — vẫn dùng TTL mặc định
            var (token, exp) = _jwt.GenerateToken(user.Id, user.Email, user.Role);
            var response = new { accessToken = token, expiresIn = exp };
            var msg = _localization.GetSuccess("Login");
            return Ok(ApiResponse<object>.CreateSuccess(response, msg));
        }
        catch (Exception ex)
        {
            var msg = _localization.GetError("InternalServer");
            return BadRequest(ApiResponse.CreateError(msg, "InternalServer"));
        }
    }

    public record GoogleLoginRequest(string IdToken);

    public record ForgotPasswordRequest(string Email);
    public record ResetPasswordRequest(string Email, string Code, string NewPassword);
    private static readonly Dictionary<string, (string Code, DateTime Expire)> _resetCodes = new();

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
    {
        try
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Email))
            {
                var errorMsg = _localization.GetString("Validation_Required", new object[] { "Email" });
                return BadRequest(ApiResponse.CreateError(errorMsg, "Validation_Required"));
            }

            var user = await _users.GetByEmailAsync(req.Email);
            if (user == null)
            {
                var notFoundMsg = _localization.GetError("UserNotFound");
                return NotFound(ApiResponse.CreateError(notFoundMsg, "UserNotFound"));
            }

            // Tạo mã ngẫu nhiên 6 số
            var code = new Random().Next(100000, 999999).ToString();
            _resetCodes[req.Email] = (code, DateTime.UtcNow.AddMinutes(10));

            try
            {
                using var smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("pvapro123@gmail.com", "jtkx dauy cdmt mysg"), // App password Gmail
                    EnableSsl = true
                };

                var mailMessage = new MailMessage("yourgmail@gmail.com", req.Email)
                {
                    Subject = "CanPany - Mã khôi phục mật khẩu",
                    Body = $"Mã xác thực của bạn là: {code}\nMã này sẽ hết hạn sau 10 phút.",
                    IsBodyHtml = false
                };

                await smtp.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AuthController.ForgotPassword] Failed to send email: {ex.Message}");
                return StatusCode(500, ApiResponse.CreateError(_localization.GetError("EmailNotSent"), "EmailNotSent"));
            }

            var successMsg = _localization.GetSuccess("ResetCodeSent");
            return Ok(ApiResponse.CreateSuccess(successMsg));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [AuthController.ForgotPassword] Error: {ex.Message}");
            return StatusCode(500, ApiResponse.CreateError(_localization.GetError("InternalServer"), "InternalServer"));
        }
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        try
        {
            if (req == null)
            {
                var msg = _localization.GetString("Validation_Required", new object[] { "Request" });
                return BadRequest(ApiResponse.CreateError(msg, "Validation_Required"));
            }
            if (string.IsNullOrWhiteSpace(req.Email))
            {
                var msg = _localization.GetString("Validation_Required", new object[] { "Email" });
                return BadRequest(ApiResponse.CreateError(msg, "Validation_Required"));
            }
            if (string.IsNullOrWhiteSpace(req.Code))
            {
                var msg = _localization.GetString("Validation_Required", new object[] { "Code" });
                return BadRequest(ApiResponse.CreateError(msg, "Validation_Required"));
            }
            if (string.IsNullOrWhiteSpace(req.NewPassword))
            {
                var msg = _localization.GetString("Validation_Required", new object[] { "NewPassword" });
                return BadRequest(ApiResponse.CreateError(msg, "Validation_Required"));
            }

            if (!_resetCodes.TryGetValue(req.Email, out var data))
            {
                var msg = _localization.GetError("InvalidResetCode");
                return BadRequest(ApiResponse.CreateError(msg, "InvalidResetCode"));
            }

            if (data.Expire < DateTime.UtcNow)
            {
                _resetCodes.Remove(req.Email);
                var msg = _localization.GetError("InvalidResetCode");
                return BadRequest(ApiResponse.CreateError(msg, "InvalidResetCode"));
            }

            if (data.Code != req.Code)
            {
                var msg = _localization.GetError("InvalidResetCode");
                return BadRequest(ApiResponse.CreateError(msg, "InvalidResetCode"));
            }

            var user = await _users.GetByEmailAsync(req.Email);
            if (user == null)
            {
                var msg = _localization.GetError("UserNotFound");
                return NotFound(ApiResponse.CreateError(msg, "UserNotFound"));
            }

            await _users.UpdatePasswordAsync(user.Id, req.NewPassword);
            _resetCodes.Remove(req.Email);

            var successMsg = _localization.GetSuccess("PasswordReset");
            return Ok(ApiResponse.CreateSuccess(successMsg));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [AuthController.ResetPassword] Error: {ex.Message}");
            return StatusCode(500, ApiResponse.CreateError(_localization.GetError("PasswordChangeFailed"), "PasswordChangeFailed"));
        }
    }
}
