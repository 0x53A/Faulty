using System;
using System.Linq;
using System.Xml.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System.Diagnostics;

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

	public void Execute()
	{
		LogInfo("Start");
		foreach (var t in ModuleDefinition.Types)
		{
			foreach (var m in t.Methods)
			{
				if (!m.HasBody)
					continue;

				for (int iInstr = 0; iInstr < m.Body.Instructions.Count; iInstr++)
				{
					var instr = m.Body.Instructions[iInstr];
					if (instr.OpCode.Code == Code.Call ||
						instr.OpCode.Code == Code.Callvirt)
					{
						var target = instr.Operand as MethodReference;
						if (target.FullName == "System.Void Faulty.Block::ConvertToTryFault()")
						{
							LogInfo($"Converting one in {t.Name}.{m.Name}");
							m.Body.SimplifyMacros();

							// try to find matching block
							var eh = m.Body.ExceptionHandlers
										.Where(x => x?.HandlerStart.Offset
										<= instr.Offset &&
											  x.HandlerEnd.Offset
											  >= instr.Offset)
										.OrderByDescending(x => x.HandlerStart.Offset).ThenBy(x => x.HandlerEnd.Offset)
										.FirstOrDefault();

							if (eh == null)
								throw new InvalidOperationException($"'ConvertToTryFault' can only be called inside the finally-part of a try/finally block, in {t.Name}.{m.Name} at 0x{instr.Offset:x}");

							if (eh.HandlerType != ExceptionHandlerType.Finally)
								throw new InvalidOperationException($"'ConvertToTryFault' can only be called inside the finally-part of a try/finally block, but was inside {eh.HandlerType} in {t.Name}.{m.Name} at 0x{instr.Offset:x}");

							//if (eh.CatchType.FullName != "System.Exception")
							//	throw new InvalidOperationException($"'ConvertToTryFault' can only be used in a 'catch (System.Exception)', but the exception type was {eh.CatchType.FullName} in {t.Name}.{m.Name} at 0x{instr.Offset:x}");

							// iterate over the whole block
							int end = Math.Min(eh.HandlerEnd.Offset - 1, m.Body.Instructions.Count);
							for (int i = eh.HandlerStart.Offset; i < end; i++)
							{
								var instr2 = m.Body.Instructions[i];

								if (iInstr == i)
									continue;

								if (instr2.OpCode.Code == Code.Call ||
									instr2.OpCode.Code == Code.Callvirt)
								{
									var target2 = instr2.Operand as MethodReference;
									if (target2.FullName == "System.Void Faulty.Block::ConvertToTryFault()")
									{
										throw new InvalidOperationException($"There is only one call to 'Faulty.Block::ConvertToTryFault' allowed! In {t.Name}.{m.Name} at 0x{instr2.Offset:x}");
									}
								}

								//// ref https://www.simple-talk.com/blogs/subterranean-il-exception-handling-control-flow/
								//// a catch block is left via leave
								//// a fault block must be left via endfault
								//// make sure the leave in the catch points to the end of the catch block, else throw
								//if (instr2.OpCode == OpCodes.Leave_S)
								//	throw new InvalidOperationException("Unexpected OpCode 'Leave_S', body should be simplified");
								//if (instr2.OpCode == OpCodes.Leave)
								//{
								//	if ((instr2.Operand as Instruction).Offset != eh.HandlerEnd.Offset)
								//		throw new InvalidOperationException($"'leave' instruction must point to end of catch handler ({eh.HandlerEnd.Offset}), but was ({(instr2.Operand as Instruction).Offset})");
								//	instr2.OpCode = OpCodes.Endfinally;
								//}
							}

							eh.HandlerType = ExceptionHandlerType.Fault;
							m.Body.OptimizeMacros();
							iInstr = eh.HandlerEnd.Offset;
						}
					}
				}
			}
		}
	}
}

