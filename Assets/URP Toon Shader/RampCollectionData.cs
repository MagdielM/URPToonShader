using System.Collections.Generic;
using UnityEngine;

namespace ToonShaderURP
{
    public class RampCollectionData : ScriptableObject
    {
        [HideInInspector]
        public string arrayName;
        public int resolution;
        public int height;

        public List<Gradient> ramps = new List<Gradient>();
    }
}