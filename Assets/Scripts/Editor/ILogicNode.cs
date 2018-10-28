using UniRx;

namespace Editor
{
    public interface ILogicNode
    {
        ILogicNode Parent { get; }
        string Name { get; }
        IReadOnlyReactiveProperty<double> CompleteFactor { get; }
        ReactiveProperty<double> OwnFactor { get; }
    }
}