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

/// <summary>
/// Implement this interface to create a uGenereate plugn.  uGenerate will scan all assemblies for it and load it automatically.
/// </summary>
public interface IuGeneratePlugin{
	
	/// <summary>
	/// Called after the plugin has been created and added to the list, to run any registration-time code.
	/// </summary>
	void RegisterPlugin ();

	/// <summary>
	/// Called after all plugins are registred.  You might use this to make sure your plugin is later in the list than others you depend upon, for example.
	/// </summary>
	void PostRegistration();

	/// <summary>
	/// Sometimes called to unregister the plugin.  But it's not guaronteed to be called.  More often, the plugin falls out of scope and is garbage collected at some undetermined time.
	/// </summary>
	void UnRegisterPlugin ();

	/// <summary>
	/// Asks the plugin to create CodeFiles to be genereated.
	/// </summary>
	/// <returns>The code files.</returns>
	/// <param name="lang">The langauge being genereated .</param>
	/// <param name="proj">The project for which we are generating.</param>
	/// <param name="provider">The code provider we are generating with.  Note: you don't need to execute the generation here.  uGenerate will do it for you.  The provider is just here for convenience.</param>
	bool GenerateCodeFiles (Dictionary<string,uGenerateCodeFile> codebase, uGenerate.Language lang, uGenerateProject proj, System.CodeDom.Compiler.CodeDomProvider provider, List<uGenerateWarning> warnings, out uGenerateError error);

	/// <summary>
	/// Returns the directory for this plugin given the project.  Each project has its own source directory for each plugin.
	/// Plugins should keep themselves clear of one another and keep all their project data in this directory.
	/// </summary>
	/// <returns>The directory for project.</returns>
	/// <param name="proj">Proj.</param>
	System.IO.DirectoryInfo pluginSourceDirectoryForProject(uGenerateProject proj);

}
