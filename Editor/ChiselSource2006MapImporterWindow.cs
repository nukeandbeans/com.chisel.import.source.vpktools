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
        private static GameTitle   m_Game    = GameTitle.HalfLife2;
        private        string[]    dirs      = new[] { $"" };
        private        Vector2     scrollPos = Vector2.zero;
        private        GenericMenu optionMenu;

        private string gameDir   = "";
        private string pickedVMF = "";

        private const string sourceGamesDirPrefKey = "CSL_SOURCE_GAMES_DIR";
        private const string gameTitlePrefKey      = "CSL_SOURCE_GAME_TITLE";
        private const string defaultGamesDir       = @"C:\Program Files (x86)\Steam\steamapps\common\";

        private void OnDisable()
        {
            EditorPrefs.SetString( sourceGamesDirPrefKey, gameDir );
            SourceGame.DEFAULTGAMEDIR = gameDir;
            EditorPrefs.SetInt( gameTitlePrefKey, m_Game.GetValue() );
        }

        private void OnEnable()
        {
            // set up a defaults if the preference doesnt exist

            if( !EditorPrefs.HasKey( sourceGamesDirPrefKey ) )
                EditorPrefs.SetString( sourceGamesDirPrefKey, defaultGamesDir );

            if( !EditorPrefs.HasKey( gameTitlePrefKey ) )
                EditorPrefs.SetInt( gameTitlePrefKey, m_Game.GetValue() );

            // set up all our fields

            // yes i know this is funky, but it works
            SourceGame.DEFAULTGAMEDIR = gameDir = EditorPrefs.GetString( sourceGamesDirPrefKey, defaultGamesDir );
            m_Game = (GameTitle) EditorPrefs.GetInt( gameTitlePrefKey, GameTitle.HalfLife2.GetValue() );
        }

        private void OnGUI()
        {
            SetupStyles(); // make sure our GUIStyles are set up

            if( optionMenu == null )
            {
                optionMenu = new GenericMenu();
                optionMenu.AddItem( new GUIContent( "Set '\\steamapps\\common' directory" ), false,
                                    func =>
                                    {
                                        gameDir = EditorUtility.OpenFolderPanel( "Select '\\steamapps\\Common\\' directory", @"C:\", defaultGamesDir ).Replace( '/', '\\' ) + "\\";

                                        Debug.Log( $"Chose the folder: {gameDir}" );

                                        if( SourceGame.DEFAULTGAMEDIR != gameDir || EditorPrefs.GetString( sourceGamesDirPrefKey ) != gameDir )
                                        {
                                            SourceGame.DEFAULTGAMEDIR = gameDir;
                                            EditorPrefs.SetString( sourceGamesDirPrefKey, gameDir );

                                            //Debug.Log(
                                            //       $"Prefs:\nGame: {( (GameTitle) EditorPrefs.GetInt( gameTitlePrefKey ) ).GetNameForGameTitle()}\nFolder: {EditorPrefs.GetString( sourceGamesDirPrefKey )}" );
                                        }
                                    }, null );
            }

            GUILayout.BeginHorizontal( toolbarStyle );
            {
                if( GUILayout.Button( "Options", toolbarStyle, GUILayout.Width( 48 ) ) ) { optionMenu.ShowAsContext(); }
            }
            GUILayout.EndHorizontal();


            GUILayout.BeginVertical( windowBGStyle, GUILayout.ExpandHeight( true ), GUILayout.ExpandWidth( true ) );
            {
                GUILayout.BeginVertical( "ol box" );
                {
                    GUILayout.BeginHorizontal( "helpbox" );
                    {
                        EditorGUILayout.LabelField( "Game:", GUILayout.Width( 100 ) );

                        GUILayout.FlexibleSpace();

                        m_Game = (GameTitle) EditorGUILayout.EnumPopup( m_Game, GUILayout.Width( 210 ) );

                        // $TODO: optimize this!
                        if( m_Game != (GameTitle) EditorPrefs.GetInt( gameTitlePrefKey ) ) { EditorPrefs.SetInt( gameTitlePrefKey, m_Game.GetValue() ); }
                    }
                    GUILayout.EndHorizontal();

                    EditorGUILayout.LabelField( "VPKs to check:" );
                }
                GUILayout.EndVertical();

                DrawVPKList( SourceGame.GetVPKPathsForTitle( m_Game ) );
            }
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal( "helpbox" );
            {
                EditorGUILayout.LabelField( new GUIContent( pickedVMF, pickedVMF ), helpTextStyle, GUILayout.Height( 24 ) );
                if( GUILayout.Button( "Open", "largebutton", GUILayout.Width( 50 ) ) )
                {
                    pickedVMF = EditorUtility.OpenFilePanel( "Choose VMF", SourceGame.GetDirForTitle( m_Game ), "vmf" ).Replace( '/', '\\' );
                }

                GUI.enabled = ( pickedVMF.Length > 0 );
                if( GUILayout.Button( "Import", "largebutton", GUILayout.Width( 50 ) ) )
                {
                    // $TODO: implement VPK pre-import loading
                    ImportValveMapFormat2006();
                }

                GUI.enabled = true;

                // used to force refresh GUISkins
                //if( GUILayout.Button( "Dbg", "largebutton", GUILayout.Width( 40 ) ) ) { InvalidateStyles(); }
            }
            GUILayout.EndHorizontal();

            DrawHelpBoxArea();
        }

        private void DrawHelpBoxArea()
        {
            GUILayout.BeginVertical( "helpbox" );
            {
                EditorGUILayout.LabelField( "Current search dir is:" );
                EditorGUILayout.LabelField( new GUIContent( $"{SourceGame.GetDirForTitle( m_Game )}", $"{SourceGame.GetDirForTitle( m_Game )}" ), helpTextStyleWrapped );
            }
            GUILayout.EndVertical();

            GUILayout.Label( $"Importer will search for VPKs and convert them to materials for the game '{m_Game.GetNameForGameTitle()}' automatically.", helpTextStyleWrapped );
        }

        private void DrawVPKList( string[] list )
        {
            scrollPos = GUILayout.BeginScrollView( scrollPos, GUILayout.ExpandHeight( true ), GUILayout.ExpandWidth( true ) );
            {
                int count = list.Length;
                for( int i = 0; i < count; i++ ) { EditorGUILayout.LabelField( new GUIContent( list[i], list[i] ), listItemStyle, GUILayout.ExpandWidth( true ) ); }

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndScrollView();
        }

        private void ImportValveMapFormat2006()
        {
            GameObject go = null;
            try
            {
                //string path = EditorUtility.OpenFilePanel( "Import Source Engine Map", "", "vmf" );
                if( pickedVMF.Length != 0 )
                {
                    EditorUtility.DisplayProgressBar( "Chisel: Importing Source Engine Map", "Parsing Valve Map Format File (*.vmf)...", 0.0f );
                    var importer = new ValveMapFormat2006.VmfImporter();
                    var map      = importer.Import( pickedVMF );

                    // create parent game object to store all of the imported content.
                    go = new GameObject( "Source Map - " + Path.GetFileNameWithoutExtension( pickedVMF ) );
                    go.SetActive( false );

                    // create chisel model and import all of the brushes.
                    EditorUtility.DisplayProgressBar( "Chisel: Importing Source Engine Map", "Preparing Material Searcher...", 0.0f );
                    ValveMapFormat2006.VmfWorldConverter.Import( ChiselModelManager.CreateNewModel( go.transform ), map );

#if COM_AETERNUMGAMES_CHISEL_DECALS // optional decals package: https://github.com/Henry00IS/Chisel.Decals
                    // rebuild the world as we need the collision mesh for decals.
                    EditorUtility.DisplayProgressBar("Chisel: Importing Source Engine Map", "Rebuilding the world...", 0.5f);
                    go.SetActive(true);
                    ChiselNodeHierarchyManager.Rebuild();
#endif
                    // begin converting hammer entities to unity objects.
                    ValveMapFormat2006.VmfEntityConverter.Import( go.transform, map );
                }
            }
            catch( Exception ex )
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog( "Source Engine Map Import", "An exception occurred while importing the map:\r\n" + ex.Message, "Ohno!" );
            }
            finally
            {
                if( go != null ) go.SetActive( true );
            }
        }
    }
}
