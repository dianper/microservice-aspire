﻿namespace Microservice.Aspire.Api.Services.Responses;

public abstract class ResponseBase
{
    public bool IsValid { get; protected set; }

    public string? Message { get; protected set; }

    public Exception? Exception { get; protected set; }
}
