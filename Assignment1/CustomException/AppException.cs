using Assignment1.Constant;
using System.Net;

namespace Assignment1.CustomException
{
    public class AppException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }

        public AppException(HttpStatusCode statusCode, int code, string msg)
        {
            StatusCode = statusCode;
            Code = code;
            Message = msg;
        }
        public AppException(Fail fail)
        {
            StatusCode = fail.HttpStatusCode;
            Code = fail.Code;
            Message = fail.Message;
        }
    }
}
