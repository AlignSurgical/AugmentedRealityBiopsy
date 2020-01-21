﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if !WINDOWS_UWP
// When the .NET scripting backend is enabled and C# projects are built
// The assembly that this file is part of is still built for the player,
// even though the assembly itself is marked as a test assembly (this is not
// expected because test assemblies should not be included in player builds).
// Because the .NET backend is deprecated in 2018 and removed in 2019 and this
// issue will likely persist for 2018, this issue is worked around by wrapping all
// play mode tests in this check.

using Microsoft.MixedReality.Toolkit.Experimental.UI;
using Microsoft.MixedReality.Toolkit.UI;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;
using System;
using System.Collections.Generic;

namespace Microsoft.MixedReality.Toolkit.Tests.Experimental
{
    public class ObjectManipulatorTests
    {
        private readonly List<Action> cleanupAction = new List<Action>();

        [SetUp]
        public void Setup()
        {
            PlayModeTestUtilities.Setup();
        }

        [TearDown]
        public void TearDown()
        {
            cleanupAction.ForEach(f => f?.Invoke());

            PlayModeTestUtilities.TearDown();
        }

        /// <summary>
        /// Test creating adding a ObjectManipulator to GameObject programmatically.
        /// Should be able to run scene without getting any exceptions.
        /// </summary>
        [UnityTest]
        public IEnumerator ObjectManipulatorInstantiate()
        {
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testObject.transform.localScale = Vector3.one * 0.2f;

            var manipHandler = testObject.AddComponent<ObjectManipulator>();
            // Wait for two frames to make sure we don't get null pointer exception.
            yield return null;
            yield return null;

            GameObject.Destroy(testObject);
            // Wait for a frame to give Unity a change to actually destroy the object
            yield return null;
        }

        /// <summary>
        /// Test creating ObjectManipulator and receiving hover enter/exit events
        /// from gaze provider.
        /// </summary>
        [UnityTest]
        public IEnumerator ObjectManipulatorGazeHover()
        {
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testObject.transform.localScale = Vector3.one * 0.2f;

            var manipHandler = testObject.AddComponent<ObjectManipulator>();
            int hoverEnterCount = 0;
            int hoverExitCount = 0;

            manipHandler.OnHoverEntered.AddListener((eventData) => hoverEnterCount++);
            manipHandler.OnHoverExited.AddListener((eventData) => hoverExitCount++);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.AreEqual(1, hoverEnterCount, $"ObjectManipulator did not receive hover enter event, count is {hoverEnterCount}");

            testObject.transform.Translate(Vector3.up);

            // First yield for physics. Second for normal frame step.
            // Without first one, second might happen before translation is applied.
            // Without second one services will not be stepped.
            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.AreEqual(1, hoverExitCount, "ObjectManipulator did not receive hover exit event");

            testObject.transform.Translate(5 * Vector3.up);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.IsTrue(hoverExitCount == 1, "ObjectManipulator received the second hover event");

            GameObject.Destroy(testObject);

            // Wait for a frame to give Unity a chance to actually destroy the object
            yield return null;
        }

        /// <summary>
        /// Test creates an object with ObjectManipulator verifies translation with articulated hand
        /// as well as forcefully ending of the manipulation
        /// from gaze provider.
        /// </summary>
        [UnityTest]
        public IEnumerator ObjectManipulatorForceRelease()
        {
            // set up cube with manipulation handler
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testObject.transform.localScale = Vector3.one * 0.2f;
            Vector3 initialObjectPosition = new Vector3(0f, 0f, 1f);
            testObject.transform.position = initialObjectPosition;
            var manipHandler = testObject.AddComponent<ObjectManipulator>();
            manipHandler.HostTransform = testObject.transform;
            manipHandler.SmoothingActive = false;

            // add near interaction grabbable to be able to grab the cube with the simulated articulated hand
            testObject.AddComponent<NearInteractionGrabbable>();

            yield return new WaitForFixedUpdate();
            yield return null;

            // grab the cube - move it to the right 
            var inputSimulationService = PlayModeTestUtilities.GetInputSimulationService();
            int numSteps = 30;
            
            Vector3 handOffset = new Vector3(0, 0, 0.1f);
            Vector3 initialHandPosition = new Vector3(0, 0, 0.5f);
            Vector3 rightPosition = new Vector3(1f, 0f, 1f);

            yield return PlayModeTestUtilities.ShowHand(Handedness.Right, inputSimulationService);
            yield return PlayModeTestUtilities.MoveHandFromTo(initialHandPosition, initialObjectPosition, numSteps, ArticulatedHandPose.GestureId.Open, Handedness.Right, inputSimulationService);
            yield return PlayModeTestUtilities.MoveHandFromTo(initialObjectPosition, rightPosition, numSteps, ArticulatedHandPose.GestureId.Pinch, Handedness.Right, inputSimulationService);

            yield return null;

            // test if the object was properly translated
            float maxError = 0.05f;
            Vector3 posDiff = testObject.transform.position - rightPosition;
            Assert.IsTrue(posDiff.magnitude <= maxError, "ObjectManipulator translate failed");

            // forcefully end manipulation and drag with hand back to original position - object shouldn't move with hand
            manipHandler.ForceEndManipulation();
            yield return PlayModeTestUtilities.MoveHandFromTo(rightPosition, initialObjectPosition, numSteps, ArticulatedHandPose.GestureId.Pinch, Handedness.Right, inputSimulationService);

            posDiff = testObject.transform.position - initialObjectPosition;
            Assert.IsTrue(posDiff.magnitude > maxError, "ObjectManipulator modified objects even though manipulation was forcefully ended.");
            posDiff = testObject.transform.position - rightPosition;
            Assert.IsTrue(posDiff.magnitude <= maxError, "Manipulated object didn't remain in place after forcefully ending manipulation");

            // move hand back to object
            yield return PlayModeTestUtilities.MoveHandFromTo(initialObjectPosition, rightPosition, numSteps, ArticulatedHandPose.GestureId.Open, Handedness.Right, inputSimulationService);

            // grab object again and move to original position
            yield return PlayModeTestUtilities.MoveHandFromTo(rightPosition, initialObjectPosition, numSteps, ArticulatedHandPose.GestureId.Pinch, Handedness.Right, inputSimulationService);

            // test if object was moved by ObjectManipulator
            posDiff = testObject.transform.position - initialObjectPosition;
            Assert.IsTrue(posDiff.magnitude <= maxError, "ObjectManipulator translate failed on valid manipulation after ForceEndManipulation");

            yield return PlayModeTestUtilities.HideHand(Handedness.Right, inputSimulationService);

            GameObject.Destroy(testObject);
        }

