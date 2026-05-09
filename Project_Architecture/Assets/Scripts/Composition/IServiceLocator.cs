namespace ProjectArchitecture.Composition
{
    public interface IServiceLocator
    {
        void Register<TService>(TService service) where TService : class;
        bool TryRegister<TService>(TService service) where TService : class;
        TService Get<TService>() where TService : class;
        bool TryGet<TService>(out TService service) where TService : class;
        bool Contains<TService>() where TService : class;
        bool Remove<TService>() where TService : class;
        void Clear();
    }
}
