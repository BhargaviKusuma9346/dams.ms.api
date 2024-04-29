using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dams.ms.auth.Adapters
{
    public interface IRequestContext
    {
        int UserId { get; }
        Nullable<Guid> UserGuid { get; }
        string Host { get; }
    }

    public sealed class RequestContextAdapter : IRequestContext
    {
        private readonly IHttpContextAccessor _accessor;
        public RequestContextAdapter(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public int UserId
        {
            get
            {
                try
                {
                    return Convert.ToInt32(_accessor.HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == "UserId")?.Value.ToString() ?? "0");
                }
                catch
                {
                    return 0;
                }
            }
        }

        public Nullable<Guid> UserGuid
        {
            get
            {
                try
                {
                    return Guid.Parse(_accessor.HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == "UserGuid")?.Value.ToString() ?? null);
                }
                catch
                {
                    return null;
                }
            }
        }

        public string Host
        {
            get
            {
                try
                {
                    return _accessor.HttpContext.Request.Host.Value ?? "";
                }
                catch
                {
                    return "";
                }
            }
        }
    }
}
