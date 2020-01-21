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

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Microsoft.MixedReality.Toolkit.Tests
{
    class PinchSliderTests
    {
        const string defaultPinchSliderPrefabPath = "Assets/MixedRealityToolkit.SDK/Features/UX/Prefabs/Sliders/PinchSlider.prefab";

        [SetUp]
        public void Setup()
        {
            PlayModeTestUtilities.Setup();
            TestUtilities.PlayspaceToOriginLookingForward();
        }

        [TearDown]
        public void TearDown()
        {
            PlayModeTestUtilities.TearDown();
        }

        #region Tests
        /// <summary>
        /// Tests that a slider component can be added at runtime.
        /// at runtime.
        /// </summary>
        [UnityTest]
        public IEnumerator TestAddInteractableAtRuntime()
        {
            GameObject pinchSliderObject;
            PinchSlider slider;

            // This should not throw exception
            AssembleSlider(Vector3.forward, Vector3.zero, out pinchSliderObject, out slider);

            // clean up
            GameObject.Destroy(pinchSliderObject);
            yield return null;
        }

        /// <summary>
        /// Tests that an interactable assembled at runtime can be manipulated
        /// </summary>
        [UnityTest]
        public IEnumerator TestAssembleInteractableAndNearManip()
        {
            GameObject pinchSliderObject;
            PinchSlider slider;

            // This should not throw exception
            AssembleSlider(Vector3.forward, Vector3.zero, out pinchSliderObject, out slider);

            Debug.Assert(slider.SliderValue == 0.5, "Slider should have value 0.5 at start");
            yield return DirectPinchAndMoveSlider(slider, 1.0f);
            Debug.Assert(slider.SliderValue == 1.0, "Slider should have value 1.0 after being manipulated at start");

            // clean up
            GameObject.Destroy(pinchSliderObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestLoadPrefabAndNearManip()
        {
            GameObject pinchSliderObject;
            PinchSlider slider;

            // This should not throw exception
            InstantiateDefaultSliderPrefab(Vector3.forward, Vector3.zero, out pinchSliderObject, out slider);

            Debug.Assert(slider.SliderValue == 0.5, "Slider should have value 0.5 at start");
            yield return DirectPinchAndMoveSlider(slider, 1.0f);
            Debug.Assert(slider.SliderValue == 1.0, "Slider should have value 1.0 after being manipulated at start");

            // clean up
            GameObject.Destroy(pinchSliderObject);
            yield return null;
        }

        /// <summary>
        /// Tests that slider can be assembled from code and manipulated using GGV
        /// </summary>
        [UnityTest]
        public IEnumerator TestAssembleInteractableAndFarManip()
        {
            GameObject pinchSliderObject;
            PinchSlider slider;

            // This should not throw exception
            AssembleSlider(Vector3.forward, Vector3.zero, out pinchSliderObject, out slider);

            Debug.Assert(slider.SliderValue == 0.5, "Slider should have value 0.5 at start");

            // Set up ggv simulation
            PlayModeTestUtilities.PushHandSimulationProfile();
            PlayModeTestUtilities.SetHandSimulationMode(HandSimulationMode.Gestures);

            var rightHand = new TestHand(Handedness.Right);
            Vector3 initialPos = new Vector3(0.05f, 0, 1.0f);
            yield return rightHand.Show(initialPos);
            yield return rightHand.SetGesture(ArticulatedHandPose.GestureId.Pinch);

            yield return rightHand.Move(new Vector3(0.1f, 0, 0));
            yield return rightHand.SetGesture(ArticulatedHandPose.GestureId.Open);
            yield return rightHand.Hide();

            Assert.That(slider.SliderValue, Is.GreaterThan(0.5));

            // clean up
            GameObject.Destroy(pinchSliderObject);
            PlayModeTestUtilities.PopHandSimulationProfile();
        }
        
        /// <summary>
        /// Tests that interactable raises proper events
        /// </summary>
        [UnityTest]
        public IEnumerator TestAssembeInteractableAndEventsRaised()
        {
            GameObject pinchSliderObject;
            PinchSlider slider;

            // This should not throw exception
            AssembleSlider(Vector3.forward, Vector3.zero, out pinchSliderObject, out slider);

            var rightHand = new TestHand(Handedness.Right);
            Vector3 initialPos = new Vector3(0.05f, 0, 1.0f);

            bool interactionStarted = false;
            slider.OnInteractionStarted.AddListener((x) => interactionStarted = true);
            yield return rightHand.Show(initialPos);
            yield return rightHand.SetGesture(ArticulatedHandPose.GestureId.Pinch);

            Assert.IsTrue(interactionStarted, "Slider did not raise interaction started.");

            bool interactionUpdated = false;
            slider.OnValueUpdated.AddListener((x) => interactionUpdated = true);

            yield return rightHand.Move(new Vector3(0.1f, 0, 0));

            Assert.IsTrue(interactionUpdated, "Slider did not raise SliderUpdated event.");

            bool interactionEnded = false;
            slider.OnInteractionEnded.AddListener((x) => interactionEnded = true);

            yield return rightHand.SetGesture(ArticulatedHandPose.GestureId.Open);
            yield return rightHand.Hide();

            Assert.IsTrue(interactionEnded, "Slider did not raise interaction ended.");

            Assert.That(slider.SliderValue, Is.GreaterThan(0.5));

            GameObject.Destroy(pinchSliderObject);
        }

        #endregion Tests

        #region Private methods

        private IEnumerator DirectPinchAndMoveSlider(PinchSlider slider, float toSliderValue)
        {
            Debug.Log($"moving hand to value {toSliderValue}");
            var rightHand = new TestHand(Handedness.Right);
            Vector3 initialPos = new Vector3(0.05f, 0, 1.0f);
            yield return rightHand.Show(initialPos);
            yield return rightHand.MoveTo(slider.ThumbRoot.transform.position);
            yield return rightHand.SetGesture(ArticulatedHandPose.GestureId.Pinch);
            if (!(toSliderValue >= 0 && toSliderValue <= 1))
            {
                throw new System.ArgumentException("toSliderValue must be between 0 and 1");
            }

            yield return rightHand.MoveTo(Vector3.Lerp(slider.SliderStartPosition, slider.SliderEndPosition, toSliderValue));
            yield return rightHand.SetGesture(ArticulatedHandPose.GestureId.Open);
            yield return rightHand.Hide();
        }

        /// <summary>
        /// Generates an interactable from primitives and assigns a select action.
        /// </summary>
        private void AssembleSlider(Vector3 position, Vector3 rotation, out GameObject pinchSliderObject, out PinchSlider slider, bool isNearInteractionGrabbable = true)
        {
            // Assemble an interactable out of a set of primitives
            // This will be the slider root
            pinchSliderObject = new GameObject();
            pinchSliderObject.name = "PinchSliderRoot";

            // Make the slider track
            var sliderTrack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject.Destroy(sliderTrack.GetComponent<BoxCollider>());
            sliderTrack.transform.position = Vector3.zero;
            sliderTrack.transform.localScale = new Vector3(1f, .01f, .01f);
            sliderTrack.transform.parent = pinchSliderObject.transform;

            // Make the thumb root
            var thumbRoot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            thumbRoot.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            thumbRoot.transform.parent = pinchSliderObject.transform;
            if (isNearInteractionGrabbable)
            {
                thumbRoot.AddComponent<NearInteractionGrabbable>();
            }

            slider = pinchSliderObject.AddComponent<PinchSlider>();
            slider.ThumbRoot = thumbRoot;

            pinchSliderObject.transform.position = position;
            pinchSliderObject.transform.eulerAngles = rotation;
        }

        /// <summary>
        /// Instantiates the default interactable button.
        /// </summary>
        private void InstantiateDefaultSliderPrefab(Vector3 position, Vector3 rotation, out GameObject sliderObject, out PinchSlider pinchSlider)
        {
            // Load interactable prefab
            Object sliderPrefab = AssetDatabase.LoadAssetAtPath(defaultPinchSliderPrefabPath, typeof(Object));
            sliderObject = Object.Instantiate(sliderPrefab) as GameObject;
            pinchSlider = sliderObject.GetComponent<PinchSlider>();
            Assert.IsNotNull(pinchSlider);

            // Move the object into position
            sliderObject.transform.position = position;
            sliderObject.transform.eulerAngles = rotation;
        }
        #endregion Private methods

    }
}
#endif