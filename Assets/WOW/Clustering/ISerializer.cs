using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WOW.Clustering
{
    public interface ISerializer
    {
        void Serialize<T>(ref T value);
    }
}