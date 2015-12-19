using UnityEngine;
using System.Collections;
using System.CodeDom;
using System.IO;
using System;
using UnityEditor;


/// <summary>
/// A Code File to be generated by uGenerate.  Generally, this is created by a Plugin in some way or another, when called for via the iuGenereatePlugin interface.
/// </summary>
public class uGenerateCodeFile {

	public uGenerateCodeFile (CodeCompileUnit codeCompileUnit, string[] path, GenerateMode generateMode)
	{
		this.codeCompileUnit = codeCompileUnit;
		this.path = path;
		this.generateMode = generateMode;
	}

	/// <summary>
	/// Modes of generation.
	/// For now, we don't support "refactoring."
	/// </summary>
	public enum GenerateMode{
		/// <summary>
		/// Will always overwrite the code file if it exists.
		/// </summary>
		AlwaysOverwrite,
		/// <summary>
		/// Does not generate at all.
		/// </summary>
		DoNotGenerate,
		CreateIfMissing,
	}

	/// <summary>
	/// the actual meat of the file in CodeDom form.
	/// </summary>
	public CodeCompileUnit codeCompileUnit;

	/// <summary>
	/// Relative path in the form of tokens.  The last should obviously be the full filename including the extension.  e.g. {"my_directory","myclass.cs"}
	/// </summary>
	public string[] path;

	/// <summary>
	/// The mode to generate with.
	/// </summary>
	public GenerateMode generateMode;

	public string CombinedRelativePath(){
		return string.Join ("/", path);
	}
}
