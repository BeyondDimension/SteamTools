using System;
using System.Collections.Generic;
using System.Text;

namespace System.Application.Models
{
    public class SteamGridItem
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Style { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Thumb { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> Tags { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public SteamGridItemAuthor Author { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public SteamGridItemType GridType { get; set; }
    }

    public class SteamGridItemAuthor
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Steam64 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Avatar { get; set; }
    }

    public class SteamGridItemData
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<SteamGridItem> Data { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> Errors { get; set; }
    }
}
