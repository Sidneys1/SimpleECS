using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SimpleECS {
    public class Universe<TTriggerType, TTriggerData> where TTriggerType : struct {
        private readonly ConcurrentDictionary<Entity, Entity> _entites = new ConcurrentDictionary<Entity, Entity>();
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Entity, Entity>> _categorizedEntities = new ConcurrentDictionary<Type, ConcurrentDictionary<Entity, Entity>>();

        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Component, Component>> _components = new ConcurrentDictionary<Type, ConcurrentDictionary<Component, Component>>();

        private readonly ConcurrentDictionary<System<TTriggerType, TTriggerData>, System<TTriggerType, TTriggerData>> _systems = new ConcurrentDictionary<System<TTriggerType, TTriggerData>, System<TTriggerType, TTriggerData>>();
        private readonly ConcurrentDictionary<TTriggerType, ConcurrentDictionary<System<TTriggerType, TTriggerData>, System<TTriggerType, TTriggerData>>> _categorizedSystems = new ConcurrentDictionary<TTriggerType, ConcurrentDictionary<System<TTriggerType, TTriggerData>, System<TTriggerType, TTriggerData>>>();

        public void AddEntity(Entity e) {
            _entites.TryAdd(e, e);
            foreach (var component in e.Components) {
                _categorizedEntities.GetOrAdd(component.GetType(), x => new ConcurrentDictionary<Entity, Entity>()).TryAdd(e, e);
                _components.GetOrAdd(component.GetType(), x => new ConcurrentDictionary<Component, Component>()).TryAdd(component, component);
            }
        }

        public void RemoveEntity(Entity e) {
            _entites.TryRemove(e, out e);
            foreach (var component in e.Components) {
                _categorizedEntities.GetOrAdd(component.GetType(), x => new ConcurrentDictionary<Entity, Entity>())
                    .TryRemove(e, out e);
                Component c;
                _components.GetOrAdd(component.GetType(), x => new ConcurrentDictionary<Component, Component>())
                    .TryRemove(component, out c);
            }
        }

        public void AddSystem(System<TTriggerType, TTriggerData> s) {
            _systems.TryAdd(s, s);
            foreach (var trigger in s.Triggers)
                _categorizedSystems.GetOrAdd(trigger, x => new ConcurrentDictionary<System<TTriggerType, TTriggerData>, System<TTriggerType, TTriggerData>>()).TryAdd(s, s);
        }

        public void RemoveSystem(System<TTriggerType, TTriggerData> s) {
            _systems.TryRemove(s, out s);
            foreach (var trigger in s.Triggers)
                _categorizedSystems.GetOrAdd(trigger, x => new ConcurrentDictionary<System<TTriggerType, TTriggerData>, System<TTriggerType, TTriggerData>>()).TryRemove(s, out s);
        }

        public IEnumerable<System<TTriggerType, TTriggerData>> GetSystems() => _systems.Select(p=>p.Value);

        public ParallelQuery<Component> GetComponents<TComponentType>() where TComponentType : Component => _components.GetOrAdd(typeof(TComponentType), x => new ConcurrentDictionary<Component, Component>()).AsParallel().Select(p=>p.Value);
        public ParallelQuery<Entity> GetEntitiesByComponent<TComponentType>() where TComponentType : Component => _categorizedEntities.GetOrAdd(typeof(TComponentType), x => new ConcurrentDictionary<Entity, Entity>()).AsParallel().Select(p=>p.Value);
        public ParallelQuery<TEntityType> GetEntities<TEntityType>() where TEntityType : Entity => _entites.AsParallel().Select(p=>p.Value).OfType<TEntityType>();
        public ParallelQuery<Entity> GetEntities() => _entites.AsParallel().Select(p=>p.Value);


        public void Trigger(TTriggerType trigger, TTriggerData data) {
            var systems = _categorizedSystems.GetOrAdd(trigger, x => new ConcurrentDictionary<System<TTriggerType, TTriggerData>, System<TTriggerType, TTriggerData>>()).Select(p=>p.Value).ToList();
            foreach (var system in systems) system.PreTrigger(trigger, data);
            foreach (var system in systems) system.Trigger(trigger, data);
            foreach (var system in systems) system.PostTrigger(trigger, data);
        }
    }
}