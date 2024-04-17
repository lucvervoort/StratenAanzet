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
        public string? DirectoryPath { get; set; }
        public List<string> FileNames { get; set; }

        //provincienaam   //gemeentenaam     //straatnamen
        private readonly Dictionary<string, Dictionary<string, SortedSet<string>>> _data = [];

        public FileProcessor(string path, List<string> fileNames)
        {
            DirectoryPath = path;
            FileNames = fileNames;
        }

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

            // STAP 2 - Bestand ProvinvieInfo.csv inlezen
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
            var alleGemeentes = new List<Gemeente>();
            using (var reader = new StreamReader(DirectoryPath + FileNames[gemeentenaam_Bestand]))
            {
                //Eerste lijn niet nodig, dus "over stappen"
                string? header = reader.ReadLine();

                //Alle volgende lijnen 1 per 1 lezen, zolang ze gevuld zijn
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] values = line.Split(';');

                    //Als de gemeente Nederlandstalig is, voeg toe aan lijst met gemeentes
                    //if (values[2] == "nl")
                    //{
                        Gemeente g = new Gemeente
                        (
                            int.Parse(values[0]),
                            int.Parse(values[1]), //id
                            values[2],
                            values[3] //naam
                        );
                    alleGemeentes.Add(g);
                    //}
                }
            }
            #endregion

            // STAP 4 - Bestand StraatnaamID_gemeenteID.csv inlezen
            #region stap4
            var straatnaamID_GemeenteID = new List<StraatnaamID_GemeenteID>();
            using (var reader = new StreamReader(DirectoryPath + FileNames[straatnaamID_gemeenteID_Bestand]))
            {
                //Eerste lijn niet nodig, dus "over stappen"
                string? header = reader.ReadLine();

                //Alle volgende lijnen 1 per 1 lezen, zolang ze gevuld zijn
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] values = line.Split(';');

                    //Als de eerste waarde zinloos is, niet opslaan
                    if (int.TryParse(values[0], out int i) && i > 0)
                    {
                        StraatnaamID_GemeenteID s = new StraatnaamID_GemeenteID
                            (
                              int.Parse(values[0]),
                              int.Parse(values[1])
                            );

                        straatnaamID_GemeenteID.Add(s);
                    }
                }

            }
            #endregion

            #region Informatie in dictionaries steken = snellere verwerking 
            //Informatie converteren naar dictionaries om veel sneller de uiteindelijke dictionary (_data) op te vullen
            var vlaamseProvincieGemeentes = vlaamseProvincieInfoLijst
                .Where(provincieInfo => idsVlaanderen.Contains(provincieInfo.ProvincieID))
                .GroupBy(key => key.ProvincieNaam)
                .ToDictionary(
                               group => group.Key,
                               group => group.Select( x => x.GemeenteID ).ToList() 
                );

            var vlaamseGemeentes = alleGemeentes
                .Where(gemeente => gemeente.TaalCode == "nl")
                .Where(gemeente => vlaamseProvincieGemeentes.Values.Any(lijst => lijst.Contains(gemeente.GemeenteId)))
                .ToDictionary(gemeente => gemeente.GemeenteId, gemeente => gemeente.GemeenteNaam);

            var gemeenteStratenIds = straatnaamID_GemeenteID
                .GroupBy(key => key.GemeenteID)
                .ToDictionary(
                               group => group.Key,
                               group => group.Select(x => x.StraatnaamID).ToList()
                              );

            var vlaamseGemeentesStratenIds = gemeenteStratenIds
                .Where(x => vlaamseGemeentes.Keys.Contains(x.Key))
                .ToDictionary(x => x.Key, x => x.Value);
            #endregion

            // STAP 5 Bestand Straten.csv inlezen + _data opvullen
            #region Stap 5

            //Straatnamen.csv inlezen en verwerken
            using (var reader = new StreamReader(DirectoryPath + FileNames[straatnamen_Bestand]))
            {
                //Eerste lijn niet nodig, dus "over stappen"
                string? header = reader.ReadLine();

                //Alle volgende lijnen 1 per 1 lezen, zolang ze gevuld zijn
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    const int zinlozeWaarde = 0;
                    string[] values = line.Split(';');
                    //Als de eerste waarde niet kan geparsed worden naar een int of die int is niet groter dan nul, krijgt het de "zinloze waarde"
                    var straatnaamId = int.TryParse(values[0], out int ingelezenWaarde) && ingelezenWaarde > zinlozeWaarde ? ingelezenWaarde : zinlozeWaarde ;
                    var straatnaam = values[1];

                    //Als de eerste waarde zinloos is, niet opslaan
                    if (straatnaamId > zinlozeWaarde)
                    {
                        Straatnaam straat;
                        NieuweData nieuweData;

                        //Als de straatnaamId voorkomt in de dict met gekende VLAAMSE straten
                        if (vlaamseGemeentesStratenIds.Values.Any(stratenIds => stratenIds.Contains(straatnaamId)))
                        {
                            straat = new Straatnaam(straatnaamId, straatnaam);
                            int gemeenteID = 0;
                            string? gemeente = null;
                            string? provincie = null;
                            try
                            {
                                gemeenteID = vlaamseGemeentesStratenIds.First(x => x.Value.Contains(straatnaamId)).Key;
                                gemeente = vlaamseGemeentes.First(x => x.Key == gemeenteID).Value;
                                provincie = vlaamseProvincieGemeentes.First(x => x.Value.Contains(gemeenteID)).Key;
                            }
                            catch (Exception ex)
                            {
                                //zodat het zeker zou opvallen moest er iets mislopen
                                Console.WriteLine("er liep iets mis bij try catch lijn 213");
                                System.Threading.Thread.Sleep(200000);
                                continue;
                            }

                            nieuweData = new()
                            {
                                Straatnaam = straat.Naam,
                                Gemeente = gemeente,
                                Provincie = provincie,
                            };

                            if (_data.ContainsKey(nieuweData.Provincie))
                            {
                                // Indien de gemeente al bestaat als key in _data, voeg de straat toe
                                if (_data[nieuweData.Provincie].ContainsKey(nieuweData.Gemeente))
                                {
                                    _data[nieuweData.Provincie][nieuweData.Gemeente].Add(nieuweData.Straatnaam.Trim());
                                }
                                //Indien de gemeente nog niet bestaat, voeg gemeente en straat toe
                                else
                                {
                                    SortedSet<string> straatnamenSet = new();
                                    straatnamenSet.Add(nieuweData.Straatnaam.Trim());

                                    _data[nieuweData.Provincie].Add(nieuweData.Gemeente, straatnamenSet);
                                }
                               
                            }
                            // Indien de provincie nog niet bestaat in _data, voeg alle nieuweData toe:
                            else
                            {
                                SortedSet<string> nieuweStratenSet = new();
                                nieuweStratenSet.Add(nieuweData.Straatnaam.Trim());

                                Dictionary<string, SortedSet<string>> nieuweGemeenteMetNieuweStraatSet = new();
                                nieuweGemeenteMetNieuweStraatSet.Add(nieuweData.Gemeente, nieuweStratenSet);

                                _data.Add(nieuweData.Provincie, nieuweGemeenteMetNieuweStraatSet);
                            }
                            //Om toch iets te zien gebeuren indien nodig:
                            //Console.WriteLine($"{nieuweData.Straatnaam.Trim()} in {nieuweData.Gemeente} in {nieuweData.Provincie} toegevoegd...");
                        }
                        else continue;

                    }
                    else continue;
                }
            }
            #endregion
        }

        //Mappenstructuur aanmaken
        public void CreateFolders(string name)
        {
            //map "Provincies" aanmaken
            Directory.CreateDirectory(DirectoryPath + name);

            //Subdirectories aanmaken
            for (int i = 0; i < _data.Count; i++)
            {
                //voor elke provincie een map maken
                List<string> provincies = _data.Keys.ToList();
                foreach (string provincie in provincies)
                {
                    //Indien Brussel er wel bij moet, deze if er uithalen
                    if (provincie == "BrusselHoofdstedelijkGewest")
                    {
                        continue;
                    }

                    Directory.CreateDirectory(DirectoryPath + name + "\\" + provincie);

                    //voor elke gemeente in die provincie een map aanmaken
                    List<string> gemeentes = _data[provincie].Keys.ToList();
                    foreach (string gemeente in gemeentes)
                    {
                        Directory.CreateDirectory(DirectoryPath + name + "\\" + provincie + "\\" + gemeente);

                        //alle straten in die gemeente in een txt zetten
                        string padVoorFile = DirectoryPath + name + "\\" + provincie + "\\" + gemeente + "\\"
                            + $"Straten_in_{gemeente}.txt";

                        List<string> stratenInGemeente = _data[provincie][gemeente].ToList();

                        File.AppendAllLines(padVoorFile, stratenInGemeente);
                    }
                }
            }
        }

        //Recursief mappen verwijderen
        private static void ClearFolder(string v)
        {
            if (Directory.Exists(v))
            {
                // Een folder/directory kan enkel verwijderd worden indien deze geen bestanden meer bevat
                DirectoryInfo di = new(v);
                foreach (FileInfo fi in di.GetFiles())
                {
                    //Console.WriteLine($"Deleting {fi.FullName}");
                    fi.Delete();
                }
                foreach (DirectoryInfo aDir in di.GetDirectories())
                {
                    ClearFolder(aDir.FullName); // ik roep mezelf terug op: recursiviteit!
                }
                // Nu er geen bestanden meer staan in de directory kunnen we deze uitvegen
                di.Delete();
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
