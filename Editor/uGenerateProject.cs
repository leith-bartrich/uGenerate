using UnityEngine;
using System.Collections;
using System.CodeDom;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A uGenereateProject.  These configuration objects sit in the Unity Project and tell uGenerate where
/// to find a set of Sources and where to Generate the results to.  The specifics about what kinds of
/// sources are in there and how to handle them are handled by plugins automatically.
/// </summary>
public class uGenerateProject : UnityEngine.ScriptableObject {

	/// <summary>
	/// A project relative path to a directory to generate files to. e.g. {"Assets","generated_code"}
	/// </summary>
	public string[] GeneratePath = new string[]{"Assets","generated_code"};
	public string[] SourcePath = new string[]{"uGenerate"};
	public string Namespace = "";

	public bool GetCodeCompileUnitsForLanguage(Dictionary<string,uGenerateCodeFile> codebase, uGenerate.Language lang, System.CodeDom.Compiler.CodeDomProvider provider, List<uGenerateWarning> warnings, out uGenerateError error){
		foreach (var plugin in uGenerate.Plugins) {
			if (!plugin.GenerateCodeFiles(codebase,lang,this,provider, warnings,out error)){
				return false;
			}
		}
		error = null;
		return true;
	}

	public void CheckCreatePaths(){
		var genpath = new DirectoryInfo(string.Join ("/", GeneratePath));
		var srcpath = new DirectoryInfo(string.Join ("/", SourcePath));
		if (!genpath.Exists) {
			genpath.Create ();
		}
		if (!srcpath.Exists) {
			srcpath.Create ();
		}

	}

}


