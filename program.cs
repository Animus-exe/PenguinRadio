using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using LibVLCSharp.Shared;
using NAudio.Wave;

namespace radioconsole
{
    class Program
    {  
        private static readonly Random _rng = new Random();

        ///Main Method
        static async Task Main()
        {
            string fastestUrl = GetFastestURL();
            WriteCenteredBlock(fastestUrl);
            Core.Initialize();
 
            await PullCountriesToJson(fastestUrl);
            await PullGenresToJson(fastestUrl);
            string jsonText = await File.ReadAllTextAsync("countries.json");
            JsonArray countryList = JsonNode.Parse(jsonText)!.AsArray();
            string genresJson = await File.ReadAllTextAsync("Genres.json");
            JsonArray genresList = JsonNode.Parse(genresJson)!.AsArray();

            while (true)
            {
                await Menu(countryList, genresList, fastestUrl);
            }

            

            
        }
        /// Menu 
        public static async Task Menu(JsonArray countryList, JsonArray genresList, string apiHost)
        {
            ConsoleColor introColor = GetRandomIntroColor();

            WriteCenteredBlock(@"
         ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣤⣤⣤⣤⣠⣄⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣤⡾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷⣦⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣠⣿⡷⢀⣿⣿⣿⣿⣿⣿⣿⣾⣏⡙⢿⣿⣦⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣠⣿⣷⣿⣿⣶⡟⣍⡲⡔⣆⠨⡙⢿⣿⣟⣃⣸⣿⣿⣧⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢠⣿⣿⣿⣿⣿⣿⣯⣝⡻⣽⢫⢿⣹⢲⣿⣿⣿⣿⣿⣿⣿⣿⣶⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⣀⣀⣀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣾⢯⠉⠙⣿⣿⣿⣿⣿⣿⣷⣿⣷⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⢀⡶⠏⠋⠉⠉⠓⢦⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣾⣀⣀⣤⣬⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠃⠀⣻⠃⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⢸⡆⠀⠀⠀⠀⠀⠀⠙⣦⠀⠀⠀⠀⠀⠀⠀⠀⢀⣀⣀⣤⣶⣾⣿⣿⣿⣿⣿⣿⡿⠿⠿⠿⣿⣿⣿⣿⣿⣿⣿⣿⡛⠉⠀⣤⠾⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⢠⣬⣧⣤⣤⣶⣾⡆⠀⠀⠹⡄⠀⠀⠀⣀⣤⣴⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⠛⠉⠀⠀⠀⠀⠀⠀⢉⣻⣿⣿⣿⣿⣿⣿⣦⡼⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠈⣹⣿⣿⣿⣿⣿⣿⣷⣀⣀⣀⣹⣤⣶⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠟⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠹⣿⣿⣿⣿⣿⣷⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⣠⣽⣿⣿⣿⣾⣿⣻⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠿⠋⠻⣦⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢘⣿⣿⣿⣿⣿⣿⣦⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⣿⣿⣿⣿⣿⣿⣟⣏⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠉⠀⠀⠀⠈⠛⠶⣤⣤⣄⣀⡀⠀⠀⢀⣀⡴⡟⠋⠙⣿⣿⣿⣿⣿⣿⣦⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⢹⣿⣿⣿⣿⣿⣿⣯⡷⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡟⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠉⢹⣧⣴⠻⠁⠀⠀⠀⠀⢸⣿⣿⣿⣿⣿⣿⣇⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠈⣿⣿⣿⣿⣿⣿⣿⣿⡽⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠍⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣾⣿⡿⠥⠄⠀⠀⠀⡄⠀⢿⣿⣿⣿⣿⣿⣿⣧⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⢿⣿⣿⣿⣿⣿⡿⣏⣹⣿⣿⣿⣿⣿⣿⡿⠟⠛⢶⡃⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠴⣿⣿⠃⠁⠀⠀⠀⠀⠱⠀⢸⣿⣿⢿⣿⣿⣿⣿⣷⣄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠈⠛⠻⠓⠛⠛⠛⠚⠓⠛⠛⠛⠋⠉⠁⠀⠀⢠⡟⢷⡕⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⡿⠐⠀⠀⠀⠀⠀⠀⠀⠀⠀⣿⣿⡈⢿⣿⣿⣿⣿⣿⣷⣄⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣾⠀⠐⠙⣷⡤⠀⠀⠀⠀⠀⠀⠀⠀⠀⢠⡟⠅⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣿⡿⠇⠈⢿⣿⣿⣿⣿⣿⣿⣿⣶⣄⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡏⠀⠀⠀⠉⠹⢦⣄⡀⠀⠀⠀⢀⣰⠾⠋⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣿⡗⠀⠀⠀⠻⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷⣦⣄⣀⣀⡀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢳⠀⠀⠀⠀⠀⠀⠈⡙⠛⠛⠛⠋⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣿⠇⠀⠀⠀⠀⠊⠿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣶⣶⣤⣀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡻⠂⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⡿⠀⠀⠀⠀⠀⠀⠀⠈⠻⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷⡄
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣗⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣸⡛⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠙⠻⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣟⠇
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡼⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢰⠏⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠉⠙⠛⠛⠛⠛⠿⠿⠿⠛⠛⠋⠁⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢨⠿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢠⡏⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡏⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⠟⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠠⠈⣠⠋⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣀⣠⠽⣦⣄⣀⣄⢂⡁⢂⡽⠓⠲⠤⣄⠀⠀⠀⠀⡀⢄⡐⣀⢂⣴⠶⠚⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⢀⣄⠤⠶⠄⠒⠒⠒⠛⠉⠉⠉⠁⠀⠐⠠⣌⡭⣉⢟⣻⣿⠀⠀⠀⠀⠀⠉⠙⠋⠓⢛⣳⣿⡾⡿⡟⠲⠢⢄⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠰⡏⠀⢤⠀⢀⠀⡐⠠⢀⠀⠀⠒⠒⠂⡐⠡⣈⣵⡽⠾⠋⠁⠀⠀⠀⠀⠀⠀⢀⠔⠋⠁⠀⠀⠀⠀⠀⠀⠠⠀⠈⠢⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠈⠓⠶⠤⠦⠴⠤⠥⠦⠤⠷⠖⠒⠒⠋⠉⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠰⡇⢠⢀⠄⠠⠠⠄⠠⠐⠤⠤⢀⠃⢄⣻⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠙⠲⠤⠦⠥⠦⠴⠤⠦⠤⠤⠦⠒⠋⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
            ", introColor);
            WriteCenteredBlock("Welcome to", introColor);
            WriteCenteredBlock(@"
            
            _______                         __           _______          __ __       
|   _   .-----.-----.-----.--.--|__.-----.   |   _   .---.-.--|  |__.-----.
|.  1   |  -__|     |  _  |  |  |  |     |   |.  l   |  _  |  _  |  |  _  |
|.  ____|_____|__|__|___  |_____|__|__|__|   |.  _   |___._|_____|__|_____|
|:  |               |_____|                  |:  |   |                     
|::.|                                        |::.|:. |                     
`---'                                        `--- ---'    
", introColor);
            WriteCenteredBlock("listen to your favorite radio stations from around the world!", introColor);
            WriteCenteredBlock("Browse through countries or genres to find a radio station!", introColor);
            WriteCenteredBlock("Search a country or genre to get started", introColor);
            string? input = ReadCenteredInput("> ");
            if (string.IsNullOrWhiteSpace(input))
            {
                WriteCenteredBlock("Please enter a country or genre.");
                return;
            }

            string? selectedCountry = null;
            string? selectedGenre = null;
            List<string> matchedCountries = new List<string>();
            List<string> matchedGenres = new List<string>();

            // Country matches
            for (int i = 0; i < countryList.Count; i++)
            {
                if (countryList[i] is JsonObject countryObject)
                {
                    string? name = countryObject["name"]?.ToString();
                    int stationCount = countryObject["stationcount"]?.GetValue<int>() ?? 0;
                    if (!string.IsNullOrWhiteSpace(name) &&
                        name.Contains(input, StringComparison.OrdinalIgnoreCase))
                    {
                        matchedCountries.Add(name);
                        WriteCenteredBlock($"Country {matchedCountries.Count}: {name} - {stationCount} stations");
                    }
                }
            }

            // Genre matches
            for (int i = 0; i < genresList.Count; i++)
            {
                if (genresList[i] is JsonObject genreObject)
                {
                    string? genreName = genreObject["name"]?.ToString();
                    int stationCount = genreObject["stationcount"]?.GetValue<int>() ?? 0;
                    if (!string.IsNullOrWhiteSpace(genreName) &&
                        genreName.Contains(input, StringComparison.OrdinalIgnoreCase))
                    {
                        genreName = genreName.Trim('"');
                        matchedGenres.Add(genreName);
                        WriteCenteredBlock($"Genre {matchedGenres.Count}: {genreName} - {stationCount} stations");
                    }
                }
            }

            if (matchedCountries.Count == 0 && matchedGenres.Count == 0)
            {
                WriteCenteredBlock("No matching country or genre found.");
                return;
            }

            if (matchedCountries.Count > 1)
            {
                string? pickedCountry = SelectMatchFromList("Choose a country number", matchedCountries);
                if (pickedCountry == null) return;
                selectedCountry = pickedCountry;
            }
            else if (matchedCountries.Count == 1)
            {
                selectedCountry = matchedCountries[0];
            }

            if (matchedGenres.Count > 1)
            {
                string? pickedGenre = SelectMatchFromList("Choose a genre number", matchedGenres);
                if (pickedGenre == null) return;
                selectedGenre = pickedGenre;
            }
            else if (matchedGenres.Count == 1)
            {
                selectedGenre = matchedGenres[0];
            }

            string? playInput = ReadCenteredInput("Play a radio station from these results? (Y/N)");
            if (!string.Equals(playInput, "Y", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            await PlayRadio(apiHost, selectedCountry, selectedGenre);
        }      
      ///Get URL
        static string GetFastestURL()
        {
            string baseUrl = "all.api.radio-browser.info";
            IPAddress[] ips = Dns.GetHostAddresses(baseUrl);

            long lastRoundTripTime = long.MaxValue;
            string searchUrl = "de2.api.radio-browser.info"; 

            foreach (IPAddress ipAddress in ips)
            {
                PingReply reply = new Ping().Send(ipAddress);

                if (reply != null && reply.RoundtripTime < lastRoundTripTime)
                {
                    lastRoundTripTime = reply.RoundtripTime;
                    searchUrl = ipAddress.ToString();
                }
            }

            IPHostEntry hostEntry = Dns.GetHostEntry(searchUrl);
            if (!string.IsNullOrEmpty(hostEntry.HostName))
            {
                searchUrl = hostEntry.HostName;
            }

            return searchUrl;
        }
        /// Pull countries to json format 
        public static async Task PullCountriesToJson(string apiHost)
        {
            using HttpClient client = new HttpClient();
            client.BaseAddress = new Uri($"https://{apiHost}/");

            string json = await client.GetStringAsync("json/countries");
            await File.WriteAllTextAsync("countries.json", json);
            
        }
        // Pull genres to json format 
        public static async Task PullGenresToJson(string apiHost)
        {
            using HttpClient client = new HttpClient();
            client.BaseAddress = new Uri($"https://{apiHost}/");

            string json = await client.GetStringAsync("/json/tags");

            await File.WriteAllTextAsync("Genres.json", json);
            
        }

        public static async Task PlayRadio(string apiHost, string? countryName, string? genreName)
        {
            using HttpClient client = new HttpClient();
            client.BaseAddress = new Uri($"https://{apiHost}/");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("RadioConsole/0.1");

            string endpoint;
            if (!string.IsNullOrWhiteSpace(countryName) && !string.IsNullOrWhiteSpace(genreName))
            {
                endpoint = $"json/stations/search?country={Uri.EscapeDataString(countryName)}&tag={Uri.EscapeDataString(genreName)}&limit=10&hidebroken=true";
            }
            else if (!string.IsNullOrWhiteSpace(countryName))
            {
                endpoint = $"json/stations/bycountry/{Uri.EscapeDataString(countryName)}?limit=10&hidebroken=true";
            }
            else
            {
                endpoint = $"json/stations/bytag/{Uri.EscapeDataString(genreName!)}?limit=10&hidebroken=true";
            }

            string json = await client.GetStringAsync(endpoint);
            JsonArray stations = JsonNode.Parse(json)!.AsArray();

            if (stations.Count == 0)
            {
                WriteCenteredBlock("No playable stations found.");
                return;
            }

            while (true)
            {
                for (int i = 0; i < stations.Count; i++)
                {
                    if (stations[i] is JsonObject st)
                    {
                        string name = st["name"]?.ToString() ?? "Unknown";
                        WriteCenteredBlock($"{i + 1}. {name}");
                    }
                }

                string? stationInput = ReadCenteredInput("Choose station number (or Q to go back)");
                if (string.Equals(stationInput, "Q", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (!int.TryParse(stationInput, out int stationNumber))
                {
                    WriteCenteredBlock("Invalid input.");
                    continue;
                }

                int stationIndex = stationNumber - 1;
                if (stationIndex < 0 || stationIndex >= stations.Count)
                {
                    WriteCenteredBlock("Station number out of range.");
                    continue;
                }

                if (stations[stationIndex] is not JsonObject stationObject)
                {
                    WriteCenteredBlock("Selected station is invalid.");
                    continue;
                }

                string selectedName = stationObject["name"]?.ToString() ?? "Unknown";
                string stationUrl = stationObject["url"]?.ToString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(stationUrl))
                {
                    WriteCenteredBlock("Selected station has no URL.");
                    continue;
                }

                // Start console radio player for selected station.
                using var LibVLC = new LibVLC();
                using var mediaPlayer = new MediaPlayer(LibVLC);
                using var media = new Media(LibVLC, new Uri(stationUrl));

                WriteCenteredBlock($"Connecting to {selectedName}...");
                bool started = mediaPlayer.Play(media);
                if (!started)
                {
                    WriteCenteredBlock("Failed to start playback.");
                    continue;
                }

                WriteCenteredBlock("Station playing.......");
                WriteCenteredBlock($"Playing {selectedName} at {stationUrl}");
                RunNowPlayingWave(mediaPlayer, selectedName);
                mediaPlayer.Stop();
            }



          }
            public static void WriteCenteredBlock(string block, ConsoleColor color = ConsoleColor.White)
            {
                Console.ForegroundColor = color;

                foreach (var rawLine in block.Split('\n'))
                {
                    string line = rawLine.TrimEnd('\r');

                    // Skip empty lines but preserve spacing if you want
                    if (line.Length == 0)
                    {
                        Console.WriteLine();
                        continue;
                    }

                    int pad = Math.Max((Console.WindowWidth - line.Length) / 2, 0);
                    Console.WriteLine(new string(' ', pad) + line);
                }
             
                Console.ResetColor();
            }
            static string ReadCenteredInput(string prompt, ConsoleColor color = ConsoleColor.White)
            {
                // Print centered prompt
                int promptPad = Math.Max((Console.WindowWidth - prompt.Length) / 2, 0);
                Console.ForegroundColor = color;
                Console.WriteLine(new string(' ', promptPad) + prompt);
                Console.ResetColor();

                // Move cursor to centered input start
                int inputWidth = 30; // expected input width
                int inputX = Math.Max((Console.WindowWidth - inputWidth) / 2, 0);
                Console.SetCursorPosition(inputX, Console.CursorTop);

                return Console.ReadLine() ?? string.Empty;
            }

            static void RunNowPlayingWave(MediaPlayer mediaPlayer, string stationName)
            {
                int phase = 0;
                int level = 1;
                float audioLevel = 0f;
                 string playerAscii = $@"
                ᴺᵒʷ ᵖˡᵃʸᶦⁿᵍ [ {stationName} ]
                            ♬
                .ılılılllıılılıllllıılılllıllı.
                0:24 ─●──────── -2:56
                ⇄ ◃◃ ⅠⅠ ▹▹ ↻
                                ";

                // Keep this frame short so it fits narrow terminals.
                const int waveWidth = 28;
                using var capture = new WasapiLoopbackCapture();
                capture.DataAvailable += (_, e) =>
                {
                    float peak = GetBufferPeak(e.Buffer, e.BytesRecorded, capture.WaveFormat);
                    audioLevel = (audioLevel * 0.80f) + (peak * 0.20f);
                };
                capture.StartRecording();

                
                WriteCenteredBlock(playerAscii, ConsoleColor.Magenta);
                WriteCenteredBlock("Press Enter to stop", ConsoleColor.Yellow);
                Console.WriteLine();
                int waveRow = Console.CursorTop;
                Console.WriteLine();

                while (true)
                {
                    // Real-time reactive effect based on system output amplitude.
                    int target = Math.Clamp((int)(audioLevel * 24f), 1, 12);
                    if (target > level) level++;
                    else if (target < level) level--;

                    string bars = new string('█', Math.Clamp(level, 1, 12));
                    string wave = BuildWaveFrame(waveWidth, phase);
                    WriteCenteredAt(waveRow, $"{wave}  {bars}", ConsoleColor.Green);

                    phase++;

                    if (Console.KeyAvailable && Console.ReadKey(intercept: true).Key == ConsoleKey.Enter)
                    {
                        break;
                    }

                    Thread.Sleep(90);
                }

                capture.StopRecording();
            }

            static string? SelectMatchFromList(string prompt, List<string> options)
            {
                string? selected = null;
                while (selected == null)
                {
                    string? input = ReadCenteredInput(prompt);
                    if (!int.TryParse(input, out int number))
                    {
                        WriteCenteredBlock("Invalid number.");
                        continue;
                    }

                    int index = number - 1;
                    if (index < 0 || index >= options.Count)
                    {
                        WriteCenteredBlock("Choice out of range.");
                        continue;
                    }

                    selected = options[index];
                }

                return selected;
            }

            static void WriteCenteredAt(int row, string text, ConsoleColor color = ConsoleColor.White)
            {
                if (row < 0) return;

                int width = Console.WindowWidth;
                string clipped = text.Length > width ? text[..width] : text;
                int pad = Math.Max((width - clipped.Length) / 2, 0);
                string output = (new string(' ', pad) + clipped).PadRight(width);

                int safeRow = Math.Min(row, Console.BufferHeight - 1);
                Console.SetCursorPosition(0, safeRow);
                Console.ForegroundColor = color;
                Console.Write(output);
                Console.ResetColor();
            }

            static string BuildWaveFrame(int width, int phase)
            {
                char[] chars = new char[width];
                for (int i = 0; i < width; i++)
                {
                    int t = (i + phase) % 8;
                    chars[i] = t switch
                    {
                        0 or 4 => '▁',
                        1 or 5 => '▂',
                        2 or 6 => '▃',
                        _ => '▄'
                    };
                }
                return new string(chars);
            }

            static ConsoleColor GetRandomIntroColor()
            {
                ConsoleColor[] palette =
                {
                    ConsoleColor.Cyan,
                    ConsoleColor.Magenta,
                    ConsoleColor.Green,
                    ConsoleColor.Yellow,
                    ConsoleColor.Blue,
                    ConsoleColor.Red,
                    ConsoleColor.White
                };

                return palette[_rng.Next(palette.Length)];
            }

            static float GetBufferPeak(byte[] buffer, int bytesRecorded, WaveFormat format)
            {
                float peak = 0f;

                if (format.BitsPerSample == 32 && format.Encoding == WaveFormatEncoding.IeeeFloat)
                {
                    for (int i = 0; i + 3 < bytesRecorded; i += 4)
                    {
                        float sample = MathF.Abs(BitConverter.ToSingle(buffer, i));
                        if (sample > peak) peak = sample;
                    }
                }
                else if (format.BitsPerSample == 16)
                {
                    for (int i = 0; i + 1 < bytesRecorded; i += 2)
                    {
                        float sample = Math.Abs(BitConverter.ToInt16(buffer, i) / 32768f);
                        if (sample > peak) peak = sample;
                    }
                }

                return Math.Clamp(peak, 0f, 1f);
            }
        }
    }
