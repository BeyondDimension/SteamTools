using MessagePack;
using System.Application.Entities;
using System.Linq;
using System.Properties;

namespace System.Application.Services.Implementation
{
    internal sealed class AreaResourceImpl<TArea> : IAreaResource<TArea>, IAreaResourceHelper<TArea> where TArea : class, IArea
    {
        TArea[]? areas;

        public TArea[] GetAll()
        {
            if (areas == null)
            {
                areas = MessagePackSerializer.Deserialize<TArea[]>(SR.AMap_adcode_citycode_20210406_xlsx_mpo);
            }
            return areas;
        }

        const int DefaultSelectionId = 110000;

        TArea? mDefaultSelection;

        public TArea DefaultSelection
        {
            get
            {
                if (mDefaultSelection == null)
                {
                    mDefaultSelection = GetAll().FirstOrDefault(x => x.Id == DefaultSelectionId);
                }
                return mDefaultSelection;
            }
        }
    }
}