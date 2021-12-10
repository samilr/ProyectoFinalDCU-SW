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

namespace Reconocimiento_facial
{
    public partial class Registrar : Form
    {
        
        Image<Bgr, Byte> currentFrame;
        Capture grabber;         
        HaarCascade face;
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.5d, 0.5d);
        Image<Gray, byte> result, TrainedFace = null;
        Image<Gray, byte> gray = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labels = new List<string>();
        List<string> labels1 = new List<string>();
        List<string> NamePersons = new List<string>();
        int ContTrain, NumLabels, t;
        string name;
        
        public Registrar()
        {
            InitializeComponent();

            face = new HaarCascade("haarcascade_frontalface_default.xml");
            try
            {
 
                string Labelsinfo = File.ReadAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt");
                string[] Labels = Labelsinfo.Split('%'); 
                NumLabels = Convert.ToInt32(Labels[0]);
                ContTrain = NumLabels;
                string LoadFaces;

                for (int tf = 1; tf < NumLabels + 1; tf++)
                {
                    LoadFaces = "face" + tf + ".bmp";
                    trainingImages.Add(new Image<Gray, byte>(Application.StartupPath + "/TrainedFaces/" + LoadFaces));
                    labels.Add(Labels[tf]);
                }               
            }
            catch (Exception e)
            {
                MessageBox.Show(e + " No hay ninguna cara en una Base de Datos, por favor añadir por lo menos una cara", "Cragar caras en tu Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

       
        void FrameGrabber(object sender, EventArgs e)
        {
            lblNumeroDetect.Text = "0";
            NamePersons.Add("");

      
            currentFrame = grabber.QueryFrame().Resize(640, 480, INTER.CV_INTER_NN);

            
            gray = currentFrame.Convert<Gray, Byte>();

           
            MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(face, 1.2, 10, HAAR_DETECTION_TYPE.FIND_BIGGEST_OBJECT, new Size(90, 90));

       
            foreach (MCvAvgComp f in facesDetected[0])
            {
                t = t + 1;
                result = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(173, 168, INTER.CV_INTER_CUBIC);

                
                currentFrame.Draw(f.rect, new Bgr(Color.YellowGreen), 1);
                NamePersons[t - 1] = name;
                NamePersons.Add("");

             
                lblNumeroDetect.Text = facesDetected[0].Length.ToString();
            }
            t = 0;
            
            
            imageBoxFrameGrabber.Image = currentFrame;          
            NamePersons.Clear();
        }

        private void btn_detectar_Click(object sender, EventArgs e)
        {

            grabber = new Capture();
            grabber.QueryFrame();


            Application.Idle += new EventHandler(FrameGrabber);
            this.btnDesconectar.Enabled = true;
            btn_detectar.Enabled = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Idle -= new EventHandler(FrameGrabber);
            grabber.Dispose();
            this.Close();
        }

        private void btn_agregar_Click(object sender, EventArgs e)
        {
            try
            {

                ContTrain = ContTrain + 1;

                gray = grabber.QueryGrayFrame().Resize(640, 480, INTER.CV_INTER_CUBIC);


                MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(face, 8.0, 10, HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(90, 90));


                foreach (MCvAvgComp f in facesDetected[0])
                {
                    TrainedFace = currentFrame.Copy(f.rect).Convert<Gray, byte>();
                    break;
                }

                TrainedFace = result.Resize(173, 168, INTER.CV_INTER_CUBIC);

                trainingImages.Add(TrainedFace);
                labels.Add(txt_nombre.Text);


                imbImagenGuradado.Image = TrainedFace;


                File.WriteAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt", trainingImages.ToArray().Length.ToString() + "%");


                for (int i = 1; i < trainingImages.ToArray().Length + 1; i++)
                {
                    trainingImages.ToArray()[i - 1].Save(Application.StartupPath + "/TrainedFaces/face" + i + ".bmp");
                    File.AppendAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt", labels.ToArray()[i - 1] + "%");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "Primero debe Detectar un Rostro");
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void lblNumeroDetect_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            imbImagenGuradado.Image=null;
            this.txt_codigo.Clear();
            this.txt_nombre.Clear();
        }

        private void Resgistrar_Load(object sender, EventArgs e)
        {
            imageBoxFrameGrabber.ImageLocation = "img/1.jpg";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Application.Idle -= new EventHandler(FrameGrabber);
                grabber.Dispose();
                imageBoxFrameGrabber.ImageLocation = "img/1.jpg";
                btn_detectar.Enabled = true;
                btnDesconectar.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

       
    }
}
