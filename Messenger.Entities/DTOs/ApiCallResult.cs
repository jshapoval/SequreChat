using System.Net;

namespace Messenger.Entities.DTOs
{
    public class ApiCallResult<T>
    {
        public T Data { get; set; }
        public HttpStatusCode Status { get; set; }
        public string Details { get; set; }

        public ApiCallResult()
        {
            Status = HttpStatusCode.OK;
        }

        public ApiCallResult(T data)
        {
            Data = data;
            Status = HttpStatusCode.OK;
        }

        public ApiCallResult(HttpStatusCode httpStatusCode, string details)
        {
            Status = httpStatusCode;
            Details = details;
        }
    }
}
