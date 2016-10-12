using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RadioRecorder
{
    /// <summary>
    /// Log.xaml 的交互逻辑
    /// </summary>
    public partial class Log : Window
    {
        public Log()
        {
            InitializeComponent();
        }

        public Log(FlowDocument log)
        {
            InitializeComponent();
            this.doc = log;
            RTB_Log.Document = doc;            
        }

        private FlowDocument doc;
        private void B_保存_Click(object sender, RoutedEventArgs e)
        {
            var text = new TextRange(doc.ContentStart, doc.ContentEnd);
            var sfd = new SaveFileDialog();
            sfd.AddExtension = true;
            sfd.DefaultExt = ".txt";
            sfd.FileName = "log.txt";
            if ((bool)sfd.ShowDialog())
            {
                var fs = new FileStream(sfd.FileName, FileMode.Create);
                text.Save(fs, DataFormats.Text);
                fs.Close();
            }
        }

        private void B_Clear_Click(object sender, RoutedEventArgs e)
        {
            (doc.Blocks.First() as Paragraph).Inlines.Clear();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }
    }
}
