using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JsonPrettyOffline
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string fileName = string.Empty;

        public MainWindow()
        {
            InitializeComponent();

            DataObject.AddPastingHandler(txtOriginal, OnPaste);
        }

        private void btnPretty_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtOriginal.Text)) return;

            txtPretty.Text = GetJsonPretty(txtOriginal.Text);
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            var result = ofd.ShowDialog();

            if (result == false) return;

            fileName = System.IO.Path.GetFileNameWithoutExtension(ofd.FileName);
            
            txtOriginal.Text = System.IO.File.ReadAllText(ofd.FileName);

            txtPretty.Text = "";
        }

        private void btnSaveResult_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog()
            {
                FileName = "Pretty_" + fileName,
                Filter = "Text File(*.txt)|*.txt|All(*.*)|*",
                DefaultExt = ".txt"
            };

            if (sfd.ShowDialog() == true)
            {
                System.IO.File.WriteAllText(sfd.FileName, txtPretty.Text);
            }
        }

        private static string GetJsonPretty(string original)
        {
            #region -- Clear Break Line --
            original = original.Replace("\r\n", "");
            #endregion

            int index = 0;
            bool isContinue = true;

            #region -- Pair --
            Regex regex = new Regex("\"[a-zA-Z0-9]+\":");
            MatchCollection tmp = regex.Matches(original, 0);

            do
            {
                Match result = regex.Match(original, index);

                if (result.Success)
                {
                    int signIndex = original.IndexOf(':', result.Index);

                    // 冒號 & 換行
                    original = original.Insert(signIndex + 1, " ")
                                       .Insert(result.Index, Environment.NewLine);

                    index = result.Index + result.Index + result.Length;
                }
                else
                {
                    isContinue = false;
                }
            } while (isContinue);
            #endregion

            #region -- Break Line at { } ] --
            index = 1;
            isContinue = true;

            do
            {
                int result = original.IndexOfAny(new char[] { '{', '}', ']' }, index);

                if (result > 0)
                {
                    original = original.Insert(result, Environment.NewLine);

                    index += result + 3;
                }
                else
                {
                    isContinue = !isContinue;
                }
            } while (isContinue);
            #endregion

            #region -- Add Spaces --
            List<int> tabArr = new List<int>();

            string[] linesOriginal = original.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            index = 0;
            int tabFlag = 0;
            bool inPhrase = false;
            bool endPhrase = false;
            int endIndex = linesOriginal.Length - 1;

            foreach (string line in linesOriginal)
            {
                if (line[0] == '{')
                {
                    tabFlag += 1;
                    inPhrase = true;
                }
                else if (line[0] == '}')
                {
                    tabFlag = 1;
                    inPhrase = false;
                    endPhrase = true;
                }

                tabArr.Add(tabFlag);

                if (inPhrase)
                {
                    tabFlag += 1;
                    inPhrase = false;
                }
                if (endPhrase)
                {
                    tabFlag -= 1;
                    endPhrase = false;
                }

                ++index;
            }

            if (linesOriginal[0][0] == '{')
            {
                tabArr = (from item in tabArr
                          select item - 1).ToList();
            }

            int tabMax = tabArr.Max() + 1;
            Dictionary<int, string> tabSpaceMap = new Dictionary<int, string>();
            for (int i = 0; i < tabMax; ++i)
            {
                tabSpaceMap.Add(i, GetTabSpace(i));
            }

            index = 0;
            foreach (string line in linesOriginal)
            {
                linesOriginal[index] = tabSpaceMap[tabArr[index]] + linesOriginal[index];

                ++index;
            }

            original = string.Join(Environment.NewLine, linesOriginal);
            #endregion

            return "";
        }

        private static string GetTabSpace(int tabTimes)
        {
            string result = string.Empty;

            for (int i = 0; i < tabTimes; ++i)
            {
                result += "  ";
            }

            return result;
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true)) return;

            string text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;

            txtOriginal.Text = text;
            txtPretty.Text = text;
        }
    }
}
