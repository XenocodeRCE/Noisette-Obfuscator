using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoisetteCore.Protection.ConstantDecomposition.Decompositor
{
    internal class DecompositorProcess
    {
        public static string[] Decompositor = new string[] { "add", "sub", "mul", "div", "pow", "xor", };

        public static string[] test_list = new string[] { };

        public DecompositorProcess()
        {
            List<string> list = new List<string>();
            list.Add("1");
            list.Add("2");
            test_list = list.ToArray();
        }

        public static void test()
        {
            var test = test_list.Contains<string>("2") ? 2 : 3;
            var doub = test;
        }
    }
}