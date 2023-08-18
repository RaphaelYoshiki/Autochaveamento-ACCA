// See https://aka.ms/new-console-template for more information
using System;
using System.IO;
using System.Collections.Generic;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using OfficeOpenXml;

namespace AutochaveamentoACCA
{
    public class Atleta
    {
        public string nome;
        public int idade;
        public string festival;
        public string senior;
        public char gen;
        public string faixa;
        public string agrem;
        public string categ;

        public Atleta (
            string nome, 
            int idade, 
            string festival, 
            string senior, 
            char gen, 
            string faixa, 
            string agrem, 
            string categ
            )
        {
            this.nome = nome;
            this.idade = idade;
            this.festival = festival;
            this.senior = senior;
            this.gen = gen;
            this.faixa = faixa;
            this.agrem = agrem;
            this.categ = categ;
        }
    }

    class Program
    {
        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };

        static readonly string ApplicationName = "Autochaveamento";

        static readonly string SpreadsheetID = "1sqYEUa3gYFn5Bo2lbp9t83mDFv5BlfPEt-8WdP_hvjY";

        static readonly string sheet = "Atletas";

        static SheetsService? service;

        //Criar lista de atletas
        public static List<Atleta> listAtletas;

        static void Main(string[] args)
        {
            GoogleCredential credential;
            using (var stream = new FileStream("autochaveamento-acca-20e19d3cb230.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }

            service = new SheetsService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            listAtletas = new List<Atleta>();
            ReadEntries();
            Classificar();

            (List<Atleta> teste, int freePass) = Chaveador.GenerateBracket(@"Listas de Pesagem\Masculino\Sênior\Meio_Médio.csv");
            foreach (var atleta in teste)
            {
                Console.WriteLine("Nome: " + atleta.nome + " Faixa: " + atleta.faixa + " Agrem: " + atleta.agrem);
            }
            GenerateImage.GenerateBracketImage(teste, freePass, "bracket_image.png");
        }

        static void ReadEntries()
        {
            var range = "B2:I200";
            var request = service.Spreadsheets.Values.Get(SpreadsheetID, range);

            var response = request.Execute();
            var values = response.Values;
            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    if (row.Count >= 8)
                    {
                        //Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}", row[0], row[1], row[2], row[3], row[4], row[5], row[6], row[7]);
                        string nome = row[0].ToString();  // Cast to string
                        int idade;
                        string festival = row[2].ToString();  // Cast to string
                        string senior = row[7].ToString();  // Cast to string
                        char gen = Convert.ToChar(row[3]);  // Cast to char
                        string faixa = row[4].ToString();  // Cast to string
                        string agrem = row[5].ToString();  // Cast to string
                        string categ = row[6].ToString();  // Cast to string

                        //Cálculo de idade
                        if (int.TryParse(row[1].ToString(), out int anoNasc))
                        {
                            int anoAtual = DateTime.Now.Year;
                            idade = anoAtual - anoNasc;
                            listAtletas.Add(new Atleta(nome, idade, festival, senior, gen, faixa, agrem, categ));
                        }
                        else
                        {
                            Console.WriteLine("Invalid birth year format: " + row[1]);
                        }
                        //listAtletas.Add(new Atleta(nome, idade, festival, senior, gen, faixa, agrem, categ));
                    }
                    else
                    {
                        Console.WriteLine("Coluna inválida da tabela: {0}", string.Join(", ", row));
                    }
                }
            }
            else
            {
                Console.WriteLine("No data found.");
            }
        }

