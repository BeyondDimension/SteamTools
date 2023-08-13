namespace BD.WTTS.Enums;

public enum NavigationTransitionEffect
{
    None = 0,
    //     The exiting page leaves to the right of the panel and the entering page enters
    //     from the left.
    FromLeft,
    //     The exiting page leaves to the left of the panel and the entering page enters
    //     from the right.
    FromRight,
    //     The exiting page fades out and the entering page enters from the top.
    FromTop,
    //     The exiting page fades out and the entering page enters from the bottom.
    FromBottom,

    DrillIn,

    Entrance,
}
