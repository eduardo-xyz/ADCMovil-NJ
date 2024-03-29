using AndroidAPI22ADCLibrary.Fragments;
using Android.Support.V4.App;
using System;

namespace AndroidAPI22ADCLibrary.Reports
{
    public class NotificacionesRealizadasSegunResultado : Reports.Report
    {
        // Contructor, lo �nico que debe de hacer es costruir la parte base
        // del reporte con el nombre espec�fico y unico de este reporte
        public NotificacionesRealizadasSegunResultado() : base("Notificaciones realizadas por periodo seg�n su resultado")
        {
            input_oficina = null;
            input_despacho = null;
            input_fecha_inicio = DateTime.Now;
            input_fecha_fin = DateTime.Now;
            output_error = null;
        }

        public override Fragment getInputReportFragment()
        {
            // Crea el reporte espec�fico, pasa la misma instancia del reporte
            // al segmento para que el mismo puede obtener/retornar de manera simple
            // los parametros
            return new CommonInputReport(this, false, false, false, false, true, true);
        }

        public override Fragment getOutputReportFragment()
        {
            // Crea el reporte espec�fico, pasa la misma instancia del reporte
            // al segmento para que el mismo puede obtener/retornar de manera simple
            // los parametros
            return new NotificacionesRealizadasSegunResultadoOutputReport(this);
        }
    }
}