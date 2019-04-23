using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tao.FreeGlut;
using OpenGL;
using System.IO;


namespace wykres3D
{
    public partial class Form1 : Form
    {
        Double[,] wartosci;
        double min;
        double max;
        double skala;

        private static int width = 1024;
        private static int height = 720;
        private static ShaderProgram program;
        private static VBO<Vector3> square;
        private static VBO<Vector3> squareColor;
        private static VBO<int> squareElements;
        public Form1()
        {
            InitializeComponent();
            wartosci = new Double[256, 256];
            min = 0;
            max = 0;

        }
        public double getValue(int x, int y)
        {
            return wartosci[x, y];
        }
        private void buttonLoad_Click(object sender, EventArgs e)
        {
            otworzPlik();
        }
        public void otworzPlik()
        {
            OpenFileDialog o = new OpenFileDialog();
            if (o.ShowDialog() == DialogResult.OK)
            {
                FileStream stream = new FileStream(o.FileName, FileMode.Open, FileAccess.Read);
                StreamReader streamReader = new StreamReader(stream);
                while (!streamReader.EndOfStream)
                {
                    if (streamReader.ReadLine() == "[Dane]")
                    {
                        for (int x = 0; x <= 255; ++x)
                        {
                            string tymczasowy = streamReader.ReadLine();
                            tymczasowy = tymczasowy.Replace('.', ',');
                            char rozdzielacz = ';';
                            string[] rozdzielone = tymczasowy.Split(rozdzielacz);
                            for (int y = 0; y <= 255; ++y)
                            {
                                if (rozdzielone[0] != "")
                                {
                                    wartosci[x, y] = double.Parse(rozdzielone[y]);
                                    sprawdzMinMax(x, y, double.Parse(rozdzielone[y]));
                                }
                            }
                        }
                    }
                }
                streamReader.Close();
            }
            skaluj();
            rysuj();
        }
        public void skaluj()
        {
            double przedzialLiczbowy = max - min;
            double przedzialLiczbowyDocelowy = 20;
            skala = przedzialLiczbowyDocelowy / przedzialLiczbowy;
            for (int x = 0; x <= 255; ++x)
            {
                for (int y = 0; y <= 255; ++y)
                {
                    wartosci[x, y] = (wartosci[x, y]-min) * skala;
                }
            }
        }
        public void sprawdzMinMax(int x, int y, double wartosc)
        {
            if (wartosc < min)
            {
                min = wartosci[x, y];
            }
            if (wartosc > max)
            {
                max = wartosci[x, y];
            }
        }            

        void rysuj()//string[] args)
        {
            // create an OpenGL window
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(width, height);
            Glut.glutCreateWindow("Projekt 3D");

            Glut.glutIdleFunc(OnRenderFrame);
            Glut.glutDisplayFunc(OnDisplay);

            // compile the shader program
            program = new ShaderProgram(VertexShader, FragmentShader);

            // set the view and projection matrix, which are static throughout this tutorial
            program.Use();
            program["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(0.45f, (float)width / height, 0.1f, 10000f));
            program["view_matrix"].SetValue(Matrix4.LookAt(new Vector3(0, 0, 525), Vector3.Zero, new Vector3(0, 1, 0)));

            for (int x = 0; x <= 254; ++x)
            {
                for (int y = 0; y <= 254; ++y)
                {

                }
            }

            Glut.glutMainLoop();
            
        }
        private void OnDisplay()
        {
            //square.Dispose();
            //squareColor.Dispose();
            //squareElements.Dispose();
            //program.DisposeChildren = true;
            //program.Dispose();

            //GC.Collect();
        }

        private void OnRenderFrame()
        {

            // set up the OpenGL viewport and clear both the color and depth bits
            Gl.Viewport(0, 0, width, height);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            // use our shader program
            Gl.UseProgram(program);

            for (int x = 0; x <= 254; ++x)
            {
                for (int y = 0; y <= 254; ++y)
                {
                    // create a square
                    square = new VBO<Vector3>(new Vector3[] { new Vector3(0, 0, wartosci[x, y]), new Vector3(1, 0, wartosci[x + 1, y]), new Vector3(1, 1, wartosci[x + 1, y + 1]), new Vector3(0, 1, wartosci[x, y + 1]) });
                    squareColor = new VBO<Vector3>(new Vector3[] { new Vector3(wartosci[x, y] / skala, 0, 0), new Vector3(wartosci[x + 1, y] / skala, 0, 0), new Vector3(wartosci[x + 1, y + 1] / skala, 0, 0), new Vector3(wartosci[x, y + 1] / skala, 0, 0) });
                    squareElements = new VBO<int>(new int[] { 0, 1, 2, 3 }, BufferTarget.ElementArrayBuffer);
                    //square                    
                    program["model_matrix"].SetValue(Matrix4.CreateTranslation(new Vector3(-128, -128, 0)) * Matrix4.CreateTranslation(new Vector3(x, y, 0)) * Matrix4.CreateRotationX(-(float)Math.PI / 3)); //transform the square(into position)           
                    Gl.BindBufferToShaderAttribute(square, program, "vertexPosition"); // square bind the vertex attribute arrays for the square (the easy way)
                    Gl.BindBufferToShaderAttribute(squareColor, program, "vertexColor");
                    Gl.BindBuffer(squareElements);
                    Gl.DrawElements(BeginMode.Quads, squareElements.Count, DrawElementsType.UnsignedInt, IntPtr.Zero); // draw the square
                    square.Dispose();
                    squareColor.Dispose();
                    squareElements.Dispose();
                    //program.DisposeChildren = true;
                    //program.Dispose();
                }
            }

            /*
            for (int x = 0; x <= 254; ++x)
            {
                for (int y = 0; y <= 254; ++y)
                {

                }
            }
            */

            Glut.glutSwapBuffers();
        }
        //shader of models, camera and positions
        public static string VertexShader = @"
            #version 130

            in vec3 vertexPosition;
            in vec3 vertexColor;

            out vec3 color;

            uniform mat4 projection_matrix;
            uniform mat4 view_matrix;
            uniform mat4 model_matrix;

            void main(void)
            {
                color = vertexColor;
               gl_Position = projection_matrix * view_matrix * model_matrix * vec4(vertexPosition, 1);
            }
            ";
                   //this is drawing shader
        public static string FragmentShader = @"
            #version 130
    
            in vec3 color;

            out vec4 fragment;

            void main(void)
            {
                fragment = vec4(color, 1);
            }
            ";

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}

