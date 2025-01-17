// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Avalon.Test.CoreUI.Trusted;
using Avalon.Test.CoreUI;
using Avalon.Test.CoreUI.Threading;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Threading;
using Microsoft.Test.Threading;
using Avalon.Test.CoreUI.Common;

using Avalon.Test.CoreUI.CoreInput.Common;
using Avalon.Test.CoreUI.CoreInput.Common.Controls;
using Microsoft.Test.Win32;
using Microsoft.Test.Discovery;
using Microsoft.Test;
using Microsoft.Test.Logging;

namespace Avalon.Test.CoreUI.CoreInput
{
    /// <summary>
    /// Verify IInputElement IsKeyboardFocusWithin for a ContentElement.
    /// </summary>
    /// <description>
    /// This is part of a collection of unit tests for input events.
    /// </description>
    /// <author>Microsoft</author>
 
    [CoreTestsLoader(CoreTestsTestType.MethodBase)]
    public class ContentElementIsFocusWithinApp : TestApp
    {
        /// <summary>
        /// Launch our test.
        /// </summary>
        [TestCase("2", @"CoreInput\Focus", "HwndSource", @"Compile and Verify IInputElement IsKeyboardFocusWithin for a ContentElement in HwndSource.")]
        [TestCase("2", @"CoreInput\Focus", "Browser", @"Compile and Verify IInputElement IsKeyboardFocusWithin for a ContentElement in Browser.")]
        [TestCase("2", @"CoreInput\Focus", "Window", @"Compile and Verify IInputElement IsKeyboardFocusWithin for a ContentElement in window.")]
        public static void LaunchTestCompile()
        {
            HostType hostType = (HostType)Enum.Parse(typeof(HostType), DriverState.DriverParameters["TestParameters"]);

            GenericCompileHostedCase.RunCase(
                "Avalon.Test.CoreUI.CoreInput",
                "ContentElementIsFocusWithinApp",
                "Run",
                hostType);

        }

        /// <summary>
        /// Launch our test.
        /// </summary>
        [TestCase("1", @"CoreInput\Focus", "HwndSource", @"Verify IInputElement IsKeyboardFocusWithin for a ContentElement in HwndSource.")]
        [TestCase("1", @"CoreInput\Focus", "Window", @"Verify IInputElement IsKeyboardFocusWithin for a ContentElement in window.")]
        public static void LaunchTest()
        {
            HostType hostType = (HostType)Enum.Parse(typeof(HostType),DriverState.DriverParameters["TestParameters"]);

            ExeStubContainerFramework exe = new ExeStubContainerFramework(hostType);
            exe.Run(new ContentElementIsFocusWithinApp(), "Run");

        }

        /// <summary>
        /// Setup the test.
        /// </summary>
        /// <param name="sender">App sending the callback.</param>
        /// <returns>Null object.</returns>
        public override object DoSetup(object sender)
        {


            CoreLogger.LogStatus("Constructing tree....");

            // Construct test element and child element
            InstrContentPanelHost host = new InstrContentPanelHost();
            host.Focusable = true;
            _contentElement = new InstrContentPanel("rootLeaf", "Sample", host);
            _contentElement.Focusable = true;
            host.AddChild(_contentElement);

            DisplayMe(host, 0, 0, 100, 100);

            //
            // Position mouse for test.
            //
            // Let mouse ops complete
            DispatcherHelper.DoEvents(DispatcherPriority.Loaded);

            MouseHelper.Click(host);



            return null;
        }

