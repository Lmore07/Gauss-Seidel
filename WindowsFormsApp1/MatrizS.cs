using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    class MatrizS
    {
        public int Barra { get; set; }
        public Complex Valor { get; set; } // S = P + jQ
        public String Tipo { get; set; }
        public double MagnitudVoltaje { get; set; } // Para barras PV

        public MatrizS(int barra, Complex valor, string tipo, double magnitudVoltaje=1.0)
        {
            Barra = barra;
            Valor = valor;
            Tipo = tipo;
            MagnitudVoltaje = magnitudVoltaje; // 1.0 pu por defecto si no se especifica
        }

        public override string ToString()
        {
            return $"Barra: {Barra}, Valor: {Valor}, Tipo: {Tipo}, Magnitud: {MagnitudVoltaje}";
        }
    }
}