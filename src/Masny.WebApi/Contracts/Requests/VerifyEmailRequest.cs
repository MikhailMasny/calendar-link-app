using System.ComponentModel.DataAnnotations;

namespace Masny.WebApi.Contracts.Requests
{
    public class VerifyEmailRequest
    {
        [Required]
        public string Token { get; set; }
    }
}
