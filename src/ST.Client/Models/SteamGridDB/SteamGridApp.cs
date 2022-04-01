using System;
using System.Collections.Generic;
using System.Text;

namespace System.Application.Models
{
    public class SteamGridApp
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> Types { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Verified { get; set; }
    }

    public class SteamGridAppData
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public SteamGridApp Data { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> Errors { get; set; }
    }
}
