using System.Linq;

namespace SimpleECS {
    public abstract partial class System<TTriggerType, TTriggerData> {
        #region Specific Systems

        public abstract class ComponentSystem<TComponent> : System<TTriggerType, TTriggerData>
            where TComponent : Component {
            protected ComponentSystem(Universe<TTriggerType, TTriggerData> universe) : base(universe) {}

            public override void Trigger(TTriggerType trigger, TTriggerData data) {
                Universe.GetComponents<TComponent>()
                    .ForAll(component => OnTrigger(trigger, data, component as TComponent));
            }

            public abstract void OnTrigger(TTriggerType trigger, TTriggerData data, TComponent component);
        }

        public abstract class EntitySystemByComponent<TComponent> : System<TTriggerType, TTriggerData>
            where TComponent : Component {
            protected EntitySystemByComponent(Universe<TTriggerType, TTriggerData> universe) : base(universe) {}

            public override void Trigger(TTriggerType trigger, TTriggerData data) {
                Universe.GetEntitiesByComponent<TComponent>()
                    .ForAll(entity => OnTrigger(trigger, data, entity));
            }

            public abstract void OnTrigger(TTriggerType trigger, TTriggerData data, Entity entity);
        }

        public abstract class EntitySystemByEntity<TEntity> : System<TTriggerType, TTriggerData> where TEntity : Entity {
            protected EntitySystemByEntity(Universe<TTriggerType, TTriggerData> universe) : base(universe) {}

            public override void Trigger(TTriggerType trigger, TTriggerData data) {
                Universe.GetEntities<TEntity>().ForAll(entity => OnTrigger(trigger, data, entity));
            }

            public abstract void OnTrigger(TTriggerType trigger, TTriggerData data, TEntity entity);
        }

        public abstract class EntitySystem : System<TTriggerType, TTriggerData> {
            protected EntitySystem(Universe<TTriggerType, TTriggerData> universe) : base(universe) {}

            public override void Trigger(TTriggerType trigger, TTriggerData data) {
                Universe.GetEntities().ForAll(entity => OnTrigger(trigger, data, entity));
            }

            public abstract void OnTrigger(TTriggerType trigger, TTriggerData data, Entity entity);
        }

        #endregion
    }
}