        /// <summary>
        /// Test validates throw behavior on manipulation handler. Box with disabled gravity should travel a 
        /// certain distance when being released from grab during hand movement
        /// </summary>
        [UnityTest]
        public IEnumerator ObjectManipulatorThrow()
        {
            // set up cube with manipulation handler
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testObject.transform.localScale = Vector3.one * 0.2f;
            Vector3 initialObjectPosition = new Vector3(0f, 0f, 1f);
            testObject.transform.position = initialObjectPosition;

            var rigidBody = testObject.AddComponent<Rigidbody>();
            rigidBody.useGravity = false;

            var manipHandler = testObject.AddComponent<ObjectManipulator>();
            manipHandler.HostTransform = testObject.transform;
            manipHandler.SmoothingActive = false;

            // add near interaction grabbable to be able to grab the cube with the simulated articulated hand
            testObject.AddComponent<NearInteractionGrabbable>();

            yield return new WaitForFixedUpdate();
            yield return null;

            TestHand hand = new TestHand(Handedness.Right);

            // grab the cube - move it to the right 
            var inputSimulationService = PlayModeTestUtilities.GetInputSimulationService();

            Vector3 handOffset = new Vector3(0, 0, 0.1f);
            Vector3 initialHandPosition = new Vector3(0, 0, 0.5f);
            Vector3 rightPosition = new Vector3(1f, 0f, 1f);

            yield return hand.Show(initialHandPosition);
            yield return hand.MoveTo(initialObjectPosition);
            // Note: don't wait for a physics update after releasing, because it would recompute
            // the velocity of the hand and make it deviate from the rigid body velocity!
            yield return hand.GrabAndThrowAt(rightPosition, false);

            // With simulated hand angular velocity would not be equal to 0, because of how simulation
            // moves hand when releasing the Pitch. Even though it doesn't directly follow from hand movement, there will always be some rotation.
            Assert.NotZero(rigidBody.angularVelocity.magnitude, "ObjectManipulator should apply angular velocity to rigidBody upon release.");
            Assert.AreEqual(hand.GetVelocity(), rigidBody.velocity, "ObjectManipulator should apply hand velocity to rigidBody upon release.");

            // This is just for debugging purposes, so object's movement after release can be seen.
            yield return hand.MoveTo(initialHandPosition);
            yield return hand.Hide();

            GameObject.Destroy(testObject);
            yield return null;
        }


        /// <summary>
        /// This tests the one hand near movement while camera (character) is moving around.
        /// The test will check the offset between object pivot and grab point and make sure we're not drifting
        /// out of the object on pointer rotation - this test should be the same in all rotation setups
        /// </summary>
        [UnityTest]
        public IEnumerator ObjectManipulatorOneHandMoveNear()
        {
            // set up cube with manipulation handler
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testObject.transform.localScale = Vector3.one * 0.2f;
            Vector3 initialObjectPosition = new Vector3(0f, 0f, 1f);
            testObject.transform.position = initialObjectPosition;
            var manipHandler = testObject.AddComponent<ObjectManipulator>();
            manipHandler.HostTransform = testObject.transform;
            manipHandler.SmoothingActive = false;
            manipHandler.ManipulationType = ManipulationHandFlags.OneHanded;

            // add near interaction grabbable to be able to grab the cube with the simulated articulated hand
            testObject.AddComponent<NearInteractionGrabbable>();

            yield return new WaitForFixedUpdate();
            yield return null;

            const int numCircleSteps = 10;
            const int numHandSteps = 3;

            Vector3 initialHandPosition = new Vector3(0, 0, 0.5f);
            Vector3 initialGrabPosition = new Vector3(-0.1f, -0.1f, 1f); // grab the left bottom corner of the cube 
            TestHand hand = new TestHand(Handedness.Right);

            // do this test for every one hand rotation mode
            foreach (ObjectManipulator.RotateInOneHandType type in Enum.GetValues(typeof(ObjectManipulator.RotateInOneHandType)))
            {
                manipHandler.OneHandRotationModeNear = type;

                TestUtilities.PlayspaceToOriginLookingForward();

                yield return hand.Show(initialHandPosition);
                var pointer = hand.GetPointer<SpherePointer>();
                Assert.IsNotNull(pointer);

                yield return hand.MoveTo(initialGrabPosition, numHandSteps);
                yield return hand.SetGesture(ArticulatedHandPose.GestureId.Pinch);

                // save relative pos grab point to object
                Vector3 initialGrabPoint = manipHandler.GetPointerGrabPoint(pointer.PointerId);
                Vector3 initialOffsetGrabToObjPivot = initialGrabPoint - testObject.transform.position;
                Vector3 initialGrabPointInObject = testObject.transform.InverseTransformPoint(initialGrabPoint);

                // full circle
                const int degreeStep = 360 / numCircleSteps;

                // rotating the pointer in a circle around "the user" 
                for (int i = 1; i <= numCircleSteps; ++i)
                {
                    // rotate main camera (user)
                    MixedRealityPlayspace.PerformTransformation(
                    p =>
                    {
                        p.position = MixedRealityPlayspace.Position;
                        Vector3 rotatedFwd = Quaternion.AngleAxis(degreeStep * i, Vector3.up) * Vector3.forward;
                        p.LookAt(rotatedFwd);
                    });

                    yield return null;

                    // move hand with the camera
                    Vector3 newHandPosition = Quaternion.AngleAxis(degreeStep * i, Vector3.up) * initialGrabPosition;
                    yield return hand.MoveTo(newHandPosition, numHandSteps);

                    if (type == ObjectManipulator.RotateInOneHandType.RotateAboutObjectCenter)
                    {
                        // make sure that the offset between grab and object centre hasn't changed while rotating
                        Vector3 grabPoint = manipHandler.GetPointerGrabPoint(pointer.PointerId);
                        Vector3 offsetRotated = grabPoint - testObject.transform.position;
                        TestUtilities.AssertAboutEqual(offsetRotated, initialOffsetGrabToObjPivot, $"Object offset changed during rotation using {type}");
                    }
                    else
                    {
                        // make sure that the offset between grab point and object pivot hasn't changed while rotating
                        Vector3 grabPoint = manipHandler.GetPointerGrabPoint(pointer.PointerId);
                        Vector3 cornerRotated = testObject.transform.TransformPoint(initialGrabPointInObject);
                        TestUtilities.AssertAboutEqual(cornerRotated, grabPoint, $"Grab point on object changed during rotation using {type}");
                    }
                }

                yield return hand.SetGesture(ArticulatedHandPose.GestureId.Open);
                yield return hand.Hide();

            }

        }

