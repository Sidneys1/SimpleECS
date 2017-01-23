using System.Collections.Generic;

namespace SimpleECS {
    public abstract class Entity {
        public abstract IEnumerable<Component> Components { get; }
    }
}