        //Divisão entre classes
        static void Classificar()
        {
            //Festival
            List<Atleta> festival = FiltrarFestival(listAtletas);

            //Dividir por gênero
            List<Atleta> masculinos = FiltrarPorGenero(listAtletas, 'M');
            List<Atleta> femininos = FiltrarPorGenero(listAtletas, 'F');

            //Dividir atletas sênior
            List<Atleta> senior_M = FiltrarSenior(masculinos);
            List<Atleta> senior_F = FiltrarSenior(femininos);

            //Dividir ainda mais as listas com base na idade
            //M
            List<Atleta> sub13_M = FiltrarPorIdade(masculinos, 11, 12);
            List<Atleta> sub15_M = FiltrarPorIdade(masculinos, 13, 14);
            List<Atleta> sub18_M = FiltrarPorIdade(masculinos, 15, 17);
            List<Atleta> sub21_M = FiltrarPorIdade(masculinos, 18, 20);
            //F
            List<Atleta> sub13_F = FiltrarPorIdade(femininos, 11, 12);
            List<Atleta> sub15_F = FiltrarPorIdade(femininos, 13, 14);
            List<Atleta> sub18_F = FiltrarPorIdade(femininos, 15, 17);
            List<Atleta> sub21_F = FiltrarPorIdade(femininos, 18, 20);

            // Agora, dividir ainda mais as listas com base nas categorias
            //Sub 13
            //M
            List<Atleta> sub13_SuperLigeiro_M = FiltrarPorCategoria(sub13_M, "Super Ligeiro");
            List<Atleta> sub13_Ligeiro_M = FiltrarPorCategoria(sub13_M, "Ligeiro");
            List<Atleta> sub13_MeioLeve_M = FiltrarPorCategoria(sub13_M, "Meio Leve");
            List<Atleta> sub13_Leve_M = FiltrarPorCategoria(sub13_M, "Leve");
            List<Atleta> sub13_MeioMedio_M = FiltrarPorCategoria(sub13_M, "Meio Médio");
            List<Atleta> sub13_Medio_M = FiltrarPorCategoria(sub13_M, "Médio");
            List<Atleta> sub13_MeioPesado_M = FiltrarPorCategoria(sub13_M, "Meio Pesado");
            List<Atleta> sub13_Pesado_M = FiltrarPorCategoria(sub13_M, "Pesado");
            List<Atleta> sub13_SuperPesado_M = FiltrarPorCategoria(sub13_M, "Super Pesado");
            //F
            List<Atleta> sub13_SuperLigeiro_F = FiltrarPorCategoria(sub13_F, "Super Ligeiro");
            List<Atleta> sub13_Ligeiro_F = FiltrarPorCategoria(sub13_F, "Ligeiro");
            List<Atleta> sub13_MeioLeve_F = FiltrarPorCategoria(sub13_F, "Meio Leve");
            List<Atleta> sub13_Leve_F = FiltrarPorCategoria(sub13_F, "Leve");
            List<Atleta> sub13_MeioMedio_F = FiltrarPorCategoria(sub13_F, "Meio Médio");
            List<Atleta> sub13_Medio_F = FiltrarPorCategoria(sub13_F, "Médio");
            List<Atleta> sub13_MeioPesado_F = FiltrarPorCategoria(sub13_F, "Meio Pesado");
            List<Atleta> sub13_Pesado_F = FiltrarPorCategoria(sub13_F, "Pesado");
            List<Atleta> sub13_SuperPesado_F = FiltrarPorCategoria(sub13_F, "Super Pesado");

            //Sub 15
            //M
            List<Atleta> sub15_SuperLigeiro_M = FiltrarPorCategoria(sub15_M, "Super Ligeiro");
            List<Atleta> sub15_Ligeiro_M = FiltrarPorCategoria(sub15_M, "Ligeiro");
            List<Atleta> sub15_MeioLeve_M = FiltrarPorCategoria(sub15_M, "Meio Leve");
            List<Atleta> sub15_Leve_M = FiltrarPorCategoria(sub15_M, "Leve");
            List<Atleta> sub15_MeioMedio_M = FiltrarPorCategoria(sub15_M, "Meio Médio");
            List<Atleta> sub15_Medio_M = FiltrarPorCategoria(sub15_M, "Médio");
            List<Atleta> sub15_MeioPesado_M = FiltrarPorCategoria(sub15_M, "Meio Pesado");
            List<Atleta> sub15_Pesado_M = FiltrarPorCategoria(sub15_M, "Pesado");
            List<Atleta> sub15_SuperPesado_M = FiltrarPorCategoria(sub15_M, "Super Pesado");
            //F
            List<Atleta> sub15_SuperLigeiro_F = FiltrarPorCategoria(sub15_F, "Super Ligeiro");
            List<Atleta> sub15_Ligeiro_F = FiltrarPorCategoria(sub15_F, "Ligeiro");
            List<Atleta> sub15_MeioLeve_F = FiltrarPorCategoria(sub15_F, "Meio Leve");
            List<Atleta> sub15_Leve_F = FiltrarPorCategoria(sub15_F, "Leve");
            List<Atleta> sub15_MeioMedio_F = FiltrarPorCategoria(sub15_F, "Meio Médio");
            List<Atleta> sub15_Medio_F = FiltrarPorCategoria(sub15_F, "Médio");
            List<Atleta> sub15_MeioPesado_F = FiltrarPorCategoria(sub15_F, "Meio Pesado");
            List<Atleta> sub15_Pesado_F = FiltrarPorCategoria(sub15_F, "Pesado");
            List<Atleta> sub15_SuperPesado_F = FiltrarPorCategoria(sub15_F, "Super Pesado");

            //Sub 18
            //M
            List<Atleta> sub18_SuperLigeiro_M = FiltrarPorCategoria(sub18_M, "Super Ligeiro");
            List<Atleta> sub18_Ligeiro_M = FiltrarPorCategoria(sub18_M, "Ligeiro");
            List<Atleta> sub18_MeioLeve_M = FiltrarPorCategoria(sub18_M, "Meio Leve");
            List<Atleta> sub18_Leve_M = FiltrarPorCategoria(sub18_M, "Leve");
            List<Atleta> sub18_MeioMedio_M = FiltrarPorCategoria(sub18_M, "Meio Médio");
            List<Atleta> sub18_Medio_M = FiltrarPorCategoria(sub18_M, "Médio");
            List<Atleta> sub18_MeioPesado_M = FiltrarPorCategoria(sub18_M, "Meio Pesado");
            List<Atleta> sub18_Pesado_M = FiltrarPorCategoria(sub18_M, "Pesado");
            List<Atleta> sub18_SuperPesado_M = FiltrarPorCategoria(sub18_M, "Super Pesado");
            //F
            List<Atleta> sub18_SuperLigeiro_F = FiltrarPorCategoria(sub18_F, "Super Ligeiro");
            List<Atleta> sub18_Ligeiro_F = FiltrarPorCategoria(sub18_F, "Ligeiro");
            List<Atleta> sub18_MeioLeve_F = FiltrarPorCategoria(sub18_F, "Meio Leve");
            List<Atleta> sub18_Leve_F = FiltrarPorCategoria(sub18_F, "Leve");
            List<Atleta> sub18_MeioMedio_F = FiltrarPorCategoria(sub18_F, "Meio Médio");
            List<Atleta> sub18_Medio_F = FiltrarPorCategoria(sub18_F, "Médio");
            List<Atleta> sub18_MeioPesado_F = FiltrarPorCategoria(sub18_F, "Meio Pesado");
            List<Atleta> sub18_Pesado_F = FiltrarPorCategoria(sub18_F, "Pesado");
            List<Atleta> sub18_SuperPesado_F = FiltrarPorCategoria(sub18_F, "Super Pesado");

            //Sub 21
            //M
            List<Atleta> sub21_SuperLigeiro_M = FiltrarPorCategoria(sub21_M, "Super Ligeiro");
            List<Atleta> sub21_Ligeiro_M = FiltrarPorCategoria(sub21_M, "Ligeiro");
            List<Atleta> sub21_MeioLeve_M = FiltrarPorCategoria(sub21_M, "Meio Leve");
            List<Atleta> sub21_Leve_M = FiltrarPorCategoria(sub21_M, "Leve");
            List<Atleta> sub21_MeioMedio_M = FiltrarPorCategoria(sub21_M, "Meio Médio");
            List<Atleta> sub21_Medio_M = FiltrarPorCategoria(sub21_M, "Médio");
            List<Atleta> sub21_MeioPesado_M = FiltrarPorCategoria(sub21_M, "Meio Pesado");
            List<Atleta> sub21_Pesado_M = FiltrarPorCategoria(sub21_M, "Pesado");
            List<Atleta> sub21_SuperPesado_M = FiltrarPorCategoria(sub21_M, "Super Pesado");
            //F
            List<Atleta> sub21_SuperLigeiro_F = FiltrarPorCategoria(sub21_F, "Super Ligeiro");
            List<Atleta> sub21_Ligeiro_F = FiltrarPorCategoria(sub21_F, "Ligeiro");
            List<Atleta> sub21_MeioLeve_F = FiltrarPorCategoria(sub21_F, "Meio Leve");
            List<Atleta> sub21_Leve_F = FiltrarPorCategoria(sub21_F, "Leve");
            List<Atleta> sub21_MeioMedio_F = FiltrarPorCategoria(sub21_F, "Meio Médio");
            List<Atleta> sub21_Medio_F = FiltrarPorCategoria(sub21_F, "Médio");
            List<Atleta> sub21_MeioPesado_F = FiltrarPorCategoria(sub21_F, "Meio Pesado");
            List<Atleta> sub21_Pesado_F = FiltrarPorCategoria(sub21_F, "Pesado");
            List<Atleta> sub21_SuperPesado_F = FiltrarPorCategoria(sub21_F, "Super Pesado");

            //Sênior
            //M
            List<Atleta> senior_SuperLigeiro_M = FiltrarPorCategoria(senior_M, "Super Ligeiro");
            List<Atleta> senior_Ligeiro_M = FiltrarPorCategoria(senior_M, "Ligeiro");
            List<Atleta> senior_MeioLeve_M = FiltrarPorCategoria(senior_M, "Meio Leve");
            List<Atleta> senior_Leve_M = FiltrarPorCategoria(senior_M, "Leve");
            List<Atleta> senior_MeioMedio_M = FiltrarPorCategoria(senior_M, "Meio Médio");
            List<Atleta> senior_Medio_M = FiltrarPorCategoria(senior_M, "Médio");
            List<Atleta> senior_MeioPesado_M = FiltrarPorCategoria(senior_M, "Meio Pesado");
            List<Atleta> senior_Pesado_M = FiltrarPorCategoria(senior_M, "Pesado");
            List<Atleta> senior_SuperPesado_M = FiltrarPorCategoria(senior_M, "Super Pesado");
            //F
            List<Atleta> senior_SuperLigeiro_F = FiltrarPorCategoria(senior_F, "Super Ligeiro");
            List<Atleta> senior_Ligeiro_F = FiltrarPorCategoria(senior_F, "Ligeiro");
            List<Atleta> senior_MeioLeve_F = FiltrarPorCategoria(senior_F, "Meio Leve");
            List<Atleta> senior_Leve_F = FiltrarPorCategoria(senior_F, "Leve");
            List<Atleta> senior_MeioMedio_F = FiltrarPorCategoria(senior_F, "Meio Médio");
            List<Atleta> senior_Medio_F = FiltrarPorCategoria(senior_F, "Médio");
            List<Atleta> senior_MeioPesado_F = FiltrarPorCategoria(senior_F, "Meio Pesado");
            List<Atleta> senior_Pesado_F = FiltrarPorCategoria(senior_F, "Pesado");
            List<Atleta> senior_SuperPesado_F = FiltrarPorCategoria(senior_F, "Super Pesado");

            //Exportar listas de pesagem
            //Festival
            ExportToCSV(festival, "Festival", "Festival", 'A');
            //Sub 13
            ExportToCSV(sub13_SuperLigeiro_M, "Sub_13", "Super_Ligeiro", 'M');
            ExportToCSV(sub13_Ligeiro_M, "Sub_13", "Ligeiro", 'M');
            ExportToCSV(sub13_MeioLeve_M, "Sub_13", "Meio_Leve", 'M');
            ExportToCSV(sub13_Leve_M, "Sub_13", "Leve", 'M');
            ExportToCSV(sub13_MeioMedio_M, "Sub_13", "Meio_Médio", 'M');
            ExportToCSV(sub13_Medio_M, "Sub_13", "Médio", 'M');
            ExportToCSV(sub13_MeioPesado_M, "Sub_13", "Meio_Pesado", 'M');
            ExportToCSV(sub13_Pesado_M, "Sub_13", "Pesado", 'M');
            ExportToCSV(sub13_SuperPesado_M, "Sub_13", "Super_Pesado", 'M');
            ExportToCSV(sub13_SuperLigeiro_F, "Sub_13", "Super_Ligeiro", 'F');
            ExportToCSV(sub13_Ligeiro_F, "Sub_13", "Ligeiro", 'F');
            ExportToCSV(sub13_MeioLeve_F, "Sub_13", "Meio_Leve", 'F');
            ExportToCSV(sub13_Leve_F, "Sub_13", "Leve", 'F');
            ExportToCSV(sub13_MeioMedio_F, "Sub_13", "Meio_Médio", 'F');
            ExportToCSV(sub13_Medio_F, "Sub_13", "Médio", 'F');
            ExportToCSV(sub13_MeioPesado_F, "Sub_13", "Meio_Pesado", 'F');
            ExportToCSV(sub13_Pesado_F, "Sub_13", "Pesado", 'F');
            ExportToCSV(sub13_SuperPesado_F, "Sub_13", "Super_Pesado", 'F');
            //Sub 15
            ExportToCSV(sub15_SuperLigeiro_M, "Sub_15", "Super_Ligeiro", 'M');
            ExportToCSV(sub15_Ligeiro_M, "Sub_15", "Ligeiro", 'M');
            ExportToCSV(sub15_MeioLeve_M, "Sub_15", "Meio_Leve", 'M');
            ExportToCSV(sub15_Leve_M, "Sub_15", "Leve", 'M');
            ExportToCSV(sub15_MeioMedio_M, "Sub_15", "Meio_Médio", 'M');
            ExportToCSV(sub15_Medio_M, "Sub_15", "Médio", 'M');
            ExportToCSV(sub15_MeioPesado_M, "Sub_15", "Meio_Pesado", 'M');
            ExportToCSV(sub15_Pesado_M, "Sub_15", "Pesado", 'M');
            ExportToCSV(sub15_SuperPesado_M, "Sub_15", "Super_Pesado", 'M');
            ExportToCSV(sub15_SuperLigeiro_F, "Sub_15", "Super_Ligeiro", 'F');
            ExportToCSV(sub15_Ligeiro_F, "Sub_15", "Ligeiro", 'F');
            ExportToCSV(sub15_MeioLeve_F, "Sub_15", "Meio_Leve", 'F');
            ExportToCSV(sub15_Leve_F, "Sub_15", "Leve", 'F');
            ExportToCSV(sub15_MeioMedio_F, "Sub_15", "Meio_Médio", 'F');
            ExportToCSV(sub15_Medio_F, "Sub_15", "Médio", 'F');
            ExportToCSV(sub15_MeioPesado_F, "Sub_15", "Meio_Pesado", 'F');
            ExportToCSV(sub15_Pesado_F, "Sub_15", "Pesado", 'F');
            ExportToCSV(sub15_SuperPesado_F, "Sub_15", "Super_Pesado", 'F');
            //Sub18
            ExportToCSV(sub18_SuperLigeiro_M, "Sub_18", "Super_Ligeiro", 'M');
            ExportToCSV(sub18_Ligeiro_M, "Sub_18", "Ligeiro", 'M');
            ExportToCSV(sub18_MeioLeve_M, "Sub_18", "Meio_Leve", 'M');
            ExportToCSV(sub18_Leve_M, "Sub_18", "Leve", 'M');
            ExportToCSV(sub18_MeioMedio_M, "Sub_18", "Meio_Médio", 'M');
            ExportToCSV(sub18_Medio_M, "Sub_18", "Médio", 'M');
            ExportToCSV(sub18_MeioPesado_M, "Sub_18", "Meio_Pesado", 'M');
            ExportToCSV(sub18_Pesado_M, "Sub_18", "Pesado", 'M');
            ExportToCSV(sub18_SuperPesado_M, "Sub_18", "Super_Pesado", 'M');
            ExportToCSV(sub18_SuperLigeiro_F, "Sub_18", "Super_Ligeiro", 'F');
            ExportToCSV(sub18_Ligeiro_F, "Sub_18", "Ligeiro", 'F');
            ExportToCSV(sub18_MeioLeve_F, "Sub_18", "Meio_Leve", 'F');
            ExportToCSV(sub18_Leve_F, "Sub_18", "Leve", 'F');
            ExportToCSV(sub18_MeioMedio_F, "Sub_18", "Meio_Médio", 'F');
            ExportToCSV(sub18_Medio_F, "Sub_18", "Médio", 'F');
            ExportToCSV(sub18_MeioPesado_F, "Sub_18", "Meio_Pesado", 'F');
            ExportToCSV(sub18_Pesado_F, "Sub_18", "Pesado", 'F');
            ExportToCSV(sub18_SuperPesado_F, "Sub_18", "Super_Pesado", 'F');
            //Sub 21
            ExportToCSV(sub21_SuperLigeiro_M, "Sub_21", "Super_Ligeiro", 'M');
            ExportToCSV(sub21_Ligeiro_M, "Sub_21", "Ligeiro", 'M');
            ExportToCSV(sub21_MeioLeve_M, "Sub_21", "Meio_Leve", 'M');
            ExportToCSV(sub21_Leve_M, "Sub_21", "Leve", 'M');
            ExportToCSV(sub21_MeioMedio_M, "Sub_21", "Meio_Médio", 'M');
            ExportToCSV(sub21_Medio_M, "Sub_21", "Médio", 'M');
            ExportToCSV(sub21_MeioPesado_M, "Sub_21", "Meio_Pesado", 'M');
            ExportToCSV(sub21_Pesado_M, "Sub_21", "Pesado", 'M');
            ExportToCSV(sub21_SuperPesado_M, "Sub_21", "Super_Pesado", 'M');
            ExportToCSV(sub21_SuperLigeiro_F, "Sub_21", "Super_Ligeiro", 'F');
            ExportToCSV(sub21_Ligeiro_F, "Sub_21", "Ligeiro", 'F');
            ExportToCSV(sub21_MeioLeve_F, "Sub_21", "Meio_Leve", 'F');
            ExportToCSV(sub21_Leve_F, "Sub_21", "Leve", 'F');
            ExportToCSV(sub21_MeioMedio_F, "Sub_21", "Meio_Médio", 'F');
            ExportToCSV(sub21_Medio_F, "Sub_21", "Médio", 'F');
            ExportToCSV(sub21_MeioPesado_F, "Sub_21", "Meio_Pesado", 'F');
            ExportToCSV(sub21_Pesado_F, "Sub_21", "Pesado", 'F');
            ExportToCSV(sub21_SuperPesado_F, "Sub_21", "Super_Pesado", 'F');
            //Sênior
            ExportToCSV(senior_SuperLigeiro_M, "Sênior", "Super_Ligeiro", 'M');
            ExportToCSV(senior_Ligeiro_M, "Sênior", "Ligeiro", 'M');
            ExportToCSV(senior_MeioLeve_M, "Sênior", "Meio_Leve", 'M');
            ExportToCSV(senior_Leve_M, "Sênior", "Leve", 'M');
            //ExportToCSV(senior_MeioMedio_M, "Sênior", "Meio_Médio", 'M');
            ExportToCSV(senior_Medio_M, "Sênior", "Médio", 'M');
            ExportToCSV(senior_MeioPesado_M, "Sênior", "Meio_Pesado", 'M');
            ExportToCSV(senior_Pesado_M, "Sênior", "Pesado", 'M');
            ExportToCSV(senior_SuperPesado_M, "Sênior", "Super_Pesado", 'M');
            ExportToCSV(senior_SuperLigeiro_F, "Sênior", "Super_Ligeiro", 'F');
            ExportToCSV(senior_Ligeiro_F, "Sênior", "Ligeiro", 'F');
            ExportToCSV(senior_MeioLeve_F, "Sênior", "Meio_Leve", 'F');
            ExportToCSV(senior_Leve_F, "Sênior", "Leve", 'F');
            ExportToCSV(senior_MeioMedio_F, "Sênior", "Meio_Médio", 'F');
            ExportToCSV(senior_Medio_F, "Sênior", "Médio", 'F');
            ExportToCSV(senior_MeioPesado_F, "Sênior", "Meio_Pesado", 'F');
            ExportToCSV(senior_Pesado_F, "Sênior", "Pesado", 'F');
            ExportToCSV(senior_SuperPesado_F, "Sênior", "Super_Pesado", 'F');
        }

