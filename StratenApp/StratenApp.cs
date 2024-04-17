using Domain.Library;
using System.Diagnostics;

namespace StratenApp
{
    internal class StratenApp
    {
        static void Main(string[] args)
        {
            string path = @"C:\Users\u2389\Downloads\StratenFinal\StratenFinal\StratenApp\extract\";
            List<string> fileNames =
            [
                "ProvincieIDsVlaanderen.csv", // 0
                "ProvincieInfo.csv", // 1
                "Gemeentenaam.csv", // 2
                "StraatnaamID_gemeenteID.csv",// 3
                "straatnamen.csv" // 4
            ];

            FileProcessor fileProcessor = new(path, fileNames);
            fileProcessor.Unzip(Path.Combine(Directory.GetCurrentDirectory(), "straatnamenInfo.zip"));

            {
                Stopwatch timer = new();
                timer.Start();

                //Bestanden inlezen
                fileProcessor.ReadAllFiles();

                //Mappenstructuur aanmaken
                fileProcessor.CreateFolders("Provincies");
                Console.WriteLine("Mappenstructuur succesvol aangemaakt!");

                timer.Stop();
                Console.WriteLine($"Duur: {timer.Elapsed.TotalMinutes:F0} minuten {timer.Elapsed.TotalSeconds:F0} seconden");
            }
        }
    }
}