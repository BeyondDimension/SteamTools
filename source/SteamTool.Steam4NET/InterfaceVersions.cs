using System;
using System.Collections.Generic;

namespace Steam4NET
{
    class InterfaceVersions
    {
        public static string GetInterfaceIdentifier( Type steamClass )
        {
            foreach(InteropHelp.InterfaceVersionAttribute attribute in steamClass.GetCustomAttributes(typeof(InteropHelp.InterfaceVersionAttribute), false))
            {
                return attribute.Identifier;
            }

            throw new Exception("Version identifier not found for class " + steamClass);
        }
    }
}
