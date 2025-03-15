using System.Numerics;

namespace WindowsFormsApp1
{
    public class IteracionResultado
    {
        public int Iteracion { get; set; }
        public int Barra { get; set; }
        public Complex Voltaje { get; set; }
        public double Q { get; set; }

        public IteracionResultado(int iteracion,int barra, Complex voltaje, double q)
        {
            Barra = barra;
            Voltaje = voltaje;
            Q = q;
            Iteracion = iteracion;
        }

        public override string ToString()
        {
            return $"Barra: {Barra}, Voltaje: {Voltaje}, Q: {Q}";
        }
    }
}