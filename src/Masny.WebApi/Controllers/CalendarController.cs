using Masny.WebApi.Contracts.Requests;
using Masny.WebApi.Helpers;
using Masny.WebApi.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Masny.WebApi.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("api/[controller]")]
    public class CalendarController : BaseController
    {
        private readonly ICalendarService _calendarService;

        public CalendarController(ICalendarService calendarService)
        {
            _calendarService = calendarService ?? throw new ArgumentNullException(nameof(calendarService));
        }

        /// <summary>
        /// Generate google calendar link.
        /// </summary>
        /// <param name="calendarRequest">Calendar request.</param>
        /// <response code="200">Returns the generated google calendar link.</response>
        /// <response code="400">If the request has wrong data.</response>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CalendarResponse>> GetCalendarLink(CalendarRequest calendarRequest)
        {
            CalendarResponse response = await _calendarService.GetGoogleCalendarLinkAsync(calendarRequest);
            return Ok(response);
        }
    }
}
