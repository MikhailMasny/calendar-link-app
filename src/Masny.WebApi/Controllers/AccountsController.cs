using Masny.WebApi.Contracts.Requests;
using Masny.WebApi.Contracts.Responses;
using Masny.WebApi.Helpers;
using Masny.WebApi.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

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
        public ActionResult<AuthenticateResponse> Authenticate(AuthenticateRequest model)
        {
            // TODO: operation result + response
            var response = _accountService.Authenticate(model);
            SetTokenCookie(response.RefreshToken);
            return Ok(response);
        }

        [HttpPost("refresh-token")]
        public ActionResult<AuthenticateResponse> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var response = _accountService.RefreshToken(refreshToken);
            SetTokenCookie(response.RefreshToken);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("revoke-token")]
        public IActionResult RevokeToken(RevokeTokenRequest model)
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

            _accountService.RevokeToken(token);
            return Ok(new { message = "Token revoked" });
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterRequest model)
        {
            _accountService.Register(model, Request.Headers["origin"]);
            return Ok(new { message = "Registration successful, please check your email for verification instructions" });
        }

        [HttpPost("verify-email")]
        public IActionResult VerifyEmail(VerifyEmailRequest model)
        {
            _accountService.VerifyEmail(model.Token);
            return Ok(new { message = "Verification successful, you can now login" });
        }

        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword(ForgotPasswordRequest model)
        {
            _accountService.ForgotPassword(model, Request.Headers["origin"]);
            return Ok(new { message = "Please check your email for password reset instructions" });
        }

        [HttpPost("validate-reset-token")]
        public IActionResult ValidateResetToken(ValidateResetTokenRequest model)
        {
            _accountService.ValidateResetToken(model);
            return Ok(new { message = "Token is valid" });
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword(ResetPasswordRequest model)
        {
            _accountService.ResetPassword(model);
            return Ok(new { message = "Password reset successful, you can now login" });
        }

        [Authorize(AppRoles.Admin)]
        [HttpGet]
        public ActionResult<IEnumerable<AccountResponse>> GetAll()
        {
            var accounts = _accountService.GetAll();
            return Ok(accounts);
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public ActionResult<AccountResponse> GetById(int id)
        {
            if (id != Account.Id && Account.Role != AppRoles.Admin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            var account = _accountService.GetById(id);
            return Ok(account);
        }

        [Authorize(AppRoles.Admin)]
        [HttpPost]
        public ActionResult<AccountResponse> Create(CreateRequest model)
        {
            var account = _accountService.Create(model);
            return Ok(account);
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public ActionResult<AccountResponse> Update(int id, UpdateRequest model)
        {
            if (id != Account.Id && Account.Role != AppRoles.Admin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            if (Account.Role != AppRoles.Admin)
            {
                model.Role = null;
            }

            var account = _accountService.Update(id, model);
            return Ok(account);
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            if (id != Account.Id && Account.Role != AppRoles.Admin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            _accountService.Delete(id);
            return Ok(new { message = "Account deleted successfully" });
        }

        private void SetTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                //HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }
    }
}
