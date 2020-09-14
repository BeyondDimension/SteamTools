using System;
using System.Collections.Generic;
using System.Text;

namespace Steam4NET
{
    class CallbackIdentities
    {
        public static int GetCallbackIdentity(Type callbackStruct)
        {
            foreach (InteropHelp.CallbackIdentityAttribute attribute in callbackStruct.GetCustomAttributes(typeof(InteropHelp.CallbackIdentityAttribute), false))
            {
                return attribute.Identity;
            }

            throw new Exception("Callback number not found for struct " + callbackStruct);
        }
    }
}
