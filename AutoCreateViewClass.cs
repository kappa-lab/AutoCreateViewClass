using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Reflection;
using UnityEditor.Callbacks;

public static class AutoCreateViewClass
{

	static string tempFilePath = "Temp/__AutoCreateViewClass__";

	[MenuItem("Tools/Auto Create View Class from Seleced GameObject/MonoBehaviour")]
	private static void Fabulous ()
	{
		Create("MonoBehaviour");
	}

	[MenuItem("Tools/Auto Create View Class from Seleced GameObject/Interaction View")]
	private static void Marvelous ()
	{
		Create("InteractionView");
	}

	private static void Create (string base_class_name)
	{
		if (null == Selection.gameObjects) {
			return;
		}

		if (1 != Selection.gameObjects.Length) {
			return;
		}

		GameObject target = Selection.gameObjects[0];
		string class_name = target.gameObject.name + "View";

		StringBuilder sb = new StringBuilder();

		sb.AppendLine("using UnityEngine;");
		sb.AppendLine("using System.Collections;");
		sb.AppendLine("using System.Collections.Generic;");
		sb.AppendLine("");
		sb.AppendLine("using Engine.Platforms.Unity.NGUI;");
		sb.AppendLine("");
		sb.AppendLine("namespace Game");
		sb.AppendLine("{");
		sb.AppendLine("\tpublic class " + class_name + " : " + base_class_name);
		sb.AppendLine("\t{");
		sb.AppendLine("\t\t");

		UIWidget[] wi = target.GetComponentsInChildren<UIWidget>(true);
		for (int i = 0; i < wi.Length; i++) {
			sb.AppendLine("\t\t[SerializeField] " + wi[i].GetType() + " _" + wi[i].gameObject.name + ";");
		}
		UIPanel[] pa = target.GetComponentsInChildren<UIPanel>(true);
		for (int i = 0; i < pa.Length; i++) {
			sb.AppendLine("\t\t[SerializeField] " + pa[i].GetType() + " _" + pa[i].gameObject.name + ";");
		}

		sb.AppendLine("\t\t");
		sb.AppendLine("\t\tvoid Awake() {");
		sb.AppendLine("\t\t}");
		sb.AppendLine("\t\t");
		sb.AppendLine("\t\tvoid Start() {");
		sb.AppendLine("\t\t}");
		sb.AppendLine("\t\t");
		sb.AppendLine("\t\tvoid Update() {");
		sb.AppendLine("\t\t}");
		sb.AppendLine("\t\t");
		sb.AppendLine("\t\tvoid OnDestroy() {");
		sb.AppendLine("\t\t}");
		sb.AppendLine("\t\t");
		sb.AppendLine("\t}");
		sb.AppendLine("}");

		File.WriteAllText(tempFilePath, class_name, Encoding.UTF8);

		File.WriteAllText("Assets/" + class_name + ".cs", sb.ToString(), Encoding.UTF8);
		AssetDatabase.ImportAsset("Assets/" + class_name + ".cs");
		AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

	}

	public static GameObject FindDeep (GameObject self, string name)
	{
		var children = self.GetComponentsInChildren<Transform>(false);
		foreach (var transform in children) {
			if (transform.name == name) {
				return transform.gameObject;
			}
		}
		return null;
	}

	[DidReloadScripts]
	private static void Log ()
	{
		if (File.Exists(tempFilePath)) {
			StreamReader reader = new StreamReader(tempFilePath, Encoding.UTF8);

			string scriptName = reader.ReadLine();
			reader.Close();

			Type type = GetTypeByClassName(scriptName);
			Component component = Selection.gameObjects[0].AddComponent(type);

			File.Delete(tempFilePath);

			FieldInfo[] fi = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			for (int i = 0; i < fi.Length; i++) {
				string field_name = fi[i].Name.TrimStart('_');
				fi[i].SetValue(component, FindDeep(component.gameObject, field_name).GetComponent(fi[i].FieldType));
			}

		}
	}

	public static Type GetTypeByClassName (string class_name)
	{
		foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
			foreach (Type type in assembly.GetTypes()) {
				if (type.Name == class_name) {
					return type;
				}
			}
		}
		return null;
	}
}
