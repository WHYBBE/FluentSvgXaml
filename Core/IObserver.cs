using System;

namespace FluentSvgXaml.Core
{
    public interface IObserver
    {
        void OnStarted(IObservable sender);
        void OnCompleted(IObservable sender, bool isSuccessful);
    }
}
