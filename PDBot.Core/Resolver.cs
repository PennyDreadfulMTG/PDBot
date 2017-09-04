using PDBot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core
{
    public static class Resolver
    {
        private static Dictionary<Type, Type[]> SearchResults = new Dictionary<Type, Type[]>();
        private static Dictionary<Type, object[]> Instances = new Dictionary<Type, object[]>();

        public static Type[] GetImplementations<T>()
        {
            if (!SearchResults.ContainsKey(typeof(T)))
            {
                SearchAssembly<T>(typeof(Resolver).Assembly); // Search this assembly for implementations.
            }
            return SearchResults[typeof(T)];
        }

        public static T[] GetInstances<T>()
        {
            if (!Instances.ContainsKey(typeof(T)))
            {
                Instances[typeof(T)] = new object[0];
            }

            var Implementations = GetImplementations<T>();

            if (!Implementations.Any())
            {
                // Try again
                SearchAssembly<T>(Assembly.GetEntryAssembly());
                Implementations = SearchResults[typeof(T)];
            }

            if (Instances[typeof(T)].Length != Implementations.Length)
            {
                var list = new List<object>(Instances[typeof(T)]);
                foreach (var t in Implementations)
                {
                    if (!list.Any(i => i.GetType() == t))
                        list.Add(Activator.CreateInstance(t));
                }
                Instances[typeof(T)] = list.ToArray();
            }

            return Instances[typeof(T)].Cast<T>().ToArray();
        }

        public static void SearchAssembly<T>(Assembly assembly)
        {
            List<Type> found = new List<Type>();

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types;
                Console.WriteLine("Error loading all types.");
                foreach (var le in e.LoaderExceptions)
                {
                    Console.WriteLine(le);
                }
            }
            foreach (var t in types)
            {
                if (typeof(T).IsAssignableFrom(t) && typeof(T) != t)
                {
                    found.Add(t);
                }
            }
            
            if (SearchResults.ContainsKey(typeof(T)))
            {
                SearchResults[typeof(T)] = found.Union(SearchResults[typeof(T)]).ToArray();
            }
            else
            {
                SearchResults[typeof(T)] = found.ToArray();
            }
        }

        public class Helpers
        {
            public static async Task<IGameObserver[]> GetObservers(IMatch match)
            {
                var observers = await Task.WhenAll(Resolver.GetInstances<IGameObserver>().Select(o => o.GetInstanceForMatchAsync(match)));
                return observers.Where(o => o != null).ToArray();
            }

            public static IChatDispatcher GetChatDispatcher()
            {
                return GetInstances<Core.Interfaces.IChatDispatcher>().Single();
            }
        }
    }
}
