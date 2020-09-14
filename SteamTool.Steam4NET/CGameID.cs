using System;

namespace Steam4NET
{
    public class CGameID
    {
        public enum EGameID
        {
            k_EGameIDTypeApp = 0,
            k_EGameIDTypeGameMod = 1,
            k_EGameIDTypeShortcut = 2,
            k_EGameIDTypeP2P = 3
        }

        private InteropHelp.BitVector64 gameid;

        public CGameID()
            : this( ( ulong )0 )
        {
        }
        public CGameID( UInt64 id )
        {
            gameid = new InteropHelp.BitVector64( id );
        }
        public CGameID( Int32 nAppID )
            : this()
        {
        }
        public CGameID(GameID_t gid)
            : this()
        {
            this.AppID = gid.m_nAppID & 0xFFFFFF;
            this.AppType = (EGameID)gid.m_nType;
            this.ModID = gid.m_nModID;
        }

        public static implicit operator UInt64( CGameID gid )
        {
            return gid.gameid.Data;
        }

        public static implicit operator CGameID( UInt64 id )
        {
            return new CGameID( id );
        }

        public static implicit operator CGameID(GameID_t gid)
        {
            return new CGameID(gid);
        }

        public UInt32 AppID
        {
            get
            {
                return ( UInt32 )gameid[ 0, 0xFFFFFF ];
            }
            set
            {
                gameid[ 0, 0xFFFFFF ] = ( UInt64 )value;
            }
        }
        public EGameID AppType
        {
            get
            {
                return ( EGameID )gameid[ 24, 0xFF ];
            }
            set
            {
                gameid[ 24, 0xFF ] = ( UInt64 )value;
            }
        }
        public UInt32 ModID
        {
            get
            {
                return ( UInt32 )gameid[ 32, 0xFFFFFFFF ];
            }
            set
            {
                gameid[ 32, 0xFFFFFFFF ] = ( UInt64 )value;
            }
        }

        public UInt64 ConvertToUint64()
        {
            return gameid.Data;
        }

        public override bool Equals( System.Object obj )
        {
            if ( obj == null )
                return false;

            CGameID gid = obj as CGameID;
            if ( ( System.Object )gid == null )
                return false;

            return gameid.Data == gid.gameid.Data;
        }

        public bool Equals( CGameID gid )
        {
            if ( ( object )gid == null )
                return false;

            return gameid.Data == gid.gameid.Data;
        }

        public static bool operator ==( CGameID a, CGameID b )
        {
            if ( System.Object.ReferenceEquals( a, b ) )
                return true;

            if ( ( ( object )a == null ) || ( ( object )b == null ) )
                return false;

            return a.gameid.Data == b.gameid.Data;
        }

        public static bool operator !=( CGameID a, CGameID b )
        {
            return !( a == b );
        }

        public override int GetHashCode()
        {
            return gameid.GetHashCode();
        }
    }
}