        /// <summary>
        /// This tests the one hand far movement while camera (character) is moving around.
        /// The test will check the offset between object pivot and grab point and make sure we're not drifting
        /// out of the object on pointer rotation - this test is the same for all objects that won't change 
        /// their orientation to camera while camera / pointer rotates as this will modify the far interaction grab point
        /// </summary>
        [UnityTest]
        public IEnumerator ObjectManipulatorOneHandMoveFar()
        {
            // set up cube with manipulation handler
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testObject.transform.localScale = Vector3.one * 0.2f;
            Vector3 initialObjectPosition = new Vector3(0f, 0f, 1f);
            testObject.transform.position = initialObjectPosition;
            var manipHandler = testObject.AddComponent<ObjectManipulator>();
            manipHandler.HostTransform = testObject.transform;
            manipHandler.SmoothingActive = false;
            manipHandler.ManipulationType = ManipulationHandFlags.OneHanded;

            // add near interaction grabbable to be able to grab the cube with the simulated articulated hand
            testObject.AddComponent<NearInteractionGrabbable>();

            yield return new WaitForFixedUpdate();
            yield return null;

            const int numCircleSteps = 10;
            const int numHandSteps = 3;

            // Hand pointing at middle of cube
            Vector3 initialHandPosition = new Vector3(0.044f, -0.1f, 0.45f);
            TestHand hand = new TestHand(Handedness.Right);  

            // do this test for every one hand rotation mode
            foreach (ObjectManipulator.RotateInOneHandType type in Enum.GetValues(typeof(ObjectManipulator.RotateInOneHandType)))
            {
                manipHandler.OneHandRotationModeFar = type;

                TestUtilities.PlayspaceToOriginLookingForward();

                yield return hand.Show(initialHandPosition);
                yield return PlayModeTestUtilities.WaitForInputSystemUpdate();
               
                yield return hand.SetGesture(ArticulatedHandPose.GestureId.Pinch);

                // save relative pos grab point to object - for far interaction we need to check the grab point where the pointer ray hits the manipulated object
                InputSimulationService simulationService = PlayModeTestUtilities.GetInputSimulationService();
                IMixedRealityController[] inputControllers = simulationService.GetActiveControllers();
                // assume hand is first controller and pointer for this test
                IMixedRealityController handController = inputControllers[0];
                IMixedRealityPointer handPointer = handController.InputSource.Pointers[0];
                Vector3 initialGrabPosition = handPointer.Result.Details.Point;
                Vector3 initialOffsetGrabToObjPivot = MixedRealityPlayspace.InverseTransformPoint(initialGrabPosition) - MixedRealityPlayspace.InverseTransformPoint(testObject.transform.position);

                // full circle
                const int degreeStep = 360 / numCircleSteps;

                // rotating the pointer in a circle around "the user" 
                for (int i = 1; i <= numCircleSteps; ++i)
                {

                    // rotate main camera (user)
                    MixedRealityPlayspace.PerformTransformation(
                    p =>
                    {
                        p.position = MixedRealityPlayspace.Position;
                        Vector3 rotatedFwd = Quaternion.AngleAxis(degreeStep * i, Vector3.up) * Vector3.forward;
                        p.LookAt(rotatedFwd);
                    });

                    yield return null;

                    // move hand with the camera
                    Vector3 newHandPosition = Quaternion.AngleAxis(degreeStep * i, Vector3.up) * initialHandPosition;
                    yield return hand.MoveTo(newHandPosition, numHandSteps);
                    yield return new WaitForFixedUpdate();
                    yield return null;


                    // make sure that the offset between grab point and object pivot hasn't changed while rotating
                    Vector3 newGrabPosition = handPointer.Result.Details.Point;
                    Vector3 offsetRotated = MixedRealityPlayspace.InverseTransformPoint(newGrabPosition) - MixedRealityPlayspace.InverseTransformPoint(testObject.transform.position);
                    TestUtilities.AssertAboutEqual(offsetRotated, initialOffsetGrabToObjPivot, "Grab point on object changed during rotation");
                }

                yield return hand.SetGesture(ArticulatedHandPose.GestureId.Open);
                yield return hand.Hide();

            }

        }


