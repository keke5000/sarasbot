using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
//using System.Speech;
//using System.Speech.Synthesis;
using Microsoft.Speech; //Tämä piti ladata nugetin kautta. Löytyi kun kirjoitti microsoft.speech. Edellyttää Microsoft.Speech -runtime (x86 version) ja SKD, sekä vielä suomenkielen tuen asentamista.
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Synthesis; //tämän kautta sai vasta suomenkielen Heidin pelaamaan. Miesääntä ei ole ilmeisesti tähän.

            /// TODO: 
            /// Laita omaan aliohjelmaan tekstit esim parametrin OUT komennolla.
            /// Joku exit nappi tekstistä, että pääsee valikkoon.
            /// Tee windows form ikkuna, johon tulee jotain valikkoja äänelle, sekä tekstisyöte, joka visuaalisesti imitoi  komentoriviä, mutta ottaa vastaan listasyötteen?
namespace SarasBot
{
    class Program
    {
            /// <summary>
            /// Pääohjelma jossa käyttäjä valitsee ensin tekstin, minkä jälkeen ohjelma puhuu valitun tekstin naisen äänellä
            /// </summary>
            static void Main(string[] args)
            {
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("");
            Console.WriteLine(" SarasBot");
            Console.WriteLine("");
            Console.WriteLine("––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––");
            Console.WriteLine();
            Console.WriteLine("1: Aamulenkki");
            Console.WriteLine("2: Kuoleman jalkeinen kukoistus");
            Console.WriteLine("3: Mene ja tee syntiä siis voittoa");
            Console.WriteLine("4: Kaikille kavisi paremmin jos");
            Console.WriteLine("5: Yrittaja ryhdy faaraoksi");
            Console.WriteLine("6: Kahdessa vuodessa ehtii ihmeita");
            Console.WriteLine();
            Console.Write("Valitse teksti (1-6) > ");

            int luku;
            
            while (true)
            {
                int syote = int.Parse(Console.ReadLine());
                if (syote < 1 || syote > 6)
                {
                    Console.WriteLine();
                    Console.Write("Anna luku uudestaan väliltä 1-6 >");
                }
                else
                {
                    luku = syote;
                    break;
                }
            }

            string url = TekstinValinta(luku);

            Console.Write("Valitse äänen nopeus -/+10 > ");
            int aaniRate;
            while (true)
            {
                int syote2 = int.Parse(Console.ReadLine());
                if (syote2 < -10 || syote2 > 10)
                {
                    Console.WriteLine();
                    Console.Write("Anna luku uudestaan väliltä -10/10 >");
                }
                else
                {
                    aaniRate = syote2;
                    break;
                }
            }

            List<string> nettiLista = new List<string>(); //sisältää koko html koodin.
            LueNetistaListaan(url, nettiLista);
            char[] erottimet = new char[] { ' ', ';', ',' };

            Console.WriteLine();
            Console.WriteLine("-––––––––––––––––––––––––––––––––––––––-");

            string muokattava = MuokkaaTekstia(nettiLista);
            string[] muokattuTeksti = muokattava.Trim().Split(erottimet);
            string[] puhuJaNayta = ValmisTeksti(muokattuTeksti);

            LueAaneenJaNayta(puhuJaNayta, aaniRate);
            }

        /// <summary>
        /// TODO: Tsekkaa linkit vielä.
        /// Käyttäjä valitsee tekstin valmiista teksteistä. Haave olisi ollut että aina uuden tekstin tullessa olisi päivittynyt ohjelmaan, mutta olisi ollut aika haastava.
        /// Tällä hetkellä 3 ja 4 ei toimi!
        /// </summary>
        /// <param name="syote"></param>
        /// <returns></returns>
        public static string TekstinValinta(int syote)
        {
            switch (syote)
            {
                case 1:
                    return "https://www.linnake.fi/blog/aamulenkki";
                case 2:
                    return "https://www.linnake.fi/blog/kuoleman-jalkeinen-kukoistus";
                case 3:
                    return "https://www.linnake.fi/blog/mene-ja-tee-synti%C3%A4-siis-voittoa";
                case 4:
                    return "https://www.linnake.fi/blog/kaikille-kavisi-paremmin-jos";
                case 5:
                    return "https://www.linnake.fi/blog/yrittaja-ryhdy-faaraoksi";
                case 6:
                    return "https://www.linnake.fi/blog/kahdessa-vuodessa-ehtii-ihmeita";
    
                default:
                    return "https://www.stronghold.fi/kuoleman-jalkeinen-kukoistus/";
            }
        }

