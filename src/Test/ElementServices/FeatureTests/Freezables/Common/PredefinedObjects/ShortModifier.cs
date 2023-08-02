// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/***********************************************************
 *
 *   Copyright (c) Microsoft Corporation, 2004
 *
 *   Program:   Test-hooked Int16Modifier
 
 *
 ************************************************************/

using System;


namespace                       Microsoft.Test.ElementServices.Freezables.Modifiers
{
    //--------------------------------------------------------------
    // The Freezable pattern is horribly violated by this class but I don't care
    // since I am not testing interaction with Freezable yet.

    public class                Int16Modifier              : System.Windows.Media.Animation.Int16AnimationBase
    {
        //----------------------------------------------------------

        public                  Int16Modifier ( ModifierController c, short d )
        {
            _controller = c;
            _delta = d;
        }
        protected Int16Modifier()
        {
        }
       
        //----------------------------------------------------------
        protected override void CloneCore(System.Windows.Freezable sourceFreezable)
        {
            Int16Modifier int16Modifier = (Int16Modifier)sourceFreezable;
            base.CloneCore(sourceFreezable);
            _controller = int16Modifier._controller;
            _delta = int16Modifier._delta;
   
        }
        protected override void GetAsFrozenCore(System.Windows.Freezable sourceFreezable)
        {
            Int16Modifier int16Modifier = (Int16Modifier)sourceFreezable;
            base.GetAsFrozenCore(sourceFreezable);
            _controller = int16Modifier._controller;
            _delta = int16Modifier._delta;

        }
        protected override void GetCurrentValueAsFrozenCore(System.Windows.Freezable sourceFreezable)
        {
            Int16Modifier int16Modifier = (Int16Modifier)sourceFreezable;
            base.GetCurrentValueAsFrozenCore(sourceFreezable);
            _controller = int16Modifier._controller;
            _delta = int16Modifier._delta;

        }
        public new Int16Modifier GetAsFrozen()
        {
            return (Int16Modifier)base.GetAsFrozen();
        }
        protected override System.Windows.Freezable CreateInstanceCore()
        {
            return new Int16Modifier();
        }
     
        
        protected override short  GetCurrentValueCore ( short defaultOriginValue, short baseValue, System.Windows.Media.Animation.AnimationClock clock )
        {
            if ( !_controller.UsesBaseValue )
            {
                return _delta;
            }
            else
            {
                return (short)(baseValue + _delta);
            }
        }

        //----------------------------------------------------------

        private ModifierController  _controller;
        private short               _delta;
    }
}