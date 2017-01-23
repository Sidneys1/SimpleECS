using System.Collections.Generic;

namespace SimpleECS {
    public abstract partial class System<TTriggerType, TTriggerData> where TTriggerType : struct {
        protected readonly Universe<TTriggerType, TTriggerData> Universe;
        protected System(Universe<TTriggerType, TTriggerData> universe) {
            Universe = universe;
        }

        public abstract IEnumerable<TTriggerType> Triggers { get; }

        public virtual void PreTrigger(TTriggerType trigger, TTriggerData data) { }
        public abstract void Trigger(TTriggerType trigger, TTriggerData data);
        public virtual void PostTrigger(TTriggerType trigger, TTriggerData data) { }
    }
}