using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Noisette.Core;

namespace Noisette.Protection.ConstantOutlinning
{
    public static class ConstantOutlinningProtection
    {
        /*Our arrays*/
        //Constant
        public static List<MethodDef> ProxyMethodConst = new List<MethodDef>();
        //String
        public static List<MethodDef> ProxyMethodStr = new List<MethodDef>();

        public static void OutlineConstant(ModuleDefMD module)
        {
           

            foreach (TypeDef type in module.Types)
            {
                if (type.IsGlobalModuleType)
                {
                    continue;
                }
                for (int i = 0; i < type.Methods.Count; i++)
                {
                    MethodDef method = type.Methods[i];
                    if (Helper.IsValidMethod(method) && (!ProxyMethodConst.Contains(method)))
                    {
                        for (int index = 0; index < method.Body.Instructions.Count; index++)
                        {
                            Instruction instr = method.Body.Instructions[index];
                            if (instr.IsLdcI4())
                            {
                                MethodDef proxy_method = Core.Helper.CreateReturnMethodDef(instr.GetLdcI4Value(), method);
                                type.Methods.Add(proxy_method);
                                ProxyMethodConst.Add(proxy_method);
                                instr.OpCode = OpCodes.Call;
                                instr.Operand = proxy_method;
                            }
                            else if (instr.OpCode == OpCodes.Ldc_R4)
                            {
                                MethodDef proxy_method = Core.Helper.CreateReturnMethodDef(instr, method);
                                type.Methods.Add(proxy_method);
                                ProxyMethodConst.Add(proxy_method);
                                instr.OpCode = OpCodes.Call;
                                instr.Operand = proxy_method;
                            }
                            if (instr.Operand is string && instr.OpCode == OpCodes.Ldstr)
                            {
                                MethodDef proxy_method = Core.Helper.CreateReturnMethodDef(instr, method);
                                type.Methods.Add(proxy_method);
                                ProxyMethodConst.Add(proxy_method);
                                instr.OpCode = OpCodes.Call;
                                instr.Operand = proxy_method;
                            }

                        }
                    }
                }
            }
        }
    }
}
