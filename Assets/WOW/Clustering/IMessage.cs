using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WOW.Clustering
{
    public interface IMessage
    {
        void Serialize(ISerializer serializer);
    }
}