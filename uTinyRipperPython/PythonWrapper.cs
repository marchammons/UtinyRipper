using System;
using System.Dynamic;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using uTinyRipper;
using Object = uTinyRipper.Classes.Object;
using Version = uTinyRipper.Version;
using uTinyRipper.Exporters;

namespace uTinyRipperPython
{
    public class PythonWrapper {

        public string go()
        {
            return "hello world";
        }

        public static bool AssetSelector(Object asset)
        {
            return true;
        }

        public void rip(string[] args)
        {
            Config.IsAdvancedLog = true;
            Config.IsGenerateGUIDByContent = false;
            Config.IsExportDependencies = false;

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

                // MRH : can make this an option to include/exclude.  Including will transcode, e.g. fsb (FSB5) into wav
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
                */

                // string exportPath = Path.Combine("Ripped", GameStructure.Name);
                string exportPath = $"Ripped";
                // PrepareExportDirectory(exportPath);
                GameStructure.Export(exportPath, AssetSelector);
                Console.WriteLine("Finished");
            }
            catch (Exception ex)
            {
                Console.WriteLine("error : " + ex.ToString());
            }
        }

        private void Validate()
        {
            Version[] versions = GameStructure.FileCollection.Files.Select(t => t.Version).Distinct().ToArray();
            if (versions.Count() > 1)
            {
                Console.WriteLine($"Asset collection has versions probably incompatible with each other. Here they are:");
                foreach (Version version in versions)
                {
                    Console.WriteLine(version.ToString());
                }
            }
        }

        /* MRH - don't delete, might need that
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

        public string go() {
            return wrapper.go();
        }

        public void rip(string[] args)
        {
            wrapper.rip(args);
            return;
        }
    }
}
