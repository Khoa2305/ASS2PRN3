using System.Diagnostics;

namespace FUNewsManagement_FE.HttpHandlers
{
    public class LoggingDelegatingHandler : DelegatingHandler
    {
        private readonly ILogger<LoggingDelegatingHandler> _logger;

        public LoggingDelegatingHandler(ILogger<LoggingDelegatingHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                stopwatch.Stop();

                _logger.LogInformation(
                    "HTTP {Method} {RequestUri} responded {StatusCode} in {ElapsedMilliseconds} ms",
                    request.Method,
                    request.RequestUri,
                    (int)response.StatusCode,
                    stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                    ex,
                    "HTTP {Method} {RequestUri} failed after {ElapsedMilliseconds} ms",
                    request.Method,
                    request.RequestUri,
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
