using Masny.WebApi.Contracts.Requests;
using Masny.WebApi.Contracts.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Masny.WebApi.Interfaces
{
    public interface IAccountService
    {
        Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest model);

        Task<AuthenticateResponse> RefreshTokenAsync(string token);

        Task RevokeTokenAsync(string token);

        Task RegisterAsync(RegisterRequest model, string origin);

        Task VerifyEmailAsync(string token);

        Task ForgotPasswordAsync(ForgotPasswordRequest model, string origin);

        Task ValidateResetTokenAsync(ValidateResetTokenRequest model);

        Task ResetPasswordAsync(ResetPasswordRequest model);
        
        Task<IEnumerable<AccountResponse>> GetAllAsync();

        Task<AccountResponse> GetByIdAsync(int id);

        Task<AccountResponse> CreateAsync(CreateRequest model);
        
        Task<AccountResponse> UpdateAsync(int id, UpdateRequest model);
        
        Task DeleteAsync(int id);
    }
}
