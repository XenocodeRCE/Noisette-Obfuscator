using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Noisette.Protection.AntiTampering;

namespace Noisette.Protection.ConstantMutation
{
    class ConstantMutationProtection
    {
        public static void MutateConstant(ModuleDefMD module)
        {
            //Collatz Conjecture first to replace 1s
            Protection.ConstantMutation.Mutator.CollatzConjecture();
            //Encapsulate Strings with Boolean statments
            Protection.ConstantMutation.Mutator.Booleanisator();
        }
    }

    public static class Mutator
    {
        public static void CollatzConjecture()
        {
            //inject the collatz class
            ModuleDefMD typeModule = ModuleDefMD.Load(typeof(Runtime.CollatzConjecture).Module);
            MethodDef cctor = Core.Property.module.GlobalType.FindOrCreateStaticConstructor();
            TypeDef typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(Runtime.CollatzConjecture).MetadataToken));
            IEnumerable<IDnlibDef> members = Inject_Helper.InjectHelper.Inject(typeDef, Core.Property.module.GlobalType, Core.Property.module);
            var init = (MethodDef)members.Single(method => method.Name == "ConjetMe");
            
            //check for ldci which value is '1'
            foreach(TypeDef type in Core.Property.module.Types)
            {
                if (type.IsGlobalModuleType) continue;
                foreach (MethodDef method in type.Methods)
                {
                    if (!method.HasBody) continue;
                    var instr = method.Body.Instructions;
                    for (int i = 0;  i < instr.Count; i++)
                    {
                        if (instr[i].IsLdcI4())
                        {
                            var value = instr[i].GetLdcI4Value();
                            if (value == 1)
                            {
                                Random rnd = new Random();
                                instr[i].Operand = rnd.Next(1, 15);
                                method.Body.Instructions.Insert(i+1, Instruction.Create(OpCodes.Call, init));
                                i += 2;
                            }
                        }
                    }

                }
            }
        }

        public static void Booleanisator()
        {
            foreach (TypeDef type in Core.Property.module.Types)
            {
                if (type.IsGlobalModuleType) continue;
                foreach (MethodDef method in type.Methods)
                {
                    if (!method.HasBody) continue;
                    var instr = method.Body.Instructions;
                    for (int i = 0; i < instr.Count; i++)
                    {
                        if (instr[i].OpCode == OpCodes.Callvirt)
                        {
                            if (instr[i].Operand.ToString().ToLower().Contains("bool"))
                            {
                                if (instr[i - 1].OpCode == OpCodes.Ldc_I4_0)
                                {
                                    FieldInfo fieldZ = null;
                                    Local bool_local = new Local(Core.Property.module.CorLibTypes.Boolean);
                                    method.Body.Variables.Add(bool_local);

                                    var r2 = new Instruction();

                                    var fieldarray = typeof(System.String).GetFields();
                                    foreach (var field in fieldarray)
                                    {
                                        if (field.Name == "Empty")
                                        {
                                            r2 = new Instruction(OpCodes.Ldsfld, field);
                                            fieldZ = field;
                                            break;
                                        }
                                    }

                                    instr[i - 1].OpCode = OpCodes.Ldsfld;
                                    instr[i - 1].Operand = method.Module.Import(fieldZ);
                                    instr.Insert(i, Instruction.Create(OpCodes.Call, method.Module.Import(typeof(System.String).GetMethod("IsNullOrEmpty"))));
                                    instr.Insert(i + 1,  Instruction.Create(OpCodes.Stloc_S, bool_local));
                                    instr.Insert(i + 2, Instruction.Create(OpCodes.Ldloc_S, bool_local));
                                }
                            }
                        }
                    }

                }
            }
        }
    }
}
