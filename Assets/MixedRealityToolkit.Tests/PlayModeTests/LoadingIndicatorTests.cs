﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
#if !WINDOWS_UWP
// When the .NET scripting backend is enabled and C# projects are built
// Unity doesn't include the required assemblies (i.e. the ones below).
// Given that the .NET backend is deprecated by Unity at this point it's we have
// to work around this on our end.
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Microsoft.MixedReality.Toolkit.Tests
{
    class ProgressIndicatorTests
    {
        private const string progressIndicatorLoadingBarPrefabPath = "Assets/MixedRealityToolkit.SDK/Features/UX/Prefabs/ProgressIndicators/ProgressIndicatorLoadingBar.prefab";
        private const string progressIndicatorRotatingObjectPrefabPath = "Assets/MixedRealityToolkit.SDK/Features/UX/Prefabs/ProgressIndicators/ProgressIndicatorRotatingObject.prefab";
        private const string progressIndicatorRotatingOrbsPrefabPath = "Assets/MixedRealityToolkit.SDK/Features/UX/Prefabs/ProgressIndicators/ProgressIndicatorRotatingOrbs.prefab";
        
        /// <summary>
        /// Tests that prefab can be opened and closed at runtime.
        /// </summary>
        [UnityTest]
        public IEnumerator TestOpenCloseLoadingBarPrefab()
        {
            GameObject progressIndicatorObject;
            IProgressIndicator progressIndicator;
            InstantiatePrefab(progressIndicatorLoadingBarPrefabPath, out progressIndicatorObject, out progressIndicator);
            Task testTask = TestOpenCloseProgressIndicatorAsync(progressIndicatorObject, progressIndicator);
            while (!testTask.IsCompleted)
            {
                yield return null;
            }

            // clean up
            GameObject.Destroy(progressIndicatorObject);
            yield return null;
        }

        /// <summary>
        /// Tests that prefab can be opened and closed at runtime.
        /// </summary>
        [UnityTest]
        public IEnumerator TestOpenCloseRotatingObjectPrefab()
        {
            GameObject progressIndicatorObject;
            IProgressIndicator progressIndicator;
            InstantiatePrefab(progressIndicatorRotatingObjectPrefabPath, out progressIndicatorObject, out progressIndicator);
            Task testTask = TestOpenCloseProgressIndicatorAsync(progressIndicatorObject, progressIndicator);
            while (!testTask.IsCompleted)
            {
                yield return null;
            }

            // clean up
            GameObject.Destroy(progressIndicatorObject);
            yield return null;
        }

        /// <summary>
        /// Tests that prefab can be opened and closed at runtime.
        /// </summary>
        [UnityTest]
        public IEnumerator TestOpenCloseRotatingOrbsPrefab()
        {
            GameObject progressIndicatorObject;
            IProgressIndicator progressIndicator;
            InstantiatePrefab(progressIndicatorRotatingOrbsPrefabPath, out progressIndicatorObject, out progressIndicator);
            Task testTask = TestOpenCloseProgressIndicatorAsync(progressIndicatorObject, progressIndicator, 3f);
            while (!testTask.IsCompleted)
            {
                yield return null;
            }

            // clean up
            GameObject.Destroy(progressIndicatorObject);
            yield return null;
        }

        private async Task TestOpenCloseProgressIndicatorAsync(GameObject progressIndicatorObject, IProgressIndicator progressIndicator, float timeOpen = 2f)
        {
            // Deactivate the progress indicator
            progressIndicatorObject.SetActive(false);

            // Make sure it's closed
            Assert.True(progressIndicator.State == ProgressIndicatorState.Closed, "Progress indicator was not in correct state on startup: " + progressIndicator.State);

            // Make sure we can set progress and message
            progressIndicator.Progress = 0;
            progressIndicator.Message = "Progress Test";

            // Wait for it to open
            await progressIndicator.OpenAsync();

            // Make sure it's actually open
            Assert.True(progressIndicator.State == ProgressIndicatorState.Open, "Progress indicator was not open after open async call: " + progressIndicator.State);

            // Make sure we can set its progress and message while open
            // Also make sure we can set progress to a value greater than 1 without blowing anything up
            float timeStarted = Time.time;
            while (Time.time < timeStarted + timeOpen)
            {
                progressIndicator.Progress = Time.time - timeStarted;
                progressIndicator.Message = "Current Time: " + Time.time;
                await Task.Yield();
            }

            // Wait for it to close
            await progressIndicator.CloseAsync();

            // Make sure it's actually closed
            Assert.True(progressIndicator.State == ProgressIndicatorState.Closed, "Progress indicator was not closed after close async call: " + progressIndicator.State);
        }

        private void InstantiatePrefab(string path, out GameObject progressIndicatorObject, out IProgressIndicator progressIndicator)
        {
            progressIndicatorObject = null;
            progressIndicator = null;

            #if UNITY_EDITOR
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            progressIndicatorObject = GameObject.Instantiate(prefab);
            progressIndicator = (IProgressIndicator)progressIndicatorObject.GetComponent(typeof(IProgressIndicator));
            #endif
        }
    }
}
#endif