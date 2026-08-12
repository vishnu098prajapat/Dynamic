using System;
using System.Web;
using Unity;
using Unity.Lifetime;

public class DependencyResolver
{
    private static IUnityContainer _container;

    public static void Initialize()
    {
        if (_container == null)
        {
            _container = new UnityContainer();
            RegisterTypes(_container);
        }
    }

    private static void RegisterTypes(IUnityContainer container)
    {
        container.RegisterType<ILoginService, LoginService>(new HierarchicalLifetimeManager());
        container.RegisterType<ITokenService, TokenService>(new HierarchicalLifetimeManager());
    }

    public static T Resolve<T>()
    {
        return _container.Resolve<T>();
    }
}