        static void ExportToCSV(List<Atleta> list, string category, string classe, char gender)
        {
            //Criação de pastas para armazenar listas de pesagem
            Directory.CreateDirectory("Listas de Pesagem");
            Directory.CreateDirectory(@"Listas de Pesagem\Festival");
            Directory.CreateDirectory(@"Listas de Pesagem\Masculino");
            Directory.CreateDirectory(@"Listas de Pesagem\Masculino\Sub_13");
            Directory.CreateDirectory(@"Listas de Pesagem\Masculino\Sub_15");
            Directory.CreateDirectory(@"Listas de Pesagem\Masculino\Sub_18");
            Directory.CreateDirectory(@"Listas de Pesagem\Masculino\Sub_21");
            Directory.CreateDirectory(@"Listas de Pesagem\Masculino\Sênior");
            Directory.CreateDirectory(@"Listas de Pesagem\Feminino");
            Directory.CreateDirectory(@"Listas de Pesagem\Feminino\Sub_13");
            Directory.CreateDirectory(@"Listas de Pesagem\Feminino\Sub_15");
            Directory.CreateDirectory(@"Listas de Pesagem\Feminino\Sub_18");
            Directory.CreateDirectory(@"Listas de Pesagem\Feminino\Sub_21");
            Directory.CreateDirectory(@"Listas de Pesagem\Feminino\Sênior");

            var fileName = $"{classe}.csv";
            var filePath = "";
            if (category == "Festival")
            {
                filePath = @"Listas de Pesagem\Festival";
            }
            else
            {
                switch (gender) 
                {
                    case 'M':
                        switch (category)
                        {
                            case "Sub_13":
                                filePath = @"Listas de Pesagem\Masculino\Sub_13";
                                break;
                            case "Sub_15":
                                filePath = @"Listas de Pesagem\Masculino\Sub_15";
                                break;
                            case "Sub_18":
                                filePath = @"Listas de Pesagem\Masculino\Sub_18";
                                break;
                            case "Sub_21":
                                filePath = @"Listas de Pesagem\Masculino\Sub_21";
                                break;
                            case "Sênior":
                                filePath = @"Listas de Pesagem\Masculino\Sênior";
                                break;
                        }
                        break;
                    case 'F':
                        switch (category)
                        {
                            case "Sub_13":
                                filePath = @"Listas de Pesagem\Feminino\Sub_13";
                                break;
                            case "Sub_15":
                                filePath = @"Listas de Pesagem\Feminino\Sub_15";
                                break;
                            case "Sub_18":
                                filePath = @"Listas de Pesagem\Feminino\Sub_18";
                                break;
                            case "Sub_21":
                                filePath = @"Listas de Pesagem\Feminino\Sub_21";
                                break;
                            case "Sênior":
                                filePath = @"Listas de Pesagem\Feminino\Sênior";
                                break;
                        }
                    break;
                }
            }

            filePath = Path.Combine(filePath, fileName);

            var existingAthleteNames = new HashSet<string>();

            // Check if the file already exists, and if so, read the existing athlete names
            if (File.Exists(filePath))
            {
                using (var reader = new StreamReader(filePath))
                {
                    // Skip the header row
                    reader.ReadLine();

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');
                        var athleteName = values[0]; // Assuming the name is in the first column
                        existingAthleteNames.Add(athleteName);
                    }
                }
            }

