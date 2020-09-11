using AutoMapper;
using Masny.WebApi.Contracts.Requests;
using Masny.WebApi.Helpers;
using Masny.WebApi.Interfaces;
using Masny.WebApi.Models;
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
        private readonly IMapper _mapper;

        public CalendarController(ICalendarService calendarService,
                                  IMapper mapper)
        {
            _calendarService = calendarService ?? throw new ArgumentNullException(nameof(calendarService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Generate google calendar link.
        /// </summary>
        /// <param name="calendarRequest">Calendar request.</param>
        /// <response code="200">Returns the generated google calendar link.</response>
        /// <response code="400">If the request has wrong data.</response>
        /// <response code="401">Unauthorized.</response>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<CalendarResponse>> GetCalendarLink(CalendarRequest calendarRequest)
        {
            CalendarDto calendarDto = _mapper.Map<CalendarDto>(calendarRequest);
            string link = await _calendarService.GetGoogleCalendarLinkAsync(calendarDto);
            var response = new CalendarResponse
            {
                Link = link
            };

            return Ok(response);
        }
    }
}
