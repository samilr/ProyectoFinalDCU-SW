using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Data.OleDb;
using System.Speech.Synthesis;
using System.Media;

namespace Reconocimiento_facial
{
    public partial class Reconocimiento : Form
    {
        SpeechSynthesizer vos = new SpeechSynthesizer();
        Image<Bgr, Byte> currentFrame;
        Capture grabber;
        HaarCascade face;
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.5d, 0.9d);
        Image<Gray, byte> result = null;
        Image<Gray, byte> gray = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labels = new List<string>();
        List<string> labels1 = new List<string>();
        List<string> NamePersons = new List<string>();
        int ContTrain, NumLabels, t;
        string name, names = null, Labelsinfo;


        public Reconocimiento()
        {
            InitializeComponent();
     
            face = new HaarCascade("haarcascade_frontalface_default.xml");
            try
            {
 
                Labelsinfo = File.ReadAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt");
                string[] Labels = Labelsinfo.Split('%');
                NumLabels = Convert.ToInt16(Labels[0]);
                ContTrain = NumLabels;
                string LoadFaces;
                
                    for (int rtx = 1; rtx < NumLabels + 1; rtx++)
                    {
                        LoadFaces = "face" + rtx + ".bmp";
                        trainingImages.Add(new Image<Gray, byte>(Application.StartupPath + "/TrainedFaces/" + LoadFaces));
                        labels.Add(Labels[rtx]);
                    }
            }
            catch (Exception e)
            {
                MessageBox.Show(e + "No hay ningun rosto registrado).", "Cargar rostros", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void Reconocer()
        {
  
            grabber = new Capture();
            grabber.QueryFrame();

            Application.Idle += new EventHandler(FrameGrabber); 
        }

        private void FrameGrabber(object sender, EventArgs e)
        {
            lblNumeroDetect.Text = "0";            
            NamePersons.Add("");
            

            try
            {
                currentFrame = grabber.QueryFrame().Resize(640, 480, INTER.CV_INTER_CUBIC);
            }
            catch (Exception)
            {
                lblNadie.Text = "";
                imageBoxFrameGrabber.Image = null;
            }


            gray = currentFrame.Convert<Gray, Byte>();


            MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(face, 1.2, 10, HAAR_DETECTION_TYPE.FIND_BIGGEST_OBJECT, new Size(20, 20));


            foreach (MCvAvgComp f in facesDetected[0])
            {
                t = t + 1;
                result = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(173, 168, INTER.CV_INTER_CUBIC);

  
                currentFrame.Draw(f.rect, new Bgr(Color.LightGreen), 1);

                if (trainingImages.ToArray().Length != 0)
                {                    

                    MCvTermCriteria termCrit = new MCvTermCriteria(ContTrain, 0.001);
                    

                    EigenObjectRecognizer recognizer = new EigenObjectRecognizer(trainingImages.ToArray(), labels.ToArray(), ref termCrit);
                    var fa = new Image<Gray, byte>[trainingImages.Count]; 
                    
                    name = recognizer.Recognize(result);

                    currentFrame.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.Black));                   
                }

                NamePersons[t - 1] = name;
                              

                lblNumeroDetect.Text = facesDetected[0].Length.ToString();
                lblNadie.Text = name;

            }
            t = 0;


            for (int nnn = 0; nnn < facesDetected[0].Length; nnn++)
            {
                names = names + NamePersons[nnn] +", ";
            }


            imageBoxFrameGrabber.Image = currentFrame;
           
          
            NamePersons.Clear();
        }

        private void imageBoxFrameGrabber_Click(object sender, EventArgs e)
        {

        }

        private void Desconectar()
        {
            Application.Idle -= new EventHandler(FrameGrabber);
            grabber.Dispose();
            imageBoxFrameGrabber.ImageLocation = "img/1.jpg";
            lblNadie.Text = string.Empty;
            lblNumeroDetect.Text = string.Empty;
            btnDesconectar.Text = "Conectar";
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            switch (btnDesconectar.Text)
            {
                case "Conectar":
                    Reconocer();
                    btnDesconectar.Text = "Desconectar";
                    break;
                case "Desconectar":
                    Desconectar();
                    break;
            }
        }

        private void lblNumeroDetect_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            Desconectar();
            Registrar registro = new Registrar();
            registro.Show();
        }

        private void Reconocimiento_Load(object sender, EventArgs e)
        {
            Reconocer();
        }

        private void btnEncender_Click(object sender, EventArgs e)
        {
            Reconocer();
        }

    }
}
