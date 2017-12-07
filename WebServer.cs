public class WSHelperHelper
{
	WSHelper WebSever = null;
	//string Url = @"http://10.10.0.23:8080/WebSerice/MES_Joint.asmx";
	string Url = @"http://172.18.8.33:8881/CMESSOS/services/CMESWebService";
	//string ClassName = "";
	string ClassName = "CMESWebServicePortTypeClient";
	public WSHelperHelper(string strConfig)
	{
		XmlFO.ReadConfigEx(strConfig, "Url", ref Url);
		XmlFO.ReadConfigEx(strConfig, "ClassName", ref ClassName);
		WebSever = new WSHelper();
		WebSever.Init(Url, ClassName);
	}
	public object Invoke(string MethodName, params object[] args)
	{
		return WebSever.InvokeWebService(MethodName, args);
	}
}

public class WSHelper
{
	//获取WSDL 
	WebClient wc = new WebClient();
	Stream stream;
	ServiceDescription sd;
	ServiceDescriptionImporter sdi = new ServiceDescriptionImporter();

	CodeNamespace cn;
	//生成客户端代理类代码 
	CodeCompileUnit ccu = new CodeCompileUnit();

	CSharpCodeProvider icc = new CSharpCodeProvider();
	//设定编译参数 
	CompilerParameters cplist = new CompilerParameters();
	CompilerResults cr;
	Type t = null;
	object obj;

	public void SetCredential(string loginUser, string loginPSW)
	{
		wc.Credentials = new NetworkCredential(loginUser, loginPSW);
		wc.UseDefaultCredentials = false;
	}

	public void Init(string url, string classname)
	{
		if (t != null)
			return;
		string @namespace = "EnterpriseServerBase.WebService.DynamicWebCalling";
		if ((classname == null) || (classname == ""))
		{
			classname = GetWsClassName(url);
		}

		stream = wc.OpenRead(url + "?WSDL");
		sd = ServiceDescription.Read(stream);
		cn = new CodeNamespace(@namespace);

		sdi.AddServiceDescription(sd, "", "");
		ccu.Namespaces.Add(cn);
		sdi.Import(cn, ccu);

		cplist.GenerateExecutable = false;
		cplist.GenerateInMemory = true;
		cplist.ReferencedAssemblies.Add("System.dll");
		cplist.ReferencedAssemblies.Add("System.XML.dll");
		cplist.ReferencedAssemblies.Add("System.Web.Services.dll");
		cplist.ReferencedAssemblies.Add("System.Data.dll");
		//编译代理类 
		cr = icc.CompileAssemblyFromDom(cplist, ccu);
		if (true == cr.Errors.HasErrors)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			foreach (System.CodeDom.Compiler.CompilerError ce in cr.Errors)
			{
				sb.Append(ce.ToString());
				sb.Append(System.Environment.NewLine);
			}
			throw new Exception(sb.ToString());
		}


		//生成代理实例，并调用方法 
		System.Reflection.Assembly assembly = cr.CompiledAssembly;
		t = assembly.GetType(@namespace + "." + classname, true, true);

	}

	/// < summary> 
	/// 动态调用web服务 
	/// < /summary> 
	/// < param name="url">WSDL服务地址< /param> 
	/// < param name="classname">类名< /param> 
	/// < param name="methodname">方法名< /param> 
	/// < param name="args">参数< /param> 
	/// < returns>< /returns> 
	public object InvokeWebService(string methodname, object[] args)
	{
		obj = Activator.CreateInstance(t);

		System.Reflection.MethodInfo mi = t.GetMethod(methodname);
		object Re = mi.Invoke(obj, args);
		return Re;
	}

	private string GetWsClassName(string wsUrl)
	{
		string[] parts = wsUrl.Split('/');
		string[] pps = parts[parts.Length - 1].Split('.');
		return pps[0];
	}
}

main()
{
	WSHelperHelper WebYD = null;
	if (WebYD == null)
		WebYD = new WSHelperHelper("WebServerForYD");
}


















