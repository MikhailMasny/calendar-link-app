using Masny.WebApi.Data;
using Masny.WebApi.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masny.WebApi.Middlewares
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AppSettings _appSettings;

        public JwtMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));

            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }
            _appSettings = appSettings.Value;
        }

        public async Task Invoke(HttpContext context, DataContext dataContext)
        {
            var token = context.Request.Headers["Authorization"]
                .FirstOrDefault()?
                .Split(" ")
                .Last();

            if (token != null)
            {
                await AttachAccountToContext(context, dataContext, token);
            }

            await _next(context);
        }

        private async Task AttachAccountToContext(HttpContext context, DataContext dataContext, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var accountId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

                context.Items["Account"] = await dataContext.Accounts.FindAsync(accountId);
            }
            catch
            {
            }
        }
    }
}
