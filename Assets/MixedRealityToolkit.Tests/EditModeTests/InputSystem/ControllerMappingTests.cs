﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Input.UnityInput;
using Microsoft.MixedReality.Toolkit.OpenVR.Input;
using NUnit.Framework;

namespace Microsoft.MixedReality.Toolkit.Tests.InputSystem
{
    public class ControllerMappingTests
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            TestUtilities.InitializeMixedRealityToolkitAndCreateScenes(true);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            TestUtilities.ShutdownMixedRealityToolkit();
            TestUtilities.EditorTearDownScenes();
        }

        [Test]
        public void MouseControllerUpdateTest()
        {
            MouseController controller = new MouseController(TrackingState.NotApplicable, Utilities.Handedness.Any);
            controller.SetupDefaultInteractions(Utilities.Handedness.Any);

            // Tests
            Assert.That(() => controller.Update(), Throws.Nothing);
        }

        [Test]
        public void XboxControllerUpdateTest()
        {
            XboxController controller = new XboxController(TrackingState.NotApplicable, Utilities.Handedness.None);
            controller.SetupDefaultInteractions(Utilities.Handedness.None);

            TestGenericJoystickControllerUpdate(controller);
        }

        [Test]
        public void GenericOpenVRControllerUpdateTest()
        {
            GenericOpenVRController leftController = new GenericOpenVRController(TrackingState.NotTracked, Utilities.Handedness.Left);
            leftController.SetupDefaultInteractions(Utilities.Handedness.Left);

            GenericOpenVRController rightController = new GenericOpenVRController(TrackingState.NotTracked, Utilities.Handedness.Right);
            rightController.SetupDefaultInteractions(Utilities.Handedness.Right);

            TestGenericJoystickControllerUpdate(leftController);
            TestGenericJoystickControllerUpdate(rightController);
        }

        [Test]
        public void OculusRemoteControllerUpdateTest()
        {
            OculusRemoteController controller = new OculusRemoteController(TrackingState.NotTracked, Utilities.Handedness.None);
            controller.SetupDefaultInteractions(Utilities.Handedness.None);

            TestGenericJoystickControllerUpdate(controller);
        }

        [Test]
        public void OculusTouchControllerUpdateTest()
        {
            OculusTouchController leftController = new OculusTouchController(TrackingState.NotTracked, Utilities.Handedness.Left);
            leftController.SetupDefaultInteractions(Utilities.Handedness.Left);

            OculusTouchController rightController = new OculusTouchController(TrackingState.NotTracked, Utilities.Handedness.Right);
            rightController.SetupDefaultInteractions(Utilities.Handedness.Right);

            TestGenericJoystickControllerUpdate(leftController);
            TestGenericJoystickControllerUpdate(rightController);
        }

        [Test]
        public void ViveKnucklesControllerUpdateTest()
        {
            ViveKnucklesController leftController = new ViveKnucklesController(TrackingState.NotTracked, Utilities.Handedness.Left);
            leftController.SetupDefaultInteractions(Utilities.Handedness.Left);

            ViveKnucklesController rightController = new ViveKnucklesController(TrackingState.NotTracked, Utilities.Handedness.Right);
            rightController.SetupDefaultInteractions(Utilities.Handedness.Right);

            TestGenericJoystickControllerUpdate(leftController);
            TestGenericJoystickControllerUpdate(rightController);
        }

        [Test]
        public void ViveWandControllerUpdateTest()
        {
            ViveWandController leftController = new ViveWandController(TrackingState.NotTracked, Utilities.Handedness.Left);
            leftController.SetupDefaultInteractions(Utilities.Handedness.Left);

            ViveWandController rightController = new ViveWandController(TrackingState.NotTracked, Utilities.Handedness.Right);
            rightController.SetupDefaultInteractions(Utilities.Handedness.Right);

            TestGenericJoystickControllerUpdate(leftController);
            TestGenericJoystickControllerUpdate(rightController);
        }

        [Test]
        public void WindowsMixedRealityOpenVRMotionControllerUpdateTest()
        {
            WindowsMixedRealityOpenVRMotionController leftController = new WindowsMixedRealityOpenVRMotionController(TrackingState.NotTracked, Utilities.Handedness.Left);
            leftController.SetupDefaultInteractions(Utilities.Handedness.Left);

            WindowsMixedRealityOpenVRMotionController rightController = new WindowsMixedRealityOpenVRMotionController(TrackingState.NotTracked, Utilities.Handedness.Right);
            rightController.SetupDefaultInteractions(Utilities.Handedness.Right);

            TestGenericJoystickControllerUpdate(leftController);
            TestGenericJoystickControllerUpdate(rightController);
        }

        private void TestGenericJoystickControllerUpdate(GenericJoystickController controller)
        {
            // Test
            Assert.That(() => controller.UpdateController(), Throws.Nothing);
        }
    }
}