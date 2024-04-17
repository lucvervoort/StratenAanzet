using StratenApp.Models;
using System.IO.Compression;

namespace Domain.Library
{
    public class FileProcessor
    {
        #region Hulpconstantes
        //indexes aan een constante koppelen om gaan magic numbers te hebben
        const int provincieIDsVlaanderen_Bestand = 0;
        const int provincieInfo_Bestand = 1;
        const int gemeentenaam_Bestand = 2;
        const int straatnaamID_gemeenteID_Bestand = 3;
        const int straatnamen_Bestand = 4;
        #endregion

        #region Properties
        public string? DirectoryPath { get; set; }
        public List<string> FileNames { get; set; }

        //provincienaam   //gemeentenaam     //straatnamen
        private readonly Dictionary<string, Dictionary<string, SortedSet<string>>> _data = [];
        #endregion

        #region Ctor
        public FileProcessor(string path, List<string> fileNames)
        {
            DirectoryPath = path;
            FileNames = fileNames;
        }
        #endregion

        #region Methods

        public void ReadAllFiles()
        {
            // STAP 1 - Bestand ProvincieIDsVlaanderen.csv inlezen
            #region stap1
            var idsVlaanderen = new List<int>();
            using (var reader = new StreamReader(DirectoryPath + FileNames[provincieIDsVlaanderen_Bestand]))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] values = line.Split(',');

                    foreach (var v in values)
                    {
                        idsVlaanderen.Add(int.Parse(v));
                    }
                }
            }
            #endregion

            // STAP 2 - Bestand ProvincieInfo.csv inlezen
            #region stap2
            var vlaamseProvincieInfoLijst = new List<GemeenteProvincieInfo>();
            using (var reader = new StreamReader(DirectoryPath + FileNames[provincieInfo_Bestand]))
            {
                //Eerste lijn niet nodig, dus "over stappen"
                string? header = reader.ReadLine();

                //Alle volgende lijnen 1 per 1 lezen, zolang ze gevuld zijn
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] values = line.Split(';');

                    //Als de gemeente Nederlandstalig is, voeg toe aan lijst met vlaamseProvincieInfo
                    if (values[2] == "nl")
                    {
                        GemeenteProvincieInfo info = new GemeenteProvincieInfo
                            (
                                int.Parse(values[0]), //gemeente id
                                int.Parse(values[1]), //provincie id
                                values[2], //taalcode
                                values[3] //provincie
                            );
                        vlaamseProvincieInfoLijst.Add(info);
                    }
                }

            }
            #endregion

            // STAP 3 - Bestand Gemeentenaam.csv inlezen
            #region stap3
            // Maak klasse Gemeente
            #endregion

            // STAP 4 - Bestand StraatnaamID_gemeenteID.csv inlezen
            #region stap4
            // Maak klasse StraatnaamID_GemeenteID
            #endregion

            // Latere les...
            #region Informatie in dictionaries steken = snellere verwerking 
            #endregion

            // STAP 5 Bestand Straten.csv inlezen + _data opvullen
            #region Stap 5
            #endregion
        }

        //Mappenstructuur aanmaken
        public void CreateFolders(string name)
        {
            // Vereist _data
        }

        //Recursief mappen verwijderen
        private static void ClearFolder(string v)
        {
            if (Directory.Exists(v))
            {
                // NYI
            }
        }

        public void Unzip(string fileName)
        {
            if (!string.IsNullOrEmpty(DirectoryPath))
            {
                ClearFolder(DirectoryPath);
                if (File.Exists(fileName))
                    ZipFile.ExtractToDirectory(fileName, DirectoryPath);
                else
                    throw new Exception("File not found: \"" + fileName + "\"");
            }
        }
        #endregion
    }

}
