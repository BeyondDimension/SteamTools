using Avalonia.Controls;

namespace BD.WTTS.UI.Views.Controls;

// Click and keyboard activation execute the command; only the service may change the checked state.
sealed class TrayToggleSwitch : ToggleSwitch
{
    public TrayToggleSwitch() { }

    protected override Type StyleKeyOverride => typeof(ToggleSwitch);

    protected override void Toggle() { }
}
