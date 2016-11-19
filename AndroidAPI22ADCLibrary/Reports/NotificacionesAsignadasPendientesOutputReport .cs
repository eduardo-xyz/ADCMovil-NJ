using Android.Support.V4.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidAPI22ADCLibrary.Helpers;
using Android.Content;
using System.Net;
using System;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace AndroidAPI22ADCLibrary.Reports
{
    public class NotificacionesAsignadasPendientesOutputReport : Fragment
    {
        NotificacionesAsignadasPendientesPorJornada report;

        public NotificacionesAsignadasPendientesOutputReport(NotificacionesAsignadasPendientesPorJornada report)
        {
            this.report = report;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            report.output_error = null;

            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            View self = inflater.Inflate(Resource.Layout.CommonReportOutput, null);

            if (!report.getFillCommonInfo(self, true, false, ref report.output_error))
            {
                reportErrorGoBack(report.output_error);
                return self;
            }

            TableLayout table = self.FindViewById<TableLayout>(Resource.Id.tbReporte);

            // Asegura que ningun parametro este vac�o
            if (String.IsNullOrEmpty(report.input_oficina))
            {
                reportErrorGoBack("El campo de oficina no puede ser vac�o");
                return self;
            }

            // Consulta
            // Cambiar las fechas al formato admitido por la BD
            string fecha_jornada = report.input_fecha_jornada.ToString("yyyyMMdd");

            string codigoNotificador = "";
            try
            {
                Helpers.SQLiteConeccion dbConeccion;
                dbConeccion = new Helpers.SQLiteConeccion();
                dbConeccion.consultaDatos(
                    "SELECT CodigoNotificador FROM OficialesNotificadores WHERE NombreCompleto = '" + report.input_notificador + "'",
                    this.Context, ref codigoNotificador);
            }
            catch (Exception)
            {
                reportErrorGoBack("No fue posible obtener la informaci�n requerida para le reporte (ID)");
                return self;
            }

            string query = @"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/Reportes/NotificacionesPendientesJornada" +
                "?PCodOficina=" + report.input_oficina +
                "&PCodNotificador=" + codigoNotificador +
                "&PFechaJornada=" + fecha_jornada;
      
            // Verificar si la conecci�n a internet esta disponible
            if (!coneccionInternet.verificaConeccion(this.Context))
                ReportUtils.alertNoInternetMessage(this.Context);

            WebRequest request = HttpWebRequest.Create(query);
            request.ContentType = "application/json";
            request.Method = "GET";
            string content = "";

            Console.Out.WriteLine("--XDEBUG " + query);
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Console.Out.WriteLine("Error fetching data. Server returned status code: {0}", response.StatusCode);
                    reportErrorGoBack("No fue posible obtener la informaci�n requerida para le reporte");
                    return self;
                }

                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    content = reader.ReadToEnd();

                    if (string.IsNullOrWhiteSpace(content))
                    {
                        Console.Out.WriteLine("fzeledon -- Response contained empty body...");
                        reportErrorGoBack("Los valores de entrada de la consulta no generaron resultado");
                        return self;
                    }

                    try
                    {
                        var jsonParsed = JArray.Parse(content);
                        if (jsonParsed.Count <= 0) // Esperamos al menos un resultado
                        {
                            reportErrorGoBack("La informaci�n obtenida para le informe no puede ser procesada");
                            return self;
                        }
                        

                        // Por cada despacho encontrada

                        ReportUtils.agregarFilaATabla(new string[] {"Notificando", "Expediente", "Fecha de resoluci�n", "Justificaci�n"}, Activity, table);

                        int i = 0;
                        for (i = 0; i < jsonParsed.Count; ++i)
                        {
                            string notificando = jsonParsed[i].Value<string>("Notificando");
                            string expediente = jsonParsed[i].Value<string>("Expediente");
                            string resolicion = jsonParsed[i].Value<string>("FechaResolucion");
                            string justificacion = "No disponible"; // FIXME no existia el campo en la consulta
                            DateTime date_resolucion = DateTime.Parse(resolicion);
                            resolicion = date_resolucion.ToString("dd-MMM-yyyy");
                            ReportUtils.agregarFilaATabla(new string[] { notificando, expediente, resolicion, justificacion }, Activity, table);
                            // FIXME. Son demasiadas las respuestas, filtrar por pagina o preguntar como proceder
                            if (i > 200) break;
                        }

                        ReportUtils.agregarFilaATabla(new string[] { "Total:", i.ToString(), "", ""}, Activity, table);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error descargando datos de usuario: " + ex.ToString());
                        reportErrorGoBack("Error descargando datos de usuario");
                        return self;
                    }
                }
                
            }
            
            Button btnGen = self.FindViewById<Button>(Resource.Id.btnRegresarNotificacionesEnviadas);
            btnGen.Click += (sender, e) => {
                report.output_error = null;
                Fragment frag = report.getInputReportFragment();
                FragmentManager.BeginTransaction().Replace(Resource.Id.content_frame, frag).Commit();
            };

            return self;
        }

        /* Wrapper para retornar un error al fragmento de entrada */
        private void reportErrorGoBack(string error)
        {
            report.output_error = error;
            Fragment frag = report.getInputReportFragment();
            FragmentManager.BeginTransaction().Replace(Resource.Id.content_frame, frag).Commit();
        }
    }
}