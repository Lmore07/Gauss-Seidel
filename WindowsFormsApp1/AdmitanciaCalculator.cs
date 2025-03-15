using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    // Clase para manejar las operaciones matriciales (lógica de negocio)
    public class MatrizImpedancias
    {
        private Complex[,] Z; // Matriz de impedancias
        private List<Complex>[] ZExternos; // Lista de impedancias externas por barra
        private Complex[,] Ybarra; // Matriz de admitancias
        private int n; // Número de barras
        private int m; // Número de líneas

        public MatrizImpedancias(int numeroBarras, int numeroLineas)
        {
            n = numeroBarras;
            m = numeroLineas;
            Z = new Complex[m, n];
            Ybarra = new Complex[n, n];
            ZExternos = new List<Complex>[n];

            // Inicializar matriz Z con 0
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Z[i, j] = Complex.Zero;
                }
            }

            // Inicializar matriz Ybarra con 0
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Ybarra[i, j] = Complex.Zero;
                }
            }

            // Inicializar listas de impedancias externas
            for (int i = 0; i < n; i++)
            {
                ZExternos[i] = new List<Complex>();
            }
        }

        // Verificar si la matriz Z es simétrica
        public bool EsSimetrica()
        {
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (Z[i, j] != Z[j, i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        // Método para establecer un valor en la matriz Z
        public void SetImpedancia(int i, int j, Complex impedancia)
        {
            Z[i, j] = impedancia;
            Z[j, i] = impedancia; // Asegurar simetría
        }

        // Método para agregar un valor en la lista de impedancias externas
        public void AddImpedanciaExterna(int barra, Complex impedancia)
        {
            ZExternos[barra].Add(impedancia);
        }

        //Resetear lista de impedancias externas
        public void ResetImpedanciasExternas()
        {
            for (int i = 0; i < n; i++)
            {
                ZExternos[i].Clear();
            }
        }

        //Resetear matriz de impedancias
        public void ResetImpedancias()
        {
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Z[i, j] = Complex.Zero;
                }
            }
        }

        //Resetear matriz Ybarra
        public void ResetYbarra()
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Ybarra[i, j] = Complex.Zero;
                }
            }
        }

        //Limpiar matrices
        public void Limpiar()
        {
            ResetImpedancias();
            ResetImpedanciasExternas();
            ResetYbarra();
        }

        // Calcular la matriz Ybarra
        public void CalcularYbarra()
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j)
                    {
                        // Sumar las admitancias de todas las conexiones de la barra i
                        Complex sumaAdmitancias = Complex.Zero;
                        for (int k = 0; k < n; k++)
                        {
                            if (k != i && !Z[i, k].Equals(Complex.Zero))
                            {
                                sumaAdmitancias += Complex.One / Z[i, k];
                            }
                        }
                        // Incluir la admitancia de los objetos externos
                        foreach (var impedanciaExterna in ZExternos[i])
                        {
                            sumaAdmitancias += Complex.One / impedanciaExterna;
                        }
                        Ybarra[i, i] = sumaAdmitancias;
                    }
                    else
                    {
                        // Calcular la admitancia entre las barras i y j
                        if (!Z[i, j].Equals(Complex.Zero))
                        {
                            Ybarra[i, j] = Complex.Negate(Complex.One / Z[i, j]);
                        }
                    }
                }
            }
        }

        // Obtener la matriz Ybarra
        public Complex[,] GetYbarra()
        {
            return Ybarra;
        }
    }
}