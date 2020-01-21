﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using NUnit.Framework;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Physics
{
    class InterpolatorTests
    {
        [SetUp]
        public void SetUp()
        {
            // The rest of the tests do floating point comparison, so equality comparison must have some
            // fudge factor.
            GlobalSettings.DefaultFloatingPointTolerance = .001;
        }

        [Test]
        public void TestNonLinearInterpolateToNoSpeed()
        {
            Vector3 from = new Vector3(10, 10, 10);
            Vector3 to = new Vector3(100, 100, 100);
            
            // Regardless of the time delta, a zero speed will always snap to the final location.
            Assert.That(Interpolator.NonLinearInterpolateTo(from, to, 0.0f /*deltaTime*/, 0.0f /*speed*/), Is.EqualTo(to));
            Assert.That(Interpolator.NonLinearInterpolateTo(from, to, 100.0f /*deltaTime*/, 0.0f /*speed*/), Is.EqualTo(to));
            Assert.That(Interpolator.NonLinearInterpolateTo(from, to, 1e6f /*deltaTime*/, 0.0f /*speed*/), Is.EqualTo(to));
        }

        [Test]
        public void TestNonLinearInterpolateTo()
        {
            Vector3 from = new Vector3(0, 0, 0);
            Vector3 to = new Vector3(100, 100, 100);

            Assert.That(Interpolator.NonLinearInterpolateTo(from, to, 1.0f /*deltaTime*/, 0.5f /*speed*/),
                Is.EqualTo(new Vector3(50, 50 , 50)));

            Assert.That(Interpolator.NonLinearInterpolateTo(from, to, 2.0f /*deltaTime*/, 0.5f /*speed*/),
                Is.EqualTo(new Vector3(100, 100, 100)));

            Assert.That(Interpolator.NonLinearInterpolateTo(from, to, 3.0f /*deltaTime*/, 0.5f /*speed*/), 
                Is.EqualTo(new Vector3(100, 100, 100)));
        }
    }
}
