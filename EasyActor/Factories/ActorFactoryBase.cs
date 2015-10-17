﻿using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor.Factories
{
    public class ActorFactoryBase
    {
        private static Type _IActorLifeCycleType = typeof(IActorLifeCycle);
        private static readonly ProxyGenerator _Generator;
        static ActorFactoryBase()
        {
            _Generator = new ProxyGenerator();
        }

        protected  T Create<T>(T concrete, ITaskQueue queue) where T : class
        {
             var interceptors = new IInterceptor[] { new QueueDispatcherInterceptor(queue) };
            return  _Generator.CreateInterfaceProxyWithTargetInterface<T>(concrete,  interceptors);
        }

        protected  T CreateIActorLifeCycle<T>(T concrete, ITaskQueue queue, IInterceptor interceptor) where T : class
        {
            var interceptors = new IInterceptor[] { interceptor, new QueueDispatcherInterceptor(queue) };
            return (T)_Generator.CreateInterfaceProxyWithTargetInterface(typeof(T), new Type[] { _IActorLifeCycleType }, concrete, interceptors);
        }
    }
}