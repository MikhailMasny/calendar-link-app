using Masny.WebApi.Contracts.Requests;
using Masny.WebApi.Contracts.Responses;
using Masny.WebApi.Helpers;
using Masny.WebApi.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Masny.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : BaseController
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        }

        [HttpPost("authenticate")]
        public async Task<ActionResult<AuthenticateResponse>> AuthenticateAsync(AuthenticateRequest model)
        {
            // TODO: operation result + response
            AuthenticateResponse response = await _accountService.AuthenticateAsync(model);
            await SetTokenCookieAsync(response.RefreshToken);
            return Ok(response);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthenticateResponse>> RefreshTokenAsync()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            AuthenticateResponse response = await _accountService.RefreshTokenAsync(refreshToken);
            await SetTokenCookieAsync(response.RefreshToken);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeTokenAsync(RevokeTokenRequest model)
        {
            var token = !string.IsNullOrEmpty(model.Token)
                ? model.Token
                : Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Token is required" });
            }

            if (!Account.OwnsToken(token) && Account.Role != AppRoles.Admin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            await _accountService.RevokeTokenAsync(token);
            return Ok(new { message = "Token revoked" });
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(RegisterRequest model)
        {
            await _accountService.RegisterAsync(model, Request.Headers["origin"]);
            return Ok(new { message = "Registration successful, please check your email for verification instructions" });
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmailAsync(VerifyEmailRequest model)
        {
            await _accountService.VerifyEmailAsync(model.Token);
            return Ok(new { message = "Verification successful, you can now login" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPasswordAsync(ForgotPasswordRequest model)
        {
            await _accountService.ForgotPasswordAsync(model, Request.Headers["origin"]);
            return Ok(new { message = "Please check your email for password reset instructions" });
        }

        [HttpPost("validate-reset-token")]
        public async Task<IActionResult> ValidateResetTokenAsync(ValidateResetTokenRequest model)
        {
            await _accountService.ValidateResetTokenAsync(model);
            return Ok(new { message = "Token is valid" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPasswordAsync(ResetPasswordRequest model)
        {
            await _accountService.ResetPasswordAsync(model);
            return Ok(new { message = "Password reset successful, you can now login" });
        }

        [Authorize(AppRoles.Admin)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccountResponse>>> GetAllAsync()
        {
            IEnumerable<AccountResponse> accounts = await _accountService.GetAllAsync();
            return Ok(accounts);
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AccountResponse>> GetByIdAsync(int id)
        {
            if (id != Account.Id && Account.Role != AppRoles.Admin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            AccountResponse account = await _accountService.GetByIdAsync(id);
            return Ok(account);
        }

        [Authorize(AppRoles.Admin)]
        [HttpPost]
        public async Task<ActionResult<AccountResponse>> CreateAsync(CreateRequest model)
        {
            AccountResponse account = await _accountService.CreateAsync(model);
            return Ok(account);
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<AccountResponse>> UpdateAsync(int id, UpdateRequest model)
        {
            if (id != Account.Id && Account.Role != AppRoles.Admin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            if (Account.Role != AppRoles.Admin)
            {
                model.Role = null;
            }

            AccountResponse account = await _accountService.UpdateAsync(id, model);
            return Ok(account);
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (id != Account.Id && Account.Role != AppRoles.Admin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            await _accountService.DeleteAsync(id);
            return Ok(new { message = "Account deleted successfully" });
        }

        private Task SetTokenCookieAsync(string token)
        {
            var cookieOptions = new CookieOptions
            {
                //HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("refreshToken", token, cookieOptions);

            return Task.CompletedTask;
        }
    }
}
