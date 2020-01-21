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

using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using System.IO;
using Microsoft.MixedReality.Toolkit.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Microsoft.MixedReality.Toolkit.Tests
{
    public class PlayModeTestUtilities
    {

        // Unity's default scene name for a recently created scene
        const string playModeTestSceneName = "MixedRealityToolkit.PlayModeTestScene";

        private static Stack<MixedRealityInputSimulationProfile> inputSimulationProfiles = new Stack<MixedRealityInputSimulationProfile>();

        /// <summary>
        /// Creates a play mode test scene, creates an MRTK instance, initializes playspace.
        /// </summary>
        public static void Setup()
        {
            Assert.True(Application.isPlaying, "This setup method should only be used during play mode tests. Use TestUtilities.");

            bool sceneExists = false;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene playModeTestScene = SceneManager.GetSceneAt(i);
                if (playModeTestScene.name == playModeTestSceneName && playModeTestScene.isLoaded)
                {
                    SceneManager.SetActiveScene(playModeTestScene);
                    sceneExists = true;
                }
            }

            if (!sceneExists)
            {
                Scene playModeTestScene = SceneManager.CreateScene(playModeTestSceneName);
                SceneManager.SetActiveScene(playModeTestScene);
            }

            // Create an MRTK instance and set up playspace
            TestUtilities.InitializeMixedRealityToolkit(true);
            TestUtilities.InitializePlayspace();
        }

        /// <summary>
        /// Destroys all objects in the play mode test scene, if it has been loaded, and shuts down MRTK instance.
        /// </summary>
        public static void TearDown()
        {
            TestUtilities.ShutdownMixedRealityToolkit();

            Scene playModeTestScene = SceneManager.GetSceneByName(playModeTestSceneName);
            if (playModeTestScene.isLoaded)
            {
                foreach (GameObject gameObject in playModeTestScene.GetRootGameObjects())
                {
                    GameObject.Destroy(gameObject);
                }
            }

            // If we created a temporary untitled scene in edit mode to get us started, unload that now
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene editorScene = SceneManager.GetSceneAt(i);
                if (string.IsNullOrEmpty(editorScene.name))
                {   // We've found our editor scene. Unload it.
                    SceneManager.UnloadSceneAsync(editorScene);
                }
            }
        }

        public static SimulatedHandData.HandJointDataGenerator GenerateHandPose(ArticulatedHandPose.GestureId gesture, Handedness handedness, Vector3 worldPosition, Quaternion rotation)
        {
            return (jointsOut) =>
            {
                ArticulatedHandPose gesturePose = ArticulatedHandPose.GetGesturePose(gesture);
                Quaternion worldRotation = rotation * CameraCache.Main.transform.rotation;
                gesturePose.ComputeJointPoses(handedness, worldRotation, worldPosition, jointsOut);
            };
        }

        public static IMixedRealityInputSystem GetInputSystem()
        {
            Assert.IsNotNull(CoreServices.InputSystem, "MixedRealityInputSystem is null!");
            return CoreServices.InputSystem;
        }

        /// <summary>
        /// Utility function to simplify code for getting access to the running InputSimulationService
        /// </summary>
        /// <returns>Returns InputSimulationService registered for playmode test scene</returns>
        public static InputSimulationService GetInputSimulationService()
        {
            IMixedRealityInputSystem inputSystem = GetInputSystem();
            InputSimulationService inputSimulationService = (inputSystem as IMixedRealityDataProviderAccess).GetDataProvider<InputSimulationService>();
            Assert.IsNotNull(inputSimulationService, "InputSimulationService is null!");
            inputSimulationService.UserInputEnabled = false;
            return inputSimulationService;
        }

        /// <summary>
        /// Make sure there is a MixedRealityInputModule on the main camera, which is needed for using Unity UI with MRTK.
        /// </summary>
        /// <remarks>
        /// Workaround for #5061
        /// </remarks>
        public static void EnsureInputModule()
        {
            if (CameraCache.Main)
            {
                var inputModule = CameraCache.Main.gameObject.GetComponent<MixedRealityInputModule>();
                if (inputModule == null)
                {
                    CameraCache.Main.gameObject.AddComponent<MixedRealityInputModule>();
                }
                inputModule.forceModuleActive = true;
            }
        }

        /// <summary>
        /// Destroy the input module to ensure it gets initialized cleanly for the next test.
        /// </summary>
        /// <remarks>
        /// Workaround for #5116
        /// </remarks>
        public static void TeardownInputModule()
        {
            if (CameraCache.Main)
            {
                var inputModule = CameraCache.Main.gameObject.GetComponent<MixedRealityInputModule>();
                if (inputModule)
                {
                    UnityEngine.Object.DestroyImmediate(inputModule);
                }
            }
        }

        /// <summary>
        /// Initializes the MRTK such that there are no other input system listeners
        /// (global or per-interface).
        /// </summary>
        internal static IEnumerator SetupMrtkWithoutGlobalInputHandlers()
        {
            if (!MixedRealityToolkit.IsInitialized)
            {
                Debug.LogError("MixedRealityToolkit must be initialized before it can be configured.");
                yield break;
            }

            Assert.IsNotNull(CoreServices.InputSystem, "Input system must be initialized");

            // Let input system to register all cursors and managers.
            yield return null;

            // Switch off / Destroy all input components, which listen to global events
            UnityEngine.Object.Destroy(CoreServices.InputSystem.GazeProvider.GazeCursor as Behaviour);
            CoreServices.InputSystem.GazeProvider.Enabled = false;

            var diagnosticsVoiceControls = UnityEngine.Object.FindObjectsOfType<DiagnosticsSystemVoiceControls>();
            foreach (var diagnosticsComponent in diagnosticsVoiceControls)
            {
                diagnosticsComponent.enabled = false;
            }

            // Let objects be destroyed
            yield return null;

            // Forcibly unregister all other input event listeners.
            BaseEventSystem baseEventSystem = CoreServices.InputSystem as BaseEventSystem;
            MethodInfo unregisterHandler = baseEventSystem.GetType().GetMethod("UnregisterHandler");

            // Since we are iterating over and removing these values, we need to snapshot them
            // before calling UnregisterHandler on each handler.
            var eventHandlersByType = new Dictionary<System.Type, List<BaseEventSystem.EventHandlerEntry>>(((BaseEventSystem)CoreServices.InputSystem).EventHandlersByType);
            foreach (var typeToEventHandlers in eventHandlersByType)
            {
                var handlerEntries = new List<BaseEventSystem.EventHandlerEntry>(typeToEventHandlers.Value);
                foreach (var handlerEntry in handlerEntries)
                {
                    unregisterHandler.MakeGenericMethod(typeToEventHandlers.Key)
                        .Invoke(baseEventSystem,
                                new object[] { handlerEntry.handler });
                }
            }

            // Check that input system is clean
            CollectionAssert.IsEmpty(((BaseEventSystem)CoreServices.InputSystem).EventListeners, "Input event system handler registry is not empty in the beginning of the test.");
            CollectionAssert.IsEmpty(((BaseEventSystem)CoreServices.InputSystem).EventHandlersByType, "Input event system handler registry is not empty in the beginning of the test.");

            yield return null;
        }

        public static void PushHandSimulationProfile()
        {
            var iss = GetInputSimulationService();
            inputSimulationProfiles.Push(iss.InputSimulationProfile);
        }

        public static void PopHandSimulationProfile()
        {
            var iss = GetInputSimulationService();
            iss.InputSimulationProfile = inputSimulationProfiles.Pop();
        }

        internal static void SetHandSimulationMode(HandSimulationMode mode)
        {
            var iss = GetInputSimulationService();
            iss.HandSimulationMode = mode;
        }

        internal static IEnumerator SetHandState(Vector3 handPos, ArticulatedHandPose.GestureId gestureId, Handedness handedness, InputSimulationService inputSimulationService)
        {
            yield return MoveHandFromTo(handPos, handPos, 2, ArticulatedHandPose.GestureId.Pinch, handedness, inputSimulationService);
        }

        public static T GetPointer<T>(Handedness handedness) where T : class, IMixedRealityPointer
        {
            InputSimulationService simulationService = GetInputSimulationService();
            var hand = simulationService.GetHandDevice(handedness);
            foreach (var pointer in hand.InputSource.Pointers)
            {
                if (pointer is T)
                {
                    return pointer as T;
                }
            }
            return null;
        }

        internal static IEnumerator MoveHandFromTo(
            Vector3 startPos, Vector3 endPos, int numSteps,
            ArticulatedHandPose.GestureId gestureId, Handedness handedness, InputSimulationService inputSimulationService)
        {
            Debug.Assert(handedness == Handedness.Right || handedness == Handedness.Left, "handedness must be either right or left");
            bool isPinching = gestureId == ArticulatedHandPose.GestureId.Grab || gestureId == ArticulatedHandPose.GestureId.Pinch || gestureId == ArticulatedHandPose.GestureId.PinchSteadyWrist;

            for (int i = 1; i <= numSteps; i++)
            {
                float t = i / (float) numSteps;
                Vector3 handPos = Vector3.Lerp(startPos, endPos, t);
                var handDataGenerator = GenerateHandPose(
                        gestureId,
                        handedness,
                        handPos,
                        Quaternion.identity);
                SimulatedHandData handData = handedness == Handedness.Right ? inputSimulationService.HandDataRight : inputSimulationService.HandDataLeft;
                handData.Update(true, isPinching, handDataGenerator);
                yield return null;
            }
        }

        internal static IEnumerator SetHandRotation(Quaternion fromRotation, Quaternion toRotation, Vector3 handPos, ArticulatedHandPose.GestureId gestureId,
            Handedness handedness, int numSteps, InputSimulationService inputSimulationService)
        {
            Debug.Assert(handedness == Handedness.Right || handedness == Handedness.Left, "handedness must be either right or left");
            bool isPinching = gestureId == ArticulatedHandPose.GestureId.Grab || gestureId == ArticulatedHandPose.GestureId.Pinch || gestureId == ArticulatedHandPose.GestureId.PinchSteadyWrist;

            for (int i = 1; i <= numSteps; i++)
            {
                float t = i / (float)numSteps;
                Quaternion handRotation = Quaternion.Lerp(fromRotation, toRotation, t);
                var handDataGenerator = GenerateHandPose(
                        gestureId,
                        handedness,
                        handPos,
                        handRotation);
                SimulatedHandData handData = handedness == Handedness.Right ? inputSimulationService.HandDataRight : inputSimulationService.HandDataLeft;
                handData.Update(true, isPinching, handDataGenerator);
                yield return null;
            }
        }

        internal static IEnumerator HideHand(Handedness handedness, InputSimulationService inputSimulationService)
        {
            yield return null;

            SimulatedHandData handData = handedness == Handedness.Right ? inputSimulationService.HandDataRight : inputSimulationService.HandDataLeft;
            handData.Update(false, false, GenerateHandPose(ArticulatedHandPose.GestureId.Open, handedness, Vector3.zero, Quaternion.identity));

            // Wait one frame for the hand to actually disappear
            yield return null;
        }

        /// <summary>
        /// Shows the hand in the open state, at the origin
        /// </summary>
        internal static IEnumerator ShowHand(Handedness handedness, InputSimulationService inputSimulationService)
        {
            yield return ShowHand(handedness, inputSimulationService, ArticulatedHandPose.GestureId.Open, Vector3.zero);
        }

        internal static IEnumerator ShowHand(Handedness handedness, InputSimulationService inputSimulationService, ArticulatedHandPose.GestureId handPose, Vector3 handLocation)
        {
            yield return null;

            SimulatedHandData handData = handedness == Handedness.Right ? inputSimulationService.HandDataRight : inputSimulationService.HandDataLeft;
            handData.Update(true, false, GenerateHandPose(handPose, handedness, handLocation, Quaternion.identity));

            // Wait one frame for the hand to actually appear
            yield return null;
        }

        internal static void InstallTextMeshProEssentials()
        {
#if UNITY_EDITOR
            // Import the TMP Essential Resources package
            string packageFullPath = Path.GetFullPath("Packages/com.unity.textmeshpro");
            if (Directory.Exists(packageFullPath))
            {
                AssetDatabase.ImportPackage(packageFullPath + "/Package Resources/TMP Essential Resources.unitypackage", false);
            }
            else
            {
                Debug.LogError("Unable to locate the Text Mesh Pro package.");
            }
#endif
        }

        /// <summary>
        /// Waits for the user to press the enter key before a test continues.
        /// Not actually used by any test, but it is useful when debugging since you can
        /// pause the state of the test and inspect the scene.
        /// </summary>
        internal static IEnumerator WaitForEnterKey()
        {
            Debug.Log(Time.time + "Press Enter...");
            while (!UnityEngine.Input.GetKeyDown(KeyCode.Return))
            {
                yield return null;
            }
        }

        /// <summary>
        /// Sometimes it take a few frames for inputs raised via InputSystem.OnInput*
        /// to actually get sent to input handlers. This method waits for enough frames
        /// to pass so that any events raised actually have time to send to handlers.
        /// We set it fairly conservatively to ensure that after waiting
        /// all input events have been sent.
        /// </summary>
        internal static IEnumerator WaitForInputSystemUpdate()
        {
            const int inputSystemUpdateFrames = 10;
            for (int i = 0; i < inputSystemUpdateFrames; i++)
            {
                yield return null;
            }
        }

    }
}
#endif
