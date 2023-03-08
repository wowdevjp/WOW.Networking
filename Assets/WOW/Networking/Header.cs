using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WOW.Networking
{
    /// <summary>
    /// ヘッダーのConstants
    /// </summary>
    public class Header
    {
        public const string CacheControl = "Cache-Control";
        public const string NoCache = "no-cache";
        public const string CORS = "Access-Control-Allow-Origin";
        public const string Any = "*";
        public const string ContentType = "Content-Type";
        public const string Accept = "Accept";
    }
}