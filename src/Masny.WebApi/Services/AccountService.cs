using AutoMapper;
using Masny.WebApi.Contracts.Requests;
using Masny.WebApi.Contracts.Responses;
using Masny.WebApi.Data;
using Masny.WebApi.Entities;
using Masny.WebApi.Helpers;
using Masny.WebApi.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BC = BCrypt.Net.BCrypt;

namespace Masny.WebApi.Services
{
    public class AccountService : IAccountService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly AppSettings _appSettings;
        private readonly IEmailService _emailService;

        public AccountService(DataContext context,
                              IMapper mapper,
                              IOptions<AppSettings> appSettings,
                              IEmailService emailService)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }
            _appSettings = appSettings.Value;

            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public async Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest model)
        {
            Account account = await _context.Accounts.SingleOrDefaultAsync(a => a.Email == model.Email);

            if (account == null
                || !account.IsVerified
                || !BC.Verify(model.Password, account.PasswordHash))
            {
                throw new AppException("Email or password is incorrect");
            }

            string jwtToken = await GenerateJwtTokenAsync(account);
            RefreshToken refreshToken = await GenerateRefreshTokenAsync();

            account.RefreshTokens.Add(refreshToken);
            _context.Update(account);
            await _context.SaveChangesAsync();

            AuthenticateResponse response = _mapper.Map<AuthenticateResponse>(account);
            response.JwtToken = jwtToken;
            response.RefreshToken = refreshToken.Token;

            return response;
        }

        public async Task<AuthenticateResponse> RefreshTokenAsync(string token)
        {
            (RefreshToken refreshToken, Account account) = await GetRefreshTokenAsync(token);

            var newRefreshToken = await GenerateRefreshTokenAsync();
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            account.RefreshTokens.Add(newRefreshToken);
            _context.Update(account);
            await _context.SaveChangesAsync();

            string jwtToken = await GenerateJwtTokenAsync(account);

            AuthenticateResponse response = _mapper.Map<AuthenticateResponse>(account);
            response.JwtToken = jwtToken;
            response.RefreshToken = newRefreshToken.Token;

            return response;
        }

        public async Task RevokeTokenAsync(string token)
        {
            (RefreshToken refreshToken, Account account) = await GetRefreshTokenAsync(token);

            refreshToken.Revoked = DateTime.UtcNow;
            _context.Update(account);
            await _context.SaveChangesAsync();
        }

        public async Task RegisterAsync(RegisterRequest model, string origin)
        {
            bool alreadyRegistered = await _context.Accounts.AnyAsync(a => a.Email == model.Email);
            if (alreadyRegistered)
            {
                await SendAlreadyRegisteredEmailAsync(model.Email, origin);
                return;
            }

            Account account = _mapper.Map<Account>(model);

            // first registered account is an admin
            //var isFirstAccount = _context.Accounts.Count() == 0;
            //account.Role = isFirstAccount ? AppRoles.Admin : AppRoles.User;

            account.Role = AppRoles.User;
            account.Created = DateTime.UtcNow;
            account.VerificationToken = await RandomTokenStringAsync();
            account.PasswordHash = BC.HashPassword(model.Password);
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();

            await SendVerificationEmailAsync(account, origin);
        }

        public async Task VerifyEmailAsync(string token)
        {
            Account account = await _context.Accounts.SingleOrDefaultAsync(a => a.VerificationToken == token);
            if (account == null)
            {
                throw new AppException("Verification failed");
            }

            account.Verified = DateTime.UtcNow;
            account.VerificationToken = null;
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequest model, string origin)
        {
            Account account = await _context.Accounts.SingleOrDefaultAsync(a => a.Email == model.Email);
            if (account == null)
            {
                return;
            }

            account.ResetToken = await RandomTokenStringAsync();
            account.ResetTokenExpires = DateTime.UtcNow.AddDays(24); // TODO: To constants
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();

            await SendPasswordResetEmailAsync(account, origin);
        }

        public async Task ValidateResetTokenAsync(ValidateResetTokenRequest model)
        {
            Account account = await _context.Accounts.SingleOrDefaultAsync(a =>
                a.ResetToken == model.Token &&
                a.ResetTokenExpires > DateTime.UtcNow);

            if (account == null)
            {
                // TODO: To constants or resources
                throw new AppException("Invalid token");
            }
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest model)
        {
            Account account = await _context.Accounts.SingleOrDefaultAsync(a =>
                a.ResetToken == model.Token &&
                a.ResetTokenExpires > DateTime.UtcNow);

            if (account == null)
            {
                throw new AppException("Invalid token");
            }

            account.PasswordHash = BC.HashPassword(model.Password);
            account.PasswordReset = DateTime.UtcNow;
            account.ResetToken = null;
            account.ResetTokenExpires = null;
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AccountResponse>> GetAllAsync()
        {
            List<Account> accounts = await _context.Accounts.AsNoTracking().ToListAsync();
            return _mapper.Map<IList<AccountResponse>>(accounts);
        }

        public async Task<AccountResponse> GetByIdAsync(int id)
        {
            Account account = await GetAccountByIdAsync(id);
            return _mapper.Map<AccountResponse>(account);
        }

        public async Task<AccountResponse> CreateAsync(CreateRequest model)
        {
            bool alreadyRegistered = await _context.Accounts.AnyAsync(a => a.Email == model.Email);
            if (alreadyRegistered)
            {
                throw new AppException($"Email '{model.Email}' is already registered");
            }

            Account account = _mapper.Map<Account>(model);
            account.Created = DateTime.UtcNow;
            account.Verified = DateTime.UtcNow;
            account.PasswordHash = BC.HashPassword(model.Password);
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();

            return _mapper.Map<AccountResponse>(account);
        }

        public async Task<AccountResponse> UpdateAsync(int id, UpdateRequest model)
        {
            Account account = await GetAccountByIdAsync(id);
            bool alreadyRegistered = await _context.Accounts.AnyAsync(a => a.Email == model.Email);
            if (account.Email != model.Email && alreadyRegistered)
            {
                throw new AppException($"Email '{model.Email}' is already taken");
            }

            if (!string.IsNullOrEmpty(model.Password))
            {
                account.PasswordHash = BC.HashPassword(model.Password);
            }

            _mapper.Map(model, account);
            account.Updated = DateTime.UtcNow;
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();

            return _mapper.Map<AccountResponse>(account);
        }

        public async Task DeleteAsync(int id)
        {
            Account account = await GetAccountByIdAsync(id);
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
        }

        private async Task<Account> GetAccountByIdAsync(int id)
        {
            Account account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                throw new KeyNotFoundException("Account not found");
            }

            return account;
        }

        private async Task<(RefreshToken, Account)> GetRefreshTokenAsync(string token)
        {
            Account account = await _context.Accounts.SingleOrDefaultAsync(a => a.RefreshTokens.Any(rt => rt.Token == token));
            if (account == null)
            {
                throw new AppException("Invalid token");
            }

            var refreshToken = account.RefreshTokens.Single(rt => rt.Token == token);
            if (!refreshToken.IsActive)
            {
                throw new AppException("Invalid token");
            }

            return (refreshToken, account);
        }

        private Task<string> GenerateJwtTokenAsync(Account account)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            byte[] key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", account.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return Task.FromResult(tokenHandler.WriteToken(token));
        }

        private async Task<RefreshToken> GenerateRefreshTokenAsync()
        {
            return new RefreshToken
            {
                Token = await RandomTokenStringAsync(),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow
            };
        }

        private Task<string> RandomTokenStringAsync()
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[40];
            rngCryptoServiceProvider.GetBytes(randomBytes);

            return Task.FromResult(BitConverter.ToString(randomBytes).Replace("-", ""));
        }

        private async Task SendVerificationEmailAsync(Account account, string origin)
        {
            string message;
            if (!string.IsNullOrEmpty(origin))
            {
                var verifyUrl = $"{origin}/account/verify-email?token={account.VerificationToken}";
                message = $@"<p>Please click the below link to verify your email address:</p>
                             <p><a href=""{verifyUrl}"">{verifyUrl}</a></p>";
            }
            else
            {
                message = $@"<p>Please use the below token to verify your email address with the <code>/account/verify-email</code> api route:</p>
                             <p><code>{account.VerificationToken}</code></p>";
            }

            await _emailService.SendAsync(
                to: account.Email,
                subject: "Sign-up Verification API - Verify Email",
                html: $@"<h4>Verify Email</h4>
                         <p>Thanks for registering!</p>
                         {message}"
            );
        }

        private async Task SendAlreadyRegisteredEmailAsync(string email, string origin)
        {
            string message;
            if (!string.IsNullOrEmpty(origin))
            {
                message = $@"<p>If you don't know your password please visit the <a href=""{origin}/account/forgot-password"">forgot password</a> page.</p>";
            }
            else
            {
                message = "<p>If you don't know your password you can reset it via the <code>/account/forgot-password</code> api route.</p>";
            }

            await _emailService.SendAsync(
                to: email,
                subject: "Sign-up Verification API - Email Already Registered",
                html: $@"<h4>Email Already Registered</h4>
                         <p>Your email <strong>{email}</strong> is already registered.</p>
                         {message}"
            );
        }

        private async Task SendPasswordResetEmailAsync(Account account, string origin)
        {
            string message;
            if (!string.IsNullOrEmpty(origin))
            {
                var resetUrl = $"{origin}/account/reset-password?token={account.ResetToken}";
                message = $@"<p>Please click the below link to reset your password, the link will be valid for 1 day:</p>
                             <p><a href=""{resetUrl}"">{resetUrl}</a></p>";
            }
            else
            {
                message = $@"<p>Please use the below token to reset your password with the <code>/account/reset-password</code> api route:</p>
                             <p><code>{account.ResetToken}</code></p>";
            }

            await _emailService.SendAsync(
                to: account.Email,
                subject: "Sign-up Verification API - Reset Password",
                html: $@"<h4>Reset Password Email</h4>
                         {message}"
            );
        }
    }
}
