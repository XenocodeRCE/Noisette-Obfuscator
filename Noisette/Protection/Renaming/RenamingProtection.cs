using dnlib.DotNet;
using NoisetteCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoisetteCore.Protection.Renaming
{
    internal class RenamingProtection
    {
        public ModuleDefMD _module;

        public RenamingProtection(ModuleDefMD module)
        {
            _module = module;
        }

        public void RenameModule()
        {
            Obfuscation.RenameAnalyzer RA = new Obfuscation.RenameAnalyzer(_module);
            RA.PerformAnalyze();
        }

        //I don't think I really want to instance a new RenamingProtection each time
        public static string GenerateNewName()
        {
            SafeRandom random = new SafeRandom();
            List<String> Methname = new List<string>(NoisetteCore.Renaming.Method1);
            return Methname[random.Next(0, Methname.Count)];
        }
    }
}