            using (var writer = new StreamWriter(filePath, append: true))
            {
                // Write the header row
                if (new FileInfo(filePath).Length == 0)
                {
                    writer.WriteLine("Nome,Idade,Gênero,Faixa,Agremiação,Categoria");
                }

                // Write the data rows
                foreach (var atleta in list)
                {
                    // Replace any commas in the name with a different character to avoid issues with CSV format
                    var nome = atleta.nome.Replace(',', '-');

                    // Join the values with commas to form the CSV line
                    var csvLine = $"{nome},{atleta.idade},{atleta.gen},{atleta.faixa},{atleta.agrem},{atleta.categ}";

                    if (!existingAthleteNames.Contains(nome))
                    {
                        writer.WriteLine(csvLine);
                        existingAthleteNames.Add(nome); // Add the name to the existing names set
                    }
                }
            }
        }

        //Filtrar por gênero
        static List<Atleta> FiltrarPorGenero(List<Atleta> atletas, char genero)
        {
            return atletas.FindAll(atleta => atleta.gen == genero);
        }

        //Filtrar por idade
        static List<Atleta> FiltrarPorIdade(List<Atleta> atletas, int idadeMinima, int idadeMaxima = int.MaxValue)
        {
            return atletas.FindAll(atleta => atleta.idade >= idadeMinima && atleta.idade <= idadeMaxima);
        }

        //Filtrar atletas do festival
        static List<Atleta> FiltrarFestival(List<Atleta> atletas)
        {
            return atletas.FindAll(atleta => atleta.festival == "Sim" && atleta.idade <= 10);
        }

        //Filtrar atletas do sênior
        static List<Atleta> FiltrarSenior(List<Atleta> atletas)
        {
            return atletas.FindAll(atleta => (atleta.senior == "Sim" 
                                    || atleta.senior == "APENAS Sênior") 
                                    && atleta.idade >= 15);
        }

        //Filtrar por categoria
        static List<Atleta> FiltrarPorCategoria(List<Atleta> atletas, string categoria)
        {
            return atletas.FindAll(atleta => atleta.categ.Trim() == categoria.Trim());
        }
    }
}