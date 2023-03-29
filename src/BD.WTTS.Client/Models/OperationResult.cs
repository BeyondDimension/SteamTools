namespace BD.WTTS.Models;

/// <inheritdoc cref="OperationResultBase{T}"/>
public class OperationResult : OperationResultBase<object>
{
    /// <inheritdoc cref="OperationResultBase{T}.OperationResultBase(OperationResultType)"/>
    public OperationResult()
    {
    }

    /// <inheritdoc cref="OperationResultBase{T}.OperationResultBase(OperationResultType)"/>
    public OperationResult(OperationResultType resultType) : base(resultType)
    {
    }

    /// <inheritdoc cref="OperationResultBase{T}.OperationResultBase(OperationResultType, string)"/>
    public OperationResult(OperationResultType resultType, string message) : base(resultType, message)
    {
    }

    /// <inheritdoc cref="OperationResultBase{T}.OperationResultBase(OperationResultType, string, T)"/>
    public OperationResult(OperationResultType resultType, string message, object? appendData) : base(resultType, message, appendData)
    {
    }

    /// <inheritdoc cref="OperationResultBase{T}.OperationResultBase(OperationResultType, string, string)"/>
    public OperationResult(OperationResultType resultType, string message, string logMessage) : base(resultType, message, logMessage)
    {
    }

    /// <inheritdoc cref="OperationResultBase{T}.OperationResultBase(OperationResultType, string, string, T)"/>
    public OperationResult(OperationResultType resultType, string message, string logMessage, object? appendData) : base(resultType, message, logMessage, appendData)
    {
    }

    public override object AppendData
    {
        get => null!;
        set { }
    }
}

/// <inheritdoc cref="OperationResultBase{T}"/>
public class OperationResult<T> : OperationResultBase<T>
{
    /// <inheritdoc cref="OperationResultBase{T}.OperationResultBase(OperationResultType)"/>
    public OperationResult()
    {
    }

    /// <inheritdoc cref="OperationResultBase{T}.OperationResultBase(OperationResultType)"/>
    public OperationResult(OperationResultType resultType) : base(resultType)
    {
    }

    /// <inheritdoc cref="OperationResultBase{T}.OperationResultBase(OperationResultType, string)"/>
    public OperationResult(OperationResultType resultType, string message) : base(resultType, message)
    {
    }

    /// <inheritdoc cref="OperationResultBase{T}.OperationResultBase(OperationResultType, string, T)"/>
    public OperationResult(OperationResultType resultType, string message, T appendData) : base(resultType, message, appendData)
    {
    }

    /// <inheritdoc cref="OperationResultBase{T}.OperationResultBase(OperationResultType, string, string)"/>
    public OperationResult(OperationResultType resultType, string message, string logMessage) : base(resultType, message, logMessage)
    {
    }

    /// <inheritdoc cref="OperationResultBase{T}.OperationResultBase(OperationResultType, string, string, T)"/>
    public OperationResult(OperationResultType resultType, string message, string logMessage, T appendData) : base(resultType, message, logMessage, appendData)
    {
    }
}