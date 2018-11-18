using dnlib.DotNet;
using dnlib.DotNet.Emit;
using NoisetteCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoisetteCore.Protection.ControlFlow
{
    public class ControlFlowProtection
    {
        public ModuleDefMD _module;

        public ControlFlowProtection(ModuleDefMD module) : base()
        {
            _module = module;
        }

        public void Process()
        {
            ProtectModule(_module);
        }

        public void ProtectModule(ModuleDef module)
        {
            foreach (TypeDef type in module.Types)
            {
                ProtectType(type);
            }
        }

        public void ProtectType(TypeDef type)
        {
            if (type.IsRuntimeSpecialName || type.IsWindowsRuntime)
                return;

            foreach (MethodDef method in type.Methods)
            {
                ProtectMethod(method);
            }
        }

        public void ProtectMethod(MethodDef method)
        {
            if (!method.HasBody)
                return;

            if (!method.Body.HasInstructions)
                return;

            int variableValue = 0;
            FieldDef variable = CreateInt32Field(method);
            IList<Instruction> instructions = method.Body.Instructions;
            for (int i = 0; i < instructions.Count; i++)
            {
                if (instructions[i].IsLdcI4())
                {
                    int index = i == 0 ? 0 : i - 1;
                    int currentValue = instructions[i].GetLdcI4Value();

                    int difference = variableValue - currentValue;
                    int differencePositive = Math.Abs(difference);

                    // write new body
                    instructions[i] = new Instruction(OpCodes.Ldsfld, variable);
                    instructions.Insert(index, new Instruction(OpCodes.Stsfld, variable));
                    instructions.Insert(index, new Instruction(difference < 0 ? OpCodes.Add : OpCodes.Sub));
                    instructions.Insert(index, new Instruction(OpCodes.Ldc_I4, differencePositive));
                    instructions.Insert(index, new Instruction(OpCodes.Ldsfld, variable));

                    // record current value
                    variableValue = currentValue;

                    i += 4;
                }
            }
        }

        private FieldDef CreateInt32Field(TypeDef type)
        {
            Importer importer = new Importer(type.Module);
            ITypeDefOrRef reference = importer.Import(typeof(Int32));

            TypeSig signature = reference.ToTypeSig();
            FieldAttributes attributes = FieldAttributes.Static | FieldAttributes.Public;

            FieldDef field = new FieldDefUser(GetRandomString(60), new FieldSig(signature), attributes);
            type.Fields.Add(field);

            return field;
        }

        private FieldDef CreateInt32Field(MethodDef method)
        {
            return CreateInt32Field(method.DeclaringType);
        }

        private string GetRandomString(int len)
        {
            string alpha = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            StringBuilder b = new StringBuilder(len);
            for (int i = 0; i < b.Capacity; i++)
                b.Append(alpha[SafeRandom.GetNext(0, alpha.Length)]);
            return b.ToString();
        }
    }
}
