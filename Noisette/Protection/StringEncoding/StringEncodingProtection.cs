using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using NoisetteCore.Helper;

namespace NoisetteCore.Protection.StringEncoding
{
    internal class StringEncodingProtection
    {
        //TODO: Create unique key for every assembly based on a "profile".

        private static MethodDef _injectedDecryptionMethodDef;
        private const int Keysize = 256;
        private const int DerivationIterations = 1000;
        private ModuleDefMD _moduleDef;

        public StringEncodingProtection(ModuleDefMD module)
        {
            _moduleDef = module;
        }

        public void DoProcess()
        {
            InjectDecryptionClass(_moduleDef);

            foreach (TypeDef type in _moduleDef.GetTypes())
            {
                if (type.IsGlobalModuleType || type.Name == "Resources" || type.Name == "Settings") continue;

                foreach (MethodDef method in type.Methods)
                {
                    if (!method.HasBody) continue;
                    var instr = method.Body.Instructions;
                    for (int i = 0; i < instr.Count - 3; i++)
                    {
                        if (instr[i].OpCode == OpCodes.Ldstr)
                        {
                            var originalStr = instr[i].Operand as string;
                            var encodedStr = StrEncrypt(originalStr);
                            instr[i].Operand = encodedStr;
                            instr.Insert(i + 1, Instruction.Create(OpCodes.Call, _injectedDecryptionMethodDef));
                        }
                    }
                    method.Body.SimplifyBranches();
                    method.Body.OptimizeBranches();
                }
            }
        }

        private void InjectDecryptionClass(ModuleDef module)
        {
            ModuleDefMD typeModule = ModuleDefMD.Load(typeof(StringDecodingHelper).Module);
            TypeDef typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(StringDecodingHelper).MetadataToken));
            IEnumerable<IDnlibDef> members = Inject_Helper.InjectHelper.Inject(typeDef, module.GlobalType, module);
            _injectedDecryptionMethodDef = (MethodDef)members.Single(method => method.Name == "StrDecrypt");

            foreach (MethodDef md in module.GlobalType.Methods)
            {
                if (md.Name == ".ctor")
                {
                    module.GlobalType.Remove(md);
                    break;
                }
            }
        }

        private string StrEncrypt(string plainText)
        {
            string passPhrase = "2MNNBVC43XXZlk89jhgfdsaPO8934IUYTrewq1";
            var saltStringBytes = Generate256BitsOfRandomEntropy();
            var ivStringBytes = Generate256BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        private byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32];
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
}
