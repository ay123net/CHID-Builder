using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Windows;


namespace CHIDBuilder
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        /// <summary>
        /// Get CHID from chid tool
        /// </summary>
        /// <param name="SystemManufacturer"></param>
        /// <param name="ProductName"></param>
        /// <param name="BiosVendor"></param>
        /// <param name="BiosVersion"></param>
        /// <param name="BiosMajorVersion"></param>
        /// <param name="BiosMinorVersion"></param>
        /// <returns>CHID</returns>
        String GetCHID(
               String SystemManufacturer,
               String ProductName,
               String BiosVendor,
               String BiosVersion,
               String BiosMajorVersion,
               String BiosMinorVersion
         )
        {
            Process p = new Process();                               // use Process to run a extern exe program.
            try
            {
                p.StartInfo.UseShellExecute        = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.CreateNoWindow         = true;           // 不创建执行窗口
                p.StartInfo.FileName               = getCHIDTool();  // 从程序释放出来的CHID tool路径
                p.StartInfo.Arguments = ("/ver " + BiosVersion + 
                                         " /ven " + BiosVendor + 
                                         " /major " + BiosMajorVersion + 
                                         " /minor " + BiosMinorVersion + 
                                         " /mfg " + SystemManufacturer + 
                                         " /product " + ProductName).Trim();

                try
                {
                    p.Start();      // 启动程序
                    // To avoid deadlocks, always read the output stream first and then wait.
                    string output = p.StandardOutput.ReadToEnd();   // 抓取程序输出结果
                    p.WaitForExit();

                    Console.WriteLine(p.StartInfo.Arguments);
                    return output;
                }
                catch (Exception e)
                {
                    BarMessage.Content = e.ToString();
                    return null;
                }
            }
            finally {
                if (p != null)
                    p.Close();
            }
        }
        /// <summary>
        /// 1.取得当前程序路径
        /// 2.获取系统临时文件路
        /// 3.读取当前程序所嵌入的exe文件并将该文件释放到系统临时文件目录
        /// 4.返回所释放的临时文件路径
        /// </summary>
        /// <returns>
        /// 临时文件路径
        /// </returns>
        public String getCHIDTool()
        {
            String s           = System.IO.Path.GetTempPath() + "computerhardwareids.exe";
            String projectName = Assembly.GetExecutingAssembly().GetName().Name.ToString();

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(projectName + ".computerhardwareids.exe"))
            {
                if (stream != null)
                {
                    Byte[] b = new Byte[stream.Length];
                    stream.Read(b, 0, b.Length);
                    
                    if (!File.Exists(s))
                    {
                        using (FileStream f = File.Create(s))
                        {
                            f.Write(b, 0, b.Length);
                        }
                    }
                    return s;
                }
                else
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        BarMessage.Content = "Failed\n" + s + projectName;
                    }));
                    return null;
                }
            }
        }
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnCHIDGenetate_Click(object sender, RoutedEventArgs e)
        {
            String manufacturer = Manufacturer.Text;
            String productName  = ProductName.Text;
            String biosVendor   = BIOSVendor.Text;
            String biosVersion  = BIOSVersion.Text;
            String majorRelease = MajorRelease.Text;
            String minorRelease = MinorRelease.Text;

            if (manufacturer == String.Empty ||
                productName  == String.Empty ||
                biosVendor   == String.Empty ||
                biosVersion  == String.Empty ||
                majorRelease == String.Empty ||
                minorRelease == String.Empty
                ) {
                    BarMessage.Content = "One or more input was empty, generated CHID failed!";
                    return;
            }

            String output = this.GetCHID("\"7YCN22WW\"", "\"LENOVO\"", "\"1\"", "\"22\"", "\"LENOVO\"", "\"81F9\"");

            String output1 = this.GetCHID(
                manufacturer,
                productName,
                biosVendor,
                biosVersion,
                majorRelease,
                minorRelease
                );
            //Console.Write(output);
            if (output1 != null)
            {
                Regex reg = new Regex("\\{[^\\[\\]\n]*\\}");
                MatchCollection mc = reg.Matches(output1);

                this.Dispatcher.Invoke(new Action(() =>
                {
                    CHIDString.Text = mc[1].ToString();
                    BarMessage.Content = "CHID Generate successfully！";
                }));
            }
        }
        private void BtnCHIDCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(CHIDString.Text);
            BarMessage.Content = "Copy to Clickboard successfully！";
        }
    }
}
