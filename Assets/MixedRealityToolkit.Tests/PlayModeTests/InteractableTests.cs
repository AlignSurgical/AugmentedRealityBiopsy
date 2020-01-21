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

using Microsoft.MixedReality.Toolkit.Editor;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Microsoft.MixedReality.Toolkit.Tests
{
    public class InteractableTests : BasePlayModeTests
    {
        private const float ButtonPressAnimationDelay = 0.25f;
        private const float ButtonReleaseAnimationDelay = 0.25f;
        private const float EaseDelay = 0.25f;
        private const string DefaultInteractablePrefabAssetPath = "Assets/MixedRealityToolkit.Examples/Demos/UX/Interactables/Prefabs/Model_PushButton.prefab";
        private const string RadialSetPrefabAssetPath = "Assets/MixedRealityToolkit.SDK/Features/UX/Interactable/Prefabs/RadialSet.prefab";
        private const string PressableHoloLens2TogglePrefabPath = "Assets/MixedRealityToolkit.SDK/Features/UX/Interactable/Prefabs/PressableButtonHoloLens2Toggle.prefab";
        private const string PressableHoloLens2PrefabPath = "Assets/MixedRealityToolkit.SDK/Features/UX/Interactable/Prefabs/PressableButtonHoloLens2.prefab";
        private const string RadialPrefabAssetPath = "Assets/MixedRealityToolkit.SDK/Features/UX/Interactable/Prefabs/Radial.prefab";
        private static string DisabledOnStartPrefabAssetPath = "Assets/MixedRealityToolkit.Tests/PlayModeTests/Prefabs/Model_PushButton_DisabledOnStart.prefab";

        private readonly Color DefaultColor = Color.blue;
        private readonly Color FocusColor = Color.yellow;
        private readonly Color DisabledColor = Color.gray;

        private static readonly Quaternion DefaultRotation = Quaternion.LookRotation(Vector3.up);
        private static readonly Quaternion DefaultRotationToggle = Quaternion.LookRotation(Vector3.forward);

        private static InputSimulationService inputSimulationService = null;
        private const int MoveHandNumSteps = 32;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            TestUtilities.PlayspaceToOriginLookingForward();

            inputSimulationService = PlayModeTestUtilities.GetInputSimulationService();
        }

        /// <summary>
        /// Instantiates a push button prefab and uses simulated hand input to press it.
        /// </summary>
        [UnityTest]
        public IEnumerator TestHandInputOnPrefab()
        {
            // Load interactable prefab
            Interactable interactable;
            Transform translateTargetObject;

            InstantiatePressableButtonPrefab(
                new Vector3(0.025f, 0.05f, 0.5f),
                DefaultRotation,
                DefaultInteractablePrefabAssetPath,
                "Cylinder",
                out interactable,
                out translateTargetObject);

            // Subscribe to interactable's on click so we know the click went through
            bool wasClicked = false;
            interactable.OnClick.AddListener(() => { wasClicked = true; });

            Vector3 targetStartPosition = translateTargetObject.localPosition;

            yield return TestClickPushButton(targetStartPosition, translateTargetObject);

            Assert.True(wasClicked, "Interactable was not clicked.");

            //Cleanup
            GameObject.Destroy(interactable.gameObject);
        }

        /// <summary>
        /// Assembles a push button from primitives and uses simulated hand input to press it.
        /// </summary>
        [UnityTest]
        public IEnumerator TestHandInputOnRuntimeAssembled()
        {
            // Load interactable
            Interactable interactable;
            Transform translateTargetObject;

            AssembleInteractableButton(
                out interactable,
                out translateTargetObject);

            interactable.transform.position = new Vector3(0.025f, 0.05f, 0.65f);
            interactable.transform.eulerAngles = new Vector3(-90f, 0f, 0f);

            // Subscribe to interactable's on click so we know the click went through
            bool wasClicked = false;
            interactable.OnClick.AddListener(() => { wasClicked = true; });

            Vector3 targetStartPosition = translateTargetObject.transform.localPosition;

            yield return null;

            // Add a touchable and configure for touch events
            NearInteractionTouchable touchable = interactable.gameObject.AddComponent<NearInteractionTouchable>();
            touchable.EventsToReceive = TouchableEventType.Touch;
            touchable.SetBounds(Vector2.one);
            touchable.SetLocalForward(Vector3.up);
            touchable.SetLocalUp(Vector3.forward);
            touchable.SetLocalCenter(Vector3.up * 2.75f);

            // Add a touch handler and link touch started / touch completed events
            TouchHandler touchHandler = interactable.gameObject.AddComponent<TouchHandler>();
            touchHandler.OnTouchStarted.AddListener((HandTrackingInputEventData e) => interactable.SetInputDown());
            touchHandler.OnTouchCompleted.AddListener((HandTrackingInputEventData e) => interactable.SetInputUp());

            yield return TestClickPushButton(targetStartPosition, translateTargetObject);

            Assert.True(wasClicked, "Interactable was not clicked.");

            //Cleanup
            GameObject.Destroy(interactable.gameObject);
        }

        /// <summary>
        /// Instantiates a push button prefab and uses simulated global input events to press it. Test that global input behaves correctly when Interactable IsEnabled and not
        /// </summary>
        [UnityTest]
        public IEnumerator TestSelectGlobalInput()
        {
            // Load interactable prefab
            Interactable interactable;
            Transform translateTargetObject;

            // Place out of the way of any pointers
            InstantiatePressableButtonPrefab(
                new Vector3(10f, 0.0f, 0.5f),
                DefaultRotation,
                DefaultInteractablePrefabAssetPath,
                "Cylinder",
                out interactable,
                out translateTargetObject);

            // Subscribe to interactable's on click so we know the click went through
            bool wasClicked = false;
            interactable.OnClick.AddListener(() => { wasClicked = true; });

            // Set interactable to global and disabled
            interactable.IsEnabled = false;
            interactable.IsGlobal = true;

            Vector3 targetStartPosition = translateTargetObject.localPosition;

            yield return null;

            // Find an input source to associate with the input event (doesn't matter which one)
            IMixedRealityInputSource defaultInputSource = CoreServices.InputSystem.DetectedInputSources.FirstOrDefault();
            Assert.NotNull(defaultInputSource, "At least one input source must be present for this test to work.");

            yield return RunGlobalClick(defaultInputSource, interactable.InputAction, targetStartPosition, translateTargetObject, false);

            Assert.False(wasClicked, "Interactable was not clicked.");
            Assert.False(interactable.HasFocus, "Interactable had focus");
            Assert.False(interactable.IsVisited, "Interactable was not visited");

            interactable.IsEnabled = true;

            yield return RunGlobalClick(defaultInputSource, interactable.InputAction, targetStartPosition, translateTargetObject);

            Assert.True(wasClicked, "Interactable was not clicked.");
            Assert.False(interactable.HasFocus, "Interactable had focus");
            Assert.True(interactable.IsVisited, "Interactable was not visited");

            // Unregister global handlers
            interactable.IsGlobal = false;

            // Remove as global listener and cleanup
            GameObject.Destroy(interactable.gameObject);
        }

        /// <summary>
        /// Assembles a push button from primitives and uses simulated input events to press it.
        /// </summary>
        [UnityTest]
        public IEnumerator TestInputActions()
        {
            Interactable interactable;
            Transform translateTargetObject;

            AssembleInteractableButton(
                out interactable,
                out translateTargetObject);

            interactable.transform.position = new Vector3(0.0f, 0.0f, 0.5f);
            interactable.transform.eulerAngles = new Vector3(-90f, 0f, 0f);

            // Subscribe to interactable's on click and on press receiver so we know the click went through
            bool wasClicked = false;
            interactable.OnClick.AddListener(() => { wasClicked = true; });
            var pressReceiver = interactable.AddReceiver<InteractableOnPressReceiver>();
            bool wasPressed = false;
            pressReceiver.OnPress.AddListener(() => { wasPressed = true; Debug.Log("pressReciever wasPressed true"); });
            bool wasReleased = false;
            pressReceiver.OnRelease.AddListener(() => { wasReleased = true; Debug.Log("pressReciever wasReleased true"); });

            Vector3 targetStartPosition = translateTargetObject.localPosition;

            yield return null;

            //
            // Test Select Input Action
            //

            // Find an input source to associate with the input event (doesn't matter which one)
            IMixedRealityInputSource defaultInputSource = CoreServices.InputSystem.DetectedInputSources.FirstOrDefault();
            Assert.NotNull(defaultInputSource, "At least one input source must be present for this test to work.");

            yield return RunGlobalClick(defaultInputSource, interactable.InputAction, targetStartPosition, translateTargetObject);

            Assert.True(wasClicked, "Interactable was not clicked.");
            Assert.True(wasPressed, "interactable not pressed");
            Assert.True(wasReleased, "interactable not released");
            Assert.AreEqual(targetStartPosition, translateTargetObject.localPosition, "Transform target object was not translated back by action.");

            //
            // Test Menu Input Action
            //

            // Find the menu action from the input system profile
            MixedRealityInputAction menuAction = CoreServices.InputSystem.InputSystemProfile.InputActionsProfile.InputActions.Where(m => m.Description == "Menu").FirstOrDefault();
            Assert.NotNull(menuAction.Description, "Couldn't find menu input action in input system profile.");

            // Set the interactable to respond to a 'menu' input action
            interactable.InputAction = menuAction;
            // Reset state tracking
            wasClicked = wasPressed = wasReleased = false;

            yield return PlayModeTestUtilities.WaitForInputSystemUpdate();

            yield return RunGlobalClick(defaultInputSource, menuAction, targetStartPosition, translateTargetObject);

            Assert.True(wasClicked, "Interactable was not clicked.");
            Assert.True(wasPressed, "interactable not pressed");
            Assert.True(wasReleased, "interactable not released");
            Assert.AreEqual(targetStartPosition, translateTargetObject.localPosition, "Transform target object was not translated back by action.");

            //Cleanup
            GameObject.Destroy(interactable.gameObject);
        }

        /// <summary>
        /// Assemble an Interactable GameObject and test various SelectionModes
        /// </summary>
        [UnityTest]
        public IEnumerator TestDimensions()
        {
            // Load interactable
            Interactable interactable;
            Transform translateTargetObject;

            AssembleInteractableButton(
                out interactable,
                out translateTargetObject);

            // Test Button type
            interactable.NumOfDimensions = 1;
            Assert.AreEqual(SelectionModes.Button, interactable.ButtonMode, "Interactable should be in button selection mode");

            interactable.IsToggled = true;
            Assert.IsFalse(interactable.IsToggled, "Interactable should not be in toggle state because not in Toggle selection mode");

            // Test Multi-Dimension type
            interactable.NumOfDimensions = 4;
            interactable.CurrentDimension = 1;
            Assert.AreEqual(SelectionModes.MultiDimension, interactable.ButtonMode, "Interactable should be in MultiDimension selection mode");

            interactable.IsToggled = true;
            Assert.IsFalse(interactable.IsToggled, "Interactable should not be in toggle state because not in Toggle selection mode");

            // Test Toggle type
            interactable.NumOfDimensions = 2;
            interactable.CurrentDimension = 0;
            Assert.AreEqual(SelectionModes.Toggle, interactable.ButtonMode, "Interactable should not be in button selection mode");
            Assert.IsFalse(interactable.IsToggled, "Switching dimensions and setting CurrentDimension to 0 should make IsToggled off");

            interactable.IsToggled = true;
            Assert.IsTrue(interactable.IsToggled, "Interactable should be in toggle state because in Toggle selection mode");

            // Test Invalid type
            interactable.NumOfDimensions = -1;
            Assert.AreEqual(2, interactable.NumOfDimensions, "Interactable should be in Toggle selection mode");
            Assert.AreEqual(1, interactable.CurrentDimension, "Interactable should be Toggled");
            Assert.AreEqual(SelectionModes.Toggle, interactable.ButtonMode, "Interactable should be in Toggle selection mode");
            Assert.IsTrue(interactable.IsToggled, "Invalid Dimension should not change state");

            // Test toggle off via code API
            interactable.TriggerOnClick();
            Assert.AreEqual(2, interactable.NumOfDimensions, "Interactable should be in Toggle selection mode");
            Assert.AreEqual(0, interactable.CurrentDimension, "Interactable should be Toggled off");
            Assert.AreEqual(SelectionModes.Toggle, interactable.ButtonMode, "Interactable should be in Toggle selection mode");
            Assert.False(interactable.IsToggled, "Invalid Dimension should not change state");

            //Clean up
            GameObject.Destroy(interactable.gameObject);

            yield return null;
        }

        /// <summary>
        /// Instantiates a push button prefab and uses simulated voice input events to press it, both when IsEnabled and not
        /// </summary>
        [UnityTest]
        public IEnumerator TestVoiceInputOnPrefab()
        {
            // Load interactable prefab
            Interactable interactable;
            Transform translateTargetObject;

            InstantiatePressableButtonPrefab(
                new Vector3(0.0f, 0.0f, 0.5f),
                DefaultRotation,
                DefaultInteractablePrefabAssetPath,
                "Cylinder",
                out interactable,
                out translateTargetObject);

            // Subscribe to interactable's on click so we know the click went through
            bool wasClicked = false;
            interactable.OnClick.AddListener(() => { wasClicked = true; });

            Vector3 targetStartPosition = translateTargetObject.localPosition;

            // Set up its voice command
            interactable.VoiceCommand = "Select";
            interactable.VoiceRequiresFocus = false;

            // Find an input source to associate with the input event (doesn't matter which one)
            IMixedRealityInputSource defaultInputSource = CoreServices.InputSystem.DetectedInputSources.FirstOrDefault();
            Assert.NotNull(defaultInputSource, "At least one input source must be present for this test to work.");

            //
            // Test speech when disabled
            //

            interactable.IsEnabled = false;

            yield return PlayModeTestUtilities.WaitForInputSystemUpdate();
            yield return FireSpeechCommand(interactable.VoiceCommand, interactable.InputAction, defaultInputSource);
            yield return CheckButtonTranslation(targetStartPosition, translateTargetObject, false);

            Assert.False(wasClicked, "Interactable was clicked.");
            Assert.False(interactable.IsVisited, "Interactable was visited.");

            //
            // Test speech when enabled
            //

            interactable.IsEnabled = true;

            yield return PlayModeTestUtilities.WaitForInputSystemUpdate();
            yield return FireSpeechCommand(interactable.VoiceCommand, interactable.InputAction, defaultInputSource);
            yield return CheckButtonTranslation(targetStartPosition, translateTargetObject);

            Assert.True(wasClicked, "Interactable was not clicked.");
            Assert.True(interactable.IsVisited, "Interactable was not visited.");

            //Cleanup
            GameObject.Destroy(interactable.gameObject);
        }

        /// <summary>
        /// Test touch input on Interactable by looking at state changes, both for when IsEnabled and not
        /// </summary>
        [UnityTest]
        public IEnumerator TestTouchInput()
        {
            Interactable interactable;
            Transform translateTargetObject;

            InstantiatePressableButtonPrefab(
                new Vector3(0.0f, 0.0f, 0.5f),
                DefaultRotation,
                DefaultInteractablePrefabAssetPath,
                "Cylinder",
                out interactable,
                out translateTargetObject);

            var touchableVolume = interactable.gameObject.AddComponent<NearInteractionTouchableVolume>();

            //
            // Test touch when disabled
            //

            interactable.IsEnabled = false;

            yield return MoveHandToButton();
            yield return PlayModeTestUtilities.WaitForInputSystemUpdate();

            Assert.False(interactable.HasPhysicalTouch);
            Assert.False(interactable.HasPress);

            yield return MoveHandAwayFromButton();
            yield return PlayModeTestUtilities.WaitForInputSystemUpdate();

            //
            // Test touch when enabled
            //

            interactable.IsEnabled = true;

            yield return MoveHandToButton();
            yield return PlayModeTestUtilities.WaitForInputSystemUpdate();

            Assert.True(interactable.HasPhysicalTouch);
            Assert.True(interactable.HasPress);

            yield return MoveHandAwayFromButton();
            yield return PlayModeTestUtilities.WaitForInputSystemUpdate();

            Assert.False(interactable.HasPhysicalTouch);
            Assert.False(interactable.HasPress);

            //Cleanup
            GameObject.Destroy(interactable.gameObject);
        }

        /// <summary>
        /// Instantiates a runtime assembled Interactable and set Interactable state to disabled (not disabling the GameObject/component)
        /// </summary>
        [UnityTest]
        public IEnumerator TestDisabledStateThemes()
        {
            Interactable interactable;
            Transform translateTargetObject;

            AssembleInteractableButton(
                out interactable,
                out translateTargetObject);

            CameraCache.Main.transform.LookAt(interactable.transform.position);

            yield return new WaitForSeconds(EaseDelay);
            var propBlock = InteractableThemeShaderUtils.GetPropertyBlock(translateTargetObject.gameObject);
            Assert.AreEqual(propBlock.GetColor("_Color"), FocusColor);

            interactable.IsEnabled = false;

            yield return new WaitForSeconds(EaseDelay);
            propBlock = InteractableThemeShaderUtils.GetPropertyBlock(translateTargetObject.gameObject);
            Assert.AreEqual(propBlock.GetColor("_Color"), DisabledColor);
            Assert.False(interactable.IsEnabled);
            Assert.False(interactable.HasFocus);

            //Cleanup
            GameObject.Destroy(interactable.gameObject);
        }

        /// <summary>
        /// Instantiates a runtime assembled Interactable and destroy the Interactable component
        /// </summary>
        [UnityTest]
        public IEnumerator TestDestroy()
        {
            // Load interactable
            Interactable interactable;
            Transform translateTargetObject;

            AssembleInteractableButton(
                out interactable,
                out translateTargetObject);

            // Put GGV focus on the Interactable button
            CameraCache.Main.transform.LookAt(interactable.transform.position);

            yield return new WaitForSeconds(EaseDelay);
            var propBlock = InteractableThemeShaderUtils.GetPropertyBlock(translateTargetObject.gameObject);
            Assert.AreEqual(propBlock.GetColor("_Color"), FocusColor);

            // Destroy the interactable component
            GameObject.Destroy(interactable);

            // Remove focus
            CameraCache.Main.transform.LookAt(Vector3.zero);

            yield return null;
            propBlock = InteractableThemeShaderUtils.GetPropertyBlock(translateTargetObject.gameObject);
            Assert.AreEqual(propBlock.GetColor("_Color"), FocusColor);
        }

        /// <summary>
        /// Tests button depth and focus state after enabling, disabling and re-enabling Interactable 
        /// internally via IsEnabled. The focus state after re-enabling should be false and button
        /// depth should be in its default position.  This test is specifically addressing behavior described 
        /// in issue 4967.
        /// </summary>
        [UnityTest]
        public IEnumerator TestDisableOnClick()
        {
            // Load the Model_PushButton interactable prefab
            Interactable interactable;
            Transform innerCylinderTransform;

            InstantiatePressableButtonPrefab(
                new Vector3(0.0f, 0.0f, 0.5f),
                DefaultRotation,
                DefaultInteractablePrefabAssetPath,
                "Cylinder",
                out interactable,
                out innerCylinderTransform);

            Assert.True(interactable.IsEnabled);

            // OnClick, set Interactable IsEnabled false or aka disabled
            interactable.OnClick.AddListener(() => { interactable.IsEnabled = false; });

            // Get start position of the inner cylinder before button is pressed
            Vector3 innerCylinderStartPosition = innerCylinderTransform.localPosition;

            yield return TestClickPushButton(innerCylinderStartPosition, innerCylinderTransform);

            Assert.False(interactable.IsEnabled, "Interactable should be disabled");

            // Re-enable Interactable
            interactable.IsEnabled = true;
            yield return PlayModeTestUtilities.WaitForInputSystemUpdate();

            // Make sure the button depth is back at the starting position when re-enable the gameObject and states have reset
            Assert.True(innerCylinderTransform.localPosition == innerCylinderStartPosition);
            Assert.False(interactable.HasFocus, "Interactable has focus");
            Assert.True(interactable.IsVisited, "Interactable was not visited");

            GameObject.Destroy(interactable.gameObject);
        }

        /// <summary>
        /// Tests that Interactable configured not Enabled on start works as expected.
        /// Enabled on start is an editor level setting only that is applied on Awake/Start
        /// </summary>
        [UnityTest]
        public IEnumerator TestDisabledOnStart()
        {
            // Instantiate model_pushbutton prefab but with enabled on start false
            var prefab = InstantiateInteractableFromPath(
                                new Vector3(0.025f, 0.05f, 0.5f),
                                DefaultRotation,
                                DisabledOnStartPrefabAssetPath);
            Interactable interactable = prefab.GetComponent<Interactable>();

            Assert.False(interactable.IsEnabled, "Test Prefab has been corrupted. Should be disabled on start");

            // Find the target object for the interactable transformation
            var pressButtonCylinder = interactable.transform.Find("Cylinder");
            Assert.IsNotNull(pressButtonCylinder, "Object 'Cylinder' could not be found under example object Model_PushButton.");

            // Subscribe to interactable's on click so we know the click went through
            bool wasClicked = false;
            interactable.OnClick.AddListener(() => { wasClicked = true; });

            Vector3 targetStartPosition = pressButtonCylinder.localPosition;

            //
            // Test starting as disabled
            //
            yield return TestClickPushButton(targetStartPosition, pressButtonCylinder, false);

            Assert.False(wasClicked, "Interactable was clicked.");

            //
            // Test when enabled
            //
            interactable.IsEnabled = true;

            yield return TestClickPushButton(targetStartPosition, pressButtonCylinder, true);

            Assert.True(wasClicked, "Interactable was not clicked.");

            // Cleanup
            GameObject.Destroy(interactable.gameObject);
        }

        /// <summary>
        /// Test the TriggerOnClick API for Interactable both when IsEnabled and not. Button should fire OnClick and move UI
        /// </summary>
        [UnityTest]
        public IEnumerator TestTriggerOnClick()
        {
            Interactable interactable;
            Transform innerCylinderTransform;

            InstantiatePressableButtonPrefab(
                new Vector3(0.0f, 0.0f, 0.5f),
                DefaultRotation,
                DefaultInteractablePrefabAssetPath,
                "Cylinder",
                out interactable,
                out innerCylinderTransform);

            // Subscribe to interactable's on click so we know the click went through
            bool wasClicked = false;
            interactable.OnClick.AddListener(() => { wasClicked = true; });

            Vector3 targetStartPosition = innerCylinderTransform.localPosition;

            //
            // Test TriggerOnClick when disabled
            //
            interactable.IsEnabled = false;

            interactable.TriggerOnClick();

            Assert.False(wasClicked, "Interactable was clicked.");
            Assert.False(interactable.IsVisited, "Interactable was visited.");

            //
            // Test TriggerOnClick when enabled
            //
            interactable.IsEnabled = true;

            interactable.TriggerOnClick();
            yield return new WaitForSeconds(ButtonReleaseAnimationDelay);

            Assert.True(wasClicked, "Interactable was not clicked.");
            Assert.True(interactable.IsVisited, "Interactable was not visited.");

            // Cleanup
            GameObject.Destroy(interactable.gameObject);
        }

        /// <summary>
        /// Tests that radial buttons can be selected and deselected, and that a radial button
        /// set allows just one button to be selected at a time
        /// </summary>
        [UnityTest]
        public IEnumerator TestRadialSetPrefab()
        {
            var radialSet = InstantiateInteractableFromPath(Vector3.forward, Quaternion.identity, RadialSetPrefabAssetPath);
            var firstRadialButton = radialSet.transform.Find("Radial (1)").GetComponent<Interactable>();
            var secondRadialButton = radialSet.transform.Find("Radial (2)").GetComponent<Interactable>();
            var thirdRadialButton = radialSet.transform.Find("Radial (3)").GetComponent<Interactable>();
            var testHand = new TestHand(Handedness.Right);
            yield return testHand.Show(Vector3.zero);

            Assert.IsTrue(firstRadialButton.IsToggled);
            Assert.IsFalse(secondRadialButton.IsToggled);
            Assert.IsFalse(thirdRadialButton.IsToggled);

            var aBitBack = Vector3.forward * -0.2f;
            yield return testHand.MoveTo(secondRadialButton.transform.position);
            yield return testHand.Move(aBitBack);

            Assert.IsFalse(firstRadialButton.IsToggled);
            Assert.IsFalse(firstRadialButton.HasFocus);
            Assert.IsTrue(secondRadialButton.IsToggled);
            Assert.IsTrue(secondRadialButton.HasFocus);
            Assert.IsFalse(thirdRadialButton.IsToggled);
            Assert.IsFalse(thirdRadialButton.HasFocus);

            yield return testHand.MoveTo(thirdRadialButton.transform.position);
            yield return testHand.Move(aBitBack);

            Assert.IsFalse(firstRadialButton.IsToggled);
            Assert.IsFalse(firstRadialButton.HasFocus);
            Assert.IsFalse(secondRadialButton.IsToggled);
            Assert.IsFalse(secondRadialButton.HasFocus);
            Assert.IsTrue(thirdRadialButton.IsToggled);
            Assert.IsTrue(thirdRadialButton.HasFocus);

            //Cleanup
            GameObject.Destroy(radialSet);
        }

        /// <summary>
        /// Tests that the toggle button states consistently return to original state
        /// after subsequent clicks (front plate does not move back after every click).
        /// </summary>
        [UnityTest]
        public IEnumerator TestPressableToggleHoloLens2()
        {
            var rightHand = new TestHand(Handedness.Right);
            Vector3 p2 = new Vector3(0.015f, 0f, 0.3f);

            Interactable interactable;
            Transform frontPlateTransform;

            InstantiatePressableButtonPrefab(
                new Vector3(0.0f, 0.1f, 0.4f),
                DefaultRotationToggle,
                PressableHoloLens2TogglePrefabPath,
                "CompressableButtonVisuals/FrontPlate",
                out interactable,
                out frontPlateTransform);

            Assert.True(interactable.IsEnabled);

            bool wasClicked = false;
            interactable.OnClick.AddListener(() => { wasClicked = true; });

            // Get start position of the front plate before button is pressed
            Vector3 frontPlateStartPosition = frontPlateTransform.localPosition;

            yield return rightHand.Show(p2);
            yield return PlayModeTestUtilities.WaitForInputSystemUpdate();
            Assert.IsTrue(interactable.HasFocus, "Interactable does not have focus when hand is pointing at it.");

            int numClicks = 3;
            for (int i = 0; i < numClicks; i++)
            {
                wasClicked = false;
                yield return rightHand.Click();
                // Wait for button animation to complete
                yield return new WaitForSeconds(0.33f);

                Assert.True(wasClicked, "Toggle button was not clicked");
                Assert.AreEqual((i + 1) % 2, interactable.CurrentDimension, $"Toggle button is in incorrect toggle state on click {i}");
                
                // Make sure the button depth is back at the starting position
                Assert.True(frontPlateTransform.localPosition == frontPlateStartPosition, "Toggle button front plate did not return to starting position.");                
            }

            GameObject.Destroy(interactable.gameObject);
        }

        /// <summary>
        /// Test InteractableToggleCollection CurrentIndex updates
        /// </summary>
        [UnityTest]
        public IEnumerator TestInteractableToggleCollectionIndexUpdate()
        {
            InteractableToggleCollection interactableToggleCollection;
            int numRadials = 6;

            AssembleInteractableToggleCollection(
                out interactableToggleCollection,
                numRadials,
                Vector3.forward);

            var toggleList = interactableToggleCollection.ToggleList;

            int[] onClickEventCalled = new int[numRadials];

            // Add listener to each toggle in the toggle collection
            for (int i = 0; i < toggleList.Length; i++)
            {
                int indexClick = i;
                toggleList[i].OnClick.AddListener(() => { onClickEventCalled[indexClick] = 1; });
            }

            for (int j = 0; j < numRadials; j++)
            {
                interactableToggleCollection.CurrentIndex = j;
                yield return null;

                // If the CurrentIndex is changed the toggle should be visually updated and events should be triggered
                for (int i = 0; i < numRadials; i++)
                {
                    bool shouldBeSelected = (i == interactableToggleCollection.CurrentIndex);
                    Assert.AreEqual(shouldBeSelected, interactableToggleCollection.ToggleList[i].IsToggled);

                    int expectedClickCount = (i <= j ? 1 : 0);
                    Assert.AreEqual(onClickEventCalled[i], expectedClickCount);
                }
            }

            //Cleanup
            GameObject.Destroy(interactableToggleCollection.gameObject);
        }

        /// <summary>
        /// Test if Button state is reset when it goes out of focus from a pressed state
        /// </summary>
        [UnityTest]
        public IEnumerator TestButtonStateResetWhenFocusLostAfterPinch()
        {
            Interactable interactable;
            Transform interactableTransform;

            InstantiatePressableButtonPrefab(
                new Vector3(0.0f, 0.1f, 0.4f),
                DefaultRotationToggle,
                PressableHoloLens2PrefabPath,
                "CompressableButtonVisuals/FrontPlate",
                out interactable,
                out interactableTransform);

            Assert.True(interactable.IsEnabled);

            var rightHand = new TestHand(Handedness.Right);
            Vector3 focusPosition = new Vector3(0.015f, 0.015f, 0.3f);
            Vector3 releaseDelta = new Vector3(0.05f, 0, 0);

            // Focus the hand on the Button using the far ray pointer
            yield return rightHand.Show(focusPosition);
            yield return PlayModeTestUtilities.WaitForInputSystemUpdate();

            Assert.True(interactable.HasFocus);
            Assert.False(interactable.HasPress);
            Assert.False(interactable.HasGesture);
            Assert.True(interactable.StateManager.CurrentState().Index == (int)InteractableStates.InteractableStateEnum.Focus, "Interactable State is not Focus");

            // While keeping focus on the Button, engage the pinch gesture
            yield return rightHand.SetGesture(ArticulatedHandPose.GestureId.Pinch);
            yield return PlayModeTestUtilities.WaitForInputSystemUpdate();

            Assert.True(interactable.HasFocus);
            Assert.True(interactable.HasPress);
            Assert.False(interactable.HasGesture);
            Assert.True(interactable.StateManager.CurrentState().Index == (int)InteractableStates.InteractableStateEnum.Pressed, "Interactable State is not Pressed");

            // Move Hand to remove focus. Button should go to Default State
            yield return rightHand.Move(releaseDelta);
            yield return PlayModeTestUtilities.WaitForInputSystemUpdate();

            Assert.False(interactable.HasFocus);
            Assert.False(interactable.HasPress);
            Assert.False(interactable.HasGesture);
            Assert.True(interactable.StateManager.CurrentState().Index == (int)InteractableStates.InteractableStateEnum.Default, "Interactable State is not Default");

            // Open hand. Button should stay on Default State
            yield return rightHand.SetGesture(ArticulatedHandPose.GestureId.Open);
            yield return PlayModeTestUtilities.WaitForInputSystemUpdate();

            Assert.False(interactable.HasFocus);
            Assert.False(interactable.HasPress);
            Assert.False(interactable.HasGesture);
            Assert.True(interactable.StateManager.CurrentState().Index == (int)InteractableStates.InteractableStateEnum.Default, "Interactable State is not Default");

            // Move Hand back to Initial position and Pinch. Button should go to Pressed State
            yield return rightHand.Move(-releaseDelta);
            yield return rightHand.SetGesture(ArticulatedHandPose.GestureId.Pinch);
            yield return PlayModeTestUtilities.WaitForInputSystemUpdate();

            Assert.True(interactable.HasFocus);
            Assert.True(interactable.HasPress);
            Assert.False(interactable.HasGesture);
            Assert.True(interactable.StateManager.CurrentState().Index == (int)InteractableStates.InteractableStateEnum.Pressed, "Interactable State is not Pressed");

            // Open Hand. Button should go to Focus State
            yield return rightHand.SetGesture(ArticulatedHandPose.GestureId.Open);
            yield return PlayModeTestUtilities.WaitForInputSystemUpdate();

            Assert.True(interactable.HasFocus);
            Assert.False(interactable.HasPress);
            Assert.False(interactable.HasGesture);
            Assert.True(interactable.StateManager.CurrentState().Index == (int)InteractableStates.InteractableStateEnum.Focus, "Interactable State is not Focus");

            GameObject.Destroy(interactable.gameObject);
        }

        #region Test Helpers

        /// <summary>
        /// Generates an interactable from primitives and assigns a select action.
        /// </summary>
        private void AssembleInteractableButton(out Interactable interactable, out Transform translateTargetObject, string selectActionDescription = "Select")
        {
            // Assemble an interactable out of a set of primitives
            // This will be the button housing
            var interactableObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            interactableObject.name = "RuntimeInteractable";
            interactableObject.transform.position = new Vector3(0.05f, 0.05f, 0.625f);
            interactableObject.transform.localScale = new Vector3(0.15f, 0.025f, 0.15f);
            interactableObject.transform.eulerAngles = new Vector3(90f, 0f, 180f);

            // This will be the part that gets scaled
            GameObject childObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            var renderer = childObject.GetComponent<Renderer>();
            renderer.material.color = DefaultColor;
            renderer.material.shader = StandardShaderUtility.MrtkStandardShader;

            childObject.transform.parent = interactableObject.transform;
            childObject.transform.localScale = new Vector3(0.9f, 1f, 0.9f);
            childObject.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            childObject.transform.localRotation = Quaternion.identity;
            // Only use a collider on the main object
            GameObject.Destroy(childObject.GetComponent<Collider>());

            translateTargetObject = childObject.transform;

            // Add an interactable
            interactable = interactableObject.AddComponent<Interactable>();

            var themeDefinition = ThemeDefinition.GetDefaultThemeDefinition<ScaleOffsetColorTheme>().Value;
            // themeDefinition.Easing.Enabled = false;
            // Set the offset state property (index = 1) to move on the Pressed state (index = 2)
            themeDefinition.StateProperties[1].Values = new List<ThemePropertyValue>()
            {
                new ThemePropertyValue() { Vector3 = Vector3.zero},
                new ThemePropertyValue() { Vector3 = Vector3.zero},
                new ThemePropertyValue() { Vector3 = new Vector3(0.0f, -0.32f, 0.0f)},
                new ThemePropertyValue() { Vector3 = Vector3.zero},
            };
            // Set the color state property (index = 2) values
            themeDefinition.StateProperties[2].Values = new List<ThemePropertyValue>()
            {
                new ThemePropertyValue() { Color = DefaultColor},
                new ThemePropertyValue() { Color = FocusColor},
                new ThemePropertyValue() { Color = Color.green},
                new ThemePropertyValue() { Color = DisabledColor},
            };

            Theme testTheme = ScriptableObject.CreateInstance<Theme>();
            testTheme.States = interactable.States;
            testTheme.Definitions = new List<ThemeDefinition>() { themeDefinition };

            interactable.Profiles = new List<InteractableProfileItem>()
            {
                new InteractableProfileItem()
                {
                    Themes = new List<Theme>() { testTheme },
                    Target = translateTargetObject.gameObject,
                },
            };

            // Set the interactable to respond to the requested input action
            MixedRealityInputAction selectAction = CoreServices.InputSystem.InputSystemProfile.InputActionsProfile.InputActions.Where(m => m.Description == selectActionDescription).FirstOrDefault();
            Assert.NotNull(selectAction.Description, "Couldn't find " + selectActionDescription + " input action in input system profile.");
            interactable.InputAction = selectAction;
        }

        /// <summary>
        /// Generates an InteractableToggleCollection from radial prefabs
        /// </summary>
        private void AssembleInteractableToggleCollection(out InteractableToggleCollection interactableToggleCollection, int numRadials, Vector3 pos)
        {
            GameObject toggleCollection = new GameObject("ToggleCollection");
            interactableToggleCollection = toggleCollection.AddComponent<InteractableToggleCollection>();

            // Instantiate radial prefabs with toggleCollection as the parent
            for (int i = 0; i < numRadials; i++)
            {
                var radial = InstantiateInteractableFromPath(pos + new Vector3(0.1f, i * 0.1f, 0), Quaternion.identity, RadialPrefabAssetPath);
                radial.name = "Radial " + i;
                Assert.IsNotNull(radial);
                radial.transform.parent = toggleCollection.transform;
            }

            interactableToggleCollection.ToggleList = toggleCollection.GetComponentsInChildren<Interactable>();
        }

        private GameObject InstantiateInteractableFromPath(Vector3 position, Quaternion rotation, string path)
        {
            // Load interactable prefab
            Object interactablePrefab = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
            GameObject result = Object.Instantiate(interactablePrefab) as GameObject;
            Assert.IsNotNull(result);

            // Move the object into position
            result.transform.position = position;
            result.transform.rotation = rotation;
            return result;
        }

        /// <summary>
        /// Instantiates Hololens Pressable Button from different Prefabs
        /// </summary>
        private void InstantiatePressableButtonPrefab(Vector3 position, Quaternion rotation, string prefabPath, string translateTargetPath, out Interactable interactable, out Transform translateTargetTransform)
        {
            // Load interactable prefab
            var interactableObject = InstantiateInteractableFromPath(position, rotation, prefabPath);
            interactable = interactableObject.GetComponent<Interactable>();
            Assert.IsNotNull(interactable);

            // Find the target object for the interactable transformation
            translateTargetTransform = interactableObject.transform.Find(translateTargetPath);

            Assert.IsNotNull(translateTargetTransform, $"Object {translateTargetPath} could not be found under Button instantiated from {prefabPath}.");
        }

        private IEnumerator CheckButtonTranslation(Vector3 targetStartPosition, Transform translateTarget, bool shouldTranslate = true)
        {
            bool wasTranslated = false;
            float pressEndTime = Time.time + ButtonPressAnimationDelay;
            while (Time.time < pressEndTime)
            {   // If the transform is moved at any point during this interval, we were successful
                yield return new WaitForFixedUpdate();
                wasTranslated |= targetStartPosition != translateTarget.localPosition;
            }

            Assert.AreEqual(shouldTranslate, wasTranslated, "Transform target object did or did not translate properly by action.");
        }

        private IEnumerator MoveHandToButton()
        {
            Vector3 p1 = Vector3.zero;
            Vector3 p2 = new Vector3(0.05f, 0f, 0.51f);

            // Move the hand towards
            yield return PlayModeTestUtilities.ShowHand(Handedness.Right, inputSimulationService);
            yield return PlayModeTestUtilities.MoveHandFromTo(p1, p2, MoveHandNumSteps , ArticulatedHandPose.GestureId.Poke, Handedness.Right, inputSimulationService);
        }

        private IEnumerator MoveHandAwayFromButton()
        {
            Vector3 p2 = new Vector3(0.05f, 0f, 0.51f);
            Vector3 p3 = Vector3.zero;

            // Move the hand back
            yield return PlayModeTestUtilities.MoveHandFromTo(p2, p3, MoveHandNumSteps , ArticulatedHandPose.GestureId.Poke, Handedness.Right, inputSimulationService);
            yield return PlayModeTestUtilities.HideHand(Handedness.Right, inputSimulationService);
        }

        private IEnumerator TestClickPushButton(Vector3 targetStartPosition, Transform translateTargetObject, bool shouldClick = true)
        {
            yield return MoveHandToButton();

            yield return CheckButtonTranslation(targetStartPosition, translateTargetObject, shouldClick);

            yield return MoveHandAwayFromButton();

            yield return new WaitForSeconds(ButtonReleaseAnimationDelay);
        }

        private IEnumerator FireSpeechCommand(string voiceCommand, MixedRealityInputAction inputAction, IMixedRealityInputSource inputSource = null)
        {
            if (inputSource == null)
            {
                // Find an input source to associate with the input event (doesn't matter which one)
                IMixedRealityInputSource defaultInputSource = CoreServices.InputSystem.DetectedInputSources.FirstOrDefault();
                Assert.NotNull(defaultInputSource, "At least one input source must be present for this test to work.");
                inputSource = defaultInputSource;
            }

            // Raise a voice select input event, then wait for transition to take place
            // Wait for at least one frame explicitly to ensure the input goes through
            SpeechCommands commands = new SpeechCommands(voiceCommand, KeyCode.None, inputAction);
            CoreServices.InputSystem.RaiseSpeechCommandRecognized(inputSource, RecognitionConfidenceLevel.High, new System.TimeSpan(100), System.DateTime.Now, commands);
            yield return PlayModeTestUtilities.WaitForInputSystemUpdate();
        }

        private IEnumerator RunGlobalClick(IMixedRealityInputSource defaultInputSource, 
            MixedRealityInputAction inputAction, 
            Vector3 targetStartPosition, 
            Transform translateTargetObject, 
            bool shouldTranslate = true)
        {
            // Raise a select down input event, then wait for transition to take place
            // Wait for at least one frame explicitly to ensure the input goes through
            CoreServices.InputSystem.RaiseOnInputDown(defaultInputSource, Handedness.Right, inputAction);
            yield return PlayModeTestUtilities.WaitForInputSystemUpdate();

            yield return CheckButtonTranslation(targetStartPosition, translateTargetObject, shouldTranslate);

            // Raise a select up input event, then wait for transition to take place
            CoreServices.InputSystem.RaiseOnInputUp(defaultInputSource, Handedness.Right, inputAction);
            yield return PlayModeTestUtilities.WaitForInputSystemUpdate();
            // Wait for at button release animation to finish
            yield return new WaitForSeconds(ButtonReleaseAnimationDelay);
        }

        #endregion
    }
}
#endif