        /// <summary>
        /// This tests the one hand near rotation and applying different rotation constraints to the object.
        /// </summary>
        [UnityTest]
        public IEnumerator ObjectManipulatorOneHandRotateWithConstraint()
        {
            // set up cube with manipulation handler
            GameObject parentObject = new GameObject("Test Object Parent");
            
            // In case of error, this object won't be cleaned up, so clean up at the end of the test
            cleanupAction.Add(() => { if (parentObject != null) UnityEngine.Object.Destroy(parentObject); });
            
            GameObject testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testObject.transform.parent = parentObject.transform;

            // Rotate the parent object, as we differ when constraining on local vs world
            Quaternion initialQuaternion = Quaternion.Euler(30f, 30f, 30f);
            parentObject.transform.rotation = initialQuaternion;
            
            testObject.transform.localScale = Vector3.one * 0.2f;
            Vector3 initialObjectPosition = new Vector3(0f, 0f, 1f);
            testObject.transform.position = initialObjectPosition;
            var manipHandler = testObject.AddComponent<ObjectManipulator>();
            manipHandler.HostTransform = testObject.transform;
            manipHandler.SmoothingActive = false;
            manipHandler.ManipulationType = ManipulationHandFlags.OneHanded;
            manipHandler.OneHandRotationModeFar = ObjectManipulator.RotateInOneHandType.RotateAboutGrabPoint;
            manipHandler.ReleaseBehavior = 0;
            manipHandler.AllowFarManipulation = false;

            var rotateConstraint = manipHandler.EnsureComponent<RotationAxisConstraint>();
            rotateConstraint.TargetTransform = testObject.transform;
            rotateConstraint.ConstraintOnRotation = 0; // No constraint

            // add near interaction grabbable to be able to grab the cube with the simulated articulated hand
            testObject.AddComponent<NearInteractionGrabbable>();

            yield return new WaitForFixedUpdate();
            yield return null;

            TestHand hand = new TestHand(Handedness.Right);

            yield return hand.Show(initialObjectPosition);
            yield return null;

            // grab the object
            yield return hand.SetGesture(ArticulatedHandPose.GestureId.Pinch);
            yield return new WaitForFixedUpdate();
            yield return null;

            float testRotation = 45;
            const int numRotSteps = 10;
            Quaternion testQuaternion = Quaternion.Euler(testRotation, testRotation, testRotation);

            /*********************************/
            /*** TEST WORLD SPACE ROTATION ***/
            /*********************************/

            // rotate without constraint
            yield return hand.SetRotation(testQuaternion, numRotSteps);
            float diffAngle = Quaternion.Angle(testObject.transform.rotation, Quaternion.Euler(testRotation, testRotation, testRotation) * initialQuaternion);
            Assert.IsTrue(Mathf.Approximately(diffAngle, 0.0f), "object didn't rotate with hand (world space)");

            yield return hand.SetRotation(Quaternion.identity, numRotSteps);
            diffAngle = Quaternion.Angle(testObject.transform.rotation, initialQuaternion);
            Assert.IsTrue(Mathf.Approximately(diffAngle, 0.0f), "object didn't rotate with hand (world space)");

            // rotate with x axis only
            rotateConstraint.ConstraintOnRotation = AxisFlags.YAxis | AxisFlags.ZAxis;
            yield return hand.SetRotation(testQuaternion, numRotSteps);
            diffAngle = Quaternion.Angle(testObject.transform.rotation, Quaternion.Euler(testRotation, 0, 0) * initialQuaternion);
            Assert.IsTrue(Mathf.Approximately(diffAngle, 0.0f), "constraint on x axis did not lock axis correctly (world space)");

            yield return hand.SetRotation(Quaternion.identity, numRotSteps);
            diffAngle = Quaternion.Angle(testObject.transform.rotation, initialQuaternion);
            Assert.IsTrue(Mathf.Approximately(diffAngle, 0.0f), "constraint on x axis did not lock axis correctly (world space)");

            // rotate with y axis only
            rotateConstraint.ConstraintOnRotation = AxisFlags.XAxis | AxisFlags.ZAxis;
            yield return hand.SetRotation(testQuaternion, numRotSteps);
            diffAngle = Quaternion.Angle(testObject.transform.rotation, Quaternion.Euler(0, testRotation, 0) * initialQuaternion);
            Assert.IsTrue(Mathf.Approximately(diffAngle, 0.0f), "constraint on Y axis did not lock axis correctly (world space)");

            yield return hand.SetRotation(Quaternion.identity, numRotSteps);
            diffAngle = Quaternion.Angle(testObject.transform.rotation, initialQuaternion);
            Assert.IsTrue(Mathf.Approximately(diffAngle, 0.0f), "constraint on Y axis did not lock axis correctly (world space)");

            // rotate with z axis only
            rotateConstraint.ConstraintOnRotation = AxisFlags.XAxis | AxisFlags.YAxis;
            yield return hand.SetRotation(testQuaternion, numRotSteps);
            diffAngle = Quaternion.Angle(testObject.transform.rotation, Quaternion.Euler(0, 0, testRotation) * initialQuaternion);
            Assert.IsTrue(Mathf.Approximately(diffAngle, 0.0f), "constraint on Z axis did not lock axis correctly (world space)");

            yield return hand.SetRotation(Quaternion.identity, numRotSteps);
            diffAngle = Quaternion.Angle(testObject.transform.rotation, initialQuaternion);
            Assert.IsTrue(Mathf.Approximately(diffAngle, 0.0f), "constraint on Z axis did not lock axis correctly (world space)");

            /*********************************/
            /*** TEST LOCAL SPACE ROTATION ***/
            /*********************************/

            rotateConstraint.UseLocalSpaceForConstraint = true;
            // rotate with x axis only
            rotateConstraint.ConstraintOnRotation = AxisFlags.YAxis | AxisFlags.ZAxis;
            yield return hand.SetRotation(testQuaternion, numRotSteps);
            diffAngle = Quaternion.Angle(testObject.transform.localRotation, Quaternion.Euler(testRotation, 0, 0));
            Assert.IsTrue(Mathf.Approximately(diffAngle, 0.0f), "constraint on x axis did not lock axis correctly (local space)");

            yield return hand.SetRotation(Quaternion.identity, numRotSteps);
            diffAngle = Quaternion.Angle(testObject.transform.localRotation, Quaternion.identity);
            Assert.IsTrue(Mathf.Approximately(diffAngle, 0.0f), "constraint on x axis did not lock axis correctly (local space)");

            // rotate with y axis only
            rotateConstraint.ConstraintOnRotation = AxisFlags.XAxis | AxisFlags.ZAxis;
            yield return hand.SetRotation(testQuaternion, numRotSteps);
            diffAngle = Quaternion.Angle(testObject.transform.localRotation, Quaternion.Euler(0, testRotation, 0));
            Assert.IsTrue(Mathf.Approximately(diffAngle, 0.0f), "constraint on Y axis did not lock axis correctly (local space)");

            yield return hand.SetRotation(Quaternion.identity, numRotSteps);
            diffAngle = Quaternion.Angle(testObject.transform.localRotation, Quaternion.identity);
            Assert.IsTrue(Mathf.Approximately(diffAngle, 0.0f), "constraint on Y axis did not lock axis correctly (local space)");

            // rotate with z axis only
            rotateConstraint.ConstraintOnRotation = AxisFlags.XAxis | AxisFlags.YAxis;
            yield return hand.SetRotation(testQuaternion, numRotSteps);
            diffAngle = Quaternion.Angle(testObject.transform.localRotation, Quaternion.Euler(0, 0, testRotation));
            Assert.IsTrue(Mathf.Approximately(diffAngle, 0.0f), "constraint on Z axis did not lock axis correctly (local space)");

            yield return hand.SetRotation(Quaternion.identity, numRotSteps);
            diffAngle = Quaternion.Angle(testObject.transform.localRotation, Quaternion.identity);
            Assert.IsTrue(Mathf.Approximately(diffAngle, 0.0f), "constraint on Z axis did not lock axis correctly (local space)");

            yield return hand.SetGesture(ArticulatedHandPose.GestureId.Open);
            yield return hand.Hide();
            
            UnityEngine.Object.Destroy(parentObject);
        }

        private class OriginOffsetTest
        {
            const int numSteps = 10;

            public struct TestData
            {
                // transform data
                public MixedRealityPose pose;
                public Vector3 scale;

                // used to print where error occurred
                public string manipDescription;
            }

            public List<TestData> data = new List<TestData>();

