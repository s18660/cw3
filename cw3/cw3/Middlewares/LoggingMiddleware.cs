using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cw3.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            var httpMethod = "Method: " + httpContext.Request.Method;
            var httpPath = "Path: " + httpContext.Request.Path;
            var bodyStream = "Body: \n";
            using (var reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8, true, 1024, true))
            {
                bodyStream += await reader.ReadToEndAsync();
            }
            var queryString = "queryString: " + httpContext.Request.QueryString;

            using (StreamWriter sw = File.AppendText("requestsLog.txt"))
            {
                sw.WriteLine(httpMethod);
                sw.WriteLine(httpPath);
                sw.WriteLine(bodyStream);
                sw.WriteLine(queryString);
                sw.WriteLine();
            }

            await _next(httpContext);
        }
    }
}
