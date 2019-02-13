using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Blob;
using System.IO;

namespace ImageProcessing4
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //ロードイベントでバージョンをタイトル横に表示
            this.Text = this.Text + "     Ver  " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        }

        //終了ボタン
        private void button3_Click(object sender, EventArgs e)
        {
            //確認メッセージ
            if (MessageBox.Show("終了します。", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                //終了ボタン
                this.Close();
            }
        }

        //実行ボタン
        private void button2_Click(object sender, EventArgs e)
        {
            //確認メッセージ
            if (MessageBox.Show("解析を実行します。", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            //入力チェック
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("解析する画像ファイルを指定してや！");
                return;
            }
            //テキストボックスからファイル名を取得
            string FileName = textBox1.Text;

            //入力チェック
            if (string.IsNullOrWhiteSpace(textBox4.Text))
            {
                MessageBox.Show("出力先を指定してや！");
                return;
            }
            //テキストボックスからパスを取得
            string FolderPath = textBox4.Text;


            //DataGridView初期化（データクリア）
            //dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();

            //グレースケールで画像を読み込み
            Mat src = new Mat(FileName, ImreadModes.GrayScale);


            //二値化
            Mat bin = src.Clone();
            //Cv2.Threshold(src, bin,0,255,ThresholdTypes.Otsu);

            //事前チェック「
            string binTH_check = textBox2.Text;
            int i = 0;
            bool result = int.TryParse(binTH_check, out i);
            if(result == true)
            {
                int binTH = Int32.Parse(textBox2.Text);
                Cv2.Threshold(src, bin, binTH, 255, ThresholdTypes.Binary);

            }
            else
            {
                MessageBox.Show("適切な二値化閾値を入力してや！", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }




            //ラベリング
            Mat labelView = bin.EmptyClone(); //ラベリング画像用Matを用意
            Mat rectView = bin.CvtColor(ColorConversionCodes.GRAY2BGR);　//外接矩形用Matを用意

            ConnectedComponents cc = Cv2.ConnectedComponentsEx(bin);
            if(cc.LabelCount <= 1)
                return;




            //ラベリング後のブロブ個数を出力する
            //背景を除くため-1
            int blobNum = cc.LabelCount -1;
            textBox3.Text = textBox3.Text + "抽出したブロブ個数 = " + blobNum.ToString() + "\r\n";


            //ラベリング結果の描画 draw labels
            cc.RenderBlobs(labelView);

            //ラベリング画像に文字列を書き込む用の画像を用意
            Mat labelView2 = labelView.Clone();

            
            foreach (var blob in cc.Blobs.Skip(1))
            {
                //draw bonding boxes except background
                rectView.Rectangle(blob.Rect, Scalar.Red);

                //特徴量を取得する
                int blobnumber = blob.Label; //番号
                int blobarea = blob.Area; //面積
                int blobwidth = blob.Width; //幅
                int blobheight = blob.Height; //高さ
                double centroid_x = blob.Centroid.X; //重心X座標
                int centroid_x_int = (int)centroid_x; //整数にキャスト
                double centroid_y = blob.Centroid.Y; //重心Y座標
                int centroid_y_int = (int)centroid_y; //整数にキャスト


                //http://upa-pc.blogspot.com/2014/04/cappendtextselectedtext.html
                textBox3.Text = textBox3.Text + "No. = " + blobnumber.ToString() + "," +
                    " 面積 = " + blobarea.ToString() + 
                    ", 幅 = " + blobwidth.ToString() +
                    ", 高さ = " + blobheight.ToString() +
                    ", 重心X = " + centroid_x_int.ToString() +
                    ",重心Y = " + centroid_y_int.ToString() +
                    "\r\n";

                // DataGridView にデータを追加 
                this.dataGridView1.Rows.Add(
                    blobnumber.ToString(),
                    blobarea.ToString(),
                    blobheight.ToString(),
                    blobwidth.ToString(),
                    centroid_x_int.ToString(),
                    centroid_y_int.ToString());
                
                //ラベリング画像に文字列を追加
                //blobnumberを描画する
                string text = blobnumber.ToString(); //描画する文字列を指定
                Cv2.PutText(labelView2, text, new OpenCvSharp.Point(centroid_x_int, centroid_y_int), HersheyFonts.HersheyComplexSmall, 1, new Scalar(0, 0, 255), 1, LineTypes.AntiAlias);

            }

            //Contour処理・・・なかなかうまくいかない
            //https://searchcode.com/codesearch/view/88850698/
            /*
            CvSeq<CvPoint> contours;
            CvMemStorage storage = new CvMemStorage();
            // native style
            Cv.FindContours(img, storage, out contours, CvContour.SizeOf, ContourRetrieval.Tree, ContourChain.ApproxSimple);
            contours = Cv2.ApproxPolyDP(contours, CvContour.SizeOf, storage, ApproxPolyMethod.DP, 3, true);
            */

            //なんか画像に表示されるようにはなったけど、輪郭ではないな・・・
            /*
            Mat bin2 = bin.EmptyClone(); //
            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchyIndexes;
            Cv2.FindContours(
                bin,
                out contours,
                out hierarchyIndexes,
                mode: RetrievalModes.External,
                method: ContourApproximationModes.ApproxNone);
            Cv2.DrawContours(bin2, contours, -1, new Scalar(0, 0, 255));
            Cv2.ImShow("test", bin2);
            */


            //画像に文字列を書いてみる
            //Mat labelView2 = labelView.Clone();
            //Cv2.PutText(labelView2, "Moon", new OpenCvSharp.Point(64,64), HersheyFonts.HersheyComplexSmall, 1, new Scalar(255, 0, 255), 1, LineTypes.AntiAlias);

            //参考URL
            //http://tryoutartprogramming.blogspot.com/2017/02/opencv.html
            //http://imagingsolution.blog.fc2.com/blog-entry-202.html
            //https://github.com/VahidN/OpenCVSharp-Samples/blob/master/OpenCVSharpSample18/SimpleOCR.cs
            //https://github.com/VahidN/OpenCVSharp-Samples/blob/master/OpenCVSharpSample12/Program.cs

            //最大ブロブを抽出
            var maxBlob = cc.GetLargestBlob();
            var filtered = new Mat();
            cc.FilterByBlob(bin, filtered, maxBlob);
 
            //int width = maxBlob.Width;
            //int height = maxBlob.Height;

            //画像保存
            Cv2.ImWrite(FolderPath + "/二値化処理画像.jpg", bin); //二値化画像
            Cv2.ImWrite(FolderPath + "/ラベリング画像.jpg", labelView);　//ラベリング画像
            Cv2.ImWrite(FolderPath + "/No.付きラベリング画像.jpg", labelView2);　//ラベリング画像
            Cv2.ImWrite(FolderPath + "/外接矩形画像.jpg", rectView);
            Cv2.ImWrite(FolderPath + "/最大面積ブロブ画像.jpg", filtered);

            //Bitmap canvas1 = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Bitmap canvas2 = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            Bitmap canvas3 = new Bitmap(pictureBox3.Width, pictureBox3.Height);

            //Graphics g1 = Graphics.FromImage(canvas1);
            Graphics g2 = Graphics.FromImage(canvas2);
            Graphics g3 = Graphics.FromImage(canvas3);

            //g1.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g2.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g3.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

            //Bitmap bitimg1 = MatToBitmap(src);
            Bitmap bitimg2 = MatToBitmap(bin);
            Bitmap bitimg3 = MatToBitmap(labelView2);

            //g1.DrawImage(bitimg1, 0, 0, 300, 300);
            g2.DrawImage(bitimg2, 0, 0, 300, 300);
            g3.DrawImage(bitimg3, 0, 0, 300, 300);

            //bitimg1.Dispose();
            bitimg2.Dispose();
            bitimg3.Dispose();

            //pictureBox1.Image = canvas1;
            pictureBox2.Image = canvas2;
            pictureBox3.Image = canvas3;

            //処理が終わったら、メッセージを表示する
            MessageBox.Show("終わりましたよ～。", "", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        //参照ボタン
        private void button1_Click(object sender, EventArgs e)
        {
            //オープンファイルダイアログを生成する
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "ファイルを開く";
            dlg.InitialDirectory = @"C:\";
            //dlg.FileName = @"hoge.txt";
            dlg.Filter = "画像ファイル(*.jpg;*.jpeg;*png;*bmp)|*.jpg;*.jpeg;*png;*bmp|すべてのファイル(*.*)|*.*";
            dlg.FilterIndex = 1;

            //オープンファイルダイアログを表示する
            DialogResult result = dlg.ShowDialog();

            if (result == DialogResult.OK)
            {
                //「開く」ボタンが選択された時の処理
                //string fileName = dlg.FileName;  //こんな感じで選択されたファイルのパスが取得できる
                textBox1.Text = dlg.FileName;

                //先に画像表示する
                Mat src = new Mat(dlg.FileName, ImreadModes.GrayScale);
                Bitmap canvas1 = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                Graphics g1 = Graphics.FromImage(canvas1);
                g1.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                Bitmap bitimg1 = MatToBitmap(src);
                g1.DrawImage(bitimg1, 0, 0, 300, 300);
                bitimg1.Dispose();
                pictureBox1.Image = canvas1;

            }
            else if (result == DialogResult.Cancel)
            {
                //「キャンセル」ボタンまたは「×」ボタンが選択された時の処理
            }


            //フォルダダイアログのメソッドを使用する場合
            //ShowFolderDialog();

        }

        //MatをBitmapに変換するメソッド
        public static Bitmap MatToBitmap(Mat image)
        {
            return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image);
        }

        //フォルダダイアログのメソッド
        private void ShowFolderDialog()
        {
            //参考
            //https://diannao.work/cs%E3%83%80%E3%82%A4%E3%82%A2%E3%83%AD%E3%82%B0%E3%81%A7%E3%83%95%E3%82%A9%E3%83%AB%E3%83%80%E3%82%92%E9%81%B8%E6%8A%9E%E3%81%99%E3%82%8B/

            // FolderBrowserDialogクラスのインスタンス生成
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            // ダイアログタイトルを設定
            dlg.Description = "フォルダを選択してください";
            // ルートフォルダの設定（RootFolderに何も指定しなければデスクトップがルートになる）
            dlg.RootFolder = Environment.SpecialFolder.Desktop;
            // 初期選択されているフォルダの設定（今回は、ローカルディスクのc:\を指定している）
            // 初期選択フォルダは、RootFolder以下にあるフォルダである必要がある
            dlg.SelectedPath = @"c:\";
            // ユーザーが新しいフォルダを作成できるようにする設定（デフォルトでtrue）
            dlg.ShowNewFolderButton = true;

            //ダイアログを表示する
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                /*
                // 選択したフォルダ名を取得
                string myPath = dlg.SelectedPath;
                //選択されたフォルダを表示する
                MessageBox.Show(myPath + "が選択されました", "選択結果", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Console.WriteLine(myPath);
                */
                textBox1.Text = dlg.SelectedPath;
            }
        }

        //処理結果出力参照ボタン
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                var dialog = new FolderBrowserDialog();
                //var dialog = new CommonOpenFileDialog();

                //// フォルダー設定
                //dialog.IsFolderPicker = true;
                //// 読み取り専用フォルダ/コントロールパネルは開かない
                //dialog.EnsureReadOnly = false;
                //dialog.AllowNonFileSystemItems = false;
                //// パス指定
                //dialog.DefaultDirectory = Environment.SpecialFolder.Personal.ToString();
                // 開く
                var result = dialog.ShowDialog();
                // もし開かれているなら
                if (result == DialogResult.OK)
                {
                    textBox4.Text = dialog.SelectedPath;

                }

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
