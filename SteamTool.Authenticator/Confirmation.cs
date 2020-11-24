using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamAuth
{
    public class Confirmation
    {
        /// <summary>
        /// The ID of this confirmation
        /// </summary>
        public ulong ID;

        /// <summary>
        /// The unique key used to act upon this confirmation.
        /// </summary>
        public ulong Key;

        /// <summary>
        /// The value of the data-type HTML attribute returned for this contribution.
        /// </summary>
        public int IntType;

        /// <summary>
        /// Represents either the Trade Offer ID or market transaction ID that caused this confirmation to be created.
        /// </summary>
        public ulong Creator;

        /// <summary>
        /// The type of this confirmation.
        /// </summary>
        public ConfirmationType ConfType;
        
        public Confirmation(ulong id, ulong key, int type, ulong creator)
        {
            this.ID = id;
            this.Key = key;
            this.IntType = type;
            this.Creator = creator;

            //Do a switch simply because we're not 100% certain of all the possible types.
            switch (type)
            {
                case 1:
                    this.ConfType = ConfirmationType.GenericConfirmation;
                    break;
                case 2:
                    this.ConfType = ConfirmationType.Trade;
                    break;
                case 3:
                    this.ConfType = ConfirmationType.MarketSellTransaction;
                    break;
                default:
                    this.ConfType = ConfirmationType.Unknown;
                    break;
            }
        }

        public enum ConfirmationType
        {
            GenericConfirmation,
            Trade,
            MarketSellTransaction,
            Unknown
        }
    }
}