        /// <summary>
        /// Execute stuff.
        /// </summary>
        /// <param name="arg">Not used.</param>
        /// <returns>Null object.</returns>
        protected override object DoExecute(object arg)
        {
            // STEP 0
            CoreLogger.LogStatus("Clearing capture....");
            bool bCaptured = Mouse.Capture(null);
            CoreLogger.LogStatus("Capture remains? (expect false) " + bCaptured);

            CoreLogger.LogStatus("Saving startup focus values (on startup)....");
            bool bWasFocused = _rootElement.IsKeyboardFocused;
            CoreLogger.LogStatus("Root element focused? (expect yes) " + bWasFocused);

            bool bWasFocusWithin = ((FrameworkElement)_rootElement).IsKeyboardFocusWithin;
            bool bWasFocusWithinFocusedChild = _contentElement.IsKeyboardFocusWithin;
            CoreLogger.LogStatus("Focus within parent,focusedcontent? (expect yes,no)  " +
                bWasFocusWithin + "," +
                bWasFocusWithinFocusedChild
            );

            _bWasStartupFocusedCorrectly = (bWasFocused) && (bWasFocusWithin) && (!bWasFocusWithinFocusedChild);

            // STEP 2
            CoreLogger.LogStatus("Focusing on the parent....");
            _rootElement.Focus();

            // STEP 3
            CoreLogger.LogStatus("Saving parent focus values (on parent)....");

            bWasFocused = _rootElement.IsKeyboardFocused;
            CoreLogger.LogStatus("Root element focused? " + bWasFocused);

            bWasFocusWithin = ((FrameworkElement)_rootElement).IsKeyboardFocusWithin;
            bWasFocusWithinFocusedChild = _contentElement.IsKeyboardFocusWithin;
            CoreLogger.LogStatus("Focus within parent,focusedcontent " +
                bWasFocusWithin + "," +
                bWasFocusWithinFocusedChild
            );

            _bWasParentFocusedCorrectly = (bWasFocused) && (bWasFocusWithin) && (!bWasFocusWithinFocusedChild);

            // STEP 4
            CoreLogger.LogStatus("Focusing on a content element....");
            bool bResult = _contentElement.Focus();
            CoreLogger.LogStatus("Content element focused? " + bResult);
            CoreLogger.LogStatus("Element with focus: (expect content element) " + Keyboard.FocusedElement.ToString());

            base.DoExecute(arg);
            return null;
        }

        /// <summary>
        /// Validate the test.
        /// </summary>
        /// <param name="arg">Not used.</param>
        /// <returns>Null object.</returns>
        public override object DoValidate(object arg)
        {
            CoreLogger.LogStatus("Validating...");

            // Note: for this test we need to make sure the root does not have focus
            // We also need to make sure that an element within it does have focus.

            CoreLogger.LogStatus("Element with focus: (expect content element) " + Keyboard.FocusedElement.ToString());
            bool bFocused = _rootElement.IsKeyboardFocused;
            CoreLogger.LogStatus("Root element focused? (expect false) " + bFocused);

            bool bFocusWithin = ((FrameworkElement)_rootElement).IsKeyboardFocusWithin;
            bool bFocusWithinFocusedChild = _contentElement.IsKeyboardFocusWithin;
            CoreLogger.LogStatus("Focus within parent,focusedcontent? (expect true,true) " +
                bFocusWithin + "," +
                bFocusWithinFocusedChild
            );

            CoreLogger.LogStatus("Was focus correct on startup? (expect yes) " + _bWasStartupFocusedCorrectly);
            CoreLogger.LogStatus("Was focus correct after focusing parent? (expect yes) " + _bWasParentFocusedCorrectly);

            bool expected = (!bFocused) && (bFocusWithin) && (bFocusWithinFocusedChild) && (_bWasStartupFocusedCorrectly) && (_bWasParentFocusedCorrectly);
            bool actual = true;
            bool eventFound = (expected == actual);

            CoreLogger.LogStatus("Setting log result to " + eventFound);
            this.TestPassed = eventFound;

            CoreLogger.LogStatus("Validation complete!");

            return null;
        }

        /// <summary>
        /// Store content element on our canvas.
        /// </summary>
        private ContentElement _contentElement;

        /// <summary>
        /// Were FocusWithin properties set correctly at app startup?
        /// </summary>
        private bool _bWasStartupFocusedCorrectly = false;

        /// <summary>
        /// Were FocusWithin properties set correctly after focus set to parent?
        /// </summary>
        private bool _bWasParentFocusedCorrectly = false;

    }
}
