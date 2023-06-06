using BD.WTTS.UI.Views.Controls;

namespace BD.WTTS.UI.ViewModels.Abstractions;

public abstract class StepperViewModel : ItemViewModel
{
    public StepperViewModel()
    {
        Stepper.Nexting += StepperOnNexting;
        Stepper.Backing += StepperOnBacking;
        Stepper.Skiping += StepperOnSkiping;
    }

    protected abstract void StepperOnSkiping(Stepper sender, CancelEventArgs args);

    protected abstract void StepperOnBacking(Stepper sender, CancelEventArgs args);

    protected abstract void StepperOnNexting(Stepper sender, CancelEventArgs args);

    protected override void Dispose(bool disposing)
    {
        Stepper.Nexting -= StepperOnNexting;
        Stepper.Backing -= StepperOnBacking;
        Stepper.Skiping -= StepperOnSkiping;
    }
}