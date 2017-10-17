using System;

namespace MVC{
    public interface IView : IDisposable {
        void Bind();
        void SetController(IController controller);
		void SetController<TController>() where TController : IController, new();
        void SetController(Type controllerType);
        IController CreateController();
        IController CreateController(Type controllerType);
		IController CreateController<TController>() where TController : IController,new();
    }
}