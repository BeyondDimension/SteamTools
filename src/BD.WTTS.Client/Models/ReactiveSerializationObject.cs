namespace BD.WTTS.Models;

/// <summary>
/// <see cref="ReactiveObject"/> 的序列化忽略基类
/// </summary>
public abstract class ReactiveSerializationObject : ReactiveObject
{
    /// <inheritdoc cref="ReactiveObject.Changing" />
    [XmlIgnore, IgnoreDataMember, SystemTextJsonIgnore, NewtonsoftJsonIgnore, MPIgnore, MP2Ignore]
    public new IObservable<IReactivePropertyChangedEventArgs<IReactiveObject>> Changing => base.Changing;

    /// <inheritdoc cref="ReactiveObject.Changed" />
    [XmlIgnore, IgnoreDataMember, SystemTextJsonIgnore, NewtonsoftJsonIgnore, MPIgnore, MP2Ignore]
    public new IObservable<IReactivePropertyChangedEventArgs<IReactiveObject>> Changed => base.Changed;

    /// <inheritdoc cref="ReactiveObject.ThrownExceptions" />
    [XmlIgnore, IgnoreDataMember, SystemTextJsonIgnore, NewtonsoftJsonIgnore, MPIgnore, MP2Ignore]
    public new IObservable<Exception> ThrownExceptions => base.ThrownExceptions;
}