using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Noisette.Obfuscation
{
    /// <summary>
    /// This class contains methods and functions passed to do some verifications 
    /// on the file
    /// </summary>
    public static class PreProcessing
    {
        /// <summary>
        /// Used for further references in this class
        /// </summary>
        public static ModuleDefMD module = Core.Property.module;

        /// <summary>
        /// A basic void method to call all our Analyzers functions
        /// and method
        /// </summary>
        public static void AnalyzePhase()
        {

            //Let's check first for System.Reflection references 
            PreProcessingCheckers.ReflectionAnalyzer(module);


        }


    }


    /// <summary>
    /// Internal Class which contains void and bool method
    /// mostly for analyzing puposes
    /// </summary>
    internal class PreProcessingCheckers
    {
        /// <summary>
        /// Check for <see cref="System.Reflection"/> references in the file
        /// </summary>
        /// <param name="module">the file to protect</param>
        public static void ReflectionAnalyzer(ModuleDefMD module)
        {
            foreach (TypeDef type in module.Types)
            {
                foreach (MethodDef method in type.Methods)
                {
                    if (!method.HasBody) continue;
                    if (!method.Body.HasInstructions) continue;
                    foreach (Instruction instr in method.Body.Instructions)
                    {
                        if (instr.OpCode == OpCodes.Call || instr.OpCode == OpCodes.Calli ||
                            instr.OpCode == OpCodes.Callvirt)
                        {
                            if (instr.Operand.ToString().ToLower().Contains("reflection"))
                            {
                                //if the method contains a reference to System.Reflection
                                //We add it to the ContainsReflectionReference list
                                Core.Property.ContainsReflectionReference.Add(method);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
