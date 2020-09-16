/* * * * * * * * * * * * * * * * * * * * * *
AeternumGames.Chisel.Import.Source.Editor.ChiselSource2006MapImporterWindow.cs

License: MIT
Author: Daniel Cornelius

* * * * * * * * * * * * * * * * * * * * * */

using System;
using System.Collections.Generic;
using System.IO;
using Chisel.Components;
using UnityEditor;
using UnityEngine;

namespace AeternumGames.Chisel.Import.Source.Editor
{
    // $TODO: optimize out GUILayout stuff - make it speedy, gonzales!
    public partial class ChiselSource2006MapImporterWindow : EditorWindow
    {
        private GUIStyle helpTextStyleWrapped;
        private GUIStyle helpTextStyle;
        private GUIStyle windowBGStyle;
        private GUIStyle listItemStyle;
        private GUIStyle toolbarStyle;
        private Color32  lightSkinFontColor = new Color32( 0,   0,   0,   255 );
        private Color32  darkSkinFontColor  = new Color32( 200, 200, 200, 255 );

        private void SetupStyles()
        {
            // helptextstylewrapped

            helpTextStyleWrapped ??= new GUIStyle( "WordWrappedMiniLabel" )
            {
                    normal = new GUIStyleState() { textColor = GetTextColor() }
            };

            if( helpTextStyleWrapped != null )
                helpTextStyleWrapped.normal.textColor = GetTextColor();

            // helptextstyle

            helpTextStyle ??= new GUIStyle( "MiniLabel" )
            {
                    normal = new GUIStyleState() { textColor = GetTextColor() }
            };

            if( helpTextStyle != null )
                helpTextStyle.normal.textColor = GetTextColor();

            // windowbgstyle

            windowBGStyle ??= new GUIStyle( "AnimationEventBackground" )
            {
            };

            // listitemstyle

            listItemStyle ??= new GUIStyle( "TV Selection" )
            {
                    normal = new GUIStyleState() { textColor = GetTextColor() },
                    fontStyle = FontStyle.Normal,
                    wordWrap = false,
                    //padding = new RectOffset( 6, 6, 3, 3 ),
                    //fontSize = 11
            };

            // toolbarstyle

            toolbarStyle ??= new GUIStyle( "ToolbarButtonFlat" )
            {
            };
        }

        private Color GetTextColor()
        {
            return ( EditorGUIUtility.isProSkin ) ? darkSkinFontColor : lightSkinFontColor;
        }

        /*
        private void InvalidateStyles()
        {
            helpTextStyleWrapped = null;
            windowBGStyle = null;
            windowBGStyle = null;
            listItemStyle = null;
        }
        */
    }
}
