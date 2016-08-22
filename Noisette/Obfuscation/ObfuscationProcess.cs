using dnlib.DotNet;
using dnlib.DotNet.Writer;
using System;
using System.Collections.Generic;
using System.IO;

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
            //Inject Antitamper class
            Protection.AntiTampering.AntiTamperingProtection ATP = new Protection.AntiTampering.AntiTamperingProtection(_module);
            ATP.Process();

            //Melt Constant
            //Protection.ConstantMelting.ConstantMeltingProtection.MeltConstant(module);
            ////outline constant
            //Protection.ConstantOutlinning.ConstantOutlinningProtection.OutlineConstant(module);
            ////Mutate Constant
            //Protection.ConstantMutation.ConstantMutationProtection.MutateConstant(module);

            //invalid metadata
            //Protection.InvalidMetadata.InvalidMD.InsertInvalidMetadata(module);
            //todo : something is wrong

            SaveAssembly();

            //post-stage antitamper
            //todo : make a proper post-process class
            Protection.AntiTampering.AntiTamperingProtection.Md5(Path.GetDirectoryName(_module.Location) + @"\" + Path.GetFileNameWithoutExtension(_module.Location) + "_nutsed.exe");
        }

        public void SaveAssembly()
        {
            Core.Property.opts.Logger = DummyLogger.NoThrowInstance;
            _module.Write(Path.GetDirectoryName(_module.Location) + @"\" + Path.GetFileNameWithoutExtension(_module.Location) + "_nutsed.exe", Core.Property.opts);
        }
    }
}