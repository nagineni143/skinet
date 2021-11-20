namespace API.Errors
{
    public class ApiResponse
    {
        public ApiResponse(int statusCode, string message = null)
        {
            StatusCode = statusCode;
            Message = message ?? GetDefaultMessageForStatusCode(statusCode);
        }

        public int StatusCode { get; set; }
        public string Message { get; set; }

        private string GetDefaultMessageForStatusCode(int statusCode)
        {
            return statusCode switch
            {
                400 => "A Bad Request, You have made",
                401 => "Authorized, You are not",
                404 => "Resource found, It was not",
                500 => "Errors are path to the dark side, dark leads to anger, anger leads to hate, hate leads" +
                "to career change",
                _ => null
            };
        }

    }
}
