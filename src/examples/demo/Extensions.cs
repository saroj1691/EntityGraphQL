using System.Linq.Expressions;
using System.Reflection;
using System;
using System.Linq;

namespace demo
{
    public static class Extensions
    {
        public static Delegate ConvertToDelegate(this MethodInfo methodInfo, object target)
        {
            var parmTypes = methodInfo.GetParameters().Select(parm => parm.ParameterType);
            var parmAndReturnTypes = parmTypes.Append(methodInfo.ReturnType).ToArray();
            var delegateType = Expression.GetDelegateType(parmAndReturnTypes);

            if (methodInfo.IsStatic)
                return methodInfo.CreateDelegate(delegateType);
            return methodInfo.CreateDelegate(delegateType, target);
        }
    }
}
