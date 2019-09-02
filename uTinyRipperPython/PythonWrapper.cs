using System;
using System.Dynamic;
using System.Linq;
using uTinyRipper;
using uTinyRipper.Exporters;
using AudioClip = uTinyRipper.Classes.AudioClip;

using Object = uTinyRipper.Classes.Object;
using Version = uTinyRipper.Version;

// MRH - quick python wrapper based upon the command line interface 
//     - TODO - supply a string-based filter through rip() and apply via AssetSelector
namespace uTinyRipperPython
{
    public class PythonLogger : ILogger
    {
        public PythonLogger()
        {
        }

        public void Log(LogType type, LogCategory category, string message)
        {
#if !DEBUG
            if (category == LogCategory.Debug)
            {
                return;
            }
#endif
            Console.WriteLine($"{type}:{category}: {message}");
        }

        public static PythonLogger Instance { get; } = new PythonLogger();
    }

    public class PythonWrapper {

        public static bool AssetSelector(Object asset)
        {
            // MRH - filter on AudioClips
            if(asset.ClassID == ClassIDType.AudioClip) 
            {
                AudioClip audio = (AudioClip)asset;
                Console.WriteLine("queued {0}\\{1}", audio.ExportPath, audio.ValidName);
                return true;
            }
            return false;
        }

        public void rip(string[] args, string targetDir)
        {
            Logger.Instance = PythonLogger.Instance;
            Config.IsAdvancedLog = true;
            Config.IsGenerateGUIDByContent = false;
            Config.IsExportDependencies = true;

            if (args.Length == 0)
            {
                Console.WriteLine("No arguments");
                return;
            }

            foreach (string arg in args)
            {
                if (FileMultiStream.Exists(arg))
                {
                    continue;
                }
                if (DirectoryUtils.Exists(arg))
                {
                    continue;
                }
                Console.WriteLine(FileMultiStream.IsMultiFile(arg) ?
                    $"File '{arg}' doesn't has all parts for combining" :
                    $"Neither file nor directory with path '{arg}' exists");
                return;
            }

            try
            {
                GameStructure = GameStructure.Load(args);
                Validate();

                // MRH - can make this an option to include/exclude.  Including will transcode, e.g. fsb (FSB5) into wav
                GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.AudioClip, new AudioAssetExporter());

                /* MRH - borrowing from the GUI... there seems to be much more work there than in the CLI
                uTinyRipperGUI.Exporters.TextureAssetExporter textureExporter = new uTinyRipperGUI.Exporters.TextureAssetExporter();
                GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Texture2D, textureExporter);
                GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Cubemap, textureExporter);
                GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Sprite, textureExporter);
                GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Shader, new uTinyRipperGUI.Exporters.ShaderAssetExporter());
                GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.TextAsset, new TextAssetExporter());
                GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Font, new FontAssetExporter());
                GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.MovieTexture, new MovieTextureAssetExporter());

                EngineAssetExporter engineExporter = new EngineAssetExporter();
                GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Material, engineExporter);
                GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Texture2D, engineExporter);
                GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Mesh, engineExporter);
                GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Shader, engineExporter);
                GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Font, engineExporter);
                GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Sprite, engineExporter);
                GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.MonoBehaviour, engineExporter);

                PrepareExportDirectory(targetDir);
                */

                GameStructure.Export(targetDir, AssetSelector);
                Logger.Log(LogType.Info, LogCategory.General, "Finished");
            }
            catch (Exception ex)
            {
                Logger.Log(LogType.Error, LogCategory.General, ex.ToString());
            }
        }

        private void Validate()
        {
            Version[] versions = GameStructure.FileCollection.Files.Select(t => t.Version).Distinct().ToArray();
            if (versions.Count() > 1)
            {
                Logger.Log(LogType.Warning, LogCategory.Import, $"Asset collection has versions probably incompatible with each other. Here they are:");
                foreach (Version version in versions)
                {
                    Logger.Log(LogType.Warning, LogCategory.Import, version.ToString());
                }
            }
        }

        /* MRH - don't delete, might need later
        private void PrepareExportDirectory(string path)
        {
            if (DirectoryUtils.Exists(path))
            {
                Console.WriteLine(path);
                DirectoryUtils.Delete(path, true);
            }
        }
        */

        private GameStructure GameStructure { get; set; }
    }

    public class uTinyRipperWrapper : DynamicObject { 

        private PythonWrapper wrapper;

        public uTinyRipperWrapper()
        {
            wrapper = new PythonWrapper();
        }

        public void rip(string[] args, string targetDir)
        {
            wrapper.rip(args, targetDir);
            return;
        }
    }
}
