using System.Net;
using System.Text.Json;

namespace ERP.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Beklenmeyen hata: {Message} | Path: {Path} | User: {User}",
                    ex.Message,
                    context.Request.Path,
                    context.User?.Identity?.Name ?? "Anonim");

                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, message) = exception switch
            {
                // İş kuralı ihlalleri (stok yetersiz vb.)
                InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message),

                // Kayıt bulunamadı
                KeyNotFoundException => (HttpStatusCode.NotFound, exception.Message),

                // Yetkisiz erişim
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Bu işlem için yetkiniz yok."),

                // Concurrency çakışması (RowVersion)
                Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException =>
                    (HttpStatusCode.Conflict, "Kayıt başka bir kullanıcı tarafından değiştirildi. Lütfen sayfayı yenileyip tekrar deneyin."),

                // Veritabanı hatası
                Microsoft.EntityFrameworkCore.DbUpdateException =>
                    (HttpStatusCode.BadRequest, "Veritabanı işlemi sırasında hata oluştu."),

                // Diğer tüm hatalar
                _ => (HttpStatusCode.InternalServerError, "Sunucu hatası oluştu. Lütfen daha sonra tekrar deneyiniz.")
            };

            context.Response.StatusCode = (int)statusCode;

            var response = new ErrorResponse
            {
                StatusCode = (int)statusCode,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }

    // Standart hata modeli — frontend bu yapıyı bekleyecek
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
