/************************************************************************************

Filename    :   MoonlightBuild.cs
Content     :   Provides menu commands that copy scripts from a specific folder
                prior to building.
Created     :   March 14, 2014
Authors     :   Jonathan E. Wright

Copyright   :   Copyright 2014 Oculus VR, LLC. All Rights reserved.

Licensed under the Oculus VR Rift SDK License Version 3.1 (the "License"); 
you may not use the Oculus VR Rift SDK except in compliance with the License, 
which is provided at the time of installation or download, or which 
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

http://www.oculusvr.com/licenses/LICENSE-3.1 

Unless required by applicable law or agreed to in writing, the Oculus VR SDK 
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

************************************************************************************/

using UnityEngine;
using UnityEditor;
using System;
using System.IO;

class MoonlightEditor
{
    enum eContentCompareResult
    {
        SAME,
        DIFFERENT
    };

    enum eDateCompareResult
    {

        SOURCE_NEWER = -1,
        SAME_TIME = 0,
        DEST_NEWER = 1
    };

    struct FileCompareResults
    {
        public bool TestSucceeded;  // if this is false, at least one of the files can't be accessed
        public eDateCompareResult DateResult;
        public eContentCompareResult ContentResult;
    };

    static bool CompareFiles( string sourceFilePath, string destFilePath, out FileCompareResults result )
    {
        result.TestSucceeded = false;
        result.ContentResult = eContentCompareResult.SAME;
        result.DateResult = eDateCompareResult.SAME_TIME;

        System.IO.FileInfo sourceFileInfo = null;
        System.IO.FileInfo destFileInfo = null;
        DateTime sourceFileTime;
        DateTime destFileTime;
        long sourceFileLen;
        long destFileLen;
        try
        {
            sourceFileInfo = new System.IO.FileInfo( sourceFilePath );
            if ( !sourceFileInfo.Exists )
            {
                return false;   // if no source, fail
            }
            sourceFileTime = sourceFileInfo.LastWriteTimeUtc;
            sourceFileLen = sourceFileInfo.Length;
            destFileInfo = new System.IO.FileInfo( destFilePath );
            if ( !destFileInfo.Exists )
            {
                result.TestSucceeded = true;
                result.DateResult = eDateCompareResult.SOURCE_NEWER; // if no dest, consider source newer
                return false;
            }
            destFileTime = destFileInfo.LastWriteTimeUtc;
            destFileLen = destFileInfo.Length;
        }
        catch ( System.IO.IOException )
        {
            return false; // error testing file times
        }

        result.TestSucceeded = true;

        if ( sourceFileTime > destFileTime )
        {
            result.DateResult = eDateCompareResult.SOURCE_NEWER;
            return false;
        }
        else if ( destFileTime > sourceFileTime )
        {
            result.DateResult = eDateCompareResult.DEST_NEWER;
            return false;
        }

        if ( sourceFileLen != destFileLen )
        {
            result.ContentResult = eContentCompareResult.DIFFERENT;
            return false;
        }

        // times and lengths are the same so compare the file contents
        byte[] sourceFile = null;
        byte[] destFile = null;
        try
        {
            sourceFile = System.IO.File.ReadAllBytes( sourceFilePath );
            destFile = System.IO.File.ReadAllBytes( destFilePath );
        }
        catch ( Exception )
        {
            return false;
        }
        if ( sourceFile.Length != destFile.Length )
        {
            // this shouldn't happen since we check the length above!
            Debug.LogError( "File lengths should be the same!" );
            result.ContentResult = eContentCompareResult.DIFFERENT;
            return false;
        }
        for ( int i = 0; i < sourceFile.Length; ++i )
        {
            if ( sourceFile[i] != destFile[i] )
            {
                result.ContentResult = eContentCompareResult.DIFFERENT;
                return false;
            }
        }
        return true;
    }

