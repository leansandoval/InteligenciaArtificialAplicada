using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace QuizCraft.Infrastructure.Services
{
    /// <summary>
    /// Rate limiter para controlar las peticiones a la API de Gemini
    /// Implementa límites por minuto, día y tokens
    /// </summary>
    public class GeminiRateLimiter
    {
        private readonly ILogger<GeminiRateLimiter> _logger;
        private readonly int _requestsPerMinute;
        private readonly int _requestsPerDay;
        private readonly int _tokensPerMinute;

        // Cola de timestamps de peticiones para control por minuto
        private readonly ConcurrentQueue<DateTime> _requestTimestamps = new();
        
        // Contador de peticiones por día
        private int _dailyRequestCount = 0;
        private DateTime _dailyResetTime = DateTime.UtcNow.Date.AddDays(1);

        // Control de tokens por minuto
        private int _tokensUsedInCurrentMinute = 0;
        private DateTime _tokenResetTime = DateTime.UtcNow.AddMinutes(1);

        // Lock para sincronización
        private readonly object _lockObject = new();

        public GeminiRateLimiter(
            ILogger<GeminiRateLimiter> logger,
            int requestsPerMinute,
            int requestsPerDay,
            int tokensPerMinute)
        {
            _logger = logger;
            _requestsPerMinute = requestsPerMinute;
            _requestsPerDay = requestsPerDay;
            _tokensPerMinute = tokensPerMinute;

            _logger.LogInformation(
                "GeminiRateLimiter initialized - RPM: {RPM}, RPD: {RPD}, TPM: {TPM}",
                requestsPerMinute, requestsPerDay, tokensPerMinute);
        }

        /// <summary>
        /// Espera si es necesario para cumplir con los límites de rate
        /// </summary>
        /// <param name="estimatedTokens">Tokens estimados que se usarán en la petición</param>
        /// <returns>Tiempo esperado en milisegundos</returns>
        public async Task<int> WaitIfNeededAsync(int estimatedTokens = 0)
        {
            lock (_lockObject)
            {
                var now = DateTime.UtcNow;

                // Reset diario
                if (now >= _dailyResetTime)
                {
                    _dailyRequestCount = 0;
                    _dailyResetTime = now.Date.AddDays(1);
                    _logger.LogInformation("Daily request counter reset");
                }

                // Reset de tokens por minuto
                if (now >= _tokenResetTime)
                {
                    _tokensUsedInCurrentMinute = 0;
                    _tokenResetTime = now.AddMinutes(1);
                }

                // Verificar límite diario
                if (_dailyRequestCount >= _requestsPerDay)
                {
                    var waitTime = (int)(_dailyResetTime - now).TotalMilliseconds;
                    _logger.LogWarning(
                        "Daily request limit reached ({Limit}). Reset in {Hours}h {Minutes}m",
                        _requestsPerDay,
                        (_dailyResetTime - now).Hours,
                        (_dailyResetTime - now).Minutes);
                    
                    throw new InvalidOperationException(
                        $"Límite diario de peticiones alcanzado ({_requestsPerDay}). " +
                        $"Se restablecerá en {(_dailyResetTime - now).Hours}h {(_dailyResetTime - now).Minutes}m");
                }

                // Limpiar timestamps antiguos (más de 1 minuto)
                while (_requestTimestamps.TryPeek(out var oldestTimestamp))
                {
                    if (now - oldestTimestamp > TimeSpan.FromMinutes(1))
                    {
                        _requestTimestamps.TryDequeue(out _);
                    }
                    else
                    {
                        break;
                    }
                }

                // Verificar límite por minuto
                var requestsInLastMinute = _requestTimestamps.Count;
                if (requestsInLastMinute >= _requestsPerMinute)
                {
                    var oldestRequest = _requestTimestamps.TryPeek(out var oldest) ? oldest : now;
                    var waitTime = (int)(60000 - (now - oldestRequest).TotalMilliseconds);
                    
                    if (waitTime > 0)
                    {
                        _logger.LogWarning(
                            "Rate limit per minute reached ({Current}/{Limit}). Waiting {Wait}ms",
                            requestsInLastMinute, _requestsPerMinute, waitTime);
                        
                        Task.Delay(waitTime).Wait();
                        return waitTime;
                    }
                }

                // Verificar límite de tokens por minuto
                if (estimatedTokens > 0 && _tokensUsedInCurrentMinute + estimatedTokens > _tokensPerMinute)
                {
                    var waitTime = (int)(_tokenResetTime - now).TotalMilliseconds;
                    
                    if (waitTime > 0)
                    {
                        _logger.LogWarning(
                            "Token limit per minute reached ({Current}+{Estimated}/{Limit}). Waiting {Wait}ms",
                            _tokensUsedInCurrentMinute, estimatedTokens, _tokensPerMinute, waitTime);
                        
                        Task.Delay(waitTime).Wait();
                        
                        // Reset después de esperar
                        _tokensUsedInCurrentMinute = 0;
                        _tokenResetTime = DateTime.UtcNow.AddMinutes(1);
                        
                        return waitTime;
                    }
                }

                // Registrar la petición
                _requestTimestamps.Enqueue(now);
                _dailyRequestCount++;
                
                if (estimatedTokens > 0)
                {
                    _tokensUsedInCurrentMinute += estimatedTokens;
                }

                _logger.LogDebug(
                    "Request allowed - Minute: {Min}/{MaxMin}, Day: {Day}/{MaxDay}, Tokens: {Tokens}/{MaxTokens}",
                    _requestTimestamps.Count, _requestsPerMinute,
                    _dailyRequestCount, _requestsPerDay,
                    _tokensUsedInCurrentMinute, _tokensPerMinute);
            }

            return 0;
        }

        /// <summary>
        /// Obtiene estadísticas actuales de uso
        /// </summary>
        public RateLimitStats GetStats()
        {
            lock (_lockObject)
            {
                var now = DateTime.UtcNow;
                
                // Limpiar timestamps antiguos
                while (_requestTimestamps.TryPeek(out var oldestTimestamp))
                {
                    if (now - oldestTimestamp > TimeSpan.FromMinutes(1))
                    {
                        _requestTimestamps.TryDequeue(out _);
                    }
                    else
                    {
                        break;
                    }
                }

                return new RateLimitStats
                {
                    RequestsInLastMinute = _requestTimestamps.Count,
                    RequestsPerMinuteLimit = _requestsPerMinute,
                    DailyRequestCount = _dailyRequestCount,
                    DailyRequestLimit = _requestsPerDay,
                    TokensUsedInCurrentMinute = _tokensUsedInCurrentMinute,
                    TokensPerMinuteLimit = _tokensPerMinute,
                    DailyResetTime = _dailyResetTime,
                    MinuteResetTime = _tokenResetTime
                };
            }
        }
    }

    /// <summary>
    /// Estadísticas de uso del rate limiter
    /// </summary>
    public class RateLimitStats
    {
        public int RequestsInLastMinute { get; set; }
        public int RequestsPerMinuteLimit { get; set; }
        public int DailyRequestCount { get; set; }
        public int DailyRequestLimit { get; set; }
        public int TokensUsedInCurrentMinute { get; set; }
        public int TokensPerMinuteLimit { get; set; }
        public DateTime DailyResetTime { get; set; }
        public DateTime MinuteResetTime { get; set; }

        public double MinuteUsagePercent => RequestsPerMinuteLimit > 0 
            ? (RequestsInLastMinute * 100.0) / RequestsPerMinuteLimit 
            : 0;

        public double DailyUsagePercent => DailyRequestLimit > 0 
            ? (DailyRequestCount * 100.0) / DailyRequestLimit 
            : 0;

        public double TokenUsagePercent => TokensPerMinuteLimit > 0 
            ? (TokensUsedInCurrentMinute * 100.0) / TokensPerMinuteLimit 
            : 0;
    }
}
