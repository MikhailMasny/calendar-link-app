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

        /// <summary>
        /// Login.
        /// </summary>
        /// <param name="model">Login model.</param>
        /// <response code="200">The operation was successful.</response>
        /// <response code="400">If the request has wrong data.</response>
        /// <response code="500">Internal server error by database.</response>
        [HttpPost("authenticate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthenticateResponse>> AuthenticateAsync(AuthenticateRequest model)
        {
            AuthenticateResponse response = await _accountService.AuthenticateAsync(model);
            await SetTokenCookieAsync(response.RefreshToken);
            return Ok(response);
        }

        /// <summary>
        /// Refresh token.
        /// </summary>
        /// <response code="200">The operation was successful.</response>
        /// <response code="400">If the request has wrong data.</response>
        /// <response code="500">Internal server error by database.</response>
        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthenticateResponse>> RefreshTokenAsync()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            AuthenticateResponse response = await _accountService.RefreshTokenAsync(refreshToken);
            await SetTokenCookieAsync(response.RefreshToken);
            return Ok(response);
        }

        /// <summary>
        /// Revoke token.
        /// </summary>
        /// <param name="model">Revoke token model.</param>
        /// <response code="200">The operation was successful.</response>
        /// <response code="400">If the request has wrong data.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="500">Internal server error by database.</response>
        [Authorize]
        [HttpPost("revoke-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Registration.
        /// </summary>
        /// <param name="model">Registration model.</param>
        /// <response code="200">The operation was successful.</response>
        /// <response code="400">If the request has wrong data.</response>
        /// <response code="500">Internal server error by database.</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterAsync(RegisterRequest model)
        {
            await _accountService.RegisterAsync(model, Request.Headers["origin"]);
            return Ok(new { message = "Registration successful, please check your email for verification instructions" });
        }

        /// <summary>
        /// Verify email.
        /// </summary>
        /// <param name="model">Verify email model.</param>
        /// <response code="200">The operation was successful.</response>
        /// <response code="400">If the request has wrong data.</response>
        /// <response code="500">Internal server error by database.</response>
        [HttpPost("verify-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> VerifyEmailAsync(VerifyEmailRequest model)
        {
            await _accountService.VerifyEmailAsync(model.Token);
            return Ok(new { message = "Verification successful, you can now login" });
        }

        /// <summary>
        /// Forgot password.
        /// </summary>
        /// <param name="model">Forgot password model.</param>
        /// <response code="200">The operation was successful.</response>
        /// <response code="400">If the request has wrong data.</response>
        /// <response code="500">Internal server error by database.</response>
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ForgotPasswordAsync(ForgotPasswordRequest model)
        {
            await _accountService.ForgotPasswordAsync(model, Request.Headers["origin"]);
            return Ok(new { message = "Please check your email for password reset instructions" });
        }

        /// <summary>
        /// Validate reset token.
        /// </summary>
        /// <param name="model">Validate reset token model.</param>
        /// <response code="200">The operation was successful.</response>
        /// <response code="400">If the request has wrong data.</response>
        /// <response code="500">Internal server error by database.</response>
        [HttpPost("validate-reset-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ValidateResetTokenAsync(ValidateResetTokenRequest model)
        {
            await _accountService.ValidateResetTokenAsync(model);
            return Ok(new { message = "Token is valid" });
        }

        /// <summary>
        /// Reset password.
        /// </summary>
        /// <param name="model">Reset password model.</param>
        /// <response code="200">The operation was successful.</response>
        /// <response code="400">If the request has wrong data.</response>
        /// <response code="500">Internal server error by database.</response>
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResetPasswordAsync(ResetPasswordRequest model)
        {
            await _accountService.ResetPasswordAsync(model);
            return Ok(new { message = "Password reset successful, you can now login" });
        }

        /// <summary>
        /// Get all accounts.
        /// </summary>
        /// <response code="200">Accounts.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="500">Internal server error by database.</response>
        [Authorize(AppRoles.Admin)]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AccountResponse>>> GetAllAsync()
        {
            IEnumerable<AccountResponse> accounts = await _accountService.GetAllAsync();
            return Ok(accounts);
        }

        /// <summary>
        /// Get account by id.
        /// </summary>
        /// <param name="id">Account id.</param>
        /// <response code="200">Account.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="404">If account not found by Id.</response>
        /// <response code="500">Internal server error by database.</response>
        [Authorize]
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AccountResponse>> GetByIdAsync(int id)
        {
            if (id != Account.Id && Account.Role != AppRoles.Admin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            AccountResponse account = await _accountService.GetByIdAsync(id);
            return Ok(account);
        }

        /// <summary>
        /// Create account.
        /// </summary>
        /// <param name="model">Account.</param>
        /// <response code="200">Account created.</response>
        /// <response code="400">If the request has wrong data.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="500">Internal server error by database.</response>
        [Authorize(AppRoles.Admin)]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AccountResponse>> CreateAsync(CreateRequest model)
        {
            AccountResponse account = await _accountService.CreateAsync(model);
            return Ok(account);
        }

        /// <summary>
        /// Update accound.
        /// </summary>
        /// <param name="id">Account id.</param>
        /// <param name="model">Model to update account.</param>
        /// <response code="200">Account updated.</response>
        /// <response code="400">If the request has wrong data.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="404">If account not found by Id.</response>
        /// <response code="500">Internal server error by database.</response>
        [Authorize]
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Delete account.
        /// </summary>
        /// <param name="id">Accound id.</param>
        /// <response code="200">Account deleted.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="404">If account not found by Id.</response>
        /// <response code="500">Internal server error by database.</response>
        [Authorize]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
                Expires = DateTime.UtcNow.AddDays(Constants.TokenExpiresDays)
            };

            Response.Cookies.Append("refreshToken", token, cookieOptions);

            return Task.CompletedTask;
        }
    }
}
