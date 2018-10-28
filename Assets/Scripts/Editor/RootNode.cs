using UniRx;

namespace Editor
{
    public class RootNode : ILogicNode
    {
        private readonly ReactiveProperty<double> _completeFactor = new ReactiveProperty<double>(1);
        private readonly ReactiveProperty<double> _ownFactor = new ReactiveProperty<double>(1);
		
        public ILogicNode Parent
        {
            get { return null; }
        }

        public string Name
        {
            get { return "Root"; }
        }

        public IReadOnlyReactiveProperty<double> CompleteFactor
        {
            get { return _completeFactor; }
        }

        public ReactiveProperty<double> OwnFactor
        {
            get { return _ownFactor; }
        }
    }
}