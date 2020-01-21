﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Physics;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Tests
{
    /// <summary>
    /// A simple pointer that is just used for Unity play mode tests. It doesn't update anything itself -
    /// it is expected that any test using it will manually update data as necessary.
    /// If you would like to setup pointer parameters in data (e.g. a prefab), you can use
    /// <see cref="FocusRaycastTestProxy"/>.
    /// </summary>
    public class TestPointer : GenericPointer
    {
        public TestPointer() : base("Test Pointer", null)
        {
        }

        public override Vector3 Position => throw new System.NotImplementedException();

        public override Quaternion Rotation => throw new System.NotImplementedException();

        public override void OnPreCurrentPointerTargetChange()
        {
        }

        public override void OnPreSceneQuery()
        {
        }

        public override void OnPostSceneQuery()
        {
        }

        /// <summary>
        /// Initialize all applicable settings on this pointer from <paramref name="testProxy"/> and then set this pointer active,
        /// so that it will update its <see cref="Toolkit.Input.IMixedRealityPointer.Result"/> in the next <see cref="Toolkit.Input.FocusProvider.Update"/>.
        /// </summary>
        public void SetFromTestProxy(FocusRaycastTestProxy testProxy)
        {
            if (Rays == null || Rays.Length != testProxy.LineCastResolution)
            {
                Rays = new RayStep[testProxy.LineCastResolution];
            }

            var lineBase = testProxy.RayLineData;

            float stepSize = 1f / Rays.Length;
            Vector3 lastPoint = lineBase.GetUnClampedPoint(0f);
            for (int i = 0; i < Rays.Length; i++)
            {
                Vector3 currentPoint = lineBase.GetUnClampedPoint(stepSize * (i + 1));
                Rays[i].UpdateRayStep(ref lastPoint, ref currentPoint);
                lastPoint = currentPoint;
            }

            PrioritizedLayerMasksOverride = testProxy.PrioritizedLayerMasks;

            IsActive = true;
        }
    }
}
