using UnityEngine;
using System.Collections;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

public static class CodeDomExtensions  {

	public static CodeNamespace CheckGetNamespace(this CodeCompileUnit self, string name){
		CodeNamespace ret = null;
		CodeNamespace[] namespaces = new CodeNamespace[self.Namespaces.Capacity];
		self.Namespaces.CopyTo (namespaces, 0);
		ret = (from n in namespaces
		       where n.Name == name
		       select n).FirstOrDefault ();
		if (ret == null) {
			ret = new CodeNamespace (name);
			self.Namespaces.Add (ret);
		}
		return ret;
	}


	public static uGenerateCodeFile CheckGetUnit(this Dictionary<string, uGenerateCodeFile> self, string[] path){
		var p = string.Join ("/", path);
		return CheckGetUnit (self, p);
	}

	public static uGenerateCodeFile CheckGetUnit(this Dictionary<string, uGenerateCodeFile> self, string path){
		var tokens = path.Split (new char[]{'/'}, System.StringSplitOptions.RemoveEmptyEntries);
		if (self.ContainsKey (path)) {
			return self [path];
		} else {
			var ret = new uGenerateCodeFile (new CodeCompileUnit (), tokens, uGenerateCodeFile.GenerateMode.DoNotGenerate);
			self [path] = ret;
			return ret;
		}
	}

	public static CodeTypeDeclaration CheckGetType(this CodeNamespace self, string name){
		CodeTypeDeclaration ret = null;
		CodeTypeDeclaration[] decls = new CodeTypeDeclaration[self.Types.Capacity];
		self.Types.CopyTo (decls, 0);
		ret = (from d in decls
		       where d.Name == name
		       select d).FirstOrDefault ();
		if (ret == null) {
			ret = new CodeTypeDeclaration (name);
			self.Types.Add (ret);
		}
		return ret;
	}

	public static T CheckGetMember<T>(this CodeTypeDeclaration self, string name) where T : CodeTypeMember, new(){
		T ret = null;
		CodeTypeMember[] members = new CodeTypeMember[self.Members.Capacity];
		self.Members.CopyTo (members, 0);
		ret = (from d in members
			where d.Name == name
			select d).FirstOrDefault () as T;
		if (ret == null) {
			ret = System.Activator.CreateInstance<T>();
			ret.Name = name;
			self.Members.Add (ret);
		}
		return ret;
	}

}
