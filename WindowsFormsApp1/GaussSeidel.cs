using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    class GaussSeidel
    {
        private Complex[,] Ybarra; // Matriz de admitancias
        private MatrizS[] barras; // Información de las barras
        private Complex[] voltajes; // Voltajes calculados
        private int n; // Número de barras
        private double tolerancia; // Tolerancia para la convergencia
        private int maxIteraciones; // Máximo número de iteraciones

        public GaussSeidel(Complex[,] ybarra, MatrizS[] barrasInfo, Complex[] voltajesIniciales, int maxIteraciones, double tolerancia = 1e-6)
        {
            if (ybarra.GetLength(0) != ybarra.GetLength(1) || ybarra.GetLength(0) != voltajesIniciales.Length)
            {
                throw new ArgumentException("Dimensiones incompatibles entre Ybarra, barras y voltajes iniciales.");
            }

            n = ybarra.GetLength(0);
            Ybarra = ybarra;

            // Verificar que todas las barras estén presentes y ordenadas
            if (barrasInfo.Length != n || barrasInfo.Select(b => b.Barra).Distinct().Count() != n)
            {
                throw new ArgumentException("La lista de barras debe contener exactamente un registro por barra (0 a n-1).");
            }

            barras = new MatrizS[n];
            Array.Sort(barrasInfo, (a, b) => a.Barra.CompareTo(b.Barra)); // Ordenar por índice de barra
            Array.Copy(barrasInfo, barras, n);

            voltajes = new Complex[n];
            Array.Copy(voltajesIniciales, voltajes, n);
            this.tolerancia = tolerancia;
            this.maxIteraciones = maxIteraciones;
        }

        public List<IteracionResultado> CalcularVoltajes()
        {
            int iteracion = 0;
            double error = double.MaxValue;
            List<IteracionResultado> resultados = new List<IteracionResultado>();

            while (iteracion < maxIteraciones)
            {
                Complex[] voltajesAnteriores = (Complex[])voltajes.Clone();
                error = 0.0;

                for (int p = 0; p < n; p++)
                {

                    if (barras[p].Tipo.ToUpper() == "SLACK")
                    {
                        Console.WriteLine("Es de tipo SLACK");
                        continue;
                    }

                    Complex suma1 = Complex.Zero; // Sumatoria para q < p
                    Complex suma2 = Complex.Zero; // Sumatoria para q > p

                    // Calcular sumatorias
                    for (int q = 0; q < n; q++)
                    {
                        if (q < p)
                        {
                            suma1 += Ybarra[p, q] * voltajes[q]; // Voltajes actualizados V_q^(k+1)
                        }
                        else if (q > p)
                        {
                            suma2 += Ybarra[p, q] * voltajesAnteriores[q]; // Voltajes anteriores V_q^(k)
                        }
                    }

                    Complex denominador = Ybarra[p, p];
                    if (denominador == Complex.Zero)
                    {
                        throw new ArgumentException($"Elemento diagonal Ybarra[{p},{p}] es cero.");
                    }

                    Complex nuevoVoltaje;
                    double Qp = 0.0;

                    if (barras[p].Tipo.ToUpper() == "PQ")
                    {
                        // Ecuación para barras PQ

                        //Invertir signos de barras[p].Valor
                        Complex valorInvertido = -barras[p].Valor;
                        Complex division = Complex.Conjugate(valorInvertido / voltajesAnteriores[p]);
                        Complex numerador = division - suma1 - suma2;
                        nuevoVoltaje = numerador / denominador;
                    }
                    else if (barras[p].Tipo.ToUpper() == "PV")
                    {
                        // Calcular Q_p^(k+1) para la barra PV
                        Complex sumaTotal = Complex.Zero;
                        for (int q = 0; q < n; q++)
                        {
                            if (q < p)
                            {
                                sumaTotal += Ybarra[p, q] * voltajes[q]; // V_q^(k+1)
                            }
                            else
                            {
                                sumaTotal += Ybarra[p, q] * voltajesAnteriores[q]; // V_q^(k)
                            }
                        }
                        Complex valorInvertido = -voltajesAnteriores[p];
                        Complex potenciaCalculada = Complex.Conjugate(valorInvertido) * sumaTotal;
                        Qp = potenciaCalculada.Imaginary;

                        // Nueva S_p con el Q_p calculado
                        Complex Sp = new Complex(barras[p].Valor.Real, Qp);

                        // Calcular V_p^(k+1)
                        valorInvertido = Sp;
                        Complex numerador = Complex.Conjugate(valorInvertido / voltajesAnteriores[p]) - suma1 - suma2;
                        nuevoVoltaje = numerador / denominador;

                        // Ajustar la magnitud del voltaje para barras PV
                        double magnitudEspecificada = barras[p].MagnitudVoltaje;
                        if (magnitudEspecificada <= 0)
                        {
                            throw new ArgumentException($"Magnitud de voltaje para la barra PV {p} debe ser mayor que 0.");
                        }
                        nuevoVoltaje= nuevoVoltaje / nuevoVoltaje.Magnitude * magnitudEspecificada;
                    }
                    else
                    {
                        throw new ArgumentException($"Tipo de barra no reconocido: {barras[p].Tipo}");
                    }
                    voltajes[p] = nuevoVoltaje;
                    error = Math.Max(error, (voltajesAnteriores[p]-nuevoVoltaje).Magnitude);

                    // Agregar resultado a la lista
                    if (barras[p].Tipo.ToUpper() == "PQ" || barras[p].Tipo.ToUpper() == "PV")
                    {
                        resultados.Add(new IteracionResultado(iteracion,p, nuevoVoltaje, Qp));
                    }
                }


                iteracion++;
            }
            return resultados;
        }

        public Complex[] GetVoltajes()
        {
            return voltajes;
        }
    }
}
