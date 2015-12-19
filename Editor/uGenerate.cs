using UnityEngine;
using System.Collections;
using UnityEditor;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Linq;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;


[InitializeOnLoad]
public class uGenerate {

	/// <summary>
	/// Auto executing init code.
	/// </summary>
	static uGenerate(){
		LoadPlugins ();
	}

	/// <summary>
	/// Unlods plugins and reloads them.  Called automatically when unity recompiles the editor assembly.  But made available anyway.
	/// </summary>
	[MenuItem("uGenerate/Reload Plugins")]
	public static void LoadPlugins(){
		if (Plugins != null) {
			foreach (var p in Plugins) {
				try{
					Debug.Log ("uGenereate unregistering Pluign: " + p.GetType().FullName);
					p.UnRegisterPlugin ();
				} catch (Exception e){
					Debug.LogError("uGenerate error un-registering pluign: " + p.GetType().FullName + ": " + e.Message + "\n" + e.StackTrace);
				}
			}
		}
		Plugins = new List<IuGeneratePlugin> ();
		var assemblies = AppDomain.CurrentDomain.GetAssemblies ();
		foreach (var assembly in assemblies) {
			//Debug.Log ("uGenerate Scanning Assembly: " + assembly.FullName);
			foreach (var asstype in assembly.GetTypes()) {
				foreach (var i in asstype.GetInterfaces()) {
					if (i == typeof(IuGeneratePlugin)) {
						Debug.Log ("uGenereate found Pluign: " + asstype.FullName);
						IuGeneratePlugin plugin = null;
						try {
							plugin = Activator.CreateInstance (asstype) as IuGeneratePlugin;
						} catch (Exception e) {
							Debug.LogWarning ("uGenerate could not load plugin:  " + asstype.FullName);
						}
						if (plugin != null) {
							Plugins.Add (plugin);
							try{
								plugin.RegisterPlugin ();
							}catch (Exception e){
								Debug.LogError("uGenerate error registering pluign: " + asstype.FullName + ": " + e.Message + "\n" + e.StackTrace);
							}
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// The plugins list.  Filled automatically by uGenereate.  Available to be modified by plugins. But don't be stupid about it.
	/// </summary>
	/// <value>The plugins.</value>
	public static List<IuGeneratePlugin> Plugins { get; private set;}


	/// <summary>
	/// Genereates all uGenereateProjects in the Unity project.
	/// </summary>
	[MenuItem("uGenerate/Generate/All %g")]
	public static void GenerateAll(){
		try{
			EditorApplication.LockReloadAssemblies ();
			var projects = GetAllProjects ();
			EditorUtility.DisplayCancelableProgressBar ("uGenerate Status", "uGenerating...", 0.0f);
			var count = 0;
			var codebase = new Dictionary<string,uGenerateCodeFile>();
			var warnings = new List<uGenerateWarning>();
			uGenerateError error;
			foreach (var p in projects) {
				EditorUtility.DisplayCancelableProgressBar ("uGenerate Status", "uGenerating: " + p.name, (float)count / (float)projects.Length);
				count++;
				if (GenerateProject (p,codebase, warnings,out error)){
					//there was no error
					foreach (var w in warnings){
						Debug.LogWarning(w.message,p);
					}	
				} else {
					//there was an error
					foreach (var w in warnings){
						Debug.LogWarning(w.message,p);
						Debug.LogError(error.message,p);
						Debug.LogError("uGenereate Failed.");
						break;
					}
				}
			}
			EditorUtility.ClearProgressBar ();
			AssetDatabase.Refresh ();
			EditorApplication.UnlockReloadAssemblies ();
		}catch (Exception e){
			Debug.LogError("uGeneraete error: " + e.Message + "\n" + e.StackTrace);
		} finally {
			EditorApplication.UnlockReloadAssemblies();
			EditorUtility.ClearProgressBar();
		}
	}

	/// <summary>
	/// Generates a given project.
	/// Note, you are responsible for locking and unlocking assembly loading with the EditorApplication.
	/// This doesn't do it for you.
	/// Also, it doens't refresh the asset database for you.
	/// </summary>
	/// <param name="p">P.</param>
	public static bool GenerateProject(uGenerateProject p, Dictionary<string,uGenerateCodeFile> codebase, List<uGenerateWarning> warnings, out uGenerateError error){
		Debug.Log ("uGenerating project: " + p.name, p);
		p.CheckCreatePaths ();

		var csharpProvider = System.CodeDom.Compiler.CodeDomProvider.CreateProvider ("CSharp");
		var csharpOptions = new System.CodeDom.Compiler.CodeGeneratorOptions ();

		if (!p.GetCodeCompileUnitsForLanguage (codebase, Language.CSharp, csharpProvider, warnings, out error)) {
			return false;
		}
		foreach (var codeFile in codebase.Values) {
			var fileinfo = new System.IO.FileInfo (string.Join ("/", codeFile.path));
			if (codeFile.generateMode == uGenerateCodeFile.GenerateMode.AlwaysOverwrite) {
				GenFile (csharpProvider, csharpOptions, codeFile);
			} else if (codeFile.generateMode == uGenerateCodeFile.GenerateMode.DoNotGenerate) {
				//do nothing
			} else if (codeFile.generateMode == uGenerateCodeFile.GenerateMode.CreateIfMissing && !fileinfo.Exists) {
				GenFile (csharpProvider, csharpOptions, codeFile);
			}
		}
		Debug.Log ("done uGenerating project: " + p.name, p);
		return true;
	}

	/// <summary>
	/// Generates a single code file.
	/// Usually called by the GenerateProject method when iterating over a project.
	/// </summary>
	/// <returns>The file.</returns>
	/// <param name="tempPath">Temp path.</param>
	/// <param name="provider">Provider.</param>
	/// <param name="options">Options.</param>
	/// <param name="codeFile">Code file.</param>
	private static void GenFile (System.CodeDom.Compiler.CodeDomProvider provider, System.CodeDom.Compiler.CodeGeneratorOptions options, uGenerateCodeFile codeFile)
	{
		var tempPath = FileUtil.GetUniqueTempPathInProject ();
		var writer = new System.IO.StreamWriter (tempPath);
		provider.GenerateCodeFromCompileUnit (codeFile.codeCompileUnit, writer, options);
		writer.Flush ();
		writer.Close ();
		FileUtil.ReplaceFile (tempPath, string.Join ("/", codeFile.path));
	}

	[MenuItem("Assets/Create/uGenereate/uGenerateProject")]
	[MenuItem("uGenerate/Project/Create New")]
	public static void CreateNewProject(){
		var proj = ScriptableObjectUtility.CreateAsset<uGenerateProject> ();
	}

	/// <summary>
	/// Gets all uGenereateProjet(s) in the current Unity project.
	/// </summary>
	/// <returns>The all projects.</returns>
	public static uGenerateProject[] GetAllProjects(){
		var guids = AssetDatabase.FindAssets ("t:uGenerateProject");
		return (from g in guids select AssetDatabase.LoadAssetAtPath<uGenerateProject> (AssetDatabase.GUIDToAssetPath (g))).ToArray ();
	}

	/// <summary>
	/// Available Languages that can be generated.
	/// This is moslty dependent on System.CodeDom, which in turn is dependent on Mono.
	/// But there is the possibility of supporting other things in the future.  Somehow.
	/// </summary>
	public enum Language{
		CSharp,
		VisualBasic,
		JScript,
		Cpp,
	}


}



