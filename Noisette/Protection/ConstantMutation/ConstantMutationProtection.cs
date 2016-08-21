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
            //Then replace 0s
            Protection.ConstantMutation.Mutator.ZeroReplacer();
            //Encapsulate Strings with Boolean statments
            Protection.ConstantMutation.Mutator.Booleanisator();
        }
    }

    public static class Mutator
    {
        public static void CollatzConjecture()
        {
            //inject the collatz class
            ModuleDefMD typeModule = ModuleDefMD.Load(typeof (Runtime.CollatzConjecture).Module);
            MethodDef cctor = Core.Property.module.GlobalType.FindOrCreateStaticConstructor();
            TypeDef typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof (Runtime.CollatzConjecture).MetadataToken));
            IEnumerable<IDnlibDef> members = Inject_Helper.InjectHelper.Inject(typeDef, Core.Property.module.GlobalType,
                Core.Property.module);
            var init = (MethodDef) members.Single(method => method.Name == "ConjetMe");

            //check for ldci which value is '1'
            foreach (TypeDef type in Core.Property.module.Types)
            {
                if (type.IsGlobalModuleType) continue;
                foreach (MethodDef method in type.Methods)
                {
                    if (!method.HasBody) continue;
                    var instr = method.Body.Instructions;
                    for (int i = 0; i < instr.Count; i++)
                    {
                        if (instr[i].IsLdcI4())
                        {
                            var value = instr[i].GetLdcI4Value();
                            if (value == 1)
                            {
                                Random rnd = new Random();
                                instr[i].Operand = rnd.Next(1, 15);
                                method.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Call, init));
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

                                    var fieldarray = typeof (System.String).GetFields();
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
                                    instr.Insert(i,
                                        Instruction.Create(OpCodes.Call,
                                            method.Module.Import(typeof (System.String).GetMethod("IsNullOrEmpty"))));
                                    instr.Insert(i + 1, Instruction.Create(OpCodes.Stloc_S, bool_local));
                                    instr.Insert(i + 2, Instruction.Create(OpCodes.Ldloc_S, bool_local));
                                }
                            }
                        }
                    }

                }
            }
        }

        /*
         * You may think you know what the following code does.
         * But you dont. Trust me.
         * Fiddle with it, and youll spend many a sleepless
         * night cursing the moment you thought youd be clever
         * enough to "optimize" the code below.
         * Now close this file and go play with something else.
         */
        public static void ZeroReplacer()
        {

            bool empty = false;
            bool list = false;

            ModuleDefMD typeModule = ModuleDefMD.Load(typeof(Runtime.NewArray).Module);
            TypeDef typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(Runtime.NewArray).MetadataToken));
            MethodDef cctor = Core.Property.module.GlobalType.FindOrCreateStaticConstructor();
            IEnumerable<IDnlibDef> members = Inject_Helper.InjectHelper.Inject(typeDef, Core.Property.module.GlobalType,
                Core.Property.module);
            var init = (FieldDef)members.Single(method2 => method2.Name == "noisette");
            if (init == null) throw new ArgumentNullException(nameof(init));
            Random rnd4 = new Random();
            init.Name = init.Name + "" + rnd4.Next(0, 99999999);

            foreach (TypeDef type in Core.Property.module.Types)
            {
                if (type.IsGlobalModuleType) continue;
                foreach (MethodDef method in type.Methods)
                {
                    if (method.FullName.Contains("My.")) continue; //VB gives cancer anyway
                    if (!method.HasBody) continue;
                    if (method.Body.HasExceptionHandlers) continue;
                    var instr = method.Body.Instructions;
                    for (int i = 0; i < instr.Count; i++)
                    {
                        if (!instr[i].IsLdcI4()) continue;
                        Random rnd = new Random();

                        switch (rnd.Next(0, 2))
                        {
                            case 0:
                                if (empty)
                                {
                                goto case 1;

                            }
                            empty = true;
                            list = false;
                            Random rnd2 = new Random();
                                switch (rnd2.Next(0, 2))
                                {
                                    case 0:
                                        method.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Add));
                                        break;
                                    case 1:
                                        method.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Sub));
                                        break;
                                }
                                method.Body.Instructions.Insert(i + 1,
                                    Instruction.Create(OpCodes.Ldsfld,
                                        method.Module.Import((typeof (Type).GetField("EmptyTypes")))));
                                method.Body.Instructions.Insert(i + 2, Instruction.Create(OpCodes.Ldlen));
                                i += 5;
                                break;
                            case 1:
                            if (list)
                            {
                                goto case 0;

                            }
                            list = true;
                            empty = false;

                                Random rnd3 = new Random();
                            switch (rnd3.Next(0, 2))
                            {
                                case 0:
                                method.Body.Instructions.Insert(i+1, Instruction.Create(OpCodes.Add));
                                break;
                                case 1:
                                method.Body.Instructions.Insert(i+1, Instruction.Create(OpCodes.Sub));
                                break;
                            }
                            method.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Conv_I4));
                            method.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Ldlen));
                            method.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Callvirt, method.Module.Import(typeof(List<String>).GetMethod("ToArray"))));

                            method.Body.Instructions.Insert(i+1, Instruction.Create(OpCodes.Ldsfld, init));
                               
                            
                               
                            i += 5;
                                break;
                        }
                    }
                }
            }
        }
    }
}