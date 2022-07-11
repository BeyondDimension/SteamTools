using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering;

// ReSharper disable ConvertToLambdaExpression

namespace System.Application.UI.Views.Controls
{
    public class Rotator : ContentControl
    {
        // Minimum speed
        private double minimumSpeed = 0.0025;
        private bool _running;

        private double _speed = 0.4;

        private double _rotateDegree = 0;

        private RenderLoop _loop;
        private RenderLoopClock _loopTask;
        private TimeSpan _prev;

        public Rotator()
        {
            // Idk how it does making any sense
            // It works without any hooking / registering / attaching or etc.
            // I just BLOW MY ****ING MIND
            _loop = new RenderLoop();

            // Prepare render loop task for use.
            _loopTask = new RenderLoopClock();
            _loopTask.Subscribe(
                renderTime =>
                {
                    var delta = renderTime - _prev;
                    _rotateDegree += _speed * delta.TotalMilliseconds;
                    _prev = renderTime;

                    while (_rotateDegree > 360)
                        _rotateDegree -= 360;

                    RenderTransform = new RotateTransform(_rotateDegree);
                });
        }

        public double Speed
        {
            get => _speed;
            set => SetAndRaise(SpeedProperty, ref _speed, value);
        }

        public static readonly DirectProperty<Rotator, double> SpeedProperty =
            AvaloniaProperty.RegisterDirect(nameof(Speed),
                rotator => rotator._speed,
                (Rotator rotator, double v) =>
                {
                    rotator._speed = v;
                    OnSpeedChanged(rotator, v);
                });

        // Loop dispatcher / simple loop controller
        private static void OnSpeedChanged(Rotator rotator, double d)
        {
            // We should stop rotator if speed is lower than minimum speed
            if (Math.Abs(d) < rotator.minimumSpeed)
            {
                // Stop render loop to avoid resources waste.
                if (!rotator._running)
                    return;

                // Reset statements
                rotator._running = false;
                rotator._rotateDegree = 0;

                // Detach loop task from RenderLoop
                rotator._loop.Remove(rotator._loopTask);

                return;
            }

            if (rotator._running)
                return;

            rotator._running = true;
            // Attach loop task to RenderLoop
            rotator._loop.Add(rotator._loopTask);
        }
    }
}