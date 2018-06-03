//using System;
//using System.Threading.Tasks;

//namespace M.EventBroker
//{
//    public class UnrestrictedThreadPoolRunner : IEventHandlerRunner
//    {
//        private readonly IErrorReporter _errorReporter;

//        public void Run(params Action[] handlers)
//        {
//            foreach(Action handler in handlers)
//            {
//                var handler1 = handler;
//                Task.Run(handler1);
//            }
//        }

//        public void Dispose()
//        {
//        }
//    }
//}
