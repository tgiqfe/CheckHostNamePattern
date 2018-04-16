using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace CheckHostNamePattern
{
    class NamePattern
    {
        /*  シリアルマッチのルール
         *  ~の右辺の桁数を、左辺の末尾から削ってから連番で検索
         *  ~の右辺のマッチングは \d+[a-zA-Z]*$
         *  ~の右辺が↑のパターン以外の場合は不正パターンとして、取り扱わない
         *  <マッチング例>
         *      Host005~010         〇 Host005～Host010
         *      Host020a~030a       〇 Host020a～Host030a
         *      Host03b05~10b10     × 右辺が不正
         *      HostHost~HosU       × 右辺が不正
         *      Host001b~005c       △ Host001c～Host005c 左辺の末尾「b」は無視
         *      HostAAAA~050t       △ Host000t～Host050t 左辺の末尾「AAAA」無視。左辺末尾に数字が見つからない為、0からスタート
         *      Host0001a~030       △ Host00000～Host00030 SufName無しの為、左辺末尾を削る際、「a」を無視。「01a」を数字として扱えない為、0からスタート
         *      Host0015a~20bb      △ Host001bb～Host020bb 左辺末尾の「5a」を無視し、01からのスタート
         *                          ※正直言って、△のパターンはやめてほしい。。。
         *                          ※チェック元のマシンのホスト名とフォルダー名のマッチングを想定しているので、ホスト名やフォルダー名に使えない文字はそもそもNG
         *                          　DNSのホスト名部分、NetBIOS名、フォルダー名の全部の禁止文字を禁止した後で、「%」と「~」を許可と、大文字/小文字許可する、という考え方。
         *                          　使用可能文字を羅列すると、a～z A～Z 0～9 - ~ %
         *
         *  各部の名称
         *  Aaaa001b~099b
         *  ↓
         *  Aaaa    | 001    | b       | ~    | 099    | b
         *  ~~~~    | ~~~    | ~       |      | ~~~    | ~
         *  PreName | MinNum | SufName | 無視 | MaxNum | SufName
         *                     ↑正確にはここは無視
         */

        /*  ワイルドカードマッチのルール
         *  名前の中の「%」の部分をワイルドカードとして判断。「*」がフォルダー名に使用できないので。
         *  シリアルマッチとワイルドカードマッチは同時使用不可
         */

        //  クラスパラメータ
        private bool Available { get; set; }
        private bool IsSerialMatch { get; set; }
        private string PreName { get; set; }
        private string SufName { get; set; }
        private int Digit { get; set; }
        private int MaxNum { get; set; }
        private int MinNum { get; set; }
        private string WildCardName { get; set; }

        //  コンストラクタ
        public NamePattern() { }
        public NamePattern(string sourceText)
        {
            SetNamePattern(sourceText);
        }

        //  名前パターンをセット
        public void SetNamePattern(string sourceText)
        {
            if (Regex.IsMatch(sourceText, @"^[a-zA-Z0-9]+~\d+[a-zA-Z]*$"))
            {
                string leftSide = sourceText.Substring(0, sourceText.IndexOf("~"));
                string rightSide = sourceText.Substring(sourceText.IndexOf("~") + 1);
                string tempMaxNum = Regex.Match(rightSide, @"^\d+").Value;
                this.Digit = tempMaxNum.Length;
                this.SufName = Regex.Match(rightSide, @"\D+$").Value;
                this.PreName = leftSide.Substring(0, leftSide.Length - Digit - SufName.Length);
                this.MaxNum = int.Parse(tempMaxNum);
                this.MinNum =
                    int.TryParse(leftSide.Substring(PreName.Length, Digit), out int tempInt) ? tempInt : 0;
                this.IsSerialMatch = true;
                this.Available = true;
            }
            else if (Regex.IsMatch(sourceText, @"^[a-zA-Z0-9\-]*%[a-zA-Z0-9\-%]*$"))
            {
                this.WildCardName = sourceText.Replace("%", ".*");
                this.IsSerialMatch = false;
                this.Available = true;
            }
        }

        //  名前一致確認
        public bool CheckName(string targetName)
        {
            if (Available)
            {
                if (IsSerialMatch)
                {
                    if (targetName.StartsWith(PreName, StringComparison.OrdinalIgnoreCase) &&
                        targetName.EndsWith(SufName, StringComparison.OrdinalIgnoreCase))
                    {
                        for (int i = MinNum; i <= MaxNum; i++)
                        {
                            string tempName = string.Format("{0}{1:D" + Digit.ToString() + "}{2}", PreName, i, SufName);
                            if (targetName.Equals(tempName, StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    if (Regex.IsMatch(targetName, WildCardName, RegexOptions.IgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
