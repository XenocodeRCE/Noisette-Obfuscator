using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Writer;

namespace Noisette.Obfuscation
{
    class ObfuscationProcess
    {

        public static void DoObfusction(ModuleDefMD module)
        {
            //Lets call the Pre Processing method to pass 
            //some checkings and verifications before all :)
            Obfuscation.PreProcessing.AnalyzePhase();

            //Prepare our mdulewriter for urther usage
            Core.Property.opts = new ModuleWriterOptions(module);
            //Melt Constant
            Protection.ConstantMelting.ConstantMeltingProtection.MeltConstant(module);
            //Mutate Constant
            Protection.ConstantMutation.ConstantMutationProtection.MutateConstant(module);
            //outline constant
            Protection.ConstantOutlinning.ConstantOutlinningProtection.OutlineConstant(module);
            


            //Inject Antitamper class
            Protection.AntiTampering.AntiTamperingProtection.AddCall(module);
            //rename all
            Protection.Renaming.RenamingProtection.RenameModule(module);
            //invalid metadata
            Protection.InvalidMetadata.InvalidMD.InsertInvalidMetadata(module);

            //Save assembly
            Core.Property.opts.Logger = DummyLogger.NoThrowInstance;
            //todo : make a propre saving function because now its ridiculous
            module.Write(module.Location + "_protected.exe", Core.Property.opts);
            //post-stage antitamper
            Protection.AntiTampering.AntiTamperingProtection.Md5(module.Location + "_protected.exe");
        }


    }
}
