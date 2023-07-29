using Avalonia.Rendering.Composition;
using FluentAvalonia.UI.Media.Animation;

namespace BD.WTTS.UI;

public static class FrameEffect
{
    public static SlideNavigationTransitionEffect GetEffect(int oldIndex, int index)
    {
        if (oldIndex < 0)
            return SlideNavigationTransitionEffect.FromBottom;

        if (oldIndex > index)
            return SlideNavigationTransitionEffect.FromRight;
        else
            return SlideNavigationTransitionEffect.FromLeft;
    }
}
