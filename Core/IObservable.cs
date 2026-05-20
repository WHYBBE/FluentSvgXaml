using System;

namespace FluentSvgXaml.Core
{
    public interface IObservable
    {
        void Cancel();
        void Subscribe(IObserver observer);
    }
}
