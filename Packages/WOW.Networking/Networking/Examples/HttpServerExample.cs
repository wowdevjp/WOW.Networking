using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WOW.Networking
{
    public class HttpServerExample : HttpServerBase
    {
        protected override void AddRouters()
        {
            routers.Add(new HttpRouterExample("/example/"));
        }
    }
}