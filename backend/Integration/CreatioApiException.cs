namespace CreatioAccounts.Api.Integration;

public class CreatioApiException : Exception
{
    public int StatusCode { get; }

    public CreatioApiException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}