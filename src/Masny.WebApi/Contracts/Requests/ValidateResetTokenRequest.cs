using System.ComponentModel.DataAnnotations;

namespace Masny.WebApi.Contracts.Requests
{
    public class ValidateResetTokenRequest
    {
        [Required]
        public string Token { get; set; }
    }
}
