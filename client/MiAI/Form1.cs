using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace MiAI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // Ham goi HTTP Get len server
        public string sendGet(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        // Ham chuyen Image thanh Base 64
        public static string ConvertImageToBase64String(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
               
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        // Ham convert B64 de gui len server
        private String EscapeData(String B64)
        {
            int B64_length = B64.Length;
            if (B64_length <= 32000)
            {
                return Uri.EscapeDataString(B64);
            }


            int idx = 0;
            StringBuilder builder = new StringBuilder();
            String substr = B64.Substring(idx, 32000);
            while (idx < B64_length)
            {
                builder.Append(Uri.EscapeDataString(substr));
                idx += 32000;

                if (idx < B64_length)
                {

                    substr = B64.Substring(idx, Math.Min(32000, B64_length - idx));
                }

            }
            return builder.ToString();

        }

        // Ham goi HTTP POST len server de detect
        private String sendPOST(String url, String B64)
        {
            try
            {

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = 5000;
                var postData = "image=" + EscapeData(B64);

                var data = Encoding.ASCII.GetBytes(postData);

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                return responseString;
            }
            catch (Exception ex)
            {
                return "Exception" + ex.ToString();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Doc du lieu cac ten class tu file yolov3.txt 
            string[] lines = File.ReadAllLines("yolov3.txt");

            // Convert image to B64
            String B64 = ConvertImageToBase64String(pictureBox1.Image);
           
            // Goi len server va tra ve ket qua
            String server_ip = "192.168.8.123";
            String server_path = "http://" + server_ip + ":8000/detect";
            String retStr = sendPOST(server_path, B64);

           
            // Ve cac khung chu nhat va ten class len anh 
            Graphics newGraphics = Graphics.FromImage(pictureBox1.Image);

            String[] items = retStr.Split('|');
            for (int idx=0;idx<items.Length-1;idx++)
            {
                String[] val = items[idx].Split(',');
                // Draw it
                Pen blackPen = new Pen(Color.Black, 2);

                // Create rectangle.
                Rectangle rect = new Rectangle(int.Parse(val[1]), int.Parse(val[2]), int.Parse(val[3]), int.Parse(val[4]));

                // Draw rectangle to screen.
                newGraphics.DrawRectangle(blackPen, rect);
                newGraphics.DrawString(lines[int.Parse(val[0])], new Font("Tahoma", 8), Brushes.Black, rect);

            }

            pictureBox1.Refresh();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Resize anh cua picture box 1 de dam bao dung scale
            Image image = (Image)(new Bitmap(pictureBox1.Image, new Size(pictureBox1.Width, pictureBox1.Height)));
            pictureBox1.Image = image;
        }
    }
}
