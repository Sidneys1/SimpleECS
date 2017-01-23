using System;

namespace SimpleECS {
    public abstract class SimpleSingleton<TMySinglet> where TMySinglet : SimpleSingleton<TMySinglet> {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly object Lock = new object();
        public static TMySinglet Instance = (TMySinglet)Activator.CreateInstance(typeof(TMySinglet), true);

        protected SimpleSingleton() {
            lock (Lock) {
                if (Instance != null)
                    throw new TypeAccessException("Can not create more than once instance of Singleton.");
            }
        }
    }
}