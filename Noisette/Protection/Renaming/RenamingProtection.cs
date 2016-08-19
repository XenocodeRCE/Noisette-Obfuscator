using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dnlib.DotNet;

namespace Noisette.Protection.Renaming
{
    class RenamingProtection
    {
        public static void RenameModule(ModuleDefMD module)
        {
            List<String> Methname = new List<string>(Noisette.Renaming.Method1);
            List<String> Typename = new List<string>(Noisette.Renaming.Type1);

            Random random = new Random();

            foreach (TypeDef type in module.Types)
            {
                if (type.IsGlobalModuleType) continue;
                string new_name_type = Typename[random.Next(0, Typename.Count)];
                Typename.Remove(new_name_type);
                type.Name = new_name_type;
                foreach (MethodDef method in type.Methods)
                {
                    if (method.IsConstructor) continue;
                    if (!method.HasBody) continue;
                    if (method.FullName.Contains("My.")) continue; //VB gives cancer anyway
                    string new_name_meth = Methname[random.Next(0, Methname.Count)];
                    Methname.Remove(new_name_meth);
                    method.Name = new_name_meth;
                    foreach (Parameter arg in method.Parameters)
                    {
                        string new_name_param = Methname[random.Next(0, Methname.Count)];
                        Methname.Remove(new_name_param);
                        arg.Name = new_name_param;
                    }
                    if (!method.Body.HasVariables) continue;
                    foreach (var variable in method.Body.Variables)
                    {
                        string new_name_var = Methname[random.Next(0, Methname.Count)];
                        Methname.Remove(new_name_var);
                        variable.Name = new_name_var;
                    }
                }

                foreach (PropertyDef prop in type.Properties)
                {
                    string new_name_property = Methname[random.Next(0, Methname.Count)];
                    Methname.Remove(new_name_property);
                    prop.Name = new_name_property;
                }
                foreach (FieldDef field in type.Fields)
                {
                    string new_name_fields = Methname[random.Next(0, Methname.Count)];
                    Methname.Remove(new_name_fields);
                    field.Name = new_name_fields;
                }
            }


        }
    }
}