            /// <summary>
            /// Manipulates the given testObject in a number of ways and records the output here
            /// </summary>
            /// <param name="testObject">An unrotated primitive cube at (0, 0, 1) with scale (0.2, 0.2, 0,2)</param>
            public IEnumerator RecordTransformValues(GameObject testObject)
            {
                TestUtilities.PlayspaceToOriginLookingForward();

                float testRotation = 45;
                Quaternion testQuaternion = Quaternion.Euler(testRotation, testRotation, testRotation);

                Vector3 leftHandNearPos = new Vector3(-0.1f, 0, 1);
                Vector3 rightHandNearPos = new Vector3(0.1f, 0, 1);
                Vector3 leftHandFarPos = new Vector3(-0.06f, -0.1f, 0.5f);
                Vector3 rightHandFarPos = new Vector3(0.06f, -0.1f, 0.5f);
                TestHand leftHand = new TestHand(Handedness.Left);
                TestHand rightHand = new TestHand(Handedness.Right);

                // One hand rotate near
                yield return rightHand.MoveTo(rightHandNearPos, numSteps);
                yield return rightHand.SetGesture(ArticulatedHandPose.GestureId.Pinch);
                yield return rightHand.SetRotation(testQuaternion, numSteps);
                RecordTransform(testObject.transform, "one hand rotate near");

                // Two hand rotate/scale near
                yield return rightHand.SetRotation(Quaternion.identity, numSteps);
                yield return leftHand.MoveTo(leftHandNearPos, numSteps);
                yield return leftHand.SetGesture(ArticulatedHandPose.GestureId.Pinch);
                yield return rightHand.Move(new Vector3(0.2f, 0.2f, 0), numSteps);
                yield return leftHand.Move(new Vector3(-0.2f, -0.2f, 0), numSteps);
                RecordTransform(testObject.transform, "two hand rotate/scale near");

                // Two hand rotate/scale far
                yield return rightHand.MoveTo(rightHandNearPos, numSteps);
                yield return leftHand.MoveTo(leftHandNearPos, numSteps);
                yield return rightHand.SetGesture(ArticulatedHandPose.GestureId.Open);
                yield return leftHand.SetGesture(ArticulatedHandPose.GestureId.Open);
                yield return rightHand.MoveTo(rightHandFarPos, numSteps);
                yield return leftHand.MoveTo(leftHandFarPos, numSteps);
                yield return rightHand.SetGesture(ArticulatedHandPose.GestureId.Pinch);
                yield return leftHand.SetGesture(ArticulatedHandPose.GestureId.Pinch);
                yield return rightHand.Move(new Vector3(0.2f, 0.2f, 0), numSteps);
                yield return leftHand.Move(new Vector3(-0.2f, -0.2f, 0), numSteps);
                RecordTransform(testObject.transform, "two hand rotate/scale far");

                // One hand rotate near
                yield return rightHand.MoveTo(rightHandFarPos, numSteps);
                yield return leftHand.MoveTo(leftHandFarPos, numSteps);
                yield return leftHand.SetGesture(ArticulatedHandPose.GestureId.Open);
                yield return leftHand.Hide();

                MixedRealityPlayspace.PerformTransformation(
                p =>
                {
                    p.position = MixedRealityPlayspace.Position;
                    Vector3 rotatedFwd = Quaternion.AngleAxis(testRotation, Vector3.up) * Vector3.forward;
                    p.LookAt(rotatedFwd);
                });
                yield return null;
                
                Vector3 newHandPosition = Quaternion.AngleAxis(testRotation, Vector3.up) * rightHandFarPos;
                yield return rightHand.MoveTo(newHandPosition, numSteps);
                RecordTransform(testObject.transform, "one hand rotate far");

                yield return rightHand.SetGesture(ArticulatedHandPose.GestureId.Open);
                yield return rightHand.Hide();
            }

            private void RecordTransform(Transform transform, string description)
            {
                data.Add(new TestData
                {
                    pose = new MixedRealityPose(transform.position, transform.rotation),
                    scale = transform.localScale,
                    manipDescription = description
                });
            }
        }

        /// <summary>
        /// This test records the poses and scales of an object after various forms of manipulation,
        /// once when the object origin is at the mesh centre and again when the origin is offset from the mesh.
        /// The test then compares these poses and scales in order to ensure that they are about equal. 
        /// </summary>
        [UnityTest]
        public IEnumerator ObjectManipulatorOriginOffset()
        {
            TestUtilities.PlayspaceToOriginLookingForward();
            // Without this background object that contains a collider, Unity will rarely
            // return no colliders hit for raycasts and sphere casts, even if a ray / sphere is 
            // intersecting the collider. Causes tests to be unreliable.
            // It seems to only happen when one collider is in the scene.
            var backgroundObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backgroundObject.transform.position = Vector3.forward * 10;
            backgroundObject.transform.localScale = new Vector3(100, 100, 1);
            var backgroundmaterial = new Material(StandardShaderUtility.MrtkStandardShader);
            backgroundmaterial.color = Color.green;
            backgroundObject.GetComponent<MeshRenderer>().material = backgroundmaterial;


            // set up cube with manipulation handler
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var manipHandler = testObject.AddComponent<ObjectManipulator>();
            manipHandler.HostTransform = testObject.transform;
            manipHandler.SmoothingActive = false;
            manipHandler.ManipulationType = ManipulationHandFlags.OneHanded | ManipulationHandFlags.TwoHanded;

            // add near interaction grabbable to be able to grab the cube with the simulated articulated hand
            testObject.AddComponent<NearInteractionGrabbable>();
            testObject.transform.localScale = Vector3.one * 0.2f;
            testObject.transform.position = Vector3.forward;

            // Collect data for unmodified cube
            OriginOffsetTest expectedTest = new OriginOffsetTest();
            yield return expectedTest.RecordTransformValues(testObject);
            yield return null;

            // Modify cube mesh so origin is offset from centre
            Vector3 offset = Vector3.one * 5;

            testObject.transform.localScale = Vector3.one * 0.2f;
            testObject.transform.rotation = Quaternion.identity;
            var mesh = testObject.GetComponent<MeshFilter>().mesh;
            var vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] += offset;
            }
            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            testObject.GetComponent<BoxCollider>().center = offset;
            testObject.transform.position = Vector3.forward - testObject.transform.TransformVector(offset);

            // Collect data for modified cube

            OriginOffsetTest actualTest = new OriginOffsetTest();
            yield return actualTest.RecordTransformValues(testObject);
            yield return null;

            // Test that the results of both tests are equal
            var expectedData = expectedTest.data;
            var actualData = actualTest.data;
            Assert.AreEqual(expectedData.Count, actualData.Count);

