using UnityEngine;
using System.Collections;

public abstract class uGenereateIssue{
	public string message;
}

public class uGenerateError : uGenereateIssue{
}

public class uGenerateWarning : uGenereateIssue{
}

