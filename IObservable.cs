using System;

namespace FluentSvgXaml
{
    public interface IObservable
    {
        void Cancel();
        void Subscribe(IObserver observer);
    }
}
