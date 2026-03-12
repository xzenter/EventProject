namespace EventProject.Controllers;

/// <summary>
/// Базовый класс с основными возвращаемыми параметрами
/// </summary>
public class ApiBaseResult
{
    /// <summary>
    /// Флаг, указывающий на успешность выполненного запроса
    /// </summary>
    public required bool Success { get; set; }

    /// <summary>
    /// Список ошибок
    /// </summary>
    public List<ApiError> Errors { get; set; } = [];
}

public class ApiVoidResult : ApiBaseResult
{
    public ApiVoidResult()
    {
        Success = true;
    }

    public ApiVoidResult(Exception ex)
    {
        Success = false;
        Errors.Add(new ApiError(ex));
    }

    public ApiVoidResult(params ApiError[] errors)
    {
        Success = false;
        Errors.AddRange(errors);
    }
}

/// <summary>
/// Базовый класс с основными возвращаемыми параметрами
/// </summary>
public class ApiResult<T> : ApiBaseResult where T : class
{
    public ApiResult()
    {
    }

    public ApiResult(T? data)
    {
        Success = true;
        Data = data;
    }

    public ApiResult(Exception ex)
    {
        Success = false;
        Errors.Add(new ApiError(ex));
    }

    public ApiResult(params ApiError[] errors)
    {
        Success = false;
        Errors.AddRange(errors);
    }

    public required T? Data { get; set; }
}

public class ApiError
{
    public ApiError()
    {
    }

    public ApiError(string message)
    {
        Message = message;
    }

    public ApiError(Exception ex)
    {
        Message = ex.Message;
    }

    public string Message { get; set; } = string.Empty;
}