    public static bool CopySingleFile( System.IO.FileInfo file, string destPath, bool overwriteNewer )
    {
        string fileDestPath = System.IO.Path.Combine( destPath, file.Name );

        //Debug.Log( "Copy '" + file.FullName + "' ----> '" + fileDestPath + "'." );

        FileCompareResults results;
        if ( CompareFiles( file.FullName, fileDestPath, out results ) )
        {
            return true;
        }
        if ( !results.TestSucceeded )
        {
            Debug.Log( "File '" + fileDestPath + "': compare did not succeed." );
        }
        if ( results.DateResult == eDateCompareResult.SOURCE_NEWER )
        {
            Debug.Log( "File '" + fileDestPath + "': source is newer." );
        }
        if ( !overwriteNewer && results.ContentResult == eContentCompareResult.DIFFERENT )
        {
            Debug.Log( "Destination file '" + fileDestPath + "' is different!" );
        }

        if ( !overwriteNewer && results.DateResult == eDateCompareResult.DEST_NEWER )
        {
            Debug.LogError( "Destination file '" + fileDestPath + "' is newer!" );
            return false;
        }

        try
        {
            file.CopyTo( fileDestPath, true );
        }
        catch ( System.IO.IOException ioex )
        {
            Debug.LogError( "Failed to copy file '" + fileDestPath + "', reason: " + ioex.Message + ". Is the file checked out?" );
            return false;
        }
        catch ( System.UnauthorizedAccessException )
        {
            Debug.LogError( "Failed to copy file '" + fileDestPath + "'. Make sure the file is checked out." );
            return false;
        }

        try
        {
            System.IO.FileInfo destFileInfo = new System.IO.FileInfo( fileDestPath );
            destFileInfo.Attributes = ( destFileInfo.Attributes & ( ~System.IO.FileAttributes.ReadOnly ) );
        }
        catch ( System.IO.IOException )
        {
            Debug.LogError( "Failed to set destination file '" + fileDestPath + "' to read only after copying. This may cause future copies to fail." );
        }

        DebugUtils.Print( "Copied file '" + fileDestPath + "'..." );

        return true;
    }
    public static bool CopyFolder( string sourcePath, string destPath, bool overwriteNewer, ref int numCopiesFailed )
    {
        //DebugUtils.Print( "Copying folder '" + sourcePath + "' to '" + destPath + "'..." );
        System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo( sourcePath );
        if ( !dirInfo.Exists )
        {
            Debug.LogError( "CopyFolder: source path '" + sourcePath + "' was not found." );
            return false;
        }

        if ( !System.IO.Directory.Exists( destPath ) )
        {
            System.IO.Directory.CreateDirectory( destPath );
            if ( !System.IO.Directory.Exists( destPath ) )
            {
                Debug.LogError( "CopyFolder: failed to create dest path '" + destPath + "'." );
                return false;
            }
        }

        // copy all files in the source path
        System.IO.FileInfo[] files = dirInfo.GetFiles();
        foreach ( System.IO.FileInfo file in files )
        {
            if ( file.Extension == ".meta" )
            {
                //DebugUtils.Print( "Skipping meta file '" + file.Name + "." );
                continue;
            }

            if ( !CopySingleFile( file, destPath, overwriteNewer ) )
            {
                numCopiesFailed++;
            }
        }

        // recurse into all directories in the source path
        System.IO.DirectoryInfo[] dirs = dirInfo.GetDirectories();
        foreach ( System.IO.DirectoryInfo subdir in dirs )
        {
            string destSubDirPath = System.IO.Path.Combine( destPath, subdir.Name );
            CopyFolder( subdir.FullName, destSubDirPath, overwriteNewer, ref numCopiesFailed );
            // don't return if we failed to succeed because we want to see all errors on the first pass
        }

        return numCopiesFailed == 0;
    }

