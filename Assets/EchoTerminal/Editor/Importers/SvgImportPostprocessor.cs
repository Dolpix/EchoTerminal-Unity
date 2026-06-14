using UnityEditor;

namespace EchoTerminal.Editor.Importers
{
public class SvgImportPostprocessor : AssetPostprocessor
{
	private const int _uiToolkitVectorImage = 3;
	private const string _iconsFolder = "EchoTerminal/UI/Icons/";

	private static readonly string[] _svgTypePropertyNames = { "m_SvgType", "svgType" };

	private void OnPreprocessAsset()
	{
		if (!assetPath.EndsWith(".svg") || !assetPath.Contains(_iconsFolder))
		{
			return;
		}

		if (!assetImporter.GetType().Name.Contains("SVG"))
		{
			return;
		}

		var so = new SerializedObject(assetImporter);

		SerializedProperty svgType = null;
		foreach (string name in _svgTypePropertyNames)
		{
			svgType = so.FindProperty(name);
			if (svgType != null)
			{
				break;
			}
		}

		if (svgType == null || svgType.intValue == _uiToolkitVectorImage)
		{
			return;
		}

		svgType.intValue = _uiToolkitVectorImage;
		so.ApplyModifiedPropertiesWithoutUndo();
	}
}
}