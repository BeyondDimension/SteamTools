using System;

namespace Steam4NET
{
    public class CSteamID
    {
        private InteropHelp.BitVector64 steamid;

        public CSteamID()
            : this( 0 )
        {
        }

        public CSteamID( UInt32 unAccountID, EUniverse eUniverse, EAccountType eAccountType )
            : this()
        {
            Set( unAccountID, eUniverse, eAccountType );
        }

        public CSteamID( UInt32 unAccountID, UInt32 unInstance, EUniverse eUniverse, EAccountType eAccountType )
            : this()
        {
            InstancedSet( unAccountID, unInstance, eUniverse, eAccountType );
        }

        public CSteamID( UInt64 id )
        {
            this.steamid = new InteropHelp.BitVector64( id );
        }

        public CSteamID(SteamID_t sid)
            : this(sid.low32Bits, sid.high32Bits & 0xFFFFF, (EUniverse)(sid.high32Bits >> 24), (EAccountType)((sid.high32Bits >> 20) & 0xF))
        {
        }

        public static implicit operator UInt64( CSteamID sid )
        {
            return sid.steamid.Data;
        }

        public static implicit operator CSteamID( UInt64 id )
        {
            return new CSteamID( id );
        }

        public static implicit operator CSteamID(SteamID_t sid)
        {
            return new CSteamID(sid);
        }

        public void Set( UInt32 unAccountID, EUniverse eUniverse, EAccountType eAccountType )
        {
            this.AccountID = unAccountID;
            this.AccountUniverse = eUniverse;
            this.AccountType = eAccountType;

            if ( eAccountType == EAccountType.k_EAccountTypeClan )
            {
                this.AccountInstance = 0;
            }
            else
            {
                this.AccountInstance = 1;
            }
        }

        public void InstancedSet( UInt32 unAccountID, UInt32 unInstance, EUniverse eUniverse, EAccountType eAccountType )
        {
            this.AccountID = unAccountID;
            this.AccountUniverse = eUniverse;
            this.AccountType = eAccountType;
            this.AccountInstance = unInstance;
        }

        public void SetFromUint64( UInt64 ulSteamID )
        {
            this.steamid.Data = ulSteamID;
        }

        public UInt64 ConvertToUint64()
        {
            return this.steamid.Data;
        }

        public bool BBlankAnonAccount()
        {
            return this.AccountID == 0 && BAnonAccount() && this.AccountInstance == 0;
        }
        public bool BGameServerAccount()
        {
            return this.AccountType == EAccountType.k_EAccountTypeGameServer || this.AccountType == EAccountType.k_EAccountTypeAnonGameServer;
        }
        public bool BContentServerAccount()
        {
            return this.AccountType == EAccountType.k_EAccountTypeContentServer;
        }
        public bool BClanAccount()
        {
            return this.AccountType == EAccountType.k_EAccountTypeClan;
        }
        public bool BChatAccount()
        {
            return this.AccountType == EAccountType.k_EAccountTypeChat;
        }
        public bool IsLobby()
        {
            return ( this.AccountType == EAccountType.k_EAccountTypeChat ) && ( ( this.AccountInstance & ( 0x000FFFFF + 1 ) >> 2 ) != 0 );
        }
        public bool BAnonAccount()
        {
            return this.AccountType == EAccountType.k_EAccountTypeAnonUser || this.AccountType == EAccountType.k_EAccountTypeAnonGameServer;
        }
        public bool BAnonUserAccount()
        {
            return this.AccountType == EAccountType.k_EAccountTypeAnonUser;
        }

        public bool IsValid()
        {
            if ( this.AccountType <= EAccountType.k_EAccountTypeInvalid || this.AccountType >= EAccountType.k_EAccountTypeMax )
                return false;

            if ( this.AccountUniverse <= EUniverse.k_EUniverseInvalid || this.AccountUniverse >= EUniverse.k_EUniverseMax )
                return false;

            if ( this.AccountType == EAccountType.k_EAccountTypeIndividual )
            {
                if ( this.AccountID == 0 || this.AccountInstance != 1 )
                    return false;
            }

            if ( this.AccountType == EAccountType.k_EAccountTypeClan )
            {
                if ( this.AccountID == 0 || this.AccountInstance != 0 )
                    return false;
            }

            return true;
        }

        public UInt32 AccountID
        {
            get
            {
                return ( UInt32 )steamid[ 0, 0xFFFFFFFF ];
            }
            set
            {
                steamid[ 0, 0xFFFFFFFF ] = value;
            }
        }

        public UInt32 AccountInstance
        {
            get
            {
                return ( UInt32 )steamid[ 32, 0xFFFFF ];
            }
            set
            {
                steamid[ 32, 0xFFFFF ] = ( UInt64 )value;
            }
        }

        public EAccountType AccountType
        {
            get
            {
                return ( EAccountType )steamid[ 52, 0xF ];
            }
            set
            {
                steamid[ 52, 0xF ] = ( UInt64 )value;
            }
        }

        public EUniverse AccountUniverse
        {
            get
            {
                return ( EUniverse )steamid[ 56, 0xFF ];
            }
            set
            {
                steamid[ 56, 0xFF ] = ( UInt64 )value;
            }
        }

        public string Render()
        {
            switch ( AccountType )
            {
                case EAccountType.k_EAccountTypeInvalid:
                case EAccountType.k_EAccountTypeIndividual:
                    if ( AccountUniverse <= EUniverse.k_EUniversePublic )
                        return String.Format( "STEAM_0:{0}:{1}", AccountID & 1, AccountID >> 1 );
                    else
                        return String.Format( "STEAM_{2}:{0}:{1}", AccountID & 1, AccountID >> 1, ( int )AccountUniverse );
                default:
                    return Convert.ToString( this );
            }
        }

        public override string ToString()
        {
            return Render();
        }

        public override bool Equals( System.Object obj )
        {
            if ( obj == null )
                return false;

            CSteamID sid = obj as CSteamID;
            if ( ( System.Object )sid == null )
                return false;

            return steamid.Data == sid.steamid.Data;
        }

        public bool Equals( CSteamID sid )
        {
            if ( ( object )sid == null )
                return false;

            return steamid.Data == sid.steamid.Data;
        }

        public static bool operator ==( CSteamID a, CSteamID b )
        {
            if ( System.Object.ReferenceEquals( a, b ) )
                return true;

            if ( ( ( object )a == null ) || ( ( object )b == null ) )
                return false;

            return a.steamid.Data == b.steamid.Data;
        }

        public static bool operator !=( CSteamID a, CSteamID b )
        {
            return !( a == b );
        }

        public override int GetHashCode()
        {
            return steamid.Data.GetHashCode();
        }

    }
}