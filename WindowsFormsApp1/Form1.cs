using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1: Form
    {
        private MatrizImpedancias matriz;
        MatrizS[] S;
        private int numeroBarras;
        private int numeroLineas;

        public Form1()
        {
            InitializeComponent();
            dgvImpedancias.AutoGenerateColumns = false;
            dgvYbarra.AutoGenerateColumns = false;
            btnCalcular.Enabled = false; // Deshabilitar hasta que se ingresen las impedancias
        }

        // Evento del botón para calcular Ybarra
        private void btnCalcular_Click(object sender, EventArgs e)
        {
            try
            {
                // Leer las impedancias del DataGridView
                matriz.Limpiar();
                foreach (DataGridViewRow row in dgvImpedancias.Rows)
                {
                    int i = int.Parse(row.Cells["BarraI"].Value.ToString()) - 1;
                    int j = int.Parse(row.Cells["BarraJ"].Value.ToString()) - 1;
                    double real = double.Parse(row.Cells["Real"].Value.ToString());
                    double imaginaria = double.Parse(row.Cells["Imaginaria"].Value.ToString());

                    Complex impedancia = new Complex(real, imaginaria);
                    if (i == -1 || j == -1)
                    {
                        // Si BarraI o BarraJ es 0, se trata de un objeto externo
                        int barra = i == -1 ? j : i;
                        matriz.AddImpedanciaExterna(barra, impedancia);
                    }
                    else
                    {
                        matriz.SetImpedancia(i, j, impedancia);
                    }
                }

                // Calcular Ybarra
                matriz.CalcularYbarra();

                // Mostrar la matriz Ybarra
                MostrarYbarra(matriz.GetYbarra());
                //Mostrar ka natriz Ybarra en forma polar
                MostrarYbarraPolar(matriz.GetYbarra());
            }
            catch (Exception ex)
            {
                //lblMensaje.Text = "Error: " + ex.Message;
            }
        }

        //Manejar evento cuando da click en el boton confirmar lineas
        private void btnConfirmarLineas_Click(object sender, EventArgs e)
        {
            try
            {

                numeroBarras = int.Parse(txtNumeroBarras.Text);
                if (numeroBarras <= 0)
                {
                    throw new ArgumentException("El número de barras debe ser mayor que 0.");
                }

                numeroLineas = int.Parse(txtNumeroLineas.Text);
                if (numeroLineas <= 0 && numeroLineas>=(numeroBarras-1))
                {
                    throw new ArgumentException("El número de lineas debe ser mayor que 0 y mayor que el numero de barras -1");
                }
                // Inicializar la matriz
                matriz = new MatrizImpedancias(numeroBarras, numeroLineas);
                // Configurar el DataGridView para las impedancias
                ConfigurarDataGridImpedancias();
                // Configurar el DataGridView para los tipos de barras
                ConfigurarDataGridTiposBarras();
                btnCalcular.Enabled = true;
                //lblMensaje.Text = "Ingrese las impedancias en forma a+bj (parte real y parte imaginaria).";
            }
            catch (Exception ex)
            {
                //lblMensaje.Text = "Error: " + ex.Message;
            }
        }

        // Configurar el DataGridView para ingresar impedancias
        private void ConfigurarDataGridImpedancias()
        {
            dgvImpedancias.Columns.Clear();
            dgvImpedancias.Rows.Clear();

            // Agregar columnas para barra i, barra j, parte real y parte imaginaria
            dgvImpedancias.Columns.Add("BarraI", "Barra de origen");
            dgvImpedancias.Columns.Add("BarraJ", "Barra de destino");
            dgvImpedancias.Columns.Add("Real", "a");
            dgvImpedancias.Columns.Add("Imaginaria", "bj");

            // Hacer las columnas de barra i y j de solo lectura
            dgvImpedancias.Columns["BarraI"].ReadOnly = false;
            dgvImpedancias.Columns["BarraJ"].ReadOnly = false;

            // Agregar filas para cada línea
            for (int i = 0; i < numeroLineas; i++)
            {
                dgvImpedancias.Rows.Add(i + 1, i + 1, "0", "0");
            }
        }

        private void ConfigurarDataGridTiposBarras()
        {
            dgvTiposBarras.Columns.Clear();
            dgvTiposBarras.Rows.Clear();

            // Agregar columnas para barra y tipo
            dgvTiposBarras.Columns.Add("Barra", "Barra");

            //Hacer la columna de barra de solo lectura
            dgvTiposBarras.Columns["Barra"].ReadOnly = true;

            var comboBoxColumn = new DataGridViewComboBoxColumn
            {
                Name = "Tipo",
                HeaderText = "Tipo",
                DataSource = new string[] { "PV", "PQ", "Slack" }
            };
            dgvTiposBarras.Columns.Add(comboBoxColumn);
            dgvTiposBarras.Columns.Add("V", "V");

            // Agregar filas para cada barra
            for (int i = 0; i < numeroBarras; i++)
            {
                dgvTiposBarras.Rows.Add(i + 1, "PQ",0);
            }
        }

        //Agregar las barras que sean de tipo PV o PQ al dgvValoresPU
        private void AgregarBarrasPVyPQ()
        {
            dgvValoresPU.Columns.Clear();
            dgvValoresPU.Rows.Clear();

            // Agregar columnas para barra, real, base
            dgvValoresPU.Columns.Add("Barra", "Barra");
            dgvValoresPU.Columns.Add("S", "Valor S");

            // Hacer las columnas de barra de solo lectura
            dgvValoresPU.Columns["Barra"].ReadOnly = true;

            // Agregar filas para cada barra
            for (int i = 0; i < numeroBarras; i++)
            {
                if (dgvTiposBarras.Rows[i].Cells["Tipo"].Value.ToString() == "PV" || dgvTiposBarras.Rows[i].Cells["Tipo"].Value.ToString() == "PQ")
                {
                    dgvValoresPU.Rows.Add(i + 1, 0);
                }
            }
        }


        // Mostrar la matriz Ybarra en el DataGridView
        private void MostrarYbarra(Complex[,] ybarra)
        {
            dgvYbarra.Columns.Clear();
            dgvYbarra.Rows.Clear();

            // Agregar columnas para cada barra
            for (int j = 0; j < numeroBarras; j++)
            {
                dgvYbarra.Columns.Add($"Barra{j + 1}", $"Barra {j + 1}");
            }

            // Agregar filas con los valores de Ybarra
            for (int i = 0; i < numeroBarras; i++)
            {
                object[] fila = new object[numeroBarras];
                for (int j = 0; j < numeroBarras; j++)
                {
                    string signo = ybarra[i, j].Imaginary >= 0 ? "+" : "";
                    fila[j] = $"{ybarra[i, j].Real:F2} {signo} {ybarra[i, j].Imaginary:F2}j";
                }
                dgvYbarra.Rows.Add(fila);
            }
        }

        //Mostrar la matriz YBarra en forma polar en datagridview
        private void MostrarYbarraPolar(Complex[,] ybarra)
        {
            dgvYbarraPolar.Columns.Clear();
            dgvYbarraPolar.Rows.Clear();
            // Agregar columnas para cada barra
            for (int j = 0; j < numeroBarras; j++)
            {
                dgvYbarraPolar.Columns.Add($"Barra{j + 1}", $"Barra {j + 1}");
            }
            // Agregar filas con los valores de Ybarra
            for (int i = 0; i < numeroBarras; i++)
            {
                object[] fila = new object[numeroBarras];
                for (int j = 0; j < numeroBarras; j++)
                {
                    double magnitud = Math.Sqrt(Math.Pow(ybarra[i, j].Real, 2) + Math.Pow(ybarra[i, j].Imaginary, 2));
                    double angulo = Math.Atan2(ybarra[i, j].Imaginary, ybarra[i, j].Real) * 180 / Math.PI;
                    fila[j] = $"{magnitud:F2} ∠ {angulo:F2}°";
                }
                dgvYbarraPolar.Rows.Add(fila);
            }
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnGeneraPU = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.dgvTiposBarras = new System.Windows.Forms.DataGridView();
            this.btnConfirmarLineas = new System.Windows.Forms.Button();
            this.txtNumeroLineas = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnCalcular = new System.Windows.Forms.Button();
            this.txtNumeroBarras = new System.Windows.Forms.TextBox();
            this.dgvImpedancias = new System.Windows.Forms.DataGridView();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dgvYbarra = new System.Windows.Forms.DataGridView();
            this.dgvYbarraPolar = new System.Windows.Forms.DataGridView();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dgvValoresPU = new System.Windows.Forms.DataGridView();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.txtNumeroIteraciones = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.dgvResultados = new System.Windows.Forms.DataGridView();
            this.btnLimpiarPantalla = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTiposBarras)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvImpedancias)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvYbarra)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvYbarraPolar)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvValoresPU)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResultados)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnGeneraPU);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.dgvTiposBarras);
            this.groupBox1.Controls.Add(this.btnConfirmarLineas);
            this.groupBox1.Controls.Add(this.txtNumeroLineas);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.btnCalcular);
            this.groupBox1.Controls.Add(this.txtNumeroBarras);
            this.groupBox1.Controls.Add(this.dgvImpedancias);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(445, 768);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Ingreso de datos";
            this.groupBox1.UseCompatibleTextRendering = true;
            // 
            // btnGeneraPU
            // 
            this.btnGeneraPU.Location = new System.Drawing.Point(165, 727);
            this.btnGeneraPU.Name = "btnGeneraPU";
            this.btnGeneraPU.Size = new System.Drawing.Size(75, 23);
            this.btnGeneraPU.TabIndex = 11;
            this.btnGeneraPU.Text = "Siguiente";
            this.btnGeneraPU.UseVisualStyleBackColor = true;
            this.btnGeneraPU.Click += new System.EventHandler(this.btnGeneraPU_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 449);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(103, 16);
            this.label6.TabIndex = 10;
            this.label6.Text = "Tipos de barras";
            // 
            // dgvTiposBarras
            // 
            this.dgvTiposBarras.AllowUserToAddRows = false;
            this.dgvTiposBarras.AllowUserToDeleteRows = false;
            this.dgvTiposBarras.AllowUserToOrderColumns = true;
            this.dgvTiposBarras.AllowUserToResizeRows = false;
            this.dgvTiposBarras.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvTiposBarras.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTiposBarras.Location = new System.Drawing.Point(10, 479);
            this.dgvTiposBarras.Name = "dgvTiposBarras";
            this.dgvTiposBarras.RowHeadersVisible = false;
            this.dgvTiposBarras.RowHeadersWidth = 51;
            this.dgvTiposBarras.RowTemplate.Height = 24;
            this.dgvTiposBarras.Size = new System.Drawing.Size(420, 226);
            this.dgvTiposBarras.TabIndex = 9;
            // 
            // btnConfirmarLineas
            // 
            this.btnConfirmarLineas.Location = new System.Drawing.Point(165, 98);
            this.btnConfirmarLineas.Name = "btnConfirmarLineas";
            this.btnConfirmarLineas.Size = new System.Drawing.Size(94, 30);
            this.btnConfirmarLineas.TabIndex = 8;
            this.btnConfirmarLineas.Text = "Generar";
            this.btnConfirmarLineas.UseVisualStyleBackColor = true;
            this.btnConfirmarLineas.Click += new System.EventHandler(this.btnConfirmarLineas_Click);
            // 
            // txtNumeroLineas
            // 
            this.txtNumeroLineas.Location = new System.Drawing.Point(255, 52);
            this.txtNumeroLineas.Name = "txtNumeroLineas";
            this.txtNumeroLineas.Size = new System.Drawing.Size(144, 22);
            this.txtNumeroLineas.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(252, 33);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(113, 16);
            this.label5.TabIndex = 6;
            this.label5.Text = "Numero de lineas";
            // 
            // btnCalcular
            // 
            this.btnCalcular.Location = new System.Drawing.Point(141, 414);
            this.btnCalcular.Name = "btnCalcular";
            this.btnCalcular.Size = new System.Drawing.Size(118, 29);
            this.btnCalcular.TabIndex = 5;
            this.btnCalcular.Text = "Calcular YBarra";
            this.btnCalcular.UseVisualStyleBackColor = true;
            this.btnCalcular.Click += new System.EventHandler(this.btnCalcular_Click);
            // 
            // txtNumeroBarras
            // 
            this.txtNumeroBarras.Location = new System.Drawing.Point(75, 53);
            this.txtNumeroBarras.Name = "txtNumeroBarras";
            this.txtNumeroBarras.Size = new System.Drawing.Size(116, 22);
            this.txtNumeroBarras.TabIndex = 3;
            // 
            // dgvImpedancias
            // 
            this.dgvImpedancias.AllowUserToAddRows = false;
            this.dgvImpedancias.AllowUserToDeleteRows = false;
            this.dgvImpedancias.AllowUserToOrderColumns = true;
            this.dgvImpedancias.AllowUserToResizeRows = false;
            this.dgvImpedancias.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvImpedancias.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvImpedancias.Location = new System.Drawing.Point(9, 168);
            this.dgvImpedancias.Name = "dgvImpedancias";
            this.dgvImpedancias.RowHeadersVisible = false;
            this.dgvImpedancias.RowHeadersWidth = 51;
            this.dgvImpedancias.RowTemplate.Height = 24;
            this.dgvImpedancias.Size = new System.Drawing.Size(420, 231);
            this.dgvImpedancias.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 138);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "Valores de lineas";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(75, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Numero de barras";
            // 
            // dgvYbarra
            // 
            this.dgvYbarra.AllowUserToAddRows = false;
            this.dgvYbarra.AllowUserToDeleteRows = false;
            this.dgvYbarra.AllowUserToOrderColumns = true;
            this.dgvYbarra.AllowUserToResizeRows = false;
            this.dgvYbarra.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvYbarra.Location = new System.Drawing.Point(6, 62);
            this.dgvYbarra.Name = "dgvYbarra";
            this.dgvYbarra.RowHeadersVisible = false;
            this.dgvYbarra.RowHeadersWidth = 51;
            this.dgvYbarra.RowTemplate.Height = 24;
            this.dgvYbarra.Size = new System.Drawing.Size(486, 172);
            this.dgvYbarra.TabIndex = 1;
            // 
            // dgvYbarraPolar
            // 
            this.dgvYbarraPolar.AllowUserToAddRows = false;
            this.dgvYbarraPolar.AllowUserToDeleteRows = false;
            this.dgvYbarraPolar.AllowUserToOrderColumns = true;
            this.dgvYbarraPolar.AllowUserToResizeRows = false;
            this.dgvYbarraPolar.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvYbarraPolar.Location = new System.Drawing.Point(6, 277);
            this.dgvYbarraPolar.Name = "dgvYbarraPolar";
            this.dgvYbarraPolar.RowHeadersVisible = false;
            this.dgvYbarraPolar.RowHeadersWidth = 51;
            this.dgvYbarraPolar.RowTemplate.Height = 24;
            this.dgvYbarraPolar.Size = new System.Drawing.Size(486, 166);
            this.dgvYbarraPolar.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 258);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(122, 16);
            this.label3.TabIndex = 3;
            this.label3.Text = "Matriz YBarra Polar";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 33);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(87, 16);
            this.label4.TabIndex = 4;
            this.label4.Text = "Matriz YBarra";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.dgvYbarra);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.dgvYbarraPolar);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(464, 13);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(500, 456);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Matrices";
            this.groupBox2.Enter += new System.EventHandler(this.groupBox2_Enter);
            // 
            // dgvValoresPU
            // 
            this.dgvValoresPU.AllowUserToAddRows = false;
            this.dgvValoresPU.AllowUserToDeleteRows = false;
            this.dgvValoresPU.AllowUserToResizeColumns = false;
            this.dgvValoresPU.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvValoresPU.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvValoresPU.Location = new System.Drawing.Point(8, 23);
            this.dgvValoresPU.Name = "dgvValoresPU";
            this.dgvValoresPU.RowHeadersVisible = false;
            this.dgvValoresPU.RowHeadersWidth = 51;
            this.dgvValoresPU.RowTemplate.Height = 24;
            this.dgvValoresPU.Size = new System.Drawing.Size(483, 220);
            this.dgvValoresPU.TabIndex = 6;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.txtNumeroIteraciones);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.button1);
            this.groupBox3.Controls.Add(this.dgvValoresPU);
            this.groupBox3.Location = new System.Drawing.Point(465, 475);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(499, 304);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Valores S";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(350, 257);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(97, 38);
            this.button1.TabIndex = 12;
            this.button1.Text = "Calcular";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 268);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(143, 16);
            this.label7.TabIndex = 13;
            this.label7.Text = "Numero de iteraciones";
            // 
            // txtNumeroIteraciones
            // 
            this.txtNumeroIteraciones.Location = new System.Drawing.Point(172, 265);
            this.txtNumeroIteraciones.Name = "txtNumeroIteraciones";
            this.txtNumeroIteraciones.Size = new System.Drawing.Size(87, 22);
            this.txtNumeroIteraciones.TabIndex = 14;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.dgvResultados);
            this.groupBox4.Location = new System.Drawing.Point(984, 13);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(543, 705);
            this.groupBox4.TabIndex = 8;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Valores calculados";
            // 
            // dgvResultados
            // 
            this.dgvResultados.AllowUserToAddRows = false;
            this.dgvResultados.AllowUserToDeleteRows = false;
            this.dgvResultados.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvResultados.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvResultados.Location = new System.Drawing.Point(7, 33);
            this.dgvResultados.Name = "dgvResultados";
            this.dgvResultados.ReadOnly = true;
            this.dgvResultados.RowHeadersVisible = false;
            this.dgvResultados.RowHeadersWidth = 51;
            this.dgvResultados.RowTemplate.Height = 24;
            this.dgvResultados.Size = new System.Drawing.Size(530, 666);
            this.dgvResultados.TabIndex = 0;
            // 
            // btnLimpiarPantalla
            // 
            this.btnLimpiarPantalla.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLimpiarPantalla.Location = new System.Drawing.Point(1173, 740);
            this.btnLimpiarPantalla.Name = "btnLimpiarPantalla";
            this.btnLimpiarPantalla.Size = new System.Drawing.Size(133, 38);
            this.btnLimpiarPantalla.TabIndex = 13;
            this.btnLimpiarPantalla.Text = "Limpiar pantalla";
            this.btnLimpiarPantalla.UseVisualStyleBackColor = true;
            this.btnLimpiarPantalla.Click += new System.EventHandler(this.btnLimpiarPantalla_Click);
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(1666, 791);
            this.Controls.Add(this.btnLimpiarPantalla);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Grupo H";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTiposBarras)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvImpedancias)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvYbarra)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvYbarraPolar)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvValoresPU)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvResultados)).EndInit();
            this.ResumeLayout(false);

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void btnGeneraPU_Click(object sender, EventArgs e)
        {
            AgregarBarrasPVyPQ();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                int rows = dgvValoresPU.Rows.Count;
                S = new MatrizS[rows];
                int numeroIteraciones = 0;
                try
                {
                    numeroIteraciones = int.Parse(txtNumeroIteraciones.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (numeroIteraciones <= 0)
                {
                    throw new ArgumentException("El número de iteraciones debe ser mayor que 0.");
                }

                for (int i = 0; i < rows; i++)
                {
                    int barra = int.Parse(dgvValoresPU.Rows[i].Cells["Barra"].Value.ToString());
                    string valorS = dgvValoresPU.Rows[i].Cells["S"].Value.ToString();

                    Complex realComplex;

                    // Validar si el valor real es un número complejo
                    if (valorS.Contains("j") || valorS.Contains("J"))
                    {
                        // Eliminar espacios y convertir a minúsculas
                        valorS = valorS.Replace(" ", "").ToLower();

                        double realPart = 0;
                        double imaginaryPart = 0;

                        // Encontrar la posición del último signo '+' o '-' antes de 'j'
                        int lastSignIndex = valorS.LastIndexOfAny(new char[] { '+', '-' }, valorS.Length - 2);

                        if (lastSignIndex > 0)
                        {
                            // Extraer la parte real
                            realPart = double.Parse(valorS.Substring(0, lastSignIndex));

                            // Extraer la parte imaginaria
                            imaginaryPart = double.Parse(valorS.Substring(lastSignIndex).Replace("j",""));
                        }
                        else
                        {
                            // Solo parte imaginaria
                            imaginaryPart = double.Parse(valorS.Replace("j", ""));
                        }

                        realComplex = new Complex(realPart, imaginaryPart);
                    }
                    else
                    {
                        // Si no es un número complejo, tratarlo como un número real
                        double realPart = double.Parse(valorS);
                        realComplex = new Complex(realPart, 0);
                    }
                    // Dividir el valor real por la base
                    Complex valor = realComplex;
                    // Buscar el tipo de barra en dgvTiposBarras
                    string tipo = "";
                    double magnitudVoltaje = 1.0;
                    foreach (DataGridViewRow row in dgvTiposBarras.Rows)
                    {
                        if (int.Parse(row.Cells["Barra"].Value.ToString()) == barra)
                        {
                            tipo = row.Cells["Tipo"].Value.ToString();
                            if (tipo == "PV")
                            {
                                magnitudVoltaje = double.Parse(row.Cells["V"].Value.ToString());
                            }
                            break;
                        }
                    }
                    S[i] = new MatrizS(barra, valor, tipo, magnitudVoltaje);
                }


                //Empieza el calculo de gauss-seidel
                MatrizS[] SCompleto = new MatrizS[numeroBarras];

                int sIndex = 0;
                for (int i = 0; i < numeroBarras; i++)
                {
                    // Buscar el tipo de barra en dgvTiposBarras
                    string tipo = "";
                    double magnitudVoltaje = 1.0;
                    foreach (DataGridViewRow row in dgvTiposBarras.Rows)
                    {
                        if (int.Parse(row.Cells["Barra"].Value.ToString()) == i + 1)
                        {
                            tipo = row.Cells["Tipo"].Value.ToString();
                            if (tipo == "PV" || tipo == "Slack")
                            {
                                magnitudVoltaje = double.Parse(row.Cells["V"].Value.ToString());
                            }
                            break;
                        }
                    }

                    if (tipo == "Slack")
                    {
                        // Para barras Slack, no especificamos S (se calcula implícitamente)
                        SCompleto[i] = new MatrizS(i, Complex.Zero, "Slack", magnitudVoltaje);
                    }
                    else
                    {
                        // Usar las barras PQ y PV de S
                        SCompleto[i] = new MatrizS(i, S[sIndex].Valor, S[sIndex].Tipo, magnitudVoltaje);
                        sIndex++;
                    }
                }

                // Obtener voltajes iniciales de dgvTiposBarras
                Complex[] voltajesIniciales = new Complex[numeroBarras];
                for (int i = 0; i < numeroBarras; i++)
                {
                    double magnitud = 1.0;
                    foreach (DataGridViewRow row in dgvTiposBarras.Rows)
                    {
                        if (int.Parse(row.Cells["Barra"].Value.ToString()) == i + 1)
                        {
                            magnitud = double.Parse(row.Cells["V"].Value.ToString());
                            break;
                        }
                    }
                    voltajesIniciales[i] = new Complex(magnitud, 0); // Ángulo inicial 0
                }

                // Ejecutar Gauss-Seidel
                var gaussSeidel = new GaussSeidel(matriz.GetYbarra(), SCompleto, voltajesIniciales,numeroIteraciones);
                List<IteracionResultado> voltajesFinales = gaussSeidel.CalcularVoltajes();

                mostrarResultados(voltajesFinales);
                foreach (var value in S)
                {
                    Console.WriteLine(value);
                }
            }
            catch (Exception ex)
            {
                //lblMensaje.Text = "Error: " + ex.Message;
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void mostrarResultados(List<IteracionResultado> resultados)
        {
            dgvResultados.Columns.Clear();
            dgvResultados.Rows.Clear();
            // Agregar columnas para barra, real, base
            dgvResultados.Columns.Add("Iteracion", "Iteracion");
            dgvResultados.Columns.Add("Barra", "Barra");
            dgvResultados.Columns.Add("V", "V");
            dgvResultados.Columns.Add("Q", "Q");
            // Hacer las columnas de barra de solo lectura
            dgvResultados.Columns["Iteracion"].ReadOnly = true;
            dgvResultados.Columns["Barra"].ReadOnly = true;
            dgvResultados.Columns["V"].ReadOnly = true;
            dgvResultados.Columns["Q"].ReadOnly = true;
            // Agregar filas para cada resultado
            for (int i = 0; i < resultados.Count; i++)
            {
                string voltajeStr = FormatearComplejo(resultados[i].Voltaje);
                string qStr = FormatearComplejo(new Complex(0, resultados[i].Q), soloImaginaria:true);

                dgvResultados.Rows.Add(resultados[i].Iteracion + 1, resultados[i].Barra + 1, voltajeStr, qStr);
            }
        }

        private string FormatearComplejo(Complex c, bool soloImaginaria = false)
        {
            if (soloImaginaria)
            {
                string signo = c.Imaginary >= 0 ? "+" : "-";
                return $"{signo} j{Math.Abs(c.Imaginary):F5}";
            }
            else
            {
                string signo = c.Imaginary >= 0 ? "+" : "-";
                return $"{c.Real:F5} {signo} j{Math.Abs(c.Imaginary):F5}";
            }
        }

        private void btnLimpiarPantalla_Click(object sender, EventArgs e)
        {
            //Limpiar todos los datos
            dgvImpedancias.Rows.Clear();
            dgvImpedancias.Columns.Clear();
            dgvTiposBarras.Rows.Clear();
            dgvTiposBarras.Columns.Clear();
            dgvValoresPU.Rows.Clear();
            dgvValoresPU.Columns.Clear();
            dgvYbarra.Rows.Clear();
            dgvYbarra.Columns.Clear();
            dgvYbarraPolar.Rows.Clear();
            dgvYbarraPolar.Columns.Clear();
            dgvResultados.Rows.Clear();
            dgvResultados.Columns.Clear();
            txtNumeroBarras.Text = "";
            txtNumeroLineas.Text = "";
            txtNumeroIteraciones.Text = "";
            btnCalcular.Enabled = false;
            matriz.Limpiar();
        }
    }
}
