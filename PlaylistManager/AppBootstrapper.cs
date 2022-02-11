using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia;
using PlaylistManager.Models;
using PlaylistManager.Utilities;
using PlaylistManager.Views;
using PlaylistManager.Windows;
using Splat;

namespace PlaylistManager
{
    public class AppBootstrapper
    {
        public AppBootstrapper(Application app)
        {
            Locator.CurrentMutable.RegisterConstant(app, typeof(App));
            // ReSharper disable once ConvertClosureToMethodGroup (Reason: We want PlaylistManager's Assembly)
            Locator.CurrentMutable.RegisterLazySingleton(() => Assembly.GetExecutingAssembly());
            Locator.CurrentMutable.RegisterLazySingleton(DiFactory<PlaylistLibUtils>);
            Locator.CurrentMutable.RegisterLazySingleton(DiFactory<LevelLoader>);
            Locator.CurrentMutable.RegisterLazySingleton(DiFactory<CoverImageLoader>);
            Locator.CurrentMutable.RegisterLazySingleton(ConfigModel.Factory);
            Locator.CurrentMutable.RegisterLazySingleton(DiFactory<MainWindow>);
            Locator.CurrentMutable.RegisterLazySingleton(DiFactory<PlaylistsDetailView>);
            
#if DEBUG
            Locator.CurrentMutable.RegisterConstant(new Benchmarks(), typeof(Benchmarks));
#endif
        }

        public T? DiFactory<T>() => DiFactory<T>(Locator.Current);
        
        // A factory method that gets all args from the type's constructor and resolves its dependencies
        public T? DiFactory<T>(IReadonlyDependencyResolver dependencyResolver)
        {
            var typeInfo = typeof(T);
            var constructors = typeInfo.GetConstructors();
            var constructor = constructors.FirstOrDefault(c => c.GetParameters().Length != 0);
            if (constructor == null)
            {
                return (T?)Activator.CreateInstance(typeof(T));
            }
            List<object?> args = new List<object?>();
            foreach (var argType in constructor.GetParameters())
            {
                args.Add(dependencyResolver.GetService(argType.ParameterType));
            }
            return (T?)Activator.CreateInstance(typeof(T), args.ToArray());
        }
    }
}