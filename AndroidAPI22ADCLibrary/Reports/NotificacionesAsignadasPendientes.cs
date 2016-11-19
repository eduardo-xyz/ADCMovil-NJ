using AndroidAPI22ADCLibrary.Fragments;
using Android.Support.V4.App;
using System;

namespace AndroidAPI22ADCLibrary.Reports
{
    public class NotificacionesAsignadasPendientesPorJornada : Reports.Report
    {
        // Contructor, lo �nico que debe de hacer es costruir la parte base
        // del reporte con el nombre espec�fico y unico de este reporte
        public NotificacionesAsignadasPendientesPorJornada() : base("Notificaciones asignadas pendientes por jornada")
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
            return new CommonInputReport(this, false, true, false, true, false, false);
        }

        public override Fragment getOutputReportFragment()
        {
            // Crea el reporte espec�fico, pasa la misma instancia del reporte
            // al segmento para que el mismo puede obtener/retornar de manera simple
            // los parametros
            return new NotificacionesAsignadasPendientesOutputReport(this);
        }
    }
}