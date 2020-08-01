using Masny.WebApi.Contracts.Requests;
using Masny.WebApi.Contracts.Responses;
using System.Collections.Generic;

namespace Masny.WebApi.Interfaces
{
    public interface IAccountService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model);
        AuthenticateResponse RefreshToken(string token);
        void RevokeToken(string token);
        void Register(RegisterRequest model, string origin);
        void VerifyEmail(string token);
        void ForgotPassword(ForgotPasswordRequest model, string origin);
        void ValidateResetToken(ValidateResetTokenRequest model);
        void ResetPassword(ResetPasswordRequest model);
        IEnumerable<AccountResponse> GetAll();
        AccountResponse GetById(int id);
        AccountResponse Create(CreateRequest model);
        AccountResponse Update(int id, UpdateRequest model);
        void Delete(int id);
    }
}
