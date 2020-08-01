using System.Threading.Tasks;

namespace Masny.WebApi.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string html, string from = null);
    }
}
