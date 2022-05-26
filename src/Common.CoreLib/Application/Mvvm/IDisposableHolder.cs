namespace System.Application.Mvvm;

public interface IDisposableHolder : IDisposable
{
    ICollection<IDisposable> CompositeDisposable { get; }
}