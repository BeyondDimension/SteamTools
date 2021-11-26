using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Application.Models;
using System.Application.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SJsonSerializer = System.Text.Json.JsonSerializer;

namespace System.Application
{
    [TestFixture]
    public class ModelsTest
    {
        static readonly object[] classs;

        static ModelsTest()
        {
            classs = (from m in typeof(ApiResponse).Assembly.GetTypes()
                      let ctors = m.GetConstructors()
                      where m.IsClass && m.IsPublic && !m.IsAbstract && !m.IsGenericType
                      && ctors.Length == 1
                      && ctors[0].GetParameters().Length == 0
                      select Activator.CreateInstance(m)).ToArray();
        }

        [Test]
        public void NewtonsoftJson()
        {
            foreach (var obj in classs)
            {
                try
                {
                    var str = JsonConvert.SerializeObject(obj);
                    var obj2 = JsonConvert.DeserializeObject(str, obj.GetType());
                    var str2 = JsonConvert.SerializeObject(obj2);
                    Assert.IsTrue(str == str2, $"NJson not Equals, type: {obj.GetType()}");
                }
                catch (Exception ex)
                {
                    throw new Exception($"NJson test error, type: {obj.GetType()}", ex);
                }
            }
        }

        [Test]
        public void SystemTextJson()
        {
            foreach (var obj in classs)
            {
                try
                {
                    var str = SJsonSerializer.Serialize(obj);
                    var obj2 = SJsonSerializer.Deserialize(str, obj.GetType());
                    var str2 = SJsonSerializer.Serialize(obj2);
                    Assert.IsTrue(str == str2, $"SJson not Equals, type: {obj.GetType()}");
                }
                catch (Exception ex)
                {
                    throw new Exception($"SJson test error, type: {obj.GetType()}", ex);
                }
            }
        }

        [Test]
        public void MessagePack()
        {
            foreach (var obj in classs)
            {
                try
                {
                    var bytes = Serializable.SMP(obj.GetType(), obj);
                    var obj2 = Serializable.DMP(obj.GetType(), bytes);
                    var bytes2 = Serializable.SMP(obj2);
                    Assert.IsTrue(bytes.SequenceEqual(bytes2), $"MP not Equals, type: {obj.GetType()}");
                }
                catch (Exception ex)
                {
                    throw new Exception($"MP test error, type: {obj.GetType()}", ex);
                }
            }
        }

        [Test]
        public void Enums()
        {
            var appUpdateFailCodes = Enum2.GetAll<ApplicationUpdateFailCode>();
            foreach (var item in appUpdateFailCodes)
            {
                _ = item.ToString2();
            }
        }
    }
}
