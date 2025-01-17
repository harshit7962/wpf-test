// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Avalon.Test.CoreUI.Trusted;
using Avalon.Test.CoreUI;
using System.Windows;
using System.Windows.Controls;
using Avalon.Test.CoreUI.Common;
using System.Windows.Media;
using System.Windows.Documents;
using System.Collections;
using Avalon.Test.CoreUI.Parser;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using Avalon.Test.CoreUI.IdTest;

namespace Avalon.Test.CoreUI.Serialization
{
    /// <summary>
    /// Verify xaml files for Animation
    /// </summary>
    public class AnimationVerifiers
    {
        /// <summary>
        /// 
        /// </summary>
        public static void ColorDoubleShape_Verify(UIElement uie)
        {
            DockPanelAndCanvasVerify(uie);
            Border myBorder = uie as Border;

            CoreLogger.LogStatus("Verifying color Ellipse ...");

            Ellipse ball1 = (Ellipse)IdTestBaseCase.FindElementWithId(uie, "ball1");

            VerifyElement.VerifyDouble(ball1.Width, 80);
            VerifyElement.VerifyDouble(ball1.Height, 80);
            VerifyElement.VerifyDouble((double)ball1.GetValue(Canvas.LeftProperty), 10);
            VerifyElement.VerifyDouble((double)ball1.GetValue(Canvas.TopProperty), 60);
            
            CoreLogger.LogStatus("Verifying solid color brush ...");
            SolidColorBrush myBrush = ((Shape)ball1).Fill as SolidColorBrush;
            VerifyElement.VerifyColor(myBrush.Color, Colors.Red);
            //Verify storyboards, blocked on bug
            //Storyboard storyboard1 = IdTestBaseCase.FindElementWithId(uie, "storyboard1") as Storyboard;
            //VerifyElement.VerifyBool(null == storyboard1, false);
            //Storyboard storyboard2 = IdTestBaseCase.FindElementWithId(uie, "storyboard2") as Storyboard;
            //VerifyElement.VerifyBool(null == storyboard2, false);
        }

        /// <summary>
        /// </summary>
        public static void DoubleControlKeyspline_Verify(UIElement uie)
        {
            DockPanelAndCanvasVerify(uie);

            Border myBorder = uie as Border;

            CoreLogger.LogStatus("Verifying DoubleControlKeyspline ...");

            TextBlock text = (TextBlock)IdTestBaseCase.FindElementWithId(uie, "myText");

            VerifyElement.VerifyBool(text != null, true);
            VerifyElement.VerifyDouble(text.Opacity, 1.0);
            //Verify storyboards, blocked on bug
            //Storyboard storyboard1 = IdTestBaseCase.FindElementWithId(uie, "storyboard1") as Storyboard;
            //VerifyElement.VerifyBool(null == storyboard1, false);
            //Storyboard storyboard2 = IdTestBaseCase.FindElementWithId(uie, "storyboard2") as Storyboard;
            //VerifyElement.VerifyBool(null == storyboard2, false);

            CoreLogger.LogStatus("Verification over");
        }

        /// <summary>
        /// 
        /// </summary>
        public static void PointDoubleShape_Verify(UIElement uie)
        {
            CoreLogger.LogStatus("Inside AnimationVerifiers.PointDoubleShape_Verify()...");
            DockPanelAndCanvasVerify(uie);
            CoreLogger.LogStatus("Verifying PointDoubleShape ...");

            Rectangle rectangle = (Rectangle)IdTestBaseCase.FindElementWithId(uie, "Rectangle");

            VerifyElement.VerifyBool(rectangle != null, true);
            //Verify storyboards, blocked on bug
            //Storyboard storyboard1 = IdTestBaseCase.FindElementWithId(uie, "storyboard1") as Storyboard;
            //VerifyElement.VerifyBool(null == storyboard1, false);
            //Storyboard storyboard2 = IdTestBaseCase.FindElementWithId(uie, "storyboard2") as Storyboard;
            //VerifyElement.VerifyBool(null == storyboard2, false);

        }
        private static void DockPanelAndCanvasVerify(UIElement uie)
        {
            CoreLogger.LogStatus("Verifying border and canvas ...");

            DockPanel myPanel = uie as DockPanel;

            if (null == myPanel)
            {
                throw new Microsoft.Test.TestValidationException("Should be DockPanel");
            }

            VerifyElement.VerifyColor(((SolidColorBrush)(myPanel.Background)).Color, Colors.White);

            Canvas myCanvas = (Canvas)IdTestBaseCase.FindElementWithId(uie, "Canvas");

            if (null == myCanvas)
            {
                throw new Microsoft.Test.TestValidationException("Should be Canvas");
            }
            VerifyElement.VerifyDouble(myCanvas.Height, 500);
            VerifyElement.VerifyDouble(myCanvas.Width, 500);
        }
    }
}