    // The folder name should be relative to "Assets/"
    // The source folder is expected to be found two folders back from
    // the current application dataPath and under a Unity/Integration folder.  
    // For instance, if Application.dataPath is "C:/Mobile/Main/BlockSplosion/Assets" 
    // and folderName is "Moonlight", then the source path is expected to be 
    // "C:/Mobile/Main/Unity/Integration/Assets/Moonlight".
    private static void UpdateSourceFolder( string folderName, bool overwriteNewer )
    {
        string destPath = System.IO.Path.Combine( Application.dataPath, folderName );
        //DebugUtils.Print( "destPath = " + destPath );

        if ( !System.IO.Directory.Exists( destPath ) )
        {
            System.IO.Directory.CreateDirectory( destPath );
        }

        System.IO.DirectoryInfo parentInfo = System.IO.Directory.GetParent( Application.dataPath );
        System.IO.DirectoryInfo parentParentInfo = parentInfo.Parent;
        System.IO.DirectoryInfo mobileMainInfo = parentParentInfo.Parent;
        string sourcePath = System.IO.Path.Combine( mobileMainInfo.FullName, "Unity" );
        sourcePath = System.IO.Path.Combine( sourcePath, "Integration" );
        sourcePath = System.IO.Path.Combine( sourcePath, "Assets" );
        sourcePath = System.IO.Path.Combine( sourcePath, folderName );
        // DebugUtils.Print( "sourcePath = " + sourcePath );

        // copy all files in the folder
        int numCopiesFailed = 0;
        if ( CopyFolder( sourcePath, destPath, overwriteNewer, ref numCopiesFailed ) )
        {
            Debug.Log( "Succesfully updated folder '" + folderName + "' from Unity/Integration." );
        }
        else
        {
            if ( numCopiesFailed > 0 )
            {
                Debug.LogError( "Update of folder '" + folderName + "' failed. " + numCopiesFailed + " files failed to copy." );
            }
            else
            {
                Debug.LogError( "Update of folder '" + folderName + "' failed. See previous log messages." );
            }
        }
    }

    // this will copy Android plugins from the developer path after copying them from the bin/Unity/Integration folder
    // because the developer path is more likely to be  up to date.
    private static void CopyPluginsFromDevPath( bool overwriteNewer )
    {
        System.IO.DirectoryInfo parentInfo = System.IO.Directory.GetParent( Application.dataPath );
        System.IO.DirectoryInfo parentParentInfo = parentInfo.Parent;
        System.IO.DirectoryInfo mobileMainInfo = parentParentInfo.Parent;

        string VRLibSourcePath = System.IO.Path.Combine( mobileMainInfo.FullName, "VRLib" );
        string androidPluginPath = System.IO.Path.Combine( Application.dataPath, "Plugins/Android" );

        try
        {
            string androidDllSourcePath = System.IO.Path.Combine( VRLibSourcePath, "libs/armeabi-v7a/libOculusPlugin.so" );
            System.IO.FileInfo dllFile = new System.IO.FileInfo( androidDllSourcePath );
            CopySingleFile( dllFile, androidPluginPath, overwriteNewer );

            string androidJarSourcePath = System.IO.Path.Combine( VRLibSourcePath, "bin/vrlib.jar" );
            System.IO.FileInfo jarFile = new System.IO.FileInfo( androidJarSourcePath );
            CopySingleFile( jarFile, androidPluginPath, overwriteNewer );
        }
        catch ( System.IO.IOException ioex )
        { 
            Debug.LogError( ioex.Message );
        }
    }

#if UNITY_EDITOR
    // Copies the OVR and Moonlight folder from the Mobile/Main/Unity folder
	[MenuItem ("Moonlight/Update From Unity/Integration")]	
	public static void Moonlight_CopyUnity()
    {
        MoonlightEditor.UpdateSourceFolder( "OVR", false );
        MoonlightEditor.UpdateSourceFolder( "Moonlight", false );
        MoonlightEditor.UpdateSourceFolder( "Plugins", false );
        CopyPluginsFromDevPath( false );
    }
    // Copies the OVR and Moonlight folder from the Mobile/Main/Unity folder
	[MenuItem ("Moonlight/Update From Unity/Integration (OVERWRITE)")]	
	public static void Moonlight_CopyUnityOverwrite()
    {
        MoonlightEditor.UpdateSourceFolder( "OVR", true );
        MoonlightEditor.UpdateSourceFolder( "Moonlight", true );
        MoonlightEditor.UpdateSourceFolder( "Plugins", true );
        CopyPluginsFromDevPath( false );
    }
#endif
};