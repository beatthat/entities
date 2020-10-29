using BeatThat.Service;
using System;
using System.Collections.Generic;

namespace BeatThat.Entities
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class RegisterEntityServiceAttribute : RegisterServiceAttribute
    {
        public RegisterEntityServiceAttribute(Type serviceInterface = null, int priority = 0, Type[] proxyInterfaces = null) :
        base(serviceInterface, proxyInterfaces: proxyInterfaces, priority: priority)
        { }

        private static Type[] PROXY_INTERFACE_TYPES = new Type[] {
            typeof(HasEntities<>),
            typeof(HasEntityData<>),
            typeof(EntityResolver<>)
        };

        override public void GetProxyInterfaces(Type implType, ICollection<Type> toInterfaces)
        {
            base.GetProxyInterfaces(implType, toInterfaces);
            FindAndAddProxyTypes(implType,
                                 PROXY_INTERFACE_TYPES,
                                 implType.GetInterfaces(),
                                 toInterfaces,
                                 OnProxyInterfaceMissingPolicy.NoAction);
        }
    }
}

