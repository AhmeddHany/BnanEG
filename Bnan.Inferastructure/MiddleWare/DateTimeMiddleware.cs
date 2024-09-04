using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnan.Inferastructure.MiddleWare
{
    public class DateTimeMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly TimeZoneInfo SaudiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");

        public DateTimeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // إضافة التوقيت المحلي للسعودية إلى عناصر السياق
            context.Items["ServerDateTime"] = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, SaudiTimeZone);
            await _next(context);
        }
    }

    public static class DateTimeMiddlewareExtensions
    {
        public static IApplicationBuilder UseDateTimeMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DateTimeMiddleware>();
        }
    }
}
