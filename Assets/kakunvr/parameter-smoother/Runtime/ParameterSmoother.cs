using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

// ReSharper disable once CheckNamespace
namespace com.kakunvr.parameter_smoother.runtime
{
    [Serializable]
    public struct SmoothingConfig
    {
        public string parameterName;
        public float localSmoothness;
        public float remoteSmoothness;
    }

    [AddComponentMenu("Parameter Smoother")]
    public class ParameterSmoother : MonoBehaviour, IEditorOnly
    {
        public VRCAvatarDescriptor.AnimLayerType layerType = VRCAvatarDescriptor.AnimLayerType.FX;
        public string smoothedSuffix = "/Smoothed";
        public List<SmoothingConfig> configs = new List<SmoothingConfig>();
    }
}
