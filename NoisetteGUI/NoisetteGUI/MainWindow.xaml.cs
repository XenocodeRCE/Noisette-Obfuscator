using dnlib.DotNet;
using Noisette;
using System;
using System.Windows;
using System.Windows.Input;

namespace NoisetteGUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ExitButton_MouseEnter(object sender, MouseButtonEventArgs e)
        {
            Environment.Exit(0);
        }

        private void MinimizeButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void MaximizeButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void DragTheForm(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void textBox_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null || files.Length == 0) return;
            int error = 0;
            string errormsg = null;
            try
            {
                var filename = files[0];
                Noisette.Obfuscation.ObfuscationProcess Obf =
                    new Noisette.Obfuscation.ObfuscationProcess(ModuleDefMD.Load(filename));
                Obf.DoObfusction();
            }
            catch (Exception ex)
            {
                //something went wrong
                error = 1;
                errormsg = ex.ToString();
            }
            if (error != 0)
            {
                EndWindow end = new NoisetteGUI.EndWindow
                {
                    NoError_btn = { Visibility = Visibility.Hidden },
                    NoError_txt = { Visibility = Visibility.Hidden },
                    Error_btn = { Visibility = Visibility.Visible },
                    Error_txt = { Visibility = Visibility.Visible }
                };
                end.LogTXT.Document.Blocks.Clear();
                end.LogTXT.AppendText(errormsg);
                end.Show();
                this.Hide();
            }
            else
            {
                EndWindow end = new NoisetteGUI.EndWindow();
                end.LogTXT.Document.Blocks.Clear();
                end.LogTXT.AppendText("All is ok apparently... :)");
                end.Show();
                this.Hide();
            }
        }

        public void OnDragOver(object sender, DragEventArgs e)

        {
            e.Effects = DragDropEffects.All;

            e.Handled = true;
        }
    }
}