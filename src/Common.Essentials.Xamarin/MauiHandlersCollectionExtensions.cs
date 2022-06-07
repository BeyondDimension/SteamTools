#if MAUI
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Hosting;

public static class MauiHandlersCollectionExtensions
{
    public static IMauiHandlersCollection ReplaceHandler<TType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TTypeRender>(this IMauiHandlersCollection handlersCollection)
         where TType : IElement
         where TTypeRender : IElementHandler
    {
        var serviceType = typeof(TType);
        var items = handlersCollection.Where(x => x.ServiceType == serviceType).ToArray();
        Array.ForEach(items, x => handlersCollection.Remove(x));
        handlersCollection.AddHandler<TType, TTypeRender>();
        return handlersCollection;
    }
}
#endif