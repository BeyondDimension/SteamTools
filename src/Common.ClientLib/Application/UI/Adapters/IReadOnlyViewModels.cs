using System.Collections.Generic;

namespace System.Application.UI.Adapters
{
    public interface IReadOnlyViewModels<out TViewModel>
    {
        IReadOnlyList<TViewModel> ViewModels { get; }
    }
}