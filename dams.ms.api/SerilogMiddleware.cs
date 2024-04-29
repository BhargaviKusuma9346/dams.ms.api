using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Fabric;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace Dams.ms.auth
{
    public class SerilogMiddleware
    {
        const string MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

        static readonly ILogger Log = Serilog.Log.ForContext<SerilogMiddleware>();

        readonly RequestDelegate _next;

        static readonly JwtSecurityTokenHandler jwtHandler = new JwtSecurityTokenHandler();

        public SerilogMiddleware(RequestDelegate next)
        {
            if (next == null) throw new ArgumentNullException(nameof(next));
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, IWebHostEnvironment env)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            if (httpContext.Request.Method != "OPTIONS" && !new List<string>() { "/Values/Error", ".js", ".css", ".ico" }.Where(f => httpContext.Request.Path.Value.Contains(f)).Any())
            {
                var sw = Stopwatch.StartNew();
                string header = httpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();
                var tokenClaims = jwtHandler.CanReadToken(header) ? jwtHandler.ReadJwtToken(httpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim())?.Claims.Where(c => c.Type == "UserName").FirstOrDefault() : null;
                var user = tokenClaims != null ? tokenClaims.Value.ToString() : "Anonymous";

                try
                {
                    await _next(httpContext);
                    sw.Stop();

                    var statusCode = httpContext.Response?.StatusCode;
                    var level = statusCode > 499 ? LogEventLevel.Error : LogEventLevel.Information;

                    var log = level == LogEventLevel.Error ? LogForErrorContext(httpContext, user, env)
                                                           : Log.ForContext("User", user)
                                                                .ForContext("Environment", env.EnvironmentName);
                    log.Write(level, MessageTemplate, httpContext.Request.Method, httpContext.Request.Path, statusCode, sw.Elapsed.TotalMilliseconds);
                }
                // Never caught, because `LogException()` returns false.
                catch (Exception ex) when (LogException(httpContext, sw, ex, user, env)) { }
            }
            else { await _next(httpContext); }
        }

        static bool LogException(HttpContext httpContext, Stopwatch sw, Exception ex, string user, IWebHostEnvironment env)
        {
            sw.Stop();
            LogForErrorContext(httpContext, user, env)
                 .Error(ex, MessageTemplate, httpContext.Request.Method, httpContext.Request.Path, 500, sw.Elapsed.TotalMilliseconds);
            return false;
        }

        static ILogger LogForErrorContext(HttpContext httpContext, string user, IWebHostEnvironment env)
        {
            var request = httpContext.Request;

            var result = Log
                .ForContext("RequestHeaders", request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()), destructureObjects: true)
                .ForContext("RequestHost", request.Host)
                .ForContext("RequestProtocol", request.Protocol)
                .ForContext("User", user)
                .ForContext("Environment", env.EnvironmentName);

            if (request.HasFormContentType)
                result = result.ForContext("RequestForm", request.Form.ToDictionary(v => v.Key, v => v.Value.ToString()));

            return result;
        }

        public static void SeqLog(Exception ex)
        {
            var log = Log.ForContext("MachineName", Dns.GetHostName())
                         .ForContext("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
                         .ForContext("Error", ex.Message);
            log.Error(ex, "Exception occured in `Dams.ms.auth`");
        }
    }
}
