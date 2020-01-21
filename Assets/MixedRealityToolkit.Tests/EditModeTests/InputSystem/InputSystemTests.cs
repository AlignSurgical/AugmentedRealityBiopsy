﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Tests.Services;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Microsoft.MixedReality.Toolkit.Tests.InputSystem
{
    public class InputSystemTests
    {
        private const string TestInputSystemProfilePath = "Assets/MixedRealityToolkit.Tests/EditModeTests/Services/TestProfiles/TestMixedRealityInputSystemProfile.asset";
        private const string TestEmptyInputSystemProfilePath = "Assets/MixedRealityToolkit.Tests/EditModeTests/Services/TestProfiles/TestEmptyMixedRealityInputSystemProfile.asset";

        [TearDown]
        public void TearDown()
        {
            TestUtilities.ShutdownMixedRealityToolkit();
            TestUtilities.EditorTearDownScenes();
        }

        [Test]
        public void CreateInputSystem()
        {
            TestUtilities.InitializeMixedRealityToolkitAndCreateScenes();
            MixedRealityToolkit.Instance.ActiveProfile.InputSystemProfile = CreateDefaultInputSystemProfile();

            // Add Input System
            bool didRegister = MixedRealityToolkit.Instance.RegisterService<IMixedRealityInputSystem>(new MixedRealityInputSystem(MixedRealityToolkit.Instance.ActiveProfile.InputSystemProfile));

            // Tests
            Assert.IsTrue(didRegister);
            Assert.AreEqual(1, MixedRealityServiceRegistry.GetAllServices().Count);
            Assert.IsNotNull(MixedRealityToolkit.Instance.GetService<IMixedRealityInputSystem>());
        }

        [Test]
        public void TestGetInputSystem()
        {
            TestUtilities.InitializeMixedRealityToolkitAndCreateScenes(true);

            // Tests
            Assert.IsNotNull(CoreServices.InputSystem);
        }

        [Test]
        public void TestInputSystemDoesNotExist()
        {
            TestUtilities.InitializeMixedRealityToolkitAndCreateScenes();

            // Tests for Input System
            Assert.IsFalse(MixedRealityToolkit.Instance.IsServiceRegistered<IMixedRealityInputSystem>());
        }

        [Test]
        public void TestInputSystemExists()
        {
            TestUtilities.InitializeMixedRealityToolkitAndCreateScenes(true);

            // Tests for Input System
            Assert.IsTrue(MixedRealityToolkit.Instance.IsServiceRegistered<IMixedRealityInputSystem>());
        }


        [Test]
        public void TestEmptyDataProvider()
        {
            TestUtilities.InitializeMixedRealityToolkitAndCreateScenes();
            MixedRealityToolkit.Instance.ActiveProfile.InputSystemProfile = AssetDatabase.LoadAssetAtPath<MixedRealityInputSystemProfile>(TestEmptyInputSystemProfilePath);

            var inputSystem = new MixedRealityInputSystem(MixedRealityToolkit.Instance.ActiveProfile.InputSystemProfile);
            Assert.IsTrue(MixedRealityToolkit.Instance.RegisterService<IMixedRealityInputSystem>(inputSystem));

            // Since EditMode, we have to auto-enable MRTK input system ourselves
            MixedRealityToolkit.Instance.EnableAllServicesByType(typeof(IMixedRealityInputSystem));

            var dataProviderAccess = (inputSystem as IMixedRealityDataProviderAccess);

            // Tests
            Assert.IsNotNull(dataProviderAccess);
            Assert.IsEmpty(dataProviderAccess.GetDataProviders());
        }

        [Test]
        public void TestDataProviderRegistration()
        {
            TestUtilities.InitializeMixedRealityToolkitAndCreateScenes();
            MixedRealityToolkit.Instance.ActiveProfile.InputSystemProfile = AssetDatabase.LoadAssetAtPath<MixedRealityInputSystemProfile>(TestInputSystemProfilePath);

            var inputSystem = new MixedRealityInputSystem(MixedRealityToolkit.Instance.ActiveProfile.InputSystemProfile);
            Assert.IsTrue(MixedRealityToolkit.Instance.RegisterService<IMixedRealityInputSystem>(inputSystem));

            // Since EditMode, we have to auto-enable MRTK input system ourselves
            MixedRealityToolkit.Instance.EnableAllServicesByType(typeof(IMixedRealityInputSystem));

            Assert.AreEqual(1, MixedRealityServiceRegistry.GetAllServices().Count);
            Assert.IsNotNull(MixedRealityToolkit.Instance.GetService<IMixedRealityInputSystem>());

            var dataProviderAccess = (inputSystem as IMixedRealityDataProviderAccess);
            Assert.IsNotNull(dataProviderAccess);

            var dataProvider = dataProviderAccess.GetDataProvider<TestInputDataProvider>();
            Assert.IsNotNull(dataProvider);
            Assert.IsTrue(dataProvider.IsInitialized);
            Assert.IsTrue(dataProvider.IsEnabled);

            inputSystem.Disable();
            Assert.IsFalse(dataProvider.IsEnabled);

            inputSystem.Enable();
            // We still have reference to old dataProvider, check still disabled
            Assert.IsFalse(dataProvider.IsEnabled);

            // dataProvider has been unregistered in Disable and new one created by Enable.
            dataProvider = dataProviderAccess.GetDataProvider<TestInputDataProvider>();
            Assert.IsNotNull(dataProvider);
            Assert.IsTrue(dataProvider.IsEnabled);

            inputSystem.Reset();
            LogAssert.Expect(LogType.Log, "TestDataProvider Reset");
            Assert.IsFalse(dataProvider.IsEnabled);

            // dataProvider has been unregistered and newly created in Reset
            dataProvider = dataProviderAccess.GetDataProvider<TestInputDataProvider>();
            Assert.IsNotNull(dataProvider);
            Assert.IsTrue(dataProvider.IsInitialized);
            Assert.IsTrue(dataProvider.IsEnabled);
        }

        /// <summary>
        /// Create default Input System Profile
        /// </summary>
        /// <returns>MixedRealityInputSystemProfile ScriptableObject with default settings</returns>
        private static MixedRealityInputSystemProfile CreateDefaultInputSystemProfile()
        {
            var inputSystemProfile = ScriptableObject.CreateInstance<MixedRealityInputSystemProfile>();
            inputSystemProfile.FocusProviderType = typeof(FocusProvider);
            inputSystemProfile.RaycastProviderType = typeof(DefaultRaycastProvider);
            inputSystemProfile.InputActionsProfile = ScriptableObject.CreateInstance<MixedRealityInputActionsProfile>();
            inputSystemProfile.InputActionRulesProfile = ScriptableObject.CreateInstance<MixedRealityInputActionRulesProfile>();
            inputSystemProfile.PointerProfile = ScriptableObject.CreateInstance<MixedRealityPointerProfile>();
            inputSystemProfile.PointerProfile.GazeProviderType = typeof(GazeProvider);
            inputSystemProfile.GesturesProfile = ScriptableObject.CreateInstance<MixedRealityGesturesProfile>();
            inputSystemProfile.SpeechCommandsProfile = ScriptableObject.CreateInstance<MixedRealitySpeechCommandsProfile>();
            inputSystemProfile.ControllerVisualizationProfile = ScriptableObject.CreateInstance<MixedRealityControllerVisualizationProfile>();
            inputSystemProfile.ControllerMappingProfile = ScriptableObject.CreateInstance<MixedRealityControllerMappingProfile>();
            return inputSystemProfile;
        }
    }
}