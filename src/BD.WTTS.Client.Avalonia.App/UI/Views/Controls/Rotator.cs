namespace BD.WTTS.UI.Views.Controls;

public class Rotator : Panel
{
    private static IRenderLoop? _loopInstance;

    private static IRenderLoop LoopInstance => _loopInstance ??= new RenderLoop();

    // Minimum speed
    // Rotator will stop if speed less than this constant value.
    // It is able to change in code-behind if this value is not satisfied.
    private static double _minimumSpeed = 0.0025;

    // ReSharper disable once MemberCanBePrivate.Global
    public static double MinimumSpeed
    {
        get => _minimumSpeed;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value),
                    "MinimumSpeed should not less than zero. You can set it as zero, if you wish your rotator keep running.");

            _minimumSpeed = value;
        }
    }

    private bool _running;

    private double _speed = 0.4;

    private double _rotateDegree;

    private readonly RenderLoopClock _loopTask;
    private TimeSpan _prev;

    public Rotator()
    {
        _loopInstance ??= AvaloniaLocator.Current.GetService<IRenderLoop>() ?? LoopInstance;

        // Prepare render loop task for use.
        _loopTask = new RenderLoopClock();
        _loopTask.Subscribe(OnLoopUpdate);
    }

    public double Speed
    {
        get => _speed;
        set => SetAndRaise(SpeedProperty, ref _speed, value);
    }

    public static readonly DirectProperty<Rotator, double> SpeedProperty =
        AvaloniaProperty.RegisterDirect(nameof(Speed),
            delegate (Rotator rotator) { return rotator._speed; },
            delegate (Rotator rotator, double v)
            {
                if (rotator.IsEffectivelyVisible == false || rotator.IsEffectivelyEnabled == false)
                    return;

                rotator._speed = v;
                OnSpeedChanged(rotator, v);
            });

    // Loop dispatcher / simple loop controller
    private void OnLoopUpdate(TimeSpan renderTime)
    {
        if (IsEffectivelyVisible == false || IsEffectivelyEnabled == false)
            return;

        var delta = renderTime - _prev;
        _rotateDegree += _speed * delta.TotalMilliseconds;
        _prev = renderTime;

        while (_rotateDegree > 360)
            _rotateDegree -= 360;

        RenderTransform = new RotateTransform(_rotateDegree);
    }

    private static void OnSpeedChanged(Rotator rotator, double d)
    {
        // We should stop rotator if speed is lower than minimum speed
        if (Math.Abs(d) < _minimumSpeed)
        {
            // Stop render loop to avoid leak of performances.
            if (!rotator._running)
                return;

            // Reset statements
            rotator._running = false;
            rotator._rotateDegree = 0;

            // Detach loop task from RenderLoop
            LoopInstance.Remove(rotator._loopTask);

            return;
        }

        if (rotator._running)
            return;

        // Attach loop task to RenderLoop if not running.
        rotator._running = true;
        LoopInstance.Add(rotator._loopTask);
    }
}