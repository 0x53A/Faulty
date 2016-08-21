using System;
using System.Linq;
using System.Xml.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

public partial class ModuleWeaver
{
	public XElement Config { get; set; }
	public Action<string> LogDebug { get; set; }
	public Action<string> LogInfo { get; set; }
	public Action<string> LogWarning { get; set; }
	public IAssemblyResolver AssemblyResolver { get; set; }
	public ModuleDefinition ModuleDefinition { get; set; }

	public ModuleWeaver()
	{
		LogWarning = s => { };
		LogInfo = s => { };
		LogDebug = s => { };
	}

	MethodDefinition _x;

	public void Execute()
	{
		foreach (var t in ModuleDefinition.Types)
		{
			foreach (var m in t.Methods)
			{
				if (!m.HasBody)
					continue;

				foreach (var i in m.Body.Instructions)
				{
					if (i.OpCode.Code == Code.Call ||
						i.OpCode.Code == Code.Callvirt)
					{
						var target = i.Operand as MethodReference;
						if (target.FullName == "Faulty.Block.ConvertToTryFault")
						{
							// try to find matching block
							var eh = m.Body.ExceptionHandlers
										.Where(x => x.HandlerStart.Offset <= i.Offset &&
											  x.HandlerEnd.Offset >= i.Offset)
										.OrderByDescending(x => x.HandlerStart.Offset).ThenBy(x => x.HandlerEnd.Offset)
										.FirstOrDefault();

							if (eh == null)
								throw new InvalidOperationException("'ConvertToTryFault' can only be called inside the catch-part of a try/catch block, in {m.Name}");

							if (eh.HandlerType != ExceptionHandlerType.Catch)
								throw new InvalidOperationException($"'ConvertToTryFault' can only be called inside the catch-part of a try/catch block, but was inside {eh.HandlerType} in {m.Name}");

							eh.HandlerType = ExceptionHandlerType.Fault;
						}
					}
				}
			}
		}
	}
}