            for (int i = 0; i < expectedData.Count; i++)
            {
                Vector3 transformedOffset = actualData[i].pose.Rotation * Vector3.Scale(offset, actualData[i].scale);
                TestUtilities.AssertAboutEqual(expectedData[i].pose.Position, actualData[i].pose.Position + transformedOffset, $"Failed for position of object for {actualData[i].manipDescription}");
                TestUtilities.AssertAboutEqual(expectedData[i].pose.Rotation, actualData[i].pose.Rotation, $"Failed for rotation of object for {actualData[i].manipDescription}", 1.0f);
                TestUtilities.AssertAboutEqual(expectedData[i].scale, actualData[i].scale, $"Failed for scale of object for {actualData[i].manipDescription}");
            }
        }

        /// <summary>
        /// This test rotates the head without moving the hand.
        /// This test is set up to test using the Gestures input simulation mode as this is
        /// where we observed issues with this.
        /// If the head rotates, without moving the hand, the grabbed object should not move.
        /// </summary>
        [UnityTest]
        public IEnumerator ObjectManipulatorRotateHeadGGV()
        {
            TestUtilities.PlayspaceToOriginLookingForward();

            // Switch to Gestures
            var iss = PlayModeTestUtilities.GetInputSimulationService();
            var oldHandSimMode = iss.HandSimulationMode;
            iss.HandSimulationMode = HandSimulationMode.Gestures;

            // set up cube with manipulation handler
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testObject.transform.localScale = Vector3.one * 0.2f;
            Vector3 initialObjectPosition = new Vector3(0f, 0f, 1f);
            Quaternion initialObjectRotation = testObject.transform.rotation;
            testObject.transform.position = initialObjectPosition;

            var manipHandler = testObject.AddComponent<ObjectManipulator>();
            manipHandler.HostTransform = testObject.transform;
            manipHandler.SmoothingActive = false;
            
            Vector3 originalHandPosition = new Vector3(0, 0, 0.5f);
            TestHand hand = new TestHand(Handedness.Right);
            const int numHandSteps = 1;

            // Grab cube
            yield return hand.Show(originalHandPosition);

            // Hand position is not exactly the pointer position, this correction applies the delta
            // from the hand to the pointer.
            Vector3 correction = originalHandPosition - hand.GetPointer<GGVPointer>().Position;
            yield return hand.Move(correction, numHandSteps);

            yield return hand.SetGesture(ArticulatedHandPose.GestureId.Pinch);
            yield return null;

            // Rotate Head and readjust hand
            int numRotations = 10;
            for (int i = 0; i < numRotations; i++)
            {
                MixedRealityPlayspace.Transform.Rotate(Vector3.up, 180 / numRotations);
                correction = originalHandPosition - hand.GetPointer<GGVPointer>().Position;
                yield return hand.Move(correction, numHandSteps);
                yield return null;

                // Test Object hasn't moved
                TestUtilities.AssertAboutEqual(initialObjectPosition, testObject.transform.position, "Object moved while rotating head", 0.01f);
                TestUtilities.AssertAboutEqual(initialObjectRotation, testObject.transform.rotation, "Object rotated while rotating head", 0.25f);
            }

            // Restore the input simulation profile
            iss.HandSimulationMode = oldHandSimMode;
            yield return null;
        }

        /// <summary>
        /// For positionless input sources that use gaze, such as xbox controller, pointer position will
        /// be coincident with the head position. This was causing issues with manipulation handler, as
        /// the distance between the pointer and the head is taken as part of the move logic. 
        /// This test simulates a positionless input source by using GGV hands and setting the hand position
        /// to be the head position. It then ensures that there is no weird behaviour as a result of this.
        /// </summary>
        [UnityTest]
        public IEnumerator ObjectManipulatorPositionlessController()
        {
            TestUtilities.PlayspaceToOriginLookingForward();

            // Switch to Gestures
            var iss = PlayModeTestUtilities.GetInputSimulationService();
            var oldHandSimMode = iss.HandSimulationMode;
            iss.HandSimulationMode = HandSimulationMode.Gestures;

            // set up cube with manipulation handler
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testObject.transform.localScale = Vector3.one * 0.2f;
            testObject.transform.position = new Vector3(0f, 0f, 1f);

            var manipHandler = testObject.AddComponent<ObjectManipulator>();
            manipHandler.HostTransform = testObject.transform;
            manipHandler.SmoothingActive = false;

            TestHand hand = new TestHand(Handedness.Right);
            const int numHandSteps = 1;

            float expectedDist = Vector3.Distance(testObject.transform.position, CameraCache.Main.transform.position);

            yield return hand.Show(CameraCache.Main.transform.position);
            yield return null;

            // Hand position is not exactly the pointer position, this correction applies the delta
            // from the hand to the pointer.
            Vector3 correction = CameraCache.Main.transform.position - hand.GetPointer<GGVPointer>().Position;
            yield return hand.Move(correction, numHandSteps);
            yield return null;

            Assert.AreEqual(expectedDist, Vector3.Distance(testObject.transform.position, CameraCache.Main.transform.position), 0.02f);

            yield return hand.SetGesture(ArticulatedHandPose.GestureId.Pinch);

            // Apply correction delta again as we have changed hand pose
            correction = CameraCache.Main.transform.position - hand.GetPointer<GGVPointer>().Position;
            yield return hand.Move(correction, numHandSteps);
            yield return null;

            Assert.AreEqual(expectedDist, Vector3.Distance(testObject.transform.position, CameraCache.Main.transform.position), 0.02f);

            Vector3 delta = new Vector3(0.2f, 0, 0);
            MixedRealityPlayspace.Transform.Translate(delta);
            yield return hand.Move(delta, numHandSteps);
            yield return null;

            Assert.AreEqual(expectedDist, Vector3.Distance(testObject.transform.position, CameraCache.Main.transform.position), 2.5f);

            yield return hand.SetGesture(ArticulatedHandPose.GestureId.Open);
            yield return null;

            Assert.AreEqual(expectedDist, Vector3.Distance(testObject.transform.position, CameraCache.Main.transform.position), 0.02f);

            // Restore the input simulation profile
            iss.HandSimulationMode = oldHandSimMode;
            yield return null;
        }
        
        /// <summary>
        /// This test first moves the hand a set amount along the x-axis, records its x position, then moves
        /// it the same amount along the y-axis and records its y position. Given no constraints on manipulation,
        /// we expect these values to be the same.
        /// This test was added as a change to pointer behaviour made GGV manipulation along the y-axis sluggish.
        /// </summary>
        [UnityTest]
        public IEnumerator ObjectManipulatorMoveYAxisGGV()
        {
            TestUtilities.PlayspaceToOriginLookingForward();

            // Switch to Gestures
            var iss = PlayModeTestUtilities.GetInputSimulationService();
            var oldHandSimMode = iss.HandSimulationMode;
            iss.HandSimulationMode = HandSimulationMode.Gestures;

            // set up cube with manipulation handler
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testObject.transform.localScale = Vector3.one * 0.2f;
            testObject.transform.position = new Vector3(0f, 0f, 1f);

            var manipHandler = testObject.AddComponent<ObjectManipulator>();
            manipHandler.HostTransform = testObject.transform;
            manipHandler.SmoothingActive = false;

            TestHand hand = new TestHand(Handedness.Right);
            const int numHandSteps = 1;

            float moveBy = 0.5f;
            float xPos, yPos;

            // Grab the object
            yield return hand.Show(new Vector3(0, 0, 0.5f));
            yield return hand.SetGesture(ArticulatedHandPose.GestureId.Pinch);
            yield return null;

            yield return hand.Move(Vector3.right * moveBy, numHandSteps);
            yield return null;

            xPos = testObject.transform.position.x;

            yield return hand.Move((Vector3.left + Vector3.up) * moveBy, numHandSteps);
            yield return null;

            yPos = testObject.transform.position.y;

            Assert.AreEqual(xPos, yPos, 0.02f);

            // Restore the input simulation profile
            iss.HandSimulationMode = oldHandSimMode;
            yield return null;
        }

        /// <summary>
        /// Ensure that while manipulating an object, if that object gets
        /// deactivated, that manipulation no longer moves that object.
        /// </summary>
        [UnityTest]
        public IEnumerator ObjectManipulatorDisableObject()
        {
            TestUtilities.PlayspaceToOriginLookingForward();

            // set up cube with manipulation handler
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testObject.transform.localScale = Vector3.one * 0.2f;
            Vector3 originalObjectPos = Vector3.forward;
            testObject.transform.position = originalObjectPos;

            var manipHandler = testObject.AddComponent<ObjectManipulator>();
            manipHandler.HostTransform = testObject.transform;
            manipHandler.SmoothingActive = false;

            TestHand hand = new TestHand(Handedness.Right);
            const int numHandSteps = 1;

            // Move the object without disabling and assure it moves
            Vector3 originalHandPos = new Vector3(0.06f, -0.1f, 0.5f);
            yield return hand.Show(originalHandPos);
            yield return hand.SetGesture(ArticulatedHandPose.GestureId.Pinch);
            yield return null;

            yield return hand.Move(new Vector3(0.2f, 0, 0), numHandSteps);
            yield return null;

            Assert.AreNotEqual(originalObjectPos, testObject.transform.position);

            // Disable the object and move the hand and assure the object does not move
            yield return hand.MoveTo(originalHandPos, numHandSteps);
            yield return null;

            testObject.SetActive(false);
            yield return null;
            testObject.SetActive(true);

            yield return hand.Move(new Vector3(0.2f, 0, 0), numHandSteps);
            yield return null;

            TestUtilities.AssertAboutEqual(originalObjectPos, testObject.transform.position, "Object moved after it was disabled");
        }
		
		/// <summary>
        /// Test that OnManipulationStarted and OnManipulationEnded events call as expected
        /// for various One Handed Only.
        /// </summary>
        [UnityTest]
        public IEnumerator ObjectManipulatorEventsOneHandedOnly()
        {
            TestUtilities.PlayspaceToOriginLookingForward();

            // set up cube with manipulation handler
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testObject.transform.localScale = Vector3.one * 0.5f;
            Vector3 initialObjectPosition = new Vector3(0f, 0f, 1f);
            Quaternion initialObjectRotation = testObject.transform.rotation;
            testObject.transform.position = initialObjectPosition;

            var manipHandler = testObject.AddComponent<ObjectManipulator>();
            manipHandler.HostTransform = testObject.transform;
            manipHandler.SmoothingActive = false;

            int manipulationStartedCount = 0;
            int manipulationEndedCount = 0;
            manipHandler.OnManipulationStarted.AddListener((med) => manipulationStartedCount++);
            manipHandler.OnManipulationEnded.AddListener((med) => manipulationEndedCount++);
            
            TestHand rightHand = new TestHand(Handedness.Right);
            TestHand leftHand = new TestHand(Handedness.Left);

            yield return rightHand.Show(new Vector3(0.1f, -0.1f, 0.5f));
            yield return leftHand.Show(new Vector3(-0.1f, -0.1f, 0.5f));
            yield return null;

            // One Handed
            manipHandler.ManipulationType = ManipulationHandFlags.OneHanded;

            yield return rightHand.SetGesture(ArticulatedHandPose.GestureId.Pinch);
            Assert.AreEqual(1, manipulationStartedCount);
            Assert.AreEqual(0, manipulationEndedCount);

            yield return leftHand.SetGesture(ArticulatedHandPose.GestureId.Pinch);
            Assert.AreEqual(1, manipulationStartedCount);
            Assert.AreEqual(0, manipulationEndedCount);

            yield return rightHand.SetGesture(ArticulatedHandPose.GestureId.Open);
            Assert.AreEqual(1, manipulationStartedCount);
            Assert.AreEqual(1, manipulationEndedCount);

            yield return leftHand.SetGesture(ArticulatedHandPose.GestureId.Open);
            Assert.AreEqual(1, manipulationStartedCount);
            Assert.AreEqual(1, manipulationEndedCount);
        }
		
		/// <summary>
        /// Test that OnManipulationStarted and OnManipulationEnded events call as expected
        /// for Two Handed Only.
        /// </summary>
        [UnityTest]
        public IEnumerator ObjectManipulatorEventsEventsTwoHandedOnly()
        {
            TestUtilities.PlayspaceToOriginLookingForward();

            // set up cube with manipulation handler
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testObject.transform.localScale = Vector3.one * 0.5f;
            Vector3 initialObjectPosition = new Vector3(0f, 0f, 1f);
            Quaternion initialObjectRotation = testObject.transform.rotation;
            testObject.transform.position = initialObjectPosition;

            var manipHandler = testObject.AddComponent<ObjectManipulator>();
            manipHandler.HostTransform = testObject.transform;
            manipHandler.SmoothingActive = false;

            int manipulationStartedCount = 0;
            int manipulationEndedCount = 0;
            manipHandler.OnManipulationStarted.AddListener((med) => manipulationStartedCount++);
            manipHandler.OnManipulationEnded.AddListener((med) => manipulationEndedCount++);
            
            TestHand rightHand = new TestHand(Handedness.Right);
            TestHand leftHand = new TestHand(Handedness.Left);

            yield return rightHand.Show(new Vector3(0.1f, -0.1f, 0.5f));
            yield return leftHand.Show(new Vector3(-0.1f, -0.1f, 0.5f));
            yield return null;

            // Two Handed
            manipHandler.ManipulationType = ManipulationHandFlags.TwoHanded;

            yield return rightHand.SetGesture(ArticulatedHandPose.GestureId.Pinch);
            Assert.AreEqual(0, manipulationStartedCount);
            Assert.AreEqual(0, manipulationEndedCount);

            yield return leftHand.SetGesture(ArticulatedHandPose.GestureId.Pinch);
            Assert.AreEqual(1, manipulationStartedCount);
            Assert.AreEqual(0, manipulationEndedCount);

            yield return rightHand.SetGesture(ArticulatedHandPose.GestureId.Open);
            Assert.AreEqual(1, manipulationStartedCount);
            Assert.AreEqual(1, manipulationEndedCount);

            yield return leftHand.SetGesture(ArticulatedHandPose.GestureId.Open);
            Assert.AreEqual(1, manipulationStartedCount);
            Assert.AreEqual(1, manipulationEndedCount);
        }
		
		/// <summary>
        /// Test that OnManipulationStarted and OnManipulationEnded events call as expected
        /// for One Handed and Two Handed.
        /// </summary>
        [UnityTest]
        public IEnumerator ObjectManipulatorEventsBothHands()
        {
            TestUtilities.PlayspaceToOriginLookingForward();

            // set up cube with manipulation handler
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testObject.transform.localScale = Vector3.one * 0.5f;
            Vector3 initialObjectPosition = new Vector3(0f, 0f, 1f);
            Quaternion initialObjectRotation = testObject.transform.rotation;
            testObject.transform.position = initialObjectPosition;

            var manipHandler = testObject.AddComponent<ObjectManipulator>();
            manipHandler.HostTransform = testObject.transform;
            manipHandler.SmoothingActive = false;

            int manipulationStartedCount = 0;
            int manipulationEndedCount = 0;
            manipHandler.OnManipulationStarted.AddListener((med) => manipulationStartedCount++);
            manipHandler.OnManipulationEnded.AddListener((med) => manipulationEndedCount++);
            
            TestHand rightHand = new TestHand(Handedness.Right);
            TestHand leftHand = new TestHand(Handedness.Left);

            yield return rightHand.Show(new Vector3(0.1f, -0.1f, 0.5f));
            yield return leftHand.Show(new Vector3(-0.1f, -0.1f, 0.5f));
            yield return null;

            // One + Two Handed
            manipHandler.ManipulationType = ManipulationHandFlags.OneHanded | ManipulationHandFlags.TwoHanded;

            yield return rightHand.SetGesture(ArticulatedHandPose.GestureId.Pinch);
            Assert.AreEqual(1, manipulationStartedCount);
            Assert.AreEqual(0, manipulationEndedCount);

            yield return leftHand.SetGesture(ArticulatedHandPose.GestureId.Pinch);
            Assert.AreEqual(1, manipulationStartedCount);
            Assert.AreEqual(0, manipulationEndedCount);

            yield return rightHand.SetGesture(ArticulatedHandPose.GestureId.Open);
            Assert.AreEqual(1, manipulationStartedCount);
            Assert.AreEqual(0, manipulationEndedCount);

            yield return leftHand.SetGesture(ArticulatedHandPose.GestureId.Open);
            Assert.AreEqual(1, manipulationStartedCount);
            Assert.AreEqual(1, manipulationEndedCount);
        }

        class TestCollisionListener : MonoBehaviour
        {
            public int CollisionCount { get; private set; }

            private void OnCollisionEnter(Collision collision)
            {
                CollisionCount++;
            }
        }

        /// <summary>
        /// Test that objects with both ObjectManipulator and Rigidbody respond
        /// correctly to static colliders.
        /// </summary>
        [UnityTest]
        public IEnumerator ObjectManipulatorStaticCollision()
        {
            TestUtilities.PlayspaceToOriginLookingForward();

            // set up cube with manipulation handler
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testObject.transform.localScale = Vector3.one * 0.5f;
            testObject.transform.position = new Vector3(0f, 0f, 1f);

            var rigidbody = testObject.AddComponent<Rigidbody>();
            rigidbody.useGravity = false;

            var manipHandler = testObject.AddComponent<ObjectManipulator>();
            manipHandler.HostTransform = testObject.transform;
            manipHandler.SmoothingActive = false;

            var collisionListener = testObject.AddComponent<TestCollisionListener>();

            // set up static cube to collide with
            var backgroundObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backgroundObject.transform.localScale = Vector3.one;
            backgroundObject.transform.position = new Vector3(0f, 0f, 2f);
            var backgroundmaterial = new Material(StandardShaderUtility.MrtkStandardShader);
            backgroundmaterial.color = Color.green;
            backgroundObject.GetComponent<MeshRenderer>().material = backgroundmaterial;
            
            const int numHandSteps = 10;
            TestHand hand = new TestHand(Handedness.Right);
            yield return hand.Show(new Vector3(0.1f, -0.1f, 0.5f));
            yield return null;

            // Grab the cube and move towards the collider
            yield return hand.SetGesture(ArticulatedHandPose.GestureId.Pinch);
            yield return null;

            yield return hand.Move(Vector3.forward * 3f, numHandSteps);

            Assert.Less(testObject.transform.position.z, backgroundObject.transform.position.z);
            Assert.AreEqual(1, collisionListener.CollisionCount);
        }

        /// <summary>
        /// Test that objects with both ObjectManipulator and Rigidbody respond
        /// correctly to rigidbody colliders.
        /// </summary>
        [UnityTest]
        public IEnumerator ObjectManipulatorRigidbodyCollision()
        {
            TestUtilities.PlayspaceToOriginLookingForward();

            // set up cube with manipulation handler
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testObject.transform.localScale = Vector3.one * 0.5f;
            testObject.transform.position = new Vector3(0f, 0f, 1f);

            var rigidbody = testObject.AddComponent<Rigidbody>();
            rigidbody.useGravity = false;

            var manipHandler = testObject.AddComponent<ObjectManipulator>();
            manipHandler.HostTransform = testObject.transform;
            manipHandler.SmoothingActive = false;

            var collisionListener = testObject.AddComponent<TestCollisionListener>();

            // set up static cube to collide with
            var backgroundObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backgroundObject.transform.localScale = Vector3.one;
            backgroundObject.transform.position = new Vector3(0f, 0f, 2f);
            var backgroundmaterial = new Material(StandardShaderUtility.MrtkStandardShader);
            backgroundmaterial.color = Color.green;
            backgroundObject.GetComponent<MeshRenderer>().material = backgroundmaterial;
            var backgroundRigidbody = backgroundObject.AddComponent<Rigidbody>();
            backgroundRigidbody.useGravity = false;

            const int numHandSteps = 10;
            TestHand hand = new TestHand(Handedness.Right);
            yield return hand.Show(new Vector3(0.1f, -0.1f, 0.5f));
            yield return null;

            // Grab the cube and move towards the collider
            yield return hand.SetGesture(ArticulatedHandPose.GestureId.Pinch);
            yield return null;

            yield return hand.Move(Vector3.forward * 3f, numHandSteps);

            Assert.AreNotEqual(Vector3.zero, backgroundRigidbody.velocity);
            Assert.AreEqual(1, collisionListener.CollisionCount);
        }

        /// <summary>
        /// Ensure that a manipulated object has the same rotation as the hand
        /// when RotateAboutObjectCenter is used
        /// </summary>
        [UnityTest]
        public IEnumerator ObjectManipulatorRotateAboutObjectCenter()
        {
            TestUtilities.PlayspaceToOriginLookingForward();

            // set up cube with manipulation handler
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testObject.transform.localScale = Vector3.one * 0.2f;
            testObject.transform.position = Vector3.forward;

            var manipHandler = testObject.AddComponent<ObjectManipulator>();
            manipHandler.HostTransform = testObject.transform;
            manipHandler.SmoothingActive = false;
            manipHandler.OneHandRotationModeFar = ObjectManipulator.RotateInOneHandType.RotateAboutObjectCenter;

            Quaternion rotateTo = Quaternion.Euler(45, 45, 45);

            TestHand hand = new TestHand(Handedness.Right);
            const int numHandSteps = 1;

            // Rotate the hand and test that the rotations are equal
            yield return hand.Show(new Vector3(0.06f, -0.1f, 0.5f));
            yield return hand.SetGesture(ArticulatedHandPose.GestureId.Pinch);
            yield return null;

            yield return hand.SetRotation(rotateTo, numHandSteps);
            yield return null;

            TestUtilities.AssertAboutEqual(rotateTo, testObject.transform.rotation, "Object moved after it was disabled");

            // Rotate the hand back and test that the rotations still are equal
            yield return hand.SetRotation(Quaternion.identity, numHandSteps);
            yield return null;

            TestUtilities.AssertAboutEqual(Quaternion.identity, testObject.transform.rotation, "Object moved after it was disabled");
        }
    }
}
#endif