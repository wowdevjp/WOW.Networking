using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WOW.Networking
{
    /// <summary>
    /// StatusCode
    /// </summary>
    public enum StatusCode
    {
        None = 200,
        Success = 200,
        BadRequest = 400,
        UnAuthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        MethodNotAllowed = 405,
        RequestTimeout = 408,
        UnsupportedMediaType = 415,
        TooManyRequests = 429,
        InternalServerError = 500,
        NotImplemented = 501,
        ServiceUnavailable = 503,
    }
}