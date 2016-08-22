using dnlib.DotNet;
using dnlib.DotNet.Writer;
using System;
using System.Collections.Generic;

namespace Noisette.Obfuscation
{
    internal class ObfuscationProcess
    {
        public ModuleDefMD _module;

        public static Dictionary<Tuple<MethodDef>, bool> SetObfuscateMethod;

        // Core.Property.module =
        public ObfuscationProcess(ModuleDefMD module)
        {
            _module = module;
            SetObfuscateMethod = new Dictionary<Tuple<MethodDef>, bool>();
            Core.Property.opts = new ModuleWriterOptions(_module);

            ObfuscationAnalyzer AZ = new ObfuscationAnalyzer(_module);
            AZ.PerformAnalyze();
        }

        public void DoObfusction()
        {
            //Renaming
            Protection.Renaming.RenamingProtection RP = new Protection.Renaming.RenamingProtection(_module);
            RP.RenameModule();

            //Prepare our mdulewriter for urther usage
            //Melt Constant
            //Protection.ConstantMelting.ConstantMeltingProtection.MeltConstant(module);
            ////outline constant
            //Protection.ConstantOutlinning.ConstantOutlinningProtection.OutlineConstant(module);
            ////Mutate Constant
            //Protection.ConstantMutation.ConstantMutationProtection.MutateConstant(module);

            //Inject Antitamper class
            ////Protection.AntiTampering.AntiTamperingProtection.AddCall(module);
            //invalid metadata
            //Protection.InvalidMetadata.InvalidMD.InsertInvalidMetadata(module);
            //todo : something is wrong

            //Save assembly
            Core.Property.opts.Logger = DummyLogger.NoThrowInstance;
            //todo : make a propre saving function because now its ridiculous
            _module.Write(_module.Location + "_protected.exe", Core.Property.opts);
            //post-stage antitamper
            //Protection.AntiTampering.AntiTamperingProtection.Md5(module.Location + "_protected.exe");
        }
    }
}