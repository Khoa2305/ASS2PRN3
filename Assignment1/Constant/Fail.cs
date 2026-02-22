using System.Net;

namespace Assignment1.Constant
{
    public class Fail
    {
        public HttpStatusCode HttpStatusCode;
        public int Code;
        public string Message;
        private Fail(HttpStatusCode httpStatusCode, int code, string msg)
        {
            this.HttpStatusCode = httpStatusCode;
            this.Code = code;
            this.Message = msg;
        }
        public static readonly Fail WRONG_ACCOUNT = new(HttpStatusCode.NotFound, 401, "Username or password is not correct");
        public static readonly Fail EXPIRE_TOKEN = new(HttpStatusCode.Unauthorized, 402, "AccessToken is not valid");
        public static readonly Fail FAIL_GET_ARTICLE = new(HttpStatusCode.NotFound, 403, "Can not find out by ID");

        public static readonly Fail FAIL_UPDATE = new(HttpStatusCode.Processing, 404, "Cannot update");
        public static readonly Fail FAIL_GET_ACCOUNTS = new(HttpStatusCode.Processing, 405, "Cannot get accounts");
        public static readonly Fail FAIL_GET_ACCOUNT_DETAIL = new(HttpStatusCode.NotFound, 406, "Cannot find by id");
        public static readonly Fail EMAIL_IS_EXISTED = new(HttpStatusCode.BadRequest, 407, "Email is not valid or existed");
        public static readonly Fail ARTICAL_OWN_ACCOUTN = new(HttpStatusCode.BadRequest, 408, "Acconut owned an article");
        public static readonly Fail FAIL_GET_CATEGORY = new(HttpStatusCode.NotFound, 409, "Cannot get category");
        public static readonly Fail CATEGORY_OWN_ARTICLE = new(HttpStatusCode.NotFound, 410, "Cannot delete categry");
        public static readonly Fail FAIL_GET_TAG = new(HttpStatusCode.NotFound, 411, "Cannot get tag");
        public static readonly Fail TAG_OWN_ARTICLE = new(HttpStatusCode.BadRequest, 412, "Tag used in articles");
        public static readonly Fail NO_PERMISSION = new(HttpStatusCode.Forbidden, 413, "You do not have permission to perform this action");
        public static readonly Fail WRONG_PASSWORD = new(HttpStatusCode.Forbidden, 414, "Password is not correct");
        public static readonly Fail EXISTED_TAG = new(HttpStatusCode.Forbidden, 415, "Tag is existed");
    }
}
