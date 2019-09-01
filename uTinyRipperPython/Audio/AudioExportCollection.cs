using uTinyRipper.AssetExporters;
using uTinyRipper.Classes;

namespace uTinyRipper.Exporters
{
	public class AudioExportCollection : AssetExportCollection
	{
		public AudioExportCollection(IAssetExporter assetExporter, AudioClip asset) :
			base(assetExporter, asset)
		{
		}

		protected override string GetExportExtension(Object asset)
		{
			AudioClip audioClip = (AudioClip)asset;
			return AudioAssetExporter.IsSupported(audioClip) ? "wav" : audioClip.ExportExtension;
		}
	}
}
