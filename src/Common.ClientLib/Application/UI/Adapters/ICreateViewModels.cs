using System.Collections.Generic;

namespace System.Application.UI.Adapters
{
    public interface ICreateViewModels<TViewModel>
    {
        public IList<TViewModel> CreateViewModels();

        public IList<TViewModel> CreateViewModels(IEnumerable<TViewModel> newViewModels);
    }
}