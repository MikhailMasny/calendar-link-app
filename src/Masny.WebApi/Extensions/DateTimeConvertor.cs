using Masny.WebApi.Helpers;
using System;

namespace Masny.WebApi.Extensions
{
    public static class DateTimeConvertor
    {
        public static string ToLocalString(this DateTime dateTime)
        {
            return dateTime.AddHours(-3).ToString(Constants.DateTimeFormat);
        }
    }
}
