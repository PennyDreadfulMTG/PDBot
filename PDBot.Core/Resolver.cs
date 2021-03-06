using PDBot.Core.Interfaces;
using PDBot.Core.Tournaments;
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
        private static readonly Dictionary<Type, Type[]> SearchResults = new Dictionary<Type, Type[]>();
        private static readonly Dictionary<Type, object[]> Instances = new Dictionary<Type, object[]>();

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
            lock (Instances)
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
                        {
                            var existing = Instances.SelectMany(i => i.Value).FirstOrDefault(i => i.GetType() == t);
                            if (existing != null)
                                list.Add(existing);
                            else
                                list.Add(Activator.CreateInstance(t));
                        }
                    }
                    Instances[typeof(T)] = list.ToArray();
                }
            }

            return Instances[typeof(T)].Cast<T>().ToArray();
        }

        /// <summary>
        /// Find all implementations of T in the given assembly, and add them to our table
        /// </summary>
        /// <typeparam name="T">An interface</typeparam>
        /// <param name="assembly">The assembly to be searched</param>
        public static void SearchAssembly<T>(Assembly assembly)
        {
            var found = new List<Type>();

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
                if (typeof(T).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                {
                    found.Add(t);
                }
            }

            lock (SearchResults)
            {
                if (SearchResults.ContainsKey(typeof(T)))
                {
                    SearchResults[typeof(T)] = found.Union(SearchResults[typeof(T)]).ToArray();
                }
                else
                {
                    SearchResults[typeof(T)] = found.ToArray();
                }
            }
        }

        public class Helpers
        {
            public static async Task<IGameObserver[]> GetObserversAsync(IMatch match)
            {
                var observers = await Task.WhenAll(GetInstances<IGameObserver>().Select(o => o.GetInstanceForMatchAsync(match))).ConfigureAwait(false);
                return observers.Where(o => o != null).ToArray();
            }

            public static IChatDispatcher GetChatDispatcher()
            {
                return GetInstances<IChatDispatcher>().Single();
            }

            public static ITournamentManager GetTournamentManager()
            {
                return GetInstances<ITournamentManager>().Single();
            }

            internal static IGameList GetGameList()
            {
                return GetInstances<IGameList>().Single();
            }
        }
    }
}
