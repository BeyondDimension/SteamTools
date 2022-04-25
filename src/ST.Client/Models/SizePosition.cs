using ReactiveUI;
using System.Collections.Generic;
using System.Text;
using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;

namespace System.Application.Models
{
    [MPObj]
    public sealed class SizePosition : ReactiveObject
    {
        int _X;

        [MPKey(0)]
        public int X
        {
            get => _X;
            set => this.RaiseAndSetIfChanged(ref _X, value);
        }

        int _Y;

        [MPKey(1)]
        public int Y
        {
            get => _Y;
            set => this.RaiseAndSetIfChanged(ref _Y, value);
        }

        double _Height;

        [MPKey(2)]
        public double Height
        {
            get => _Height;
            set => this.RaiseAndSetIfChanged(ref _Height, value);
        }

        double _Width;

        [MPKey(3)]
        public double Width
        {
            get => _Width;
            set => this.RaiseAndSetIfChanged(ref _Width, value);
        }
    }
}
