using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Writer;

namespace Noisette.Core
{
    /// <summary>
    ///     Provide every used fields and other needed ojects
    /// </summary>
    class Property
    {
        /// <summary>
        ///     The main Assembly Module needed for Dnlib
        /// </summary>
        public static ModuleDefMD module { get; set; }

        /// <summary>
        ///     The modulewriter used when saving / writting modified assembly
        /// </summary>
        public static ModuleWriterOptions opts { get; set; }

        /// <summary>
        /// Directory path where the file is in
        /// </summary>
        public static string DirectoryName { get; set; }

        /// <summary>
        /// A list which will contains <see cref="MethodDef"/> who may contains
        /// <see cref="System.Reflection"/> references
        /// </summary>
        public static List<MethodDef> ContainsReflectionReference = new List<MethodDef>();

    }
}