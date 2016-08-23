using dnlib.DotNet;
using dnlib.DotNet.Emit;
using NoisetteCore.Helper;
using NoisetteCore.Obfuscation;
using NoisetteCore.Protection.AntiTampering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MethodAttributes = dnlib.DotNet.MethodAttributes;
using MethodImplAttributes = dnlib.DotNet.MethodImplAttributes;

namespace NoisetteCore.Protection.Constant
{
    internal class ConstantProtection
    {
        private ModuleDefMD _module;
        public MethodDef CollatzCtor;
        public SafeRandom random;

        public List<MethodDef> ProxyMethodConst = new List<MethodDef>();
        public List<MethodDef> ProxyMethodStr = new List<MethodDef>();

        public ConstantProtection(ModuleDefMD module)
        {
            _module = module;
            random = new SafeRandom();
            InitializeCollatz();
        }

        public void DoProcess()
        {
            foreach (TypeDef type in _module.Types)
            {
                ExplodeMember(type);
            }
        }

        public void ExplodeMember(TypeDef type)
        {
            for (int index = 0; index < type.Methods.Count; index++)
            {
                var method = type.Methods[index];
                if (CanObfuscate(method))
                    ProcessProtection(method);
            }
        }

        public void ExplodeMember()
        {
        }

        public void ProcessProtection(MethodDef method)
        {
            var instr = method.Body.Instructions;
            for (int i = 0; i < instr.Count; i++)
            {
                if (instr[i].IsLdcI4())
                {
                    ProtectIntegers(method, i);
                    i += 10;
                }
            }
        }

        public void ProtectIntegers(MethodDef method, int i)
        {
            switch (random.Next(0, 3))
            {
                case 0:
                OutelineValue(method, i);
                break;

                case 1:
                ReplaceValue(method, i);
                break;

                case 2:
                InlineInteger(method, i);
                break;
            }
        }

        public void InlineInteger(MethodDef method, int i)
        {
            if (method.DeclaringType.IsGlobalModuleType) return;
            var instr = method.Body.Instructions;
            bool is_valid_inline = true;
            switch (random.Next(0, 2))
            {
                //true
                case 0:
                is_valid_inline = true;
                break;
                //false
                case 1:
                is_valid_inline = false;
                break;
            }

            Local new_local = new Local(method.Module.CorLibTypes.String);
            method.Body.Variables.Add(new_local);
            Local new_local2 = new Local(method.Module.CorLibTypes.Int32);
            method.Body.Variables.Add(new_local2);
            var value = instr[i].GetLdcI4Value();
            var first_ldstr = Renaming.RenamingProtection.GenerateNewName();

            instr.Insert(i, Instruction.Create(OpCodes.Ldloc_S, new_local2));

            instr.Insert(i, Instruction.Create(OpCodes.Stloc_S, new_local2));
            if (is_valid_inline)
            {
                instr.Insert(i, Instruction.Create(OpCodes.Ldc_I4, value));
                instr.Insert(i, Instruction.Create(OpCodes.Ldc_I4, value + 1));
            }
            else
            {
                instr.Insert(i, Instruction.Create(OpCodes.Ldc_I4, value + 1));
                instr.Insert(i, Instruction.Create(OpCodes.Ldc_I4, value));
            }
            instr.Insert(i,
                Instruction.Create(OpCodes.Call,
                    method.Module.Import(typeof(System.String).GetMethod("op_Equality",
                        new Type[] { typeof(string), typeof(string) }))));
            instr.Insert(i, Instruction.Create(OpCodes.Ldstr, first_ldstr));
            instr.Insert(i, Instruction.Create(OpCodes.Ldloc_S, new_local));
            instr.Insert(i, Instruction.Create(OpCodes.Stloc_S, new_local));
            if (is_valid_inline)
            {
                instr.Insert(i, Instruction.Create(OpCodes.Ldstr, first_ldstr));
            }
            else
            {
                instr.Insert(i,
                    Instruction.Create(OpCodes.Ldstr,
                        Renaming.RenamingProtection.GenerateNewName()));
            }
            instr.Insert(i + 5, Instruction.Create(OpCodes.Brtrue_S, instr[i + 6]));
            instr.Insert(i + 7, Instruction.Create(OpCodes.Br_S, instr[i + 8]));

            instr.RemoveAt(i + 10);

            if (ProxyMethodConst.Contains(method))
            {
                var last = method.Body.Instructions.Count;
                if (!instr[last - 2].IsLdloc()) return;
                instr[last - 2].OpCode = OpCodes.Ldloc_S;
                instr[last - 2].Operand = new_local2;
            }
        }

        public void ReplaceValue(MethodDef method, int i)
        {
            var instr = method.Body.Instructions;
            var value = instr[i].GetLdcI4Value();
            if (value == 1)
                CollatzConjecture(method, i);
            if (value == 0)
                EmptyTypes(method, i);
        }

