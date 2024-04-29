using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Dams.ms.auth.Data
{

    public class Response
    {
        public object result { get; set; }
    }

    #region Responses
    public class SuccessResponse : JsonResult
    {
        private readonly HttpStatusCode _statusCode;

        public SuccessResponse(object json) : this(json, HttpStatusCode.OK)
        {
        }

        public SuccessResponse(object json, HttpStatusCode statusCode) : base(new SuccessResult(json))
        {
            _statusCode = statusCode;
        }

        public SuccessResponse(object json, string message, HttpStatusCode statusCode) : base(new SuccessResult(json, message))
        {
            _statusCode = statusCode;
        }

        public override void ExecuteResult(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = (int)_statusCode;
            base.ExecuteResult(context);
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = (int)_statusCode;
            return base.ExecuteResultAsync(context);
        }
    }

    public class SuccessRouteResponse : JsonResult
    {
        private readonly HttpStatusCode _statusCode;

        public SuccessRouteResponse(object json, string route, string message, HttpStatusCode statusCode) : base(new SuccessRouteResult(json, route, message))
        {
            _statusCode = statusCode;
        }

        public override void ExecuteResult(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = (int)_statusCode;
            base.ExecuteResult(context);
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = (int)_statusCode;
            return base.ExecuteResultAsync(context);
        }
    }

    public class ErrorResponse : JsonResult
    {
        private readonly HttpStatusCode _statusCode;

        public ErrorResponse(string message) : this(message, HttpStatusCode.InternalServerError)
        {
        }

        public ErrorResponse(string message, HttpStatusCode statusCode) : base(new ErrorResult(message))
        {
            _statusCode = statusCode;
        }

        public ErrorResponse(object json, string message, HttpStatusCode statusCode) : base(new ErrorResult(json, message))
        {
            _statusCode = statusCode;
        }

        public override void ExecuteResult(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = (int)_statusCode;
            base.ExecuteResult(context);
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = (int)_statusCode;
            return base.ExecuteResultAsync(context);
        }
    }
    #endregion

    #region Result Classes

    public class ErrorResult
    {
        public string status { get; set; } = "error";
        public string message { get; set; }
        public object result { get; set; }

        public ErrorResult(string message)
        {
            this.message = message;
        }

        public ErrorResult(object result, string message)
        {
            this.result = result;
            this.message = message;
        }
    }

    public class SuccessResult
    {
        public string status { get; set; } = "success";
        public string message { get; set; }
        public object result { get; set; }

        public SuccessResult(object result)
        {
            this.result = result;
        }

        public SuccessResult(object result, string message)
        {
            this.result = result;
            this.message = message;
        }
    }

    public class SuccessRouteResult
    {
        public string status { get; set; } = "success";
        public string message { get; set; }
        public string route { get; set; }
        public object result { get; set; }

        public SuccessRouteResult(object result, string route, string message = "")
        {
            this.result = result;
            this.route = route;
            this.message = message;
        }
    }

    #endregion
}
