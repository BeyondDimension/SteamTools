using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;

namespace Steam4NET
{
    public interface ICallback
    {
        void Run( IntPtr param );
    }

    public interface IAPICallback : ICallback
    {
        int GetExpectedSize();
        int GetExpectedCallback();
        void ClearAPICallHandle();
    }

    public class Callback<CallbackType> : ICallback
    {
        public delegate void DispatchDelegate( CallbackType param );
        public event DispatchDelegate OnRun;

        public Callback()
        {
            CallbackDispatcher.RegisterCallback( this, CallbackIdentities.GetCallbackIdentity( typeof(CallbackType) ) );
        }

        public Callback( DispatchDelegate myFunc)
            : this()
        {
            this.OnRun += myFunc;
        }

        ~Callback()
        {
            UnRegister();
        }

        public void UnRegister()
        {
            CallbackDispatcher.UnRegisterCallback( this, CallbackIdentities.GetCallbackIdentity( typeof(CallbackType) ) );
        }

        public void Run( IntPtr pubParam )
        {
            if ( this.OnRun != null )
                this.OnRun( ( CallbackType )Marshal.PtrToStructure( pubParam, typeof( CallbackType ) ) );
        }
    }

    public class APICallCallback<CallbackType> :  IAPICallback
    {
        private int callback;
        private int size;
        private UInt64 callhandle = 0;

        public delegate void APIDispatchDelegate( ulong callHandle, CallbackType param );
        public event APIDispatchDelegate OnRun;

        public APICallCallback()
        {
            callback = CallbackIdentities.GetCallbackIdentity( typeof(CallbackType) );
            size = Marshal.SizeOf( typeof(CallbackType) );
        }

        public APICallCallback( APIDispatchDelegate myFunc )
            : this()
        {
            this.OnRun += myFunc;
        }

        public APICallCallback( APIDispatchDelegate myFunc, UInt64 apicallhandle )
            : this( myFunc )
        {
            SetAPICallHandle( apicallhandle );
        }

        ~APICallCallback()
        {
            ClearAPICallHandle();
        }

        public void SetAPICallHandle( UInt64 newcallhandle )
        {
            if ( callhandle != 0 )
                ClearAPICallHandle();

            callhandle = newcallhandle;
            CallbackDispatcher.RegisterAPICallCallback( this, newcallhandle );
        }

        public void ClearAPICallHandle()
        {
            CallbackDispatcher.ClearAPICallCallback( this, callhandle );
        }

        public void Run( IntPtr pubParam )
        {
            if ( this.OnRun != null )
                this.OnRun( callhandle, ( CallbackType )Marshal.PtrToStructure( pubParam, typeof( CallbackType ) ) );
        }

        public int GetExpectedSize()
        {
            return size;
        }

        public int GetExpectedCallback()
        {
            return callback;
        }
    }

    public class CallbackUnhandled
    {
        public delegate void DispatchDelegate( CallbackMsg_t msg );
        public event DispatchDelegate OnRun;

        public CallbackUnhandled()
        {
            CallbackDispatcher.SetUnhandledCallback( this );
        }

        public CallbackUnhandled( DispatchDelegate myFunc )
            : this()
        {
            this.OnRun += myFunc;
        }

        public void Run( CallbackMsg_t msg )
        {
            if ( this.OnRun != null )
                this.OnRun( msg );
        }
    }

    public class CallbackDispatcher
    {
        private static Dictionary<int, ICallback> registeredCallbacks = new Dictionary<int, ICallback>();
        private static Dictionary<UInt64, IAPICallback> registeredAPICallbacks = new Dictionary<UInt64, IAPICallback>();
        private static CallbackUnhandled unhandledCallback = null;

        private static Dictionary<int, Thread> managedThreads = new Dictionary<int, Thread>();

        public static int LastActivePipe { get; private set; }
        public static Callback<SteamAPICallCompleted_t> APICallbackCompleted = new Callback<SteamAPICallCompleted_t>(RunAPICallback);


        public static void RegisterCallback( ICallback callback, int iCallback )
        {
            registeredCallbacks.Add( iCallback, callback );
        }

        public static void UnRegisterCallback( ICallback callback, int iCallback )
        {
            if( registeredCallbacks[iCallback] == callback )
            {
                registeredCallbacks.Remove(iCallback);
            }
        }

        public static void RegisterAPICallCallback( IAPICallback callback, UInt64 callhandle )
        {
            registeredAPICallbacks.Add( callhandle, callback );
        }

        public static void ClearAPICallCallback( IAPICallback callback, UInt64 callhandle )
        {
            registeredAPICallbacks.Remove( callhandle );
        }

        public static void SetUnhandledCallback( CallbackUnhandled callback )
        {
            unhandledCallback = callback;
        }

        public static void RunCallbacks( int pipe )
        {
            CallbackMsg_t callbackmsg = new CallbackMsg_t();

            if ( Steamworks.GetCallback( pipe, ref callbackmsg ) )
            {
                LastActivePipe = pipe;

                ICallback callback;
                if ( registeredCallbacks.TryGetValue( callbackmsg.m_iCallback, out callback ) )
                {
                    callback.Run( callbackmsg.m_pubParam );
                }
                else if ( unhandledCallback != null )
                {
                    unhandledCallback.Run( callbackmsg );
                }

                Steamworks.FreeLastCallback( pipe );
            }
        }

        public static void RunAPICallback( SteamAPICallCompleted_t apicall )
        {
            IAPICallback apiCallback;

            if (!registeredAPICallbacks.TryGetValue(apicall.m_hAsyncCall, out apiCallback))
                return;

            IntPtr pData = IntPtr.Zero;

            try
            {
                bool bFailed = false;
                pData = Marshal.AllocHGlobal( apiCallback.GetExpectedSize() );

                if ( !Steamworks.GetAPICallResult( LastActivePipe, apicall.m_hAsyncCall, pData, apiCallback.GetExpectedSize(), apiCallback.GetExpectedCallback(), ref bFailed ) )
                    return;

                if ( bFailed )
                    return;

                apiCallback.Run( pData );
            }
            finally
            {
                apiCallback.ClearAPICallHandle();

                Marshal.FreeHGlobal(pData);
            }
        }

        private static void DispatchThread( object param )
        {
            int pipe = ( int )param;

            while ( true )
            {
                RunCallbacks( pipe );
                Thread.Sleep( 1 );
            }
        }

        public static void SpawnDispatchThread( int pipe )
        {
            if ( managedThreads.ContainsKey( pipe ) )
                return;

            Thread dispatchThread = new Thread( DispatchThread );
            dispatchThread.Start( pipe );

            managedThreads[ pipe ] = dispatchThread;
        }

        public static void StopDispatchThread( int pipe )
        {
            Thread dispatchThread;

            if ( managedThreads.TryGetValue( pipe, out dispatchThread ) )
            {
                dispatchThread.Abort();
                dispatchThread.Join( 2500 );

                managedThreads.Remove( pipe );
            }
        }
    }
}