        /// <summary>
        /// Muokataan <p> tageihin perustuvaa taulukkoa, ja poistetaan merkkejä. Yritys saada teksti selkokieliseen muotoon.
        /// </summary>
        /// <param name="tekstitaulukko"></param>
        /// <returns></returns>
        public static string[] ValmisTeksti(string[] tekstitaulukko)
        {
            List<string> t = new List<string>();
            StringBuilder teksti = new StringBuilder();
            char[] erottimet = { '.' };

            foreach (string osa in tekstitaulukko)
            {
                if (osa.Contains("<") || osa.Contains("</"))
                {
                    string muok = osa.Replace("<", " ").Replace(">", " ").Replace("/", " ").Replace(" p", "").Replace(" em ", "").Replace("strong", "").Replace("span", "").Replace("&nbsp", "").Replace("!--more--", "").Replace("br", "");
                    if (osa.Length > 1 && !((osa.Contains("-")) || (osa.Contains("href")) || (osa.Contains("@")))) t.Add(muok);
                }
                else
                {
                    if (osa.Length > 1 && !((osa.Contains("-")) || (osa.Contains("medium")) || (osa.Contains("source")) || (osa.Contains("href")) || (osa.Contains("@"))))
                        t.Add(osa);
                }
                t.TrimExcess();
            }
            foreach (string sana in t)
            {
                teksti.Append(sana + " ");
            }

            return teksti.ToString().Split(erottimet);
        }

        /// <summary>
        /// Muokataan koko html-koodin sisältävää listaa. Haetaan vain <p> tageissa olevat teksti kappaleet.
        /// </summary>
        /// <param name="nettiLista"></param>
        /// <returns></returns>
        public static string MuokkaaTekstia(List<string> nettiLista)
        {
            StringBuilder tekstiPotkossa = new StringBuilder();

            foreach (String rivi in nettiLista)
            {
                if (rivi.Contains("<p>"))
                {
                    tekstiPotkossa.Append(rivi);
                }
            }
            return tekstiPotkossa.ToString();
        }

        /// <summary>
        /// SarasBot puhuu ääneen ja näyttää samalla tekstin. Muokataan myös äänen nopeutta.
        /// </summary>
        /// <param name="lausuttava"></param>
        /// <param name="aanenRate"></param>
        public static void LueAaneenJaNayta(string[] lausuttava, int aanenRate)
        {
            SpeechSynthesizer sarasBot = new SpeechSynthesizer();
            sarasBot.SetOutputToDefaultAudioDevice();

            if (aanenRate >= -10 && aanenRate <= 10)
                sarasBot.Rate = aanenRate;

            foreach (string puhu in lausuttava) //tämä syöte tulee komentorivi-ikkunaan.
            {
                
                Console.Write(puhu);
                sarasBot.Speak(puhu);
            }
        }

        /// <summary>
        /// Luetaan annettu url-osoite html-koodi
        /// ja laitetaan se rivi kerrallaan string listaan.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="lista"></param>
        public static void LueNetistaListaan(string url, List<string> lista)
            {
                HttpWebResponse response = null;
                StreamReader reader = null;

                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    response = (HttpWebResponse)request.GetResponse();
                    reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);

                    while (!reader.EndOfStream)
                    {
                        string rivi = reader.ReadLine();
                        lista.Add(rivi);
                    }
                }
                catch (WebException we) 
                {
                    Console.WriteLine(we.Message);
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                    if (response != null)
                        response.Close();
                }
            }
        }
    }


