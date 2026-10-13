namespace Sirstrap.SDK.Weao
{
    public class WeaoException(string message, Exception? innerException = null) : Exception(message, innerException);

    public sealed class WeaoRequestException(string message, HttpStatusCode? statusCode = null, Exception? innerException = null) : WeaoException(message, innerException)
    {
        public HttpStatusCode? StatusCode { get; } = statusCode;
    }

    public sealed class WeaoRateLimitException(RateLimitInfo? rateLimitInfo) : WeaoException(DescribeRateLimit(rateLimitInfo))
    {

        private static string DescribeRateLimit(RateLimitInfo? rateLimitInfo)
        {
            return rateLimitInfo is null ? "WEAO rate limit exceeded (HTTP 429)." : $"WEAO rate limit exceeded; retry in {rateLimitInfo.RemainingTime}s ({rateLimitInfo.RequestsRemaining} requests remaining).";
        }

        public RateLimitInfo? RateLimitInfo { get; } = rateLimitInfo;

        public TimeSpan? RetryAfter => RateLimitInfo is
        {
            RemainingTime: > 0
        } info ? TimeSpan.FromSeconds(info.RemainingTime) : null;
    }
}