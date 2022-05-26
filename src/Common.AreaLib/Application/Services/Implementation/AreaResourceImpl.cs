using MessagePack;
using System.Application.Entities;
using System.Properties;

namespace System.Application.Services.Implementation;

internal sealed class AreaResourceImpl<TArea> : IAreaResource<TArea>, IAreaResourceHelper<TArea> where TArea : class, IArea
{
    static readonly Lazy<TArea[]> areas = new(() =>
    {
        var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
        var areas = MessagePackSerializer.Deserialize<TArea[]>(SR.AMap_adcode_citycode_20210406_xlsx, lz4Options);
        return areas;
    });

    public TArea[] GetAll() => areas.Value;

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