﻿using ED2LAB1_CAMP1127922.CMP;
using ED2LAB1_CAMP1127922.DS;
using ED2LAB1_CAMP1127922.ENC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ED2LAB1_CAMP1127922
{
    public partial class Form1 : Form
    {
        ABB arbol = new ABB();
        Aritmetica code = new Aritmetica();
        LZW comp = new LZW();
        string rutaArchivo;
        string rutaCarpeta;
        string rutaCarpetaCartas=null;
        Dictionary<string, List<string>> REC;
        Dictionary<string, List<string>> CONV;        
        int ops = 0;
        public Form1()
        {
            InitializeComponent();
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Archivos CSV (*.csv)|*.csv";
                //string pruebasalv = comp.COMPRESS("ABBABBABBABB");
                //pruebasalv = comp.DECOMPRESS(pruebasalv);                

                if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                rutaArchivo = openFileDialog.FileName;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                
                CargarDatosDesdeCSV(rutaArchivo);                
                //crearCSV(rutaArchivo, "ARBOL_CODIFICADO", arbol.PrintTree());
                ops++;
                stopwatch.Stop();
                long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                showmslbl.Text = $"{elapsedMilliseconds}ms";
                button1.Enabled = false;
                button1.Text = "Archivo cargado satisfactoriamente";
                    if (ops>=3)
                    {
                        edTabControl.Enabled = true;
                    }
                   
            }
            else
            {
                MessageBox.Show("Seleccione el archivo a utilizar");
            }
            }
            catch (Exception)
            {
                MessageBox.Show("El archivo cargado es imposible de leer");
            }
        }
        private void CargarDatosDesdeCSV(string rutaArchivo)
        {
            using (var reader = new StreamReader(rutaArchivo))
            {
                while (reader.EndOfStream == false)
                {
                    var content = reader.ReadLine();
                    string action = content.Split(';')[0];
                    string info = content.Split(';')[1];
                    var persona = JsonConvert.DeserializeObject<Person>(info);
                    persona.dpi = code.Encode(persona.dpi);
                    for (int i = 0; i < persona.companies.Count; i++)
                    {
                        persona.companies[i] = code.Encode(persona.companies[i]);
                    }
                    if (action == "INSERT") { arbol.Add(persona);}                   
                    else if (action == "PATCH") { arbol.Patch(persona);}
                    else if (action == "DELETE") { arbol.Delete(persona);}
                }
                
            }
        }        

        private void MergeLetterandUser()
        {
            foreach (var keyValuePair in REC)
            {
                string dpi = keyValuePair.Key;
                List<string> cartas = keyValuePair.Value;
                string nomArchivo;
                string predpi = dpi;
                dpi = code.Encode(dpi);
                List<string> carta = new List<string>();
                List<Dictionary<string, int>> dictio = new List<Dictionary<string, int>>();
                for (int i = 0; i < cartas.Count(); i++)
                {
                    var tuplecompre = comp.COMPRESS(cartas[i]);
                    carta.Add(tuplecompre.Item1);
                    dictio.Add(tuplecompre.Item2);
                    nomArchivo = "compressed-REC-" + predpi + "-" + Convert.ToString(i + 1);
                    List<string> imagine = new List<string>();
                    imagine.Add(carta[i]);
                    crearCSV(rutaArchivo, nomArchivo, imagine, "\\COMPRESSED");
                }
                Person prueba = arbol.SearchDpi(dpi);
                if (prueba!=null)
                {
                    prueba.letters = carta;
                    prueba.dictio = dictio;
                }
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }        
        public void crearCSV(string ruta, string nombreArchivo, List<string> jsons, string nombreCarpeta)
        {
            int indiceUltimaDiagonal = ruta.LastIndexOf('\\');
            ruta = ruta.Substring(0, indiceUltimaDiagonal) + nombreCarpeta;
            CrearCarpeta(ruta, nombreArchivo);
            try
            {
                string filePath = Path.Combine(ruta, nombreArchivo + ".csv");

                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    foreach (string json in jsons)
                    {
                        writer.WriteLine(json);
                    }
                }
                //MessageBox.Show("Archivo CSV creado exitosamente en " + filePath);
                //Process.Start(filePath); // Abrir el archivo creado
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al crear el archivo CSV: " + ex.Message);
            }
        }

        public void CrearCarpeta(string ruta, string nombreCarpeta)
        {
            try
            {
                // Verifica si la carpeta no existe en la ruta proporcionada.
                if (!Directory.Exists(ruta))
                {
                    // Crea la carpeta si no existe.
                    Directory.CreateDirectory(ruta);
                    Console.WriteLine("Carpeta creada en: " + ruta);
                }
                else
                {
                    Console.WriteLine("La carpeta ya existe en: " + ruta);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al crear la carpeta: " + ex.Message);
            }
        }

        private void dpibtn_Click(object sender, EventArgs e)
        {
            string dpi = code.Encode(dpitxt.Text);
            Person persona;
            string jsonString;
            if (dpitxt.Text == "") MessageBox.Show("Ingrese un nombre a buscar");
            else
            {
                persona = arbol.SearchDpi(dpi);                
                if (persona != null)
                {
                    persona.dpi = code.Decode(dpi);                    
                    for (int i = 0; i < persona.companies.Count; i++)
                    {
                        persona.companies[i] = code.Decode(persona.companies[i]);
                    }
                    jsonString = JsonConvert.SerializeObject(persona, Formatting.Indented);
                    MessageBox.Show(jsonString);
                    persona.dpi = code.Encode(persona.dpi);
                    for (int i = 0; i < persona.companies.Count; i++)
                    {
                        persona.companies[i] = code.Encode(persona.companies[i]);
                    }
                }

                else MessageBox.Show("No se encontraron datos asociados al DPI: " + dpitxt.Text);
            }
            dpitxt.Text = "";
        }

        private void nombrebtn_Click(object sender, EventArgs e)
        {
            string nombre = nombretxt.Text;
            List<Person> listabusquedas = new List<Person>();
            Person aux;
            List<string> jsons = new List<string>();
            int cont = 0;
            if (nombretxt.Text == "") MessageBox.Show("Ingrese un nombre a buscar");
            else
            {
                listabusquedas = arbol.SearchName(nombre);
                if (listabusquedas.Count()!=0)
                {
                    cont = listabusquedas.Count();
                    for (int i = 0; i < cont; i++)
                    {
                        aux = listabusquedas[i];
                        var objpersona = new Person
                        {
                            name = aux.name,
                            dpi = aux.dpi,
                            datebirth = aux.datebirth,
                            address = aux.address,
                            companies = aux.companies
                        };
                        string jsonString = JsonConvert.SerializeObject(objpersona, Formatting.None);

                        jsons.Add(jsonString);
                    }
                    crearCSV(rutaArchivo,nombre, jsons, "\\Exports");
                    MessageBox.Show("Archivo creado satisfactoriamente\nCreado en la misma carpeta del archivo de entrada");
                }
                else MessageBox.Show("no se encontraron datos asociados al nombre de " + nombre);
            }
            nombretxt.Text = "";
        }

        static string ObtenerRutaCarpeta()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Selecciona la carpeta";
                DialogResult result = dialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    // Verificar si la carpeta contiene archivos .txt
                    string[] archivosTxt = Directory.GetFiles(dialog.SelectedPath, "*.txt");
                    if (archivosTxt.Length > 0)
                    {
                        return dialog.SelectedPath;
                    }
                    else
                    {
                        MessageBox.Show("La carpeta seleccionada no contiene archivos TXT.");
                        return null;
                    }
                }
                else
                {
                    MessageBox.Show("No se seleccionó una carpeta válida.");
                    return null;
                }
            }
        }

        static Dictionary<string, List<string>> ObtenerArchivos(string rutaCarpeta)
        {
            Dictionary<string, List<string>> archivosAgrupados = new Dictionary<string, List<string>>();
            string[] archivos = Directory.GetFiles(rutaCarpeta, "*.txt");

            foreach (string archivoEncontrado in archivos)
            {
                string contenido = File.ReadAllText(archivoEncontrado);

                // Obtener el nombre del archivo sin extensión
                string nombreArchivo = Path.GetFileNameWithoutExtension(archivoEncontrado);

                // Dividir el nombre del archivo en partes usando "-"
                string[] partesNombre = nombreArchivo.Split('-');

                if (partesNombre.Length >= 2)
                {
                    string parteCentral = partesNombre[1]; // Obtener la parte central

                    if (archivosAgrupados.ContainsKey(parteCentral))
                    {
                        archivosAgrupados[parteCentral].Add(contenido); // Agregar contenido a la lista existente
                    }
                    else
                    {
                        archivosAgrupados[parteCentral] = new List<string> { contenido }; // Crear una nueva lista
                    }
                }
            }

            return archivosAgrupados;
        }



        private void buscartasbtn_Click(object sender, EventArgs e)
        {
            string dpi = code.Encode(buscartastxt.Text);
            Person persona;
            if (buscartastxt.Text == "") MessageBox.Show("Ingrese un nombre a buscar");
            else
            {
                persona = arbol.SearchDpi(dpi);
                if (persona != null)
                {
                    
                    List<string> lista = persona.letters;
                    if (lista!=null)
                    {
                        List<Dictionary<string, int>> dictio = persona.dictio;
                        string total = persona.name + "\n\n";
                        for (int i = 0; i < lista.Count(); i++)
                        {
                            total += comp.DECOMPRESS(lista[i], dictio[i]) + "\n\n";
                        }
                        MessageBox.Show(total);
                    }
                    else MessageBox.Show("No se encontraron datos asociados al DPI: " + dpitxt.Text);
                }

               
            }
            buscartastxt.Text = "";
        }


        private void button2_Click(object sender, EventArgs e)
        {
           
        }

        private void buscartastxt_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnCartas_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            rutaCarpetaCartas = ObtenerRutaCarpeta();
            if (rutaCarpetaCartas != null)
            {
                REC = ObtenerArchivos(rutaCarpetaCartas);
                MergeLetterandUser();
                ops++;
            }
            if (ops >= 3)
            {
                edTabControl.Enabled = true;
            }
            stopwatch.Stop();
            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            showmslbl.Text = $"{elapsedMilliseconds}ms";
            btnCartas.Enabled = false;
            btnCartas.Text = "Cartas cargado satisfactoriamente";
        }

        private void btnConversaciones_Click(object sender, EventArgs e)
        {

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            rutaCarpetaCartas = ObtenerRutaCarpeta();
            if (rutaCarpetaCartas != null)
            {
                CONV = ObtenerArchivos(rutaCarpetaCartas);
                ops++;
            }
            if (ops >= 3)
            {
                edTabControl.Enabled = true;
            }
            stopwatch.Stop();
            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            showmslbl.Text = $"{elapsedMilliseconds}ms";
            btnConversaciones.Enabled = false;
            btnConversaciones.Text = "Conversaciones cargadas satisfactoriamente";
        }
    }
}
