using System;

namespace FluentSvgXaml
{
    public interface IObserver
    {
        void OnStarted(IObservable sender);
        void OnCompleted(IObservable sender, bool isSuccessful);
    }
}