        public void CollatzConjecture(MethodDef method, int i)
        {
            if (method.DeclaringType.IsGlobalModuleType) return;
            var instr = method.Body.Instructions;
            instr[i].Operand = random.Next(1, 15); //the created logic three should be little enough here
            method.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Call, CollatzCtor));
        }

        public void EmptyTypes(MethodDef method, int i)
        {
            if (method.DeclaringType.IsGlobalModuleType) return;

            switch (random.Next(0, 2))
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
                    method.Module.Import((typeof(Type).GetField("EmptyTypes")))));
            method.Body.Instructions.Insert(i + 2, Instruction.Create(OpCodes.Ldlen));
        }

        public void OutelineValue(MethodDef method, int i)
        {
            if (method.DeclaringType.IsGlobalModuleType) return;
            if ((!ProxyMethodConst.Contains(method)))
            {
                for (int index = 0; index < method.Body.Instructions.Count; index++)
                {
                    Instruction instr = method.Body.Instructions[index];
                    if (instr.IsLdcI4())
                    {
                        MethodDef proxy_method = CreateReturnMethodDef(instr.GetLdcI4Value(), method);
                        method.DeclaringType.Methods.Add(proxy_method);
                        ProxyMethodConst.Add(proxy_method);
                        instr.OpCode = OpCodes.Call;
                        instr.Operand = proxy_method;
                    }
                    else if (instr.OpCode == OpCodes.Ldc_R4)
                    {
                        MethodDef proxy_method = CreateReturnMethodDef(instr, method);
                        method.DeclaringType.Methods.Add(proxy_method);
                        ProxyMethodConst.Add(proxy_method);
                        instr.OpCode = OpCodes.Call;
                        instr.Operand = proxy_method;
                    }
                    else if (instr.Operand is string && instr.OpCode == OpCodes.Ldstr)
                    {
                        MethodDef proxy_method = CreateReturnMethodDef(instr, method);
                        method.DeclaringType.Methods.Add(proxy_method);
                        ProxyMethodConst.Add(proxy_method);
                        instr.OpCode = OpCodes.Call;
                        instr.Operand = proxy_method;
                    }
                }
            }
        }

        public void InitializeCollatz()
        {
            ModuleDefMD typeModule = ModuleDefMD.Load(typeof(Runtime.CollatzConjecture).Module);
            MethodDef cctor = _module.GlobalType.FindOrCreateStaticConstructor();
            TypeDef typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(Runtime.CollatzConjecture).MetadataToken));
            IEnumerable<IDnlibDef> members = Inject_Helper.InjectHelper.Inject(typeDef, _module.GlobalType, _module);
            CollatzCtor = (MethodDef)members.Single(method => method.Name == "ConjetMe");
        }

        public MethodDef CreateReturnMethodDef(object constantvalue, MethodDef source_method)
        {
            CorLibTypeSig corlib = null;

            if (constantvalue is int)
            {
                corlib = source_method.Module.CorLibTypes.Int32;
            }
            else
            {
                if (constantvalue is Instruction)
                {
                    var abecede = constantvalue as Instruction;
                    constantvalue = abecede.Operand;
                }
            }
            if (constantvalue is float)
            {
                corlib = source_method.Module.CorLibTypes.Single;
            }
            if (constantvalue is string)
            {
                corlib = source_method.Module.CorLibTypes.String;
            }

            var meth = new MethodDefUser("_" + source_method.Name + "_" + constantvalue.ToString(),
                MethodSig.CreateStatic(corlib),
                MethodImplAttributes.IL | MethodImplAttributes.Managed,
                MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig)
            { Body = new CilBody() };

            Local return_value = new Local(corlib);
            meth.Body.Variables.Add(return_value);

            //Method body
            meth.Body.Instructions.Add(OpCodes.Nop.ToInstruction());
            if (constantvalue is int)
            {
                meth.Body.Instructions.Add((int)constantvalue != 0
                    ? Instruction.Create(OpCodes.Ldc_I4, (Int32)constantvalue)
                    : Instruction.Create(OpCodes.Ldc_I4_0));
            }
            if (constantvalue is float)
            {
                meth.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_R4, (Single)constantvalue));
            }
            if (constantvalue is string)
            {
                meth.Body.Instructions.Add(Instruction.Create(OpCodes.Ldstr, (string)constantvalue));
            }
            meth.Body.Instructions.Add(OpCodes.Stloc_0.ToInstruction());
            var test_ldloc = new Instruction(OpCodes.Ldloc_0);
            meth.Body.Instructions.Add(test_ldloc);
            meth.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
            Instruction target = meth.Body.Instructions[3];
            meth.Body.Instructions.Insert(3, Instruction.Create(OpCodes.Br_S, target));
            return meth;
        }

        public bool CanObfuscate(MethodDef method)
        {
            foreach (var lst in ObfuscationProcess.SetObfuscateMethod)
            {
                if (lst.Key.Item1.Name == method.Name)
                    return false;
            }
            return true;
        }
    }
}