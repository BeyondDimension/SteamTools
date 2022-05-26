namespace System.Application.Columns;

public interface IResult<T>
{
    T Result { get; set; }
}