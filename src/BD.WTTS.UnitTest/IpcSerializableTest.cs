#pragma warning disable IL2070 // 'this' argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The parameter of method does not have matching annotations.
#pragma warning disable IL2075 // 'this' argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.

using dotnetCampus.Ipc.CompilerServices.Attributes;

namespace BD.WTTS.UnitTest;

public sealed class IpcSerializableTest
{
    static Type GetEmbeddedType(Type type)
    {
        if (type.IsGenericType)
        {
            var definitionType = type.GetGenericTypeDefinition();
            if (definitionType == typeof(ValueTask<>) || definitionType == typeof(Task<>))
            {
                return type.GetGenericArguments()[0];
            }
        }
        if (type.BaseType == typeof(Array))
        {
            var elementType = type.GetElementType();
            if (elementType != null)
            {
                return elementType;
            }
        }
        return type;
    }

    /// <summary>
    /// 测试带有 IpcPublicAttribute 的接口中是否存在不能被序列化的类型
    /// </summary>
    [Test]
    [Ignore("因覆盖不到所有的类型检测而暂时弃用")]
    public void TestSerializable()
    {
        var ignoreTypes = new[]
        {
            "System.Void",
            typeof(Task).FullName,
            typeof(ValueTask).FullName,
        };
        var queryType = from t in typeof(IPCPlatformService).Assembly.GetTypes()
                        where t.IsInterface
                        let attr = t.GetCustomAttribute<IpcPublicAttribute>()
                        where attr != null
                        select t;
        var queryMethods = from t in queryType
                           let m = t.GetMethods()
                           where m.Any_Nullable()
                           select m;
        var queryArgs = (from m in queryMethods.SelectMany(static x => x)
                         let returnType = m.ReturnType
                         let parameters = m.GetParameters().Select(p => p.ParameterType)
                         select (new[] { GetEmbeddedType(returnType) }.Concat(parameters.Select(GetEmbeddedType)), m)).ToArray();
        var args = queryArgs
            .SelectMany(static x => x.Item1)
            .Where(x => !ignoreTypes.Contains(x.FullName))
            .ToHashSet();

        var dict = args.ToDictionary(static x => x,
            y => queryArgs.Where(x => x.Item1.Contains(y)).Select(x => x.m).ToHashSet());

        foreach (var item in dict)
        {
            bool error = false;
            if (item.Key == typeof(object))
            {
                error = true;
            }
            else
            {
                if (item.Key.IsSerializable) continue;
                switch (item.Key.GetTypeCode())
                {
                    case TypeCode.Object:
                        var ctor = item.Key.GetConstructor(BindingFlags.Public, Array.Empty<Type>());
                        error = ctor == null;
                        break;
                    case TypeCode.Empty:
                    case TypeCode.DBNull:
                    case TypeCode.Boolean:
                    case TypeCode.Char:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                    case TypeCode.DateTime:
                    case TypeCode.String:
                        break;
                    default:
                        error = true;
                        break;
                }
            }
            if (error)
            {
                foreach (var m in item.Value)
                {
                    TestContext.WriteLine(m.ToString());
                }
                Assert.Fail(item.Key.FullName);
            }
        }
    }
}
