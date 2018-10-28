using UniRx;
using UnityEngine;

namespace Editor
{
    public class LogicNode : ILogicNode
    {
        private readonly IReadOnlyReactiveProperty<double> _completeFactor;
        private readonly ReactiveProperty<double> _ownFactor = new ReactiveProperty<double>( Random.Range(1,10) );
		
        public ILogicNode Parent { get; private set; }

        public string Name { get; private set; }

        public IReadOnlyReactiveProperty<double> CompleteFactor
        {
            get { return _completeFactor; }
        }

        public ReactiveProperty<double> OwnFactor
        {
            get { return _ownFactor; }
        }

        public LogicNode(ILogicNode parentNode, string name)
        {
            Parent = parentNode;
            Name = name;

            _completeFactor = _ownFactor.CombineLatest(parentNode.CompleteFactor, (own, parent) => own * parent).ToReactiveProperty();
        }
    }
}