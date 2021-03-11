

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;




namespace SjabloonMenuMetGenesteMenusEnSchermen
{

    class Program
    {
        //----------------------------------------------------------------------
        public static int _debug_ToonMenu_teller = 0;
        public static int _debug_ExecuteMenuItem_teller = 0;

        public const string PAD_GEBRUIKERS = @"gebruikers.txt";
        public const char FILE_SEPERATOR = ';';
        public const int WELKOMSBONUS = 200;
        const int LENGTE_ENCRYPT_ZONDER_SOM = 40;
        //----------------------------------------------------------------------
        enum MenuItem
        {
            MenuItemId = 0,//uniek
            MenuID,
            ParentId, // mag er uit denk
            SelectionValue,
            IsSelected, //als er iets in staat is het geselecteerd, anders niet
            IsVisible,
            Text
        }
        enum Gebruiker
        {
            Gebruikersnaam, //uniek
            Paswoord, //niet geëncrypt
            InlogTijd,
            Saldo,
        }

        enum SpelerStatus
        {
            NietGeset,
            Gewonnen,
            Verloren,
            Gelijk,
            BlackJack = 100
        }


        //===============================================================================================================
        static void Main(string[] args)
        {
            string[,] menu = {
                //--------------------------------------------------------------------------------------------
                //menuItemId, menuId,    parentID, SelectionValue, IsSelected, zichtbaar, text
                //--------------------------------------------------------------------------------------------
                {   "1",        "0",        "0",        "1",        "",         "*",        "Gebruiker toevoegen" },
                {   "2",        "0",        "0",        "2",        "",        "*",        "Gebruiker bewerken" },
                {   "3",        "0",        "0",        "3",        "",         "*",        "Gebruiker verwijderen" },
                {   "4",        "0",        "0",        "8",        "*",         "*",        "Inloggen" },
                //--------------------------------------------------------------------------------------------
                //menuItemId, menuId,    parentID, SelectionValue, IsSelected, text
                //--------------------------------------------------------------------------------------------
                {   "8",        "2",        "0",        "1",        "",        "*",        "Spelernaam bewerken"},
                {   "9",        "2",        "0",        "2",        "*",         "*",        "Paswoord bewerken"},
                //--------------------------------------------------------------------------------------------
                //menuItemId, menuId,    parentID, SelectionValue, IsSelected, text
                //--------------------------------------------------------------------------------------------
                {   "15",        "1",        "0",        "1",        "",         "*",        "Speel Blackjack"},
                {   "16",        "1",        "0",        "2",        "",         "*",        "Speel Slots"},
                {   "17",        "1",        "0",        "3",        "*",        "*",        "Speel Memory" },
                //{   "10",        "1",        "0",        "4",        "",        "Speel Blackjack2"},
                //{   "11",        "1",        "0",        "5",        "",         "Speel Slots2"},
                //{   "12",        "1",        "0",        "6",        "*",         "Speel Memory2" },
                //{   "13",        "1",        "0",        "7",        "",        "Speel Blackjack2"},
                //{   "14",        "1",        "0",        "8",        "",         "Speel Slots2"},
                //{   "15",        "1",        "0",        "9",        "*",         "Speel Memory2" },
                //--------------------------------------------------------------------------------------------
            };

            //Test_valideerPaswoord(); Console.ReadLine();
            //Test_valideerGebruikersnaam(); Console.ReadLine();
            //Test_Encryp_Decrypt(); Console.ReadLine();



            Console.CursorVisible = false;

            //string[] aTestGebruiker = Data_GetUser("Joske2", "Joske234!");//test
            //ToonScherm_Slots(aTestGebruiker);

            ToonMenu(menu, 0, null);


        }


        #region ========================================================================================================= Execute menuItem


        //=====================================================================================================================
        /// <summary>
        /// deze functie zelf nooit oproepen !!!
        /// </summary>
        static void ExecuteMenuItem(string[,] aMenu, int aMenuItemId, string[] aGebruiker)
        {
            Debug.WriteLine($"Je komt de methode ExecuteMenuItem({aMenuItemId}) binnen, aantal ExecuteMenuItem's op de callstack: {++_debug_ExecuteMenuItem_teller}");
            switch (aMenuItemId)
            {
                case 1:
                    ToonScherm_GebruikerToevoegen();
                    break;
                case 2:
                    Console.Clear();
                    ToonMenu(aMenu,2, aGebruiker);
                    break;
                case 3:
                    ToonScherm_Gebruiker_verwijderen(aGebruiker);
                    break;
                case 4:
                    string[] gebr = ToonScherm_Inloggen(aGebruiker);
                    if (gebr != null)
                    {
                        aGebruiker = gebr;
                        ToonMenu(aMenu, 1, gebr);
                    }
                    break;
                case 15:
                    ToonScherm_BlackJack(aGebruiker);
                    break;
                case 16:
                    ToonScherm_Slots(aGebruiker);
                    break;
                case 17:
                    ToonScherm_Memory(aGebruiker);
                    break;
                case 8:
                    ToonScherm_Gebruiker_bewerken_Gebruikersnaam();
                    break;
                case 9:
                    ToonScherm_Gebruiker_bewerken_Paswoord();
                    break;
                default:
                    ToonFatalErrorBoodschap($"menuItemId {aMenuItemId} niet gevonden in methode ExecuteMenuItem", true);
                    break;
            }

            
            Debug.WriteLine($"Je verlaat de methode ExecuteMenuItem({aMenuItemId}), aantal ExecuteMenuItem's op de callstack: {--_debug_ExecuteMenuItem_teller}");
        }


        #endregion ==========================================================================================================================




        #region ============================================================================================================ Toon Schermen


        //=====================================================================================================================
        static void ToonScherm_GebruikerToevoegen()
        {
            string naam = string.Empty;
            string paswoord = string.Empty;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("STOP om terug te keren naar hoofdscherm");
                Console.Write("geef gebruikersnaam: ");
                naam = Console.ReadLine();
                if (naam == "STOP") 
                    break;
                Console.Write("geef paswoord: ");
                paswoord = Console.ReadLine();
                if (paswoord == "STOP") 
                    break;

                if ( !IsValidGebruikersnaam(naam))
                {
                    Console.WriteLine("---------------------------------------foute invoer");
                    Console.WriteLine("de naam dat u gaf mag enkel letters of hoofdletters bevatten");
                    Console.WriteLine("----------------------------------------------------");
                    Console.WriteLine("druk een toets om opnieuw in te voeren");
                    Console.ReadKey();
                    continue;
                }
                if ( !IsValidPaswoord(paswoord))
                {
                    Console.WriteLine("---------------------------------------foute invoer");
                    Console.WriteLine("onjuist formaat voor paswoord");
                    Console.WriteLine("-paswoord moet minstens 8 tekens lang zijn");
                    Console.WriteLine("-paswoord mag maximum 20 tekens lang zijn");
                    Console.WriteLine("-paswoord moet minstens 1 hoofdletter bevatten");
                    Console.WriteLine("-paswoord moet minstens 1 speciaal karakter bevatten");
                    Console.WriteLine("-paswoord moet minstens 1 kleine letter bevatten");
                    Console.WriteLine("-paswoord moet minstens 1 cijfer bevatten");
                    Console.WriteLine("----------------------------------------------------");
                    Console.WriteLine("druk een toets om opnieuw in te voeren");
                    Console.ReadKey();
                    continue;
                }

                if (Data_GebruikerBestaat(naam))
                {
                    Console.WriteLine("---------------------------------------foute invoer");
                    Console.WriteLine("de naam dat u gaf bestaat reeds!!");
                    Console.WriteLine("gelieve een andere naam te kiezen!!");
                    Console.WriteLine("druk een toets om opnieuw in te voeren");
                    Console.WriteLine("----------------------------------------------------");
                    Console.WriteLine("druk een toets om opnieuw in te voeren");
                    Console.ReadKey();
                    continue;
                }
                else
                {
                    Data_AddUser(naam, paswoord,WELKOMSBONUS);
                    Console.WriteLine($"Welkom {naam}, je bent met succes toegevoegd");
                    Console.WriteLine("druk een toets om terug te gaan naar het hoofdscherm");
                    Console.ReadKey();
                }

                break;
            }

            Console.Clear();
        }
        //=====================================================================================================================
        static void ToonScherm_Gebruiker_bewerken_Paswoord()
        {
            Console.Clear();
            string[] alleGebruikers = Data_GetAlleGebruikersnamen();
            int huidigeSelectieIndex = 0;
            ConsoleColor currentForegroundColor = Console.ForegroundColor;
            bool isAangepast = false;

            ConsoleKeyInfo cki;
            //------------------------------------------------------------------

            if (alleGebruikers == null || alleGebruikers.Length == 0)
            {
                Console.WriteLine("geen gebruikers om te bewerken");
                Console.ReadLine();
                return;
            }

            Console.SetCursorPosition(15, 0);
            Console.WriteLine("navigeer met de [up] en [down]toetsen.");
            Console.SetCursorPosition(15, 1);
            Console.WriteLine("[esc] om terug te gaan, [enter] om paswoord te bewerken");
            Console.SetCursorPosition(15, 2);
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine();

            do
            {
                for (int i = 0; i < alleGebruikers.Length; i++)
                {
                    Console.SetCursorPosition(20, 4 + i);
                    if (huidigeSelectieIndex == i)
                        Console.ForegroundColor = ConsoleColor.Red;
                    else
                        Console.ForegroundColor = ConsoleColor.White;

                    Console.WriteLine(alleGebruikers[i]);
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(15, 5 + alleGebruikers.Length);
                Console.WriteLine("-------------------------------------------------------------");
                //===========================================wachten op input
                cki = Console.ReadKey(true);
                switch (cki.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (huidigeSelectieIndex > 0)
                        {
                            huidigeSelectieIndex--;
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if (huidigeSelectieIndex < alleGebruikers.Length - 1)
                        {
                            huidigeSelectieIndex++;
                        }
                        break;
                    case ConsoleKey.Enter:
                        bool huidigeVisibleCursor = Console.CursorVisible;
                        Console.CursorVisible = true;
                        Console.SetCursorPosition(15, 8 + alleGebruikers.Length);
                        Console.Write($"geef het oude paswoord van {alleGebruikers[huidigeSelectieIndex]}: ");
                        string oude = Console.ReadLine();
                        Console.SetCursorPosition(15, 9 + alleGebruikers.Length);
                        Console.Write($"geef het nieuwe paswoord voor {alleGebruikers[huidigeSelectieIndex]}: ");
                        string nieuwe = Console.ReadLine();


                        Console.SetCursorPosition(15, 13 + alleGebruikers.Length);

                        //proberen aan te passen in db
                        int result = (Data_UpdatePaswoord(alleGebruikers[huidigeSelectieIndex],oude, nieuwe));

                        if (result == -3)
                            ToonFatalErrorBoodschap("je probeert een gebruiker te bewerken dat niet voorkomt in de db", true);
                        else if (result == -2)
                            Console.WriteLine($"het oude paswoord is niet juist voor {alleGebruikers[huidigeSelectieIndex]}, niets is aangepast");
                        else if (result == -5)
                            Console.WriteLine($"het nieuwe  paswoord is geen geldig paswoord!! , niets is aangepast");
                        else
                            Console.WriteLine($"Het paswoord voor {alleGebruikers[huidigeSelectieIndex]} is met succes aangepast");

                        Console.SetCursorPosition(15, 15 + alleGebruikers.Length);
                        Console.WriteLine("druk een toets om terug te keren");
                        Console.ReadKey();//

                        isAangepast = true;//lus verlaten
                        Console.CursorVisible = huidigeVisibleCursor;
                        break;
                }
            }
            while (cki.Key != ConsoleKey.Escape && !isAangepast);


            Console.ForegroundColor = currentForegroundColor;
            Console.Clear();
        }
        //=====================================================================================================================
        static void ToonScherm_Gebruiker_bewerken_Gebruikersnaam()
        {
            Console.Clear();
            string[] alleGebruikers = Data_GetAlleGebruikersnamen();
            int huidigeSelectieIndex = 0;
            ConsoleColor currentForegroundColor = Console.ForegroundColor;
            bool isAangepast = false;

            ConsoleKeyInfo cki;
            //------------------------------------------------------------------

            if (alleGebruikers == null || alleGebruikers.Length == 0)
            {
                Console.WriteLine("geen gebruikers om te bewerken");
                Console.ReadLine();
                return;
            }

            Console.SetCursorPosition(15, 0);
            Console.WriteLine("navigeer met de [up] en [down]toetsen.");
            Console.SetCursorPosition(15, 1);
            Console.WriteLine("[esc] om terug te gaan, [enter] om gebruikersnaam te bewerken");
            Console.SetCursorPosition(15, 2);
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine();

            do
            {
                for (int i = 0; i < alleGebruikers.Length; i++)
                {
                    Console.SetCursorPosition(20, 4 + i);
                    if (huidigeSelectieIndex == i)
                        Console.ForegroundColor = ConsoleColor.Red;
                    else
                        Console.ForegroundColor = ConsoleColor.White;

                    Console.WriteLine(alleGebruikers[i]);
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(15, 5 + alleGebruikers.Length);
                Console.WriteLine("-------------------------------------------------------------");
                //===========================================wachten op input
                cki = Console.ReadKey(true);
                switch (cki.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (huidigeSelectieIndex > 0)
                        {
                            huidigeSelectieIndex--;
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if (huidigeSelectieIndex < alleGebruikers.Length - 1)
                        {
                            huidigeSelectieIndex++;
                        }
                        break;
                    case ConsoleKey.Enter:
                        bool huidigeVisibleCursor = Console.CursorVisible;
                        Console.CursorVisible = true;
                        Console.SetCursorPosition(15, 8 + alleGebruikers.Length);
                        Console.Write($"geef een nieuwe gebuikersnaam voor {alleGebruikers[huidigeSelectieIndex]}: ");
                        string nieuwe = Console.ReadLine();


                        Console.SetCursorPosition(15, 12 + alleGebruikers.Length);

                        //proberen aan te passen in db
                        int result = (Data_UpdateGebruikersnaam(alleGebruikers[huidigeSelectieIndex], nieuwe));

                        if (result == -3)
                            ToonFatalErrorBoodschap("je probeert een gebruiker te bewerken dat niet voorkomt in de db", true);
                        else if(result == -4)
                            Console.WriteLine($"{nieuwe} bestaad reeds in ons systeem, niets is aangepast");
                        else if (result == -5)
                            Console.WriteLine($"de gebruikersnaam is geen geldig gebruikersnaam!! , niets is aangepast");
                        else
                            Console.WriteLine($"{alleGebruikers[huidigeSelectieIndex]} is met succes aangepast door {nieuwe} ");

                        Console.SetCursorPosition(15, 14 + alleGebruikers.Length);
                        Console.WriteLine("druk een toets om terug te keren");
                        Console.ReadKey();//

                        isAangepast = true;//lus verlaten
                        Console.CursorVisible = huidigeVisibleCursor;
                        break;
                }
            }
            while (cki.Key != ConsoleKey.Escape && !isAangepast);


            Console.ForegroundColor = currentForegroundColor;
            Console.Clear();

        }
        //=====================================================================================================================
        static void ToonScherm_Gebruiker_verwijderen(string[] aGebruiker)
        {
            Console.Clear();  
            string[] alleGebruikers = Data_GetAlleGebruikersnamen();
            int huidigeSelectieIndex = 0;
            ConsoleColor currentForegroundColor = Console.ForegroundColor;
            bool isVerwijderd = false;

            if(alleGebruikers==null || alleGebruikers.Length == 0)
            {
                Console.WriteLine("geen gebruikers om te verwijderen");
                Console.ReadLine();
                return;
            }

            ConsoleKeyInfo cki;
            do
            {
                Console.SetCursorPosition(15, 0);
                Console.WriteLine("navigeer met de [up] en [down]toetsen.");
                Console.SetCursorPosition(15, 1);
                Console.WriteLine("[esc] om terug te gaan, [enter] om te verwijderen");
                Console.SetCursorPosition(15, 2);
                Console.WriteLine("-------------------------------------------------");
                Console.WriteLine();

                for (int i = 0; i < alleGebruikers.Length; i++)
                {
                    Console.SetCursorPosition(20, 5+i);
                    if (huidigeSelectieIndex == i)
                        Console.ForegroundColor = ConsoleColor.Red;
                    else
                        Console.ForegroundColor = ConsoleColor.White;

                    Console.WriteLine(alleGebruikers[i]);
                }
                Console.ForegroundColor = ConsoleColor.White;
                //===========================================wachten op input
                cki = Console.ReadKey(true);
                switch (cki.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (huidigeSelectieIndex > 0)
                        {
                            huidigeSelectieIndex--;
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if (huidigeSelectieIndex < alleGebruikers.Length - 1)
                        {
                            huidigeSelectieIndex++;
                        }
                        break;
                    case ConsoleKey.Enter:
                        Console.CursorTop = Console.CursorTop + 3;
                        Console.CursorLeft = 20;
                        if (Data_GebruikerVerwijderen(alleGebruikers[huidigeSelectieIndex]))
                        {
                            isVerwijderd = true; //verlaat hiermee de lus
                            Console.WriteLine($"{alleGebruikers[huidigeSelectieIndex]} is met succes verwijderd");
                        }
                        else
                        {
                            Console.WriteLine($"Er is een fout opgetreden, kan {alleGebruikers[huidigeSelectieIndex]} niet verwijderen");
                        }
                        Console.ReadKey();
                        break;
                }
            }
            while (cki.Key != ConsoleKey.Escape && !isVerwijderd);


            Console.ForegroundColor = currentForegroundColor;
            Console.Clear();
        }
        //=====================================================================================================================
        static string[] ToonScherm_Inloggen(string[] aGebruiker)
        {
            bool cursorVisibleTerugzetten = Console.CursorVisible;
            Console.CursorVisible = true;

            Console.Clear(); 
            Console.WriteLine("Hier kan u inloggen, daarna kan u onze geweldige spelletjes spelen");
            Console.WriteLine("------------------------------------------------------------------");
            Console.WriteLine();

            if(aGebruiker != null)
            {
                Console.WriteLine($"dag {aGebruiker[(int)Gebruiker.Gebruikersnaam]}, u bent reeds ingelogd" );
                Console.WriteLine($"druk een toets om terug te keren naar het hoofdscherm");
                Console.ReadKey(true);
            }

            Console.Write("Geef uw gebruikersnaam: ");
            string naam = Console.ReadLine();
            Console.Write("Geef uw paswoord: ");
            string pasw = Console.ReadLine();
            Console.CursorVisible = cursorVisibleTerugzetten;
            Console.WriteLine();

            string[] gebruiker = Data_GetUser(naam, pasw);

            if(gebruiker != null)
            {
                Console.WriteLine($"Welkom in ons speelpaleis {gebruiker[(int)Gebruiker.Gebruikersnaam]}");
                Console.WriteLine("Have fun, maar speel met maten!!!");
                Console.WriteLine("Druk een toets om te spelen");
                Console.ReadKey();
                Console.Clear();
                return gebruiker;
            }
            else
            {
                Console.WriteLine("we hebben je gebruikersnaam of een combinatie van gebruikersnaam");
                Console.WriteLine("en paswoord niet gevonden.");
                Console.WriteLine("Druk een toets om terug te gaan naar het hoofdmenu");
                Console.CursorVisible = cursorVisibleTerugzetten;
                Console.ReadKey();
                Console.Clear();
                return null;

            }
        }
        
        
        
        //=====================================================================================================================
        static void ToonScherm_BlackJack(string[] aGebruiker)
        {
            
            //---------------------------------------------------------------------------------- init
            const int GEENKAART = -1;
            const int INZET = 10;
            const int DelayTssKaartGeven = 500;
            string[] kleuren = { "harten", "ruiten", "klaveren", "schoppen" };
            string[] waardes = { "Joker", "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "B", "D", "H" };

            int[] X_KAARTPOSIETIES = { 20, 30, 40 , 50,60,70,80,90,100,110 };
            int Y_KAARTPOSITE_DEALER = 8;
            int Y_KAARTPOSITE_SPELER = 21;

            int[] spelerskaarten = new int[26];
            int[] computerKaarten = new int[spelerskaarten.Length];

            int bovensteKaartIndex = 0;
            int[,] kaartBoek = new int[52 ,2];

            int kaartTTspeler = 0;
            int kaartTTcomputer = 0;

            int aantalKaartenGehadComputer = 0;
            int aantalKaartenGehadSpeler = 0;

            string[,] kaartIcoontjes = LaadKaartdeck();

            //definitieve waardes aan kaartboek geven
            for (int i = 0; i < 52; i++)
            {
                kaartBoek[i, 0] = (i % 13) + 1;
                kaartBoek[i, 1] = (i /13);
            }
            ConsoleKeyInfo cki;
            //-----------------------------------------------------------------------------------------


            TekenUitBestand(@"AsciArts\ZwarteZot1.txt", ConsoleColor.DarkGray, 2);
            Console.CursorTop = 0;
            TekenUitBestand(@"AsciArts\ZwarteZot2.txt", ConsoleColor.DarkRed, 2);



            do  //gameloop
            {

                TekenSpelerInfo();
                if (Convert.ToInt32(aGebruiker[(int)Gebruiker.Saldo]) < INZET)
                {
                    Console.WriteLine("u hebt niet genoeg budget, ga naar de bank en kom terug");
                    Console.WriteLine("druk een toets om terug te gaan");
                    Console.ReadKey();
                    break;
                }

                //Console.SetCursorPosition(24, 10);
                //Console.WriteLine($"wil je een potje Black jacken? een rondje kost je {INZET}eur ");
                //Console.SetCursorPosition(20, 12);
                //Console.WriteLine("druke eender welke toets, [esc] om te verlaten");
                Console.SetCursorPosition(35, 25);
                Console.WriteLine("voor kaarten, druk eender welke toets.");
                Console.SetCursorPosition(35, 27);
                Console.WriteLine("[esc] om Black Jack te beëindigen");
                cki = Console.ReadKey(true); //wacht op input
                Console.SetCursorPosition(35, 25);
                Console.WriteLine("                                      ");
                Console.SetCursorPosition(35, 27);
                Console.WriteLine("                                      ");
                if (cki.Key == ConsoleKey.Escape) break; //lus verlaten dus ook de methode beeindigen




                //Console.Clear();
                //als je speelt, dan in geheugen en tekstbestand de waarde aanpassen
                UpdateSaldo(-INZET);

                while (true) //rondjeloop
                {
                    //speelveld wissen
                    TekenVlak(16, 8, 80, 20, ConsoleColor.Black);

                    SchudKaarten(150);
                    //KaartenBoekToString();
                    

                    GeefSpelerKaart(); TekenSpeelveld(); Thread.Sleep(DelayTssKaartGeven);
                    GeefSpelerKaart(); TekenSpeelveld(); Thread.Sleep(DelayTssKaartGeven);
                    GeefComputerKaart(); TekenSpeelveld(); Thread.Sleep(DelayTssKaartGeven);


                    if (kaartTTspeler == 21) //speler heeft blackjack
                    {
                        WinstVerliesHandler(SpelerStatus.BlackJack,INZET);
                        break; //rondje loop verlaten
                    }
                    else
                    {
                        Console.SetCursorPosition(30, 18);
                        Console.WriteLine("STOPPEN [<-]   [->] NOG EEN KAART");
                        //speler is aan de beurt
                        bool spelerheeft21_Stopt_OfKapot = false;
                        do
                        {
                            cki = Console.ReadKey(true); //wacht op input
                            switch (cki.Key)
                            {
                                case ConsoleKey.RightArrow: //nog een kaart
                                    GeefSpelerKaart(); TekenSpeelveld();
                                    if (kaartTTspeler >= 21)
                                    {
                                        spelerheeft21_Stopt_OfKapot = true;
                                    }
                                    break;
                                case ConsoleKey.LeftArrow: //stoppen
                                    spelerheeft21_Stopt_OfKapot = true;
                                    break;
                                case ConsoleKey.Enter:
                                    //voorlopig dummy
                                    break;
                                default:
                                    break;
                            }

                        } while (!spelerheeft21_Stopt_OfKapot);//input tijdens rondje loop

                        Console.SetCursorPosition(30, 18);
                        Console.WriteLine("                                  ");

                        if (kaartTTspeler > 21)
                        {
                            WinstVerliesHandler(SpelerStatus.Verloren, INZET);
                            break; //rondje loop verlaten
                        }
                        else
                        {
                            //computer is aan de beurt
                            Thread.Sleep(DelayTssKaartGeven); GeefComputerKaart(); TekenSpeelveld(); 
                            while(kaartTTcomputer<17){
                                Thread.Sleep(DelayTssKaartGeven); GeefComputerKaart(); TekenSpeelveld(); 
                            }
                            //uitbetaling (behalve blackjack)
                            //-------------------------------
                            if(kaartTTcomputer > 21 || kaartTTspeler>kaartTTcomputer)
                            {
                                WinstVerliesHandler(SpelerStatus.Gewonnen, INZET);
                                break; //rondje loop verlaten
                            }
                            else if (kaartTTcomputer == kaartTTspeler)
                            {
                                WinstVerliesHandler(SpelerStatus.Gelijk, INZET);
                                break; //rondje loop verlaten
                            }
                            else
                            {
                                WinstVerliesHandler(SpelerStatus.Verloren, INZET);
                                break; //rondje loop verlaten
                            }
                        }

                    }
                }
            } while (true);//einde gameloop

            //hier wordt de methode verlaten, de rest hieronder zijn enkel innermethodes
            //--------------------------------------------------------------------------

            Console.Clear();





            //----------------------------------------------------------------------------inner functies
            void UpdateSaldo(int aWaarde)
            {
                if (aWaarde != 0)
                {
                    //in het tekstbestand aanpassen
                    Data_UpdateSaldo( aGebruiker[(int)Gebruiker.Gebruikersnaam], aWaarde);
                    //ook in het geheugen aanpassen
                    int saldo = Convert.ToInt32( aGebruiker[(int)Gebruiker.Saldo]);
                    saldo += aWaarde;
                    aGebruiker[(int)Gebruiker.Saldo] = saldo.ToString();
                    TekenSpelerInfo();
                }
            }
            void TekenSpelerInfo()
            {
                string naamTekst = "Ingelogd als: " + aGebruiker[(int)Gebruiker.Gebruikersnaam];
                string saldoTekst = "  Saldo: " + aGebruiker[(int)Gebruiker.Saldo];
                Console.SetCursorPosition(Console.WindowWidth - naamTekst.Length - 2, 1);
                Console.WriteLine(naamTekst);
                Console.SetCursorPosition(Console.WindowWidth - saldoTekst.Length - 2, 2);
                Console.WriteLine(saldoTekst);
            }
            void TekenSpeelveld()
            {
                ConsoleColor herstelkleur = Console.ForegroundColor;

                ////teken de computerkaarten
                //Console.SetCursorPosition(60, 12);
                //Console.Write("computerkaarten: ");
                //for (int i = 0; i < computerKaarten.Length; i++)
                //{
                //    if (computerKaarten[i] == GEENKAART) break;
                //    Console.Write(KaartToConsoleString(computerKaarten[i]) + " ");
                //}
                //versie 2
                for (int i = 0; i < computerKaarten.Length; i++)
                {
                    if (computerKaarten[i] == GEENKAART) break;

                    ConsoleColor tmpKleur = ConsoleColor.Red;
                    if (kaartBoek[computerKaarten[i], 1] > 1) tmpKleur = ConsoleColor.DarkGray;
                    TekenKaartIcoontje(kaartIcoontjes, kaartBoek[computerKaarten[i], 1],
                        X_KAARTPOSIETIES[i], Y_KAARTPOSITE_DEALER, tmpKleur);
                    Console.ForegroundColor = tmpKleur;
                    Console.SetCursorPosition(X_KAARTPOSIETIES[i] + 1, Y_KAARTPOSITE_DEALER + 1);
                    Console.WriteLine(waardes[kaartBoek[computerKaarten[i], 0]]);
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(20, 15);
                Console.WriteLine("ttComputer: " + kaartTTcomputer);

                //---------------------------------------------

                //teken de spelerskaarten
                //Console.SetCursorPosition(60, 18);
                //Console.Write("sperlerskaarten: ");
                //for (int i = 0; i < spelerskaarten.Length; i++)
                //{
                //    if (spelerskaarten[i] == GEENKAART) break;
                //    Console.Write(KaartToConsoleString(spelerskaarten[i])+" ");

                //        //return ((char)(kaartBoek[aKaart, 1] + 3)).ToString() + waardes[kaartBoek[aKaart, 0]];
                //}
                //versie 2
                for (int i = 0; i < spelerskaarten.Length; i++)
                {
                    if (spelerskaarten[i] == GEENKAART) break;

                    ConsoleColor tmpKleur = ConsoleColor.Red;
                    if (kaartBoek[spelerskaarten[i], 1] > 1) tmpKleur = ConsoleColor.DarkGray;
                    TekenKaartIcoontje(kaartIcoontjes,kaartBoek[spelerskaarten[i], 1],
                        X_KAARTPOSIETIES[i], Y_KAARTPOSITE_SPELER, tmpKleur);
                    Console.ForegroundColor = tmpKleur;
                    Console.SetCursorPosition(X_KAARTPOSIETIES[i]+1, Y_KAARTPOSITE_SPELER+1);
                    Console.WriteLine(waardes[kaartBoek[spelerskaarten[i], 0]]);
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(20, 20);
                Console.Write("ttspeler: " + kaartTTspeler);

                Console.ForegroundColor = herstelkleur ;
            }
            //-------------------------------------------------------------------------------------------
            void TekenKaartIcoontje(string[,] aIcoontjes, int aIcoontjeIndex, int aX, int aY, ConsoleColor aKleur)
            {
                ConsoleColor kleurHerstellen = Console.ForegroundColor;
                Console.ForegroundColor = aKleur;
                Console.SetCursorPosition(aX, aY);
                Console.WriteLine(aIcoontjes[aIcoontjeIndex, 0]);
                Console.SetCursorPosition(aX, ++aY);
                Console.WriteLine(aIcoontjes[aIcoontjeIndex, 1]);
                Console.SetCursorPosition(aX, ++aY);
                Console.WriteLine(aIcoontjes[aIcoontjeIndex, 2]);
                Console.SetCursorPosition(aX, ++aY);
                Console.WriteLine(aIcoontjes[aIcoontjeIndex, 3]);
                Console.SetCursorPosition(aX, ++aY);
                Console.WriteLine(aIcoontjes[aIcoontjeIndex, 4]);
                Console.SetCursorPosition(aX, ++aY);
                Console.WriteLine(aIcoontjes[aIcoontjeIndex, 5]);
                Console.SetCursorPosition(aX, ++aY);
                Console.WriteLine(aIcoontjes[aIcoontjeIndex, 6]);
                Console.ForegroundColor = kleurHerstellen;

            }
            //-------------------------------------------------------------------------------------------
            void WinstVerliesHandler(SpelerStatus aStatus, int aInzet)
            {
                ConsoleColor herstelkleur = Console.ForegroundColor;
                Console.SetCursorPosition(30, 18);
                int bijTeSchrijven = 0;
                switch (aStatus)
                {
                    case SpelerStatus.BlackJack:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("proficiat BLACK JACK");
                        bijTeSchrijven = Convert.ToInt32(aInzet * 2.5);
                        break;
                    case SpelerStatus.Gewonnen:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("JE WINT :-)");
                        bijTeSchrijven = aInzet * 2;
                        break;
                    case SpelerStatus.Verloren:
                        Console.WriteLine("Dealer wint");
                        break;
                    case SpelerStatus.Gelijk:
                        Console.WriteLine("gelijke stand");
                        bijTeSchrijven = aInzet;
                        break;
                    default:
                        break;
                }
                if(bijTeSchrijven != 0)
                {
                    UpdateSaldo(bijTeSchrijven);
                }
                Console.ForegroundColor = herstelkleur;
                //Console.ReadKey();
            }
            string[,] LaadKaartdeck()
            {
                string[,] terug = new string[5, 7]; //7icoontjes 8 lijnen
                int iconTeller = 0;
                int lijnteller = 0;

                //bron: https://stackoverflow.com/questions/8037070/whats-the-fastest-way-to-read-a-text-file-line-by-line
                const Int32 BufferSize = 128;
                using (var fileStream = File.OpenRead(@"AsciArts\Carddeck.txt"))
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                {
                    String line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        terug[iconTeller++ / 7, lijnteller++ % 7] = line;
                        //Debug.WriteLine(iconTeller);
                    }
                }
                return terug;
            }
            //-------------------------------------------------------------------------------------------
            void GeefSpelerKaart()
            {
                int terug = 0;
                for(int i = 0; i < spelerskaarten.Length; i++)
                {
                    if (spelerskaarten[i]==GEENKAART)
                    {
                        spelerskaarten[i] = bovensteKaartIndex++;
                        break;
                    }
                }
                //bereken de totaalscore
                for (int i = 0; i < spelerskaarten.Length; i++)
                {
                    if (spelerskaarten[i] == GEENKAART) break;
                    if (kaartBoek[spelerskaarten[i], 0] >= 10) //10,boer,dam,heer
                        terug += 10;
                    else if (kaartBoek[spelerskaarten[i], 0] == 1) //aas
                        terug += 11;
                    else
                        terug += kaartBoek[spelerskaarten[i],0];
                }
                //we hebben 11 gerekend voor aas, als we over 21 zitten dan voor een 
                //aas trekken we 10 van terug af, nog eens chekken etc... 
                //maw het hoogst mogenlijke kleiner dan 22 teruggeven
                if (terug > 21)
                {
                    for (int i = 0; i < spelerskaarten.Length; i++)
                    {
                        if (spelerskaarten[i] == GEENKAART) break;
                        if (kaartBoek[spelerskaarten[i], 0] == 1) //aas
                            terug -= 10;
                        if (terug < 22) break;
                    }
                }
                kaartTTspeler = terug;
                //return terug;
            }
            //-------------------------------------------------------------------------------------------
            void GeefComputerKaart()
            {
                int terug = 0;
                for (int i = 0; i < computerKaarten.Length; i++)
                {
                    if (computerKaarten[i] == GEENKAART)
                    {
                        computerKaarten[i] = bovensteKaartIndex++;
                        break;
                    }
                }
                //bereken de totaalscore
                for (int i = 0; i < computerKaarten.Length; i++)
                {
                    if (computerKaarten[i] == GEENKAART) break;
                    if (kaartBoek[computerKaarten[i], 0] >= 10) //10,boer,dam,heer
                        terug += 10;
                    else if (kaartBoek[computerKaarten[i], 0] == 1) //aas
                        terug += 11;
                    else
                        terug += kaartBoek[computerKaarten[i], 0];
                }
                //we hebben 11 gerekend voor aas, als we over 21 zitten dan voor een 
                //aas trekken we 10 van terug af, nog eens chekken etc... 
                //maw het hoogst mogenlijke kleiner dan 22 teruggeven
                if (terug > 21)
                {
                    for (int i = 0; i < computerKaarten.Length; i++)
                    {
                        if (computerKaarten[i] == GEENKAART) break;
                        if (kaartBoek[computerKaarten[i], 0] == 1) //aas
                            terug -= 10;
                        if (terug < 22) break;
                    }
                }
                kaartTTcomputer = terug;
                //return terug;
            }
            //-------------------------------------------------------------------------------------------
            void SchudKaarten(int aantalShuffles)
            {
                Random rnd = new Random();
                for (int i = 0; i < aantalShuffles; i++)
                {
                    int randInt0 = rnd.Next(52);
                    int randInt1 = rnd.Next(52);
                    int[,] temp = { { kaartBoek[randInt1, 0], kaartBoek[randInt1, 1] } };
                    kaartBoek[randInt1, 0] = kaartBoek[randInt0, 0];
                    kaartBoek[randInt1, 1] = kaartBoek[randInt0, 1];
                    kaartBoek[randInt0, 0] = temp[0, 0];
                    kaartBoek[randInt0, 1] = temp[0, 1];
                }
                for(int i=0;i< spelerskaarten.Length; i++)
                {
                    spelerskaarten[i] = GEENKAART;
                    computerKaarten[i] = GEENKAART;
                }

                bovensteKaartIndex = 0;
                aantalKaartenGehadComputer = 0;
                aantalKaartenGehadSpeler = 0;
                kaartTTspeler = 0;
                kaartTTcomputer = 0;
            }
            //-------------------------------------------------------------------------------------------
            string KaartToString( int aKaart)
            {
                return kleuren[kaartBoek[aKaart, 1]]  + waardes[ kaartBoek[aKaart, 0]];
            }
            //-------------------------------------------------------------------------------------------
            string KaartToConsoleString(int aKaart)
            {
                //harten =char3, ruiten=4,klaveren=5,schoppen=6
                //return ((char)(kaartBoek[aKaart, 1]+3)).ToString() + kaartBoek[aKaart, 0];
                return ((char)(kaartBoek[aKaart, 1] + 3)).ToString() + waardes[kaartBoek[aKaart, 0]];
            }
            //-------------------------------------------------------------------------------------------
            void KaartenBoekToString()
            {
                for (int i = 0; i < 52; i++) 
                    Console.WriteLine(KaartToString(i) + " " + KaartToConsoleString(i));
            }
 
        }

        //=====================================================================================================================
        static void ToonScherm_Memory(string[] aGebruiker)
        {
            const int INZET = 20;

            const int X_MARGIN = 6;
            const int Y_MARGIN = 12;
            const int X_BEGIN_TSS_VELDJES = 10;

            const int GEEN_SELECTIE = -1;

            int[] veldjes = new int[10];
            bool[] isVeldjesZichtbaar = new bool[veldjes.Length];
            ConsoleColor[] TEKENINGKLEUREN = { 
                ConsoleColor.Red,
                ConsoleColor.Blue,
                ConsoleColor.Yellow,
                ConsoleColor.Magenta,
                ConsoleColor.Cyan,
            };
            string[,] icoontjes = LaadSlotsEnMemoryIcoontjes();

            const ConsoleColor KLEUR_VELDJE_NIET_OMGEDRAAID = ConsoleColor.White;
            const ConsoleColor KLEUR_VELDJE_NIET_OMGEDRAAID_WEL_GESELECTEERD = ConsoleColor.Green;

            int huidigGeselecteerdVeldje = -1;
           


            for (int i = 0; i < veldjes.Length; i++)
            {
                veldjes[i] = i/2;
            }


            ConsoleKeyInfo cki;

            //-----------------------------------------------------------------------------------------
            do  //gameloop
            {
                Console.Clear();
                TekenSpelerInfo();
                if (Convert.ToInt32(aGebruiker[(int)Gebruiker.Saldo]) < INZET)
                {
                    Console.WriteLine("u hebt niet genoeg budget, ga naar de bank en kom terug");
                    Console.WriteLine("druk een toets om terug te gaan");
                    Console.ReadKey();
                    break;
                }

                Console.SetCursorPosition(20, 10);
                Console.WriteLine($"wil je de uitdaging aan? een poging kost je {INZET}eur");
                Console.SetCursorPosition(22, 12);
                Console.WriteLine("druke eender welke toets, [esc] om te verlaten");

                cki = Console.ReadKey(true); //wacht op input
                if (cki.Key == ConsoleKey.Escape) break; //lus verlaten dus ook de methode beeindigen



                Console.Clear();
                //als je speelt, dan in geheugen en tekstbestand de waarde aanpassen
                UpdateSaldo(-INZET);

                //--------------------------------------------rondjesloop
                bool eindeRondjeLoop = false;
                while (!eindeRondjeLoop) //rondjeloop
                {
                    ShuffleVeldjes(50);
                    MaakAlleVeldjesVisible(true);
                    TekenVeldjes();
                    for (int i = 10; i > 0; i--)
                    {
                        Console.SetCursorPosition(50, 18);
                        Console.WriteLine("ONTHOUD DIT !!  " + i);
                        Thread.Sleep(1000);
                        Console.SetCursorPosition(50, 18);
                        Console.WriteLine("                      ");
                        //Thread.Sleep(250);
                    }
                    MaakAlleVeldjesVisible(false);
                    huidigGeselecteerdVeldje = veldjes.Length/2;
                    Console.SetCursorPosition(26, 20);
                    Console.WriteLine("gebruik [<-] [->] en [enter] om te selecteren");
                    TekenVeldjes();

                    int laatstOmgedraaidveldjeWaarde = GEEN_SELECTIE;

                    bool eindeInputloop = false;
                    while (! eindeInputloop) //inputloop
                    { 

                        cki = Console.ReadKey(true); //wacht op input
                        switch (cki.Key)
                        {
                            case ConsoleKey.RightArrow:
                                if (huidigGeselecteerdVeldje < veldjes.Length-1)
                                {
                                    huidigGeselecteerdVeldje++;
                                    TekenVeldjes();
                                }
                                break;
                            case ConsoleKey.LeftArrow:
                                if (huidigGeselecteerdVeldje > 0)
                                {
                                    huidigGeselecteerdVeldje--;
                                    TekenVeldjes();
                                }
                                break;
                            case ConsoleKey.Enter:
                                if (isVeldjesZichtbaar[huidigGeselecteerdVeldje] == false) { 
                                    isVeldjesZichtbaar[huidigGeselecteerdVeldje] = true;
                                    if (laatstOmgedraaidveldjeWaarde == GEEN_SELECTIE)
                                    {
                                        laatstOmgedraaidveldjeWaarde = veldjes[ huidigGeselecteerdVeldje];
                                        //Debug.WriteLine(laatstOmgedraaidveldjeWaarde);
                                        TekenVeldjes();
                                    }
                                    else
                                    {

                                        Console.SetCursorPosition(26, 20);
                                        Console.WriteLine("                                             ");

                                        if (laatstOmgedraaidveldjeWaarde != veldjes[huidigGeselecteerdVeldje])
                                        {
                                            Console.SetCursorPosition(26, 22);
                                            Console.WriteLine("Je verliest");
                                            Thread.Sleep(1000);
                                            MaakAlleVeldjesVisible(true);
                                            TekenVeldjes();
                                            Console.SetCursorPosition(26, 24);
                                            Console.WriteLine("druk een toets om verder te gaan");
                                            Console.ReadKey();
                                            eindeRondjeLoop = true; //uit de rondjesloop
                                            eindeInputloop = true; //uit de inputloop
                                        }
                                        else
                                        {
                                            //kijken of alle veldjes zichtbaar zijn
                                            bool alleVeldjesZijnZichtbaar= true;
                                            for (int i = 0; i < veldjes.Length; i++)
                                            {
                                                if ( !isVeldjesZichtbaar[i])
                                                {
                                                    alleVeldjesZijnZichtbaar = false; break;
                                                }
                                            }
                                            if (alleVeldjesZijnZichtbaar)
                                            {
                                                TekenVeldjes();
                                                UpdateSaldo(INZET+(INZET/2));
                                                Console.SetCursorPosition(26, 22);
                                                Console.WriteLine("Je wint");
                                                Console.SetCursorPosition(26, 24);
                                                Console.WriteLine("druk een toets om verder te gaan");
                                                Console.ReadKey();
                                                eindeRondjeLoop = true; //uit de rondjesloop
                                                eindeInputloop = true; //uit de inputloop
                                            }
                                            else
                                            {
                                                laatstOmgedraaidveldjeWaarde = GEEN_SELECTIE;
                                                //Debug.WriteLine(laatstOmgedraaidveldjeWaarde);
                                                TekenVeldjes();
                                            }

                                        }
                                    }
                                }
                                break;
                            default:
                                //dummy
                                break;
                        }
                    }


                }//einde rondjesloop

            } while (true);//einde gameloop



            //hier wordt de methode verlaten, de rest hieronder zijn enkel innermethodes
            //--------------------------------------------------------------------------
            Console.Clear();






            //----------------------------------------------------------------------------inner functies
            void TekenIcoontje(string[,] aIcoontjes,int aIcoontjeIndex, int aX, int aY, ConsoleColor aKleur)
            {
                ConsoleColor kleurHerstellen = Console.ForegroundColor;
                Console.ForegroundColor = aKleur;
                Console.SetCursorPosition(aX, aY);
                Console.WriteLine(aIcoontjes[aIcoontjeIndex,0]);
                Console.SetCursorPosition(aX, ++aY);
                Console.WriteLine(aIcoontjes[aIcoontjeIndex, 1]);
                Console.SetCursorPosition(aX, ++aY);
                Console.WriteLine(aIcoontjes[aIcoontjeIndex, 2]);
                Console.ForegroundColor = kleurHerstellen;

            }
            string[,] LaadSlotsEnMemoryIcoontjes()
            {
                string[,] terug = new string[7, 3]; //7icoontjes 8 lijnen
                int iconTeller = 0;
                int lijnteller = 0;

                //bron: https://stackoverflow.com/questions/8037070/whats-the-fastest-way-to-read-a-text-file-line-by-line
                const Int32 BufferSize = 128;
                using (var fileStream = File.OpenRead(@"AsciArts\memoryAndSlotsIcons.txt"))
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                {
                    String line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        terug[iconTeller++ / 3, lijnteller++ % 3] = line;
                        //Debug.WriteLine(iconTeller);
                    }
                }
                return terug;
            }
            void UpdateSaldo(int aWaarde)
            {
                if (aWaarde != 0)
                {
                    //in het tekstbestand aanpassen
                    Data_UpdateSaldo(aGebruiker[(int)Gebruiker.Gebruikersnaam], aWaarde);
                    //ook in het geheugen aanpassen
                    int saldo = Convert.ToInt32(aGebruiker[(int)Gebruiker.Saldo]);
                    saldo += aWaarde;
                    aGebruiker[(int)Gebruiker.Saldo] = saldo.ToString();
                    TekenSpelerInfo();
                }
            }
            void TekenSpelerInfo()
            {
                string naamTekst = "Ingelogd als: " + aGebruiker[(int)Gebruiker.Gebruikersnaam];
                string saldoTekst = "Saldo: " + aGebruiker[(int)Gebruiker.Saldo];
                Console.SetCursorPosition(Console.WindowWidth - naamTekst.Length - 2, 1);
                Console.WriteLine(naamTekst);
                Console.SetCursorPosition(Console.WindowWidth - saldoTekst.Length - 2, 2);
                Console.WriteLine(saldoTekst);
            }
            void ShuffleVeldjes(int aantalShuffles)
            {
                Random rand = new Random();
                for (int shuffleteller = 0; shuffleteller < aantalShuffles; shuffleteller++)
                {
                    int random1 = rand.Next(veldjes.Length);
                    int random2 = rand.Next(veldjes.Length);
                    int temp = veldjes[random1];
                    veldjes[random1] = veldjes[random2];
                    veldjes[random2] = temp;
                }
                huidigGeselecteerdVeldje = -1;

                //----------------testing
                for (int i = 0; i < veldjes.Length; i++) Debug.Write(veldjes[i]);
                Debug.Write("   ");
                //-----------------------
            }
            void MaakAlleVeldjesVisible(bool isVisible)
            {
                for (int i = 0; i < isVeldjesZichtbaar.Length; i++)
                {
                    isVeldjesZichtbaar[i] = isVisible;
                }
            }
            void TekenVeldjes()
            {
                //Debug.Write("k");
                ConsoleColor terugTeZettenVoorgrondkleur = Console.ForegroundColor;

                for (int i = 0; i < veldjes.Length; i++)
                {
                    int cursorX = X_MARGIN + (i * X_BEGIN_TSS_VELDJES);
                    int cursorY = Y_MARGIN;
                    Console.SetCursorPosition(cursorX, cursorY);

                    if (isVeldjesZichtbaar[i])
                    {
                        Console.ForegroundColor = TEKENINGKLEUREN[veldjes[i]];
                        //Console.Write(veldjes[i].ToString() + " ");
                        TekenIcoontje(icoontjes, veldjes[i], Console.CursorLeft, Console.CursorTop, TEKENINGKLEUREN[veldjes[i]]);
                    }
                    else
                    {
                        Console.ForegroundColor = KLEUR_VELDJE_NIET_OMGEDRAAID;
                        Console.SetCursorPosition(cursorX, cursorY);
                        Console.Write(" ?    ? ");
                        Console.SetCursorPosition(cursorX, cursorY+1);
                        Console.Write("  ???   ");
                        Console.SetCursorPosition(cursorX, cursorY+2);
                        Console.Write(" ?    ? ");
                    }
                    if (huidigGeselecteerdVeldje == i)
                    {
                        Console.ForegroundColor = KLEUR_VELDJE_NIET_OMGEDRAAID_WEL_GESELECTEERD;
                    }
                    else
                    {
                        Console.ForegroundColor = Console.BackgroundColor;
                    }
                    Console.SetCursorPosition(X_MARGIN + (i * X_BEGIN_TSS_VELDJES), Y_MARGIN +4);
                    Console.WriteLine("________");
                    Console.SetCursorPosition(X_MARGIN + (i * X_BEGIN_TSS_VELDJES), Y_MARGIN - 2);
                    Console.WriteLine("________");


                }
                //kleur terug zoals voorheen
                Console.ForegroundColor = terugTeZettenVoorgrondkleur;
            }
        }
        //=====================================================================================================================
        static void ToonScherm_Slots(string[] aGebruiker)
        {
            Console.Clear();

            const int INZET = 5;

            const int SLOTKADER_X = 28;
            const int SLOTKADER_Y = 9;
            const int INFO_ICOONTJES_X = 78;
            const int INFO_ICOONTJES_Y = 6;

            //------------------------------------------------------afblijven
            //-------deze hard coderen voor performantie en duidelijkere code
            int[] X_ICOONTJES_IN_KADER_WIEL = { 
                SLOTKADER_X + 2,
                SLOTKADER_X +13,
                SLOTKADER_X +24,

            };
            int[] Y_ICOONTJES_IN_KADER_WIEL =
            {
                SLOTKADER_Y + 11,
                SLOTKADER_Y + 10,
                SLOTKADER_Y + 9,
                SLOTKADER_Y + 8,
                SLOTKADER_Y + 7,
                SLOTKADER_Y + 6,
                SLOTKADER_Y + 5,
                SLOTKADER_Y + 4,
                SLOTKADER_Y + 3,
                SLOTKADER_Y + 2,
                SLOTKADER_Y + 1,
            };
            //------------------------------------------------
            string[,] icoontjes = LaadSlotsEnMemoryIcoontjes();

            int[,] arrWiel = new int[3, 75];
            int[] draaienTot = new int[arrWiel.GetLength(0)]; 

            double[] uitbetalingen = { 0.6, 1, 1.4, 2, 4 ,10,20 };
            
            int[] randomTePakkenIcoontjes = {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,  
                2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 
                3, 3, 3, 3, 3, 3, 3, 
                4, 4, 4, 4, 
                5, 5,5, 
                6,6,
            };
            //int[] randomTePakkenIcoontjes = {
            //    0, 0, 0, 0,
            //    1, 1, 1, 1,
            //    2, 2, 2,
            //    3, 3,
            //    4, 4,
            //    5, 5,
            //    6, 6,
            //};
            ConsoleColor[] TEKENINGKLEUREN = {
                ConsoleColor.DarkGray,
                ConsoleColor.Gray,
                ConsoleColor.Red,
                ConsoleColor.Magenta,
                ConsoleColor.Cyan,
                ConsoleColor.Green,
                ConsoleColor.Yellow,
            };



            ConsoleKeyInfo cki;
            bool isDoorspelen = true;
            //-----------------------------------------------------------------------------------------
            Console.SetCursorPosition(24, 10);
            Console.WriteLine("wil je megaSlots spelen?");
            Console.SetCursorPosition(20, 12);
            Console.WriteLine("druke eender welke toets, [esc] om te verlaten");

            cki = Console.ReadKey(true); //wacht op input
            if (cki.Key == ConsoleKey.Escape) isDoorspelen=false; //lus niet ingaan en methode beeindigen

            Console.Clear();
            TekenUitBestand(@"AsciArts\TompieSlots.txt",ConsoleColor.DarkYellow,2);
            TekenSpelerInfo();

            TekenIcoonScoreInfo();
            TekenSlotKader();

            //er wat random sloticoontjes invlammen
            Random rndBegin = new Random();


            while (isDoorspelen)  //gameloop
            {

                bool eindeInputLoop = false;
                while (!eindeInputLoop)
                {

                    Console.SetCursorPosition(30, 25);
                    Console.WriteLine("druk een toets om  te spinnen");
                    Console.SetCursorPosition(30, 27);
                    Console.WriteLine("[esc] om de zaak te verlaten");

                    cki = Console.ReadKey(true); //wacht op input
                    switch (cki.Key)
                    {
                        case ConsoleKey.Enter:
                            if (Convert.ToInt32(aGebruiker[(int)Gebruiker.Saldo]) < INZET)
                            {
                                Console.Clear();
                                TekenSpelerInfo();
                                Console.WriteLine("u hebt niet genoeg budget, ga naar de bank en kom terug");
                                Console.WriteLine("druk een toets om  de zaak dringend te verlaten");
                                Console.ReadKey();
                                isDoorspelen = false;
                            }
                            else
                            {
                                //wissen van lijnen
                                Console.SetCursorPosition(30, 22);
                                Console.WriteLine("                             ");
                                Console.SetCursorPosition(29, 25);
                                Console.WriteLine("                               ");
                                Console.SetCursorPosition(29, 27);
                                Console.WriteLine("                               ");
                            }
                            eindeInputLoop = true;
                            break;
                        case ConsoleKey.Escape:
                            eindeInputLoop = true;
                            isDoorspelen = false;
                            break;
                    }

                    if (isDoorspelen)
                    {
                        UpdateSaldo(-INZET);
                        //================================================draaien met die handel

                        Console.SetCursorPosition(40, 22);
                        Console.WriteLine("veel geluk");

                        Spin();
                        int winst = BerekenWinst();
                        if (winst != 0)
                        {
                            Console.SetCursorPosition(30, 22);
                            Console.WriteLine("Gefiliciteerd, u wint " + winst + "eur");
                            UpdateSaldo(winst);
                            Console.Beep(400, 500);
                            Thread.Sleep(2000);
                        }
                        else
                        {
                            Console.SetCursorPosition(30, 22);
                            Console.WriteLine("                             ");
                            Thread.Sleep(700);
                        }

                        //==========================================einde draaien met die handel
                    }
                }

            };//einde gameloop

            //hier wordt de methode verlaten, de rest hieronder zijn enkel innermethodes
            //--------------------------------------------------------------------------
            Console.Clear();






            //----------------------------------------------------------------------------inner functies
            int BerekenWinst()
            {
                double terug = 0;

                //ff array'ke aanmaken voor gemakkelijk te berekenen
                int[,] tmp = new int[3, 3]; //x,y
                tmp[0, 0] = arrWiel[0, draaienTot[0] + 2];
                tmp[1, 0] = arrWiel[1, draaienTot[1] + 2];
                tmp[2, 0] = arrWiel[2, draaienTot[2] + 2];
                tmp[0, 1] = arrWiel[0, draaienTot[0] + 1];
                tmp[1, 1] = arrWiel[1, draaienTot[1] + 1];
                tmp[2, 1] = arrWiel[2, draaienTot[2] + 1];
                tmp[0, 2] = arrWiel[0, draaienTot[0] + 0];
                tmp[1, 2] = arrWiel[1, draaienTot[1] + 0];
                tmp[2, 2] = arrWiel[2, draaienTot[2] + 0];

                //===============================================test
                Debug.WriteLine("");
                for (int y = 0; y < 3; y++)
                {
                    for (int x = 0; x < 3; x++)
                    {
                        Debug.Write(tmp[x, y] + " ");
                    }
                    Debug.WriteLine(" ");
                }
                //==========================================eindetest
                if (tmp[0, 0] == tmp[1, 0] && tmp[0, 0] == tmp[2, 0])
                {
                    Debug.WriteLine("win: bovenste lijn");
                    terug += uitbetalingen[tmp[0, 0]] * INZET;
                }
                if (tmp[0, 1] == tmp[1, 1] && tmp[2, 1] == tmp[0, 1]){
                    Debug.WriteLine("win: middelste lijn");
                    terug += uitbetalingen[tmp[0, 1]] * INZET;
                }
                if (tmp[0, 2] == tmp[1, 2] && tmp[1, 2] == tmp[2, 2]){
                    Debug.WriteLine("win: onderste lijn");
                    terug += uitbetalingen[tmp[0, 2]] * INZET;
                }
                if (tmp[0, 0] == tmp[1, 1] && tmp[1, 1] == tmp[2, 2]){
                    Debug.WriteLine("win: schuin nr onder");
                    terug += uitbetalingen[tmp[0, 0]] * INZET;
                }
                if (tmp[0, 2] == tmp[1, 1] && tmp[2, 0] == tmp[0, 2]) {
                    Debug.WriteLine("win: schuin nr boven");
                    terug += uitbetalingen[tmp[0, 2]] * INZET;
                }
                return Convert.ToInt32( terug);
            }
            void Spin()
            {
                shuffleWielen();

                string[] wiel1strings = new string[arrWiel.GetLength(1)*4];
                ConsoleColor[] wiel1kleuren = new ConsoleColor[wiel1strings.Length];
                string[] wiel2strings = new string[arrWiel.GetLength(1) * 4];
                ConsoleColor[] wiel2kleuren = new ConsoleColor[wiel2strings.Length];
                string[] wiel3strings = new string[arrWiel.GetLength(1) * 4];
                ConsoleColor[] wiel3kleuren = new ConsoleColor[wiel3strings.Length];


                //initaliseren van very veel stringskes
                for (int i = 0; i < arrWiel.GetLength(1); i++)
                {
                    wiel1strings[(i * 4) + 3] = "        ";
                    wiel1strings[(i * 4) + 2] = icoontjes[arrWiel[0, i], 0];
                    wiel1strings[(i * 4) + 1] = icoontjes[arrWiel[0, i], 1];
                    wiel1strings[(i * 4) + 0] = icoontjes[arrWiel[0, i], 2];
                    //----------------------------------------------------------
                    wiel1kleuren[(i * 4) + 3] = ConsoleColor.Black;
                    wiel1kleuren[(i * 4) + 2] = TEKENINGKLEUREN[arrWiel[0, i]];
                    wiel1kleuren[(i * 4) + 1] = TEKENINGKLEUREN[arrWiel[0, i]];
                    wiel1kleuren[(i * 4) + 0] = TEKENINGKLEUREN[arrWiel[0, i]];
                    //=============================================================
                    wiel2strings[(i * 4) + 3] = "        ";
                    wiel2strings[(i * 4) + 2] = icoontjes[arrWiel[1, i], 0];
                    wiel2strings[(i * 4) + 1] = icoontjes[arrWiel[1, i], 1];
                    wiel2strings[(i * 4) + 0] = icoontjes[arrWiel[1, i], 2];
                    //----------------------------------------------------------
                    wiel2kleuren[(i * 4) + 3] = ConsoleColor.Black;
                    wiel2kleuren[(i * 4) + 2] = TEKENINGKLEUREN[arrWiel[1, i]];
                    wiel2kleuren[(i * 4) + 1] = TEKENINGKLEUREN[arrWiel[1, i]];
                    wiel2kleuren[(i * 4) + 0] = TEKENINGKLEUREN[arrWiel[1, i]];
                    //=============================================================
                    wiel3strings[(i * 4) + 3] = "        ";
                    wiel3strings[(i * 4) + 2] = icoontjes[arrWiel[2, i], 0];
                    wiel3strings[(i * 4) + 1] = icoontjes[arrWiel[2, i], 1];
                    wiel3strings[(i * 4) + 0] = icoontjes[arrWiel[2, i], 2];
                    //----------------------------------------------------------
                    wiel3kleuren[(i * 4) + 3] = ConsoleColor.Black;
                    wiel3kleuren[(i * 4) + 2] = TEKENINGKLEUREN[arrWiel[2, i]];
                    wiel3kleuren[(i * 4) + 1] = TEKENINGKLEUREN[arrWiel[2, i]];
                    wiel3kleuren[(i * 4) + 0] = TEKENINGKLEUREN[arrWiel[2, i]];
                }

                //********************************************************************
                // DE EFFECTIEVE ANNIMATIE
                //********************************************************************
                ConsoleColor kleurHerstellen = Console.ForegroundColor;
                int currentWielStringCursor = 0;
                do
                {
                    if (draaienTot[0] * 4 >= currentWielStringCursor )
                    {
                        //alle 11 regels tekenen van wiel 1
                        for (int i = 0; i < 11; i++)
                        {
                            Console.ForegroundColor = wiel1kleuren[i + currentWielStringCursor];
                            Console.SetCursorPosition(X_ICOONTJES_IN_KADER_WIEL[0], Y_ICOONTJES_IN_KADER_WIEL[i]);
                            Console.WriteLine(wiel1strings[i + currentWielStringCursor]);
                        }
                    }
                    if (draaienTot[1] * 4 >= currentWielStringCursor)
                    {
                        //alle 11 regels tekenen van wiel 2
                        for (int i = 0; i < 11; i++)
                        {
                            Console.ForegroundColor = wiel2kleuren[i + currentWielStringCursor];
                            Console.SetCursorPosition(X_ICOONTJES_IN_KADER_WIEL[1], Y_ICOONTJES_IN_KADER_WIEL[i]);
                            Console.WriteLine(wiel2strings[i + currentWielStringCursor]);
                        }
                    }
                    if (draaienTot[2] * 4 >= currentWielStringCursor)
                    {
                        //alle 11 regels tekenen van wiel 3
                        for (int i = 0; i < 11; i++)
                        {
                            Console.ForegroundColor = wiel3kleuren[i + currentWielStringCursor];
                            Console.SetCursorPosition(X_ICOONTJES_IN_KADER_WIEL[2], Y_ICOONTJES_IN_KADER_WIEL[i]);
                            Console.WriteLine(wiel3strings[i + currentWielStringCursor]);
                        }
                    }
                    else
                    {
                        break;
                    }
                    currentWielStringCursor++;
                    Thread.Sleep(8);
                } while (true);//einde animatie

                Console.ForegroundColor = kleurHerstellen;


            }
            void shuffleWielen()
            {
                Random rnd = new Random();
                for (int wiel = 0; wiel < arrWiel.GetLength(0); wiel++)
                {
                    for (int vlakjeInWiel = 0; vlakjeInWiel < arrWiel.GetLength(1); vlakjeInWiel++)
                    {
                        arrWiel[wiel, vlakjeInWiel] = randomTePakkenIcoontjes[rnd.Next(randomTePakkenIcoontjes.Length)];
                    }
                }
                draaienTot[0] = rnd.Next((arrWiel.GetLength(1) / 15),((arrWiel.GetLength(1) /2)-1));
                draaienTot[1] = draaienTot[0] + rnd.Next(((arrWiel.GetLength(1) / 4) - 1));
                draaienTot[2] = draaienTot[1] + rnd.Next(((arrWiel.GetLength(1) / 4) - 1));
                Debug.Write("\nwiel 0: draaienTot: " + draaienTot[0] + ": ");
                for (int i = 0; i < arrWiel.GetLength(1); i++) Debug.Write(arrWiel[0, i]);
                Debug.Write("\nwiel 1: draaienTot: " + draaienTot[1] + ": ");
                for (int i = 0; i < arrWiel.GetLength(1); i++) Debug.Write(arrWiel[1, i]);
                Debug.Write("\nwiel 2: draaienTot: " + draaienTot[2] + ": ");
                for (int i = 0; i < arrWiel.GetLength(1); i++) Debug.Write(arrWiel[2, i]);
            }

            void TekenIcoontje(int aIcoontjeIndex, int aX, int aY, ConsoleColor aKleur)
            {
                ConsoleColor kleurHerstellen = Console.ForegroundColor;
                Console.ForegroundColor = aKleur;
                Console.SetCursorPosition(aX, aY);
                Console.WriteLine(icoontjes[aIcoontjeIndex, 0]);
                Console.SetCursorPosition(aX, ++aY);
                Console.WriteLine(icoontjes[aIcoontjeIndex, 1]);
                Console.SetCursorPosition(aX, ++aY);
                Console.WriteLine(icoontjes[aIcoontjeIndex, 2]);
                Console.ForegroundColor = kleurHerstellen;

            }
            void TekenIcoonScoreInfo()
            {
                ConsoleColor kleurHerstellen = Console.ForegroundColor;
                for (int i = 0; i < icoontjes.GetLength(0); i++)
                {
                    TekenIcoontje(i, INFO_ICOONTJES_X, INFO_ICOONTJES_Y + (i * 3), TEKENINGKLEUREN[i]);
                    Console.SetCursorPosition(INFO_ICOONTJES_X + 10, INFO_ICOONTJES_Y + (i * 3)+1);
                    Console.ForegroundColor = TEKENINGKLEUREN[i];
                    Console.WriteLine(Convert.ToInt32( INZET * uitbetalingen[i]).ToString() + "eur per lijn");
                    Console.ForegroundColor = kleurHerstellen;
                }
            }
            void TekenSlotKader()
            {
                const int HOOGTE = 12;//


                Console.SetCursorPosition(SLOTKADER_X + 10, SLOTKADER_Y - 3);
                Console.WriteLine("     '*`");
                Console.SetCursorPosition(SLOTKADER_X +10, SLOTKADER_Y-2);
                Console.WriteLine("    (o o)");
                Console.SetCursorPosition(SLOTKADER_X +10, SLOTKADER_Y-1);
                Console.WriteLine("ooO--(_)--Ooo");

                Console.SetCursorPosition(SLOTKADER_X, SLOTKADER_Y);
                Console.WriteLine("╔═════════╗╔═════════╗╔═════════╗");
                for (int i = 1; i < HOOGTE; i++)
                {
                    Console.SetCursorPosition(SLOTKADER_X, SLOTKADER_Y+i);
                    Console.WriteLine("║         ║║         ║║         ║");
                }
                Console.SetCursorPosition(SLOTKADER_X, SLOTKADER_Y + HOOGTE );
                Console.WriteLine("╚═════════╝╚═════════╝╚═════════╝");

            }
            void UpdateSaldo(int aWaarde)
            {
                if (aWaarde != 0)
                {
                    //in het tekstbestand aanpassen
                    Data_UpdateSaldo(aGebruiker[(int)Gebruiker.Gebruikersnaam], aWaarde);
                    //ook in het geheugen aanpassen
                    int saldo = Convert.ToInt32(aGebruiker[(int)Gebruiker.Saldo]);
                    saldo += aWaarde;
                    aGebruiker[(int)Gebruiker.Saldo] = saldo.ToString();
                    TekenSpelerInfo();
                }
            }
            void TekenSpelerInfo()
            {
                string naamTekst = "Ingelogd als: " + aGebruiker[(int)Gebruiker.Gebruikersnaam];
                string saldoTekst = "Saldo: " + aGebruiker[(int)Gebruiker.Saldo];
                Console.SetCursorPosition(Console.WindowWidth - naamTekst.Length - 2, 1);
                Console.WriteLine(naamTekst);
                Console.SetCursorPosition(Console.WindowWidth - saldoTekst.Length - 2, 2);
                Console.WriteLine(saldoTekst);
            }
            string[,] LaadSlotsEnMemoryIcoontjes()
            {
                string[,] terug = new string[7, 3]; //7icoontjes 8 lijnen
                int iconTeller = 0;
                int lijnteller = 0;

                //bron: https://stackoverflow.com/questions/8037070/whats-the-fastest-way-to-read-a-text-file-line-by-line
                const Int32 BufferSize = 128;
                using (var fileStream = File.OpenRead(@"AsciArts\memoryAndSlotsIcons.txt"))
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                {
                    String line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        terug[iconTeller++ / 3, lijnteller++ % 3] = line;
                        //Debug.WriteLine(iconTeller);
                    }
                }
                return terug;
            }
        }
        //=====================================================================================================================


        #endregion =======================================================================================================================




        #region ================================================================================================================= Database
        /// <summary>
        /// returnd 1 als gebruiker correct is toegevoegd
        /// returnd -1 als gebruiker reeds bestaat
        /// returnd -2 als paswoord invalid is
        /// returnd -3 als gebruikersnaam invalid is
        /// </summary>
        static int Data_AddUser(string aGebruikersnaam, string aPaswoord, int aSaldo)
        {
            //TODO: username bestaat al nog implementeren en -1 teruggeven

            if (!IsValidGebruikersnaam(aGebruikersnaam))
                return -3;
            if (!IsValidPaswoord(aPaswoord))
                return -2;

            if (Data_GebruikerBestaat(aGebruikersnaam))
                return -3;


            string encriptedPaswoord = EncriptString(aPaswoord, aGebruikersnaam);

            using (StreamWriter writer = File.AppendText(PAD_GEBRUIKERS))
            {
                writer.WriteLine(aGebruikersnaam + FILE_SEPERATOR + encriptedPaswoord + FILE_SEPERATOR + aSaldo);
            }
            return 1;
        }
        //=================================================================================================================
        /// <summary>
        /// geeft een array terug waarin gebruikersnaam, paswoord, inlogtijd en saldo staan
        /// indien niet gevonden of paswoord komt niet overeen dan returnd null
        /// </summary>
        /// <returns></returns>
        static string[] Data_GetUser(string aGebruikersnaam, string aPaswoord)
        {
            //bron: https://stackoverflow.com/questions/8037070/whats-the-fastest-way-to-read-a-text-file-line-by-line
            //volgens de bron zou dit de snelste mannier zijn om een bestand te lezen met een buffersize van 128
            //de Encoding UTF8 heb ik nodig om de 8bits karakters die ik heb weggeschreven juist te kunnen inlezen

            const Int32 BufferSize = 128;
            using (var fileStream = File.OpenRead(PAD_GEBRUIKERS))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
            {
                String line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    int positieInLijn = 0;
                    string gebrNaam = string.Empty;
                    string encryptedPasw = string.Empty;
                    string decryptedPasw = string.Empty;
                    string saldo = string.Empty;

                    //gebruikersnaam extraheren
                    while (line[positieInLijn] != FILE_SEPERATOR)
                    {
                        gebrNaam += line[positieInLijn];
                        positieInLijn++;
                    }

                    //als we de gebruiker hebben gevonden
                    if (aGebruikersnaam.ToLower() == gebrNaam.ToLower())
                    {
                        //paswoord extraheren
                        encryptedPasw = line.Substring(positieInLijn + 1, LENGTE_ENCRYPT_ZONDER_SOM + 2);

                        //paswoord decrypten
                        decryptedPasw = DecriptString(encryptedPasw, gebrNaam);

                        //paswoord komt niet overeen
                        if (aPaswoord != decryptedPasw)
                        {
                            return null;
                        }

                        //user gevonden met juiste paswoord en gebruikersnaam, saldo ook nog toevoegen
                        else
                        {
                            //saldo extraheren
                            positieInLijn += LENGTE_ENCRYPT_ZONDER_SOM + 4;
                            for (; positieInLijn < line.Length; positieInLijn++)
                            {
                                saldo += line[positieInLijn];
                            }

                            string[] gebruikerTerug = new string[4];
                            gebruikerTerug[(int)Gebruiker.Gebruikersnaam] = gebrNaam;
                            gebruikerTerug[(int)Gebruiker.Paswoord] = decryptedPasw;
                            gebruikerTerug[(int)Gebruiker.Saldo] = saldo;
                            gebruikerTerug[(int)Gebruiker.InlogTijd] = DateTime.Now.ToString();

                            //en returnen (verlaat ook de lus en stopt dus om het bestand onnodig verder uit te lezen
                            return gebruikerTerug;
                        }

                    }
                }
            }

            return null;
        }

        //=================================================================================================================
        static bool Data_GebruikerBestaat(string aGebruikersnaam)
        {



            //bron: https://stackoverflow.com/questions/8037070/whats-the-fastest-way-to-read-a-text-file-line-by-line
            const Int32 BufferSize = 128;
            using (var fileStream = File.OpenRead(PAD_GEBRUIKERS))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
            {
                String line;

                while ((line = streamReader.ReadLine()) != null)
                {
                    string gebrNaam = string.Empty;
                    int positieInLijn = 0;

                    while (line[positieInLijn] != FILE_SEPERATOR)
                    {
                        gebrNaam += line[positieInLijn];
                        positieInLijn++;
                    }

                    if (aGebruikersnaam.ToLower() == gebrNaam.ToLower())
                        return true;

                }
            }
            return false;
        }


        //=================================================================================================================
        static bool Data_GebruikerVerwijderen(string aGebruikersnaam)
        {


            //schrijven naar een tijdelijk bestand, en daarna het tijdelijke vervangen door het oude
            string tmpPad = "tmp_" + PAD_GEBRUIKERS;
            bool isVerwijderd = false;

            //eventueel hier eerst chekken of de gebruiker bestaat, 
            //aan te raden met grote bestanden

            using (var leesStream = new StreamReader(PAD_GEBRUIKERS))
            using (var schrijfStream = new StreamWriter(tmpPad))
            {
                string line;

                while ((line = leesStream.ReadLine()) != null)
                {
                    string gebrNaam = string.Empty;
                    int positieInLijn = 0;

                    while (line[positieInLijn] != FILE_SEPERATOR)
                    {
                        gebrNaam += line[positieInLijn];
                        positieInLijn++;
                    }

                    if (gebrNaam.ToLower() == aGebruikersnaam.ToLower())
                        isVerwijderd = true;
                    else
                        schrijfStream.WriteLine(line);
                }
            }

            //het originele bestand terug herstellen
            File.Delete(PAD_GEBRUIKERS);
            File.Move(tmpPad, PAD_GEBRUIKERS); //van -> naar

            return isVerwijderd;
        }
        //=================================================================================================================
        /// <summary>
        /// array van strings met gebruikersnamen
        /// </summary>
        /// <returns></returns>
        static string[] Data_GetAlleGebruikersnamen()
        {
            string alleGebruikersnamen = string.Empty;

            //bron: https://stackoverflow.com/questions/8037070/whats-the-fastest-way-to-read-a-text-file-line-by-line
            const Int32 BufferSize = 128;
            using (var fileStream = File.OpenRead(PAD_GEBRUIKERS))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
            {
                String line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    string gebrNaam = string.Empty;
                    int positieInLijn = 0;

                    while (line[positieInLijn] != FILE_SEPERATOR)
                    {
                        gebrNaam += line[positieInLijn];
                        positieInLijn++;
                    }
                    alleGebruikersnamen += gebrNaam + '#';
                }
            }
            if (alleGebruikersnamen == string.Empty)
                return null;
            else
            {
                return alleGebruikersnamen.Substring(0, alleGebruikersnamen.Length - 1).Split('#');
            }
        }
        //=================================================================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static bool Data_UpdateSaldo(string aGebruikersnaam, int aWaarde)
        {
            //schrijven naar een tijdelijk bestand, en daarna het tijdelijke vervangen door het oude
            string tmpPad = "tmp_" + PAD_GEBRUIKERS;
            bool isGeupdated = false;
            string saldo = string.Empty;

            //eventueel hier eerst chekken of de gebruiker bestaat, 
            //aan te raden met grote bestanden

            using (var leesStream = new StreamReader(PAD_GEBRUIKERS))
            using (var schrijfStream = new StreamWriter(tmpPad))
            {
                string line;

                while ((line = leesStream.ReadLine()) != null)
                {
                    string gebrNaam = string.Empty;
                    int positieInLijn = 0;

                    while (line[positieInLijn] != FILE_SEPERATOR)
                    {
                        gebrNaam += line[positieInLijn];
                        positieInLijn++;
                    }

                    if (gebrNaam.ToLower() == aGebruikersnaam.ToLower())
                    {
                        string newline = string.Empty;
                        //int positieVanSaldo = positieInLijn + LENGTE_ENCRYPT_ZONDER_SOM + 4;

                        newline = line.Substring(0, positieInLijn + LENGTE_ENCRYPT_ZONDER_SOM + 4);


                        //saldo extraheren
                        positieInLijn += LENGTE_ENCRYPT_ZONDER_SOM + 4;
                        for (; positieInLijn < line.Length; positieInLijn++)
                        {
                            saldo += line[positieInLijn];
                        }

                        int intSaldo = Convert.ToInt32(saldo);
                        intSaldo += aWaarde; //hier waarde bij tellen vh argument (of aftrekken indien negatief)

                        newline += intSaldo;


                        schrijfStream.WriteLine(newline);
                        isGeupdated = true;
                    }
                    else
                        schrijfStream.WriteLine(line);
                }
            }

            //het originele bestand terug herstellen
            File.Delete(PAD_GEBRUIKERS);
            File.Move(tmpPad, PAD_GEBRUIKERS); //van -> naar

            return isGeupdated;
        }
        //=================================================================================================================
        /// <summary>
        ///  returns: 
        ///  1 succesvol geupdated
        ///  -2 als oud paswoord niet juist is
        ///  -3 als gebruiker niet is gevonden
        ///  -5 als nieuw paswoord niet valid is
        /// </summary>
        static int Data_UpdatePaswoord(string aGebruikersnaam, string aOudpaswoord, string aNieuwPaswoord)
        {
            if ( ! IsValidPaswoord(aNieuwPaswoord)) return -5;

            string tmpPad = "tmp_" + PAD_GEBRUIKERS;
            int updatStatus = -3;



            //eventueel hier eerst chekken of de gebruiker bestaat, 
            //aan te raden met grote bestanden

            using (var leesStream = new StreamReader(PAD_GEBRUIKERS))
            using (var schrijfStream = new StreamWriter(tmpPad))
            {
                string line;

                while ((line = leesStream.ReadLine()) != null)
                {
                    string gebrNaam = string.Empty;
                    int positieInLijn = 0;
                    string encryptedPasw = string.Empty;
                    string decryptedPasw = string.Empty;
                    string saldo = string.Empty;

                    while (line[positieInLijn] != FILE_SEPERATOR)
                    {
                        gebrNaam += line[positieInLijn];
                        positieInLijn++;
                    }

                    //gebruiker gevonden
                    if (gebrNaam.ToLower() == aGebruikersnaam.ToLower())
                    {
                        //paswoord extraheren
                        encryptedPasw = line.Substring(positieInLijn + 1, LENGTE_ENCRYPT_ZONDER_SOM + 2);

                        //paswoord decrypten
                        decryptedPasw = DecriptString(encryptedPasw, gebrNaam);

                        //paswoord komt niet overeen
                        if (aOudpaswoord != decryptedPasw)
                        {
                            schrijfStream.WriteLine(line);
                            updatStatus = -2;
                        }

                        //user gevonden met juiste paswoord en gebruikersnaam, saldo ook nog extraheren
                        else
                        {
                            //saldo extraheren
                            positieInLijn += LENGTE_ENCRYPT_ZONDER_SOM + 4;
                            for (; positieInLijn < line.Length; positieInLijn++)
                            {
                                saldo += line[positieInLijn];
                            }

                            //updaten die handel
                            string encriptedPaswoord = EncriptString(aNieuwPaswoord, aGebruikersnaam);


                            schrijfStream.WriteLine(aGebruikersnaam + FILE_SEPERATOR + encriptedPaswoord + FILE_SEPERATOR + saldo);
                            updatStatus = 1;
                        }
                    }
                    else
                        schrijfStream.WriteLine(line);
                }
            }

            //het originele bestand terug herstellen
            File.Delete(PAD_GEBRUIKERS);
            File.Move(tmpPad, PAD_GEBRUIKERS); //van -> naar

            return updatStatus;
        }

        //=================================================================================================================
        /// <summary>
        ///  returns: 
        ///  1 succesvol geupdated
        ///  !!!!! -2 (er uit gehaald)paswoord komt niet overeen met gebruiker (er uit gehaald) !!!!!
        ///  -3 als gebruiker niet gevonden wordt 
        ///  -4 als uw nieuw gebruikersnaam al bestaat
        ///  -5 als nieuwe gebruikersnaam niet valid is
        /// </summary>

        static int Data_UpdateGebruikersnaam(string aOudeGebruikersnaam, string aNieuweGebruikersnaam/*, string aPaswoord*/)
        {
            if (!IsValidGebruikersnaam(aNieuweGebruikersnaam)) return -5;
            
            string tmpPad = "tmp_" + PAD_GEBRUIKERS;
            int updatStatus = -3;

            if (Data_GebruikerBestaat(aNieuweGebruikersnaam))
                return -4;


            using (var leesStream = new StreamReader(PAD_GEBRUIKERS))
            using (var schrijfStream = new StreamWriter(tmpPad))
            {
                string line;

                while ((line = leesStream.ReadLine()) != null)
                {
                    string gebrNaam = string.Empty;
                    int positieInLijn = 0;
                    string encryptedPasw = string.Empty;
                    string decryptedPasw = string.Empty;
                    string saldo = string.Empty;

                    while (line[positieInLijn] != FILE_SEPERATOR)
                    {
                        gebrNaam += line[positieInLijn];
                        positieInLijn++;
                    }

                    //gebruiker gevonden
                    if (gebrNaam.ToLower() == aOudeGebruikersnaam.ToLower())
                    {
                        //paswoord extraheren
                        encryptedPasw = line.Substring(positieInLijn + 1, LENGTE_ENCRYPT_ZONDER_SOM + 2);

                        //paswoord decrypten
                        decryptedPasw = DecriptString(encryptedPasw, gebrNaam);

                        ////paswoord komt niet overeen
                        //if (aPaswoord != decryptedPasw)
                        //{
                        //    schrijfStream.WriteLine(line);
                        //    updatStatus = -2;
                        //}

                        ////user gevonden met juiste paswoord en gebruikersnaam,
                        //else
                        //{
                            //saldo extraheren
                            positieInLijn += LENGTE_ENCRYPT_ZONDER_SOM + 4;
                            for (; positieInLijn < line.Length; positieInLijn++)
                            {
                                saldo += line[positieInLijn];
                            }

                            //updaten die handel
                            string encriptedPaswoord = EncriptString(decryptedPasw, aNieuweGebruikersnaam);


                            schrijfStream.WriteLine(aNieuweGebruikersnaam + FILE_SEPERATOR + encriptedPaswoord + FILE_SEPERATOR + saldo);
                            updatStatus = 1;
                        //}
                    }
                    else
                        schrijfStream.WriteLine(line);
                }
            }

            //het originele bestand terug herstellen
            File.Delete(PAD_GEBRUIKERS);
            File.Move(tmpPad, PAD_GEBRUIKERS); //van -> naar

            return updatStatus;
        }


        #endregion =======================================================================================================================




        #region ==================================================================================================   Encription Decription
        static string EncriptString(string aString, string aSleutel)
        {

            Thread.Sleep(1); //om te testen, nog wegdoen
            Random rnd = new Random();
            string terug = aString;
            int asciSomSleutel = SomAscciVanKarakters(aSleutel);
            //string tussentegooienrommel = "!@#$%^&*()_+=[{]};:<>|./?,-";


            terug = Somvan2stringen(terug, "ma" + aSleutel + "tr");
            terug = DraaiStringOm(terug);

            //karaktertjes nog een beetje bijwerken
            //-------------------------------------
            string tmp = string.Empty;
            for (int i = 0; i < terug.Length; i++)
            {
                if (i % 4 == 0) tmp += (char)(terug[i] + 4);
                else if (i % 4 == 1) tmp += (char)(terug[i] + 1);
                else if (i % 4 == 2) tmp += (char)(terug[i] - 1);
                else if (i % 4 == 3) tmp += (char)(terug[i] - 4);
            }
            terug = tmp;

            //en we gooien er nog wat random charkes tussen :-)
            //--------------------------------------------------
            tmp = string.Empty;
            for (int i = 0; i < terug.Length; i++)
            {
                //int randomKarakter = rnd.Next(tussentegooienrommel.Length);
                //tmp += terug[i] + tussentegooienrommel[randomKarakter].ToString();
                tmp += terug[i] + ((char)(rnd.Next(220) + 34)).ToString();
            }
            terug = tmp;

            //  !!!  terug heeft nu altijd een lengte van 2 keer het paswoord !!!

            //en nog wat charkes onderaan om de encrypty altijd even
            //lang te maken
            //-------------------------------------------------------------
            tmp = string.Empty;
            for (int i = terug.Length; i < LENGTE_ENCRYPT_ZONDER_SOM; i++)
            {
                //int randomKarakter = rnd.Next(tussentegooienrommel.Length);
                //tmp += tussentegooienrommel[randomKarakter].ToString();
                tmp += ((char)(rnd.Next(220) + 34)).ToString();
            }
            terug += tmp;


            //we hebben de lengte van het paswoord nodig in de decrypty,
            //dit verstoppen we in de 2 charkes die we achteraan invoegen 
            //(die we dan in de decryptie berekenen)
            //-------------------------------------------------------------
            int randomInt = rnd.Next(70) + 180;
            int calculated = randomInt - (aString.Length * 3);
            terug += (char)randomInt;
            terug += (char)calculated;



            //-------------------------------------------- een ramp dat maar half werkt :-(
            //int asciSomSleutel = SomAscciVanKarakters(aSleutel);
            //string tmp = string.Empty;
            //for (int i = 0; i < terug.Length; i++)
            //{
            //    tmp += (char)(((int)terug[i] + asciSomSleutel) % 256);
            //    //Console.WriteLine((((int)terug[i]) % 256));
            //    //Console.WriteLine((((int)terug[i] + asciSomSleutel)));
            //    //Console.WriteLine((((int)terug[i] + asciSomSleutel) % 128));
            //    //Console.WriteLine((((int)terug[i] + asciSomSleutel) % 256));
            //    //Console.WriteLine("-------------------------");
            //}
            //terug = tmp;
            //----------------------------------------------------------------------------

            return @terug;
        }
        //==========================================================================================
        static string DecriptString(string aString, string aSleutel)
        {
            byte b1 = (byte)aString[aString.Length - 2];
            byte b2 = (byte)aString[aString.Length - 1];

            string terug = aString.Substring(0, ((b1 - b2) / 3) * 2);

            //randomkarakters eruit filteren
            //------------------------------
            string tmp = string.Empty;
            for (int i = 0; i < terug.Length; i++)
            {
                if (i % 2 == 0)
                    tmp += terug[i];
            }
            terug = tmp;

            //bewerkte karaktertjes terug herstellen
            //--------------------------------------
            tmp = string.Empty;
            for (int i = 0; i < terug.Length; i++)
            {
                if (i % 4 == 0) tmp += (char)(terug[i] - 4);
                else if (i % 4 == 1) tmp += (char)(terug[i] - 1);
                else if (i % 4 == 2) tmp += (char)(terug[i] + 1);
                else if (i % 4 == 3) tmp += (char)(terug[i] + 4);
            }
            terug = tmp;

            terug = DraaiStringOm(terug);
            terug = Verschilvan2stringen(terug, "ma" + aSleutel + "tr");


            //-------------------------------------------- een ramp dat maar half werkt :-(
            //int asciSomSleutel = SomAscciVanKarakters(aSleutel);
            //string tmp = string.Empty;
            //for (int i = 0; i < terug.Length; i++)
            //{
            //    tmp += (char)((((int)(terug[i] - asciSomSleutel )%256)+256)%256);

            //    //Console.WriteLine("====" + ((int)terug[i]));
            //    //Console.WriteLine("====" + (asciSomSleutel - (int)terug[i]));
            //    //Console.WriteLine("====" + ((int)terug[i] - asciSomSleutel));
            //    //Console.WriteLine(((terug[i] - asciSomSleutel) % 256)+256);
            //    //Console.WriteLine("---------------------------------------");
            //}
            //terug = tmp;
            //-----------------------------------------------------------------------------

            return @terug;
        }
        //==================================================================encryptyhelpers
        static string DraaiStringOm(string aString)
        {
            string terug = string.Empty;
            for (int i = aString.Length - 1; i >= 0; i--)
                terug += aString[i];
            return terug;
        }
        //--------------------------------------------------------------------------------
        static int SomAscciVanKarakters(string aString)
        {
            int terug = 0;
            for (int i = 0; i < aString.Length; i++)
            {
                terug += (int)aString[i];
            }
            return terug;
        }
        //-------------------------------------------------------------------------------
        static string Somvan2stringen(string aDeTerugTeGevenString, string aString2)
        {
            string terug = string.Empty;
            int kleinsteString = Math.Min(aDeTerugTeGevenString.Length, aString2.Length);
            char[] bewerkt = aDeTerugTeGevenString.ToCharArray();

            for (int i = 0; i < kleinsteString; i++)
            {
                bewerkt[i] = (char)(((int)aDeTerugTeGevenString[i] + (int)aString2[i]) % 256);
            }
            for (int i = 0; i < bewerkt.Length; i++)
            {
                terug += bewerkt[i];
            }
            return @terug;
        }
        //-------------------------------------------------------------------------------
        static string Verschilvan2stringen(string aDeTerugTeGevenString, string aString2)
        {
            string terug = string.Empty;
            int kleinsteString = Math.Min(aDeTerugTeGevenString.Length, aString2.Length);

            char[] bewerkt = aDeTerugTeGevenString.ToCharArray();
            for (int i = 0; i < kleinsteString; i++)
            {
                //bewerkt[i] = (char)(((int)aDeTerugTeGevenString[i] - (int)aString2[i]) % 256);
                bewerkt[i] = (char)((((int)(aDeTerugTeGevenString[i] - (int)aString2[i]) % 256) + 256) % 256);
                //tmp +=     (char)((((int)(terug[i] -                   asciSomSleutel) % 256) + 256) % 256);
            }
            for (int i = 0; i < bewerkt.Length; i++)
            {
                terug += bewerkt[i];
            }
            return @terug;
        }

        #endregion =======================================================================================================================




        #region ===============================================================================================================  Validatie
        static bool IsValidGebruikersnaam(string aGebruikersnaam)
        {

            //cijfers, grote en kleine letters, underscores en range
            return new Regex("^[0-9A-Za-z]{5,20}$").IsMatch(aGebruikersnaam);
        }
        //===================================================================================================
        static bool IsValidPaswoord(string aPaswoordTeValideren,
            string aToegestaneSpecialeKarakters = "!@#$%^&*()_+=[{]};:<>|./?,-")
        {

            //mijne moutarde : 
            // https://stackoverflow.com/questions/34715501/validating-password-using-regex-c-sharp
            // https://nl.wikibooks.org/wiki/Programmeren_in_ASP.NET/Reguliere_expressies

            // het optionele argument aToegestaneSpecialeKarakters bevatten misschien vierkante haken, 
            // de reguliere expresie doet hier moeilijk over, deze verwacht vierkante haken 
            // met 2 backslashen voor.
            // ik wil niet dat het argument een vierkante haak vooraf gegaan is met deze backslashen, 
            // de reden hiervoor is dat we duidelijk aan de gebruiker kunnen laten zien welke tekens
            // toegelaten zijn.
            // dus we gaan ze er handmatig voor zetten.


            string specialeKarakters = string.Empty;

            for (int i = 0; i < aToegestaneSpecialeKarakters.Length; i++)
            {
                if (aToegestaneSpecialeKarakters[i] == '[' || aToegestaneSpecialeKarakters[i] == ']')
                {
                    specialeKarakters += "\\"; //of dit @"\"
                }
                specialeKarakters += aToegestaneSpecialeKarakters[i];
            }

            //string specialeKarakters = @"!@#$%^&*()_+=\[{\]};:<>|./?,-"; //officiele

            string geldigPaswoordRegString =
                // ?=.*  kijkt naar de hele inputstring
                "(?=.*[A-Z]+)"        // [A-Z] betekend een hoofdletter, het plusje betekend '1 of meer'
                +
                "(?=.*[a-z]+)"        // [a-z] betekend een kleine letter, het plusje betekend '1 of meer'
                +
                "(?=.*[0-9]+)"        // [0-9] betekend een digital, het plusje betekend '1 of meer'
                +
                $"(?=.*[{specialeKarakters}]+)"  //tekens tss aanhalingstekenz, het plusje betekend '1 of meer'
                                                 //+
                                                 //".{8,20}"         // !!! zou moeten kijken of het geheel 8tot 20 lang is, bovengrens is niet gelukt !!!
            ;


            //bovengrens probleem voorlopig opgelost
            if (aPaswoordTeValideren.Length < 8 || aPaswoordTeValideren.Length > 20) return false;


            Regex paswoordRegex = new Regex(geldigPaswoordRegString);
            return paswoordRegex.IsMatch(aPaswoordTeValideren);

        }
        #endregion =======================================================================================================================





        #region ================================================================================================================= ToonMenu

        static void ToonMenu(string[,] aMenu, int aMenuIdTeTonen, string[] aGebruiker)
        {
            Debug.WriteLine($"Je komt de methode ToonMenu({aMenuIdTeTonen}) binnen, aantal Toonmenu's op de callstack: {++_debug_ToonMenu_teller}");


            //const zijn ook static
            const ConsoleColor KLEUR_FOR_GESELECTEERD = ConsoleColor.Green;
            const ConsoleColor KLEUR_FOR_NIET_GESELECTEERD = ConsoleColor.White;
            const ConsoleColor KLEUR_BACK_GESELECTEERD = ConsoleColor.DarkGreen;
            //const ConsoleColor KLEUR_BACK_NIET_GESELECTEERD = ConsoleColor.White;


            //------------------------------------ helpers
            int _teller = 0;
            bool _gevonden;
            //--------------------------------------------

            string[,] menuItemsInDitMenu;
            int huidigeSelectieIndex = -1;

            //deze arrays lopen parallel met elkaar, 
            int[] mogenlijkeNummerKeuzes = new int[0]; 
            int[] mogenlijkeMenuItemIds = new int[0];



            //tellen de rijen die moeten voorkomen in dit menu
            //------------------------------------------------
            _teller = 0;
            for (int rij = 0; rij < aMenu.GetLength(0); rij++)
            {
                if (aMenu[rij, (int)MenuItem.MenuID] == aMenuIdTeTonen.ToString() && aMenu[rij, (int)MenuItem.IsVisible] != string.Empty)//visible toegevoegd
                {
                    _teller++;
                }
            }


            //aanmaken en initialiseren van array
            //Array.resize() was miserie met 2d array, dus dan maar handmatig
            //---------------------------------------------------------------
            menuItemsInDitMenu = new string[_teller, aMenu.GetLength(0)];
            _teller = 0;
            bool _isIsSelectedGevonden = false;
            for (int rij = 0; rij < aMenu.GetLength(0); rij++)
            {
                _gevonden = false;
                for (int kol = 0; kol < aMenu.GetLength(1); kol++)
                {
                    //menuId gevonden in menu, toevoegen aan array menuItemsInDitMenu
                    if (aMenu[rij, (int)MenuItem.MenuID] == aMenuIdTeTonen.ToString() && aMenu[rij, (int)MenuItem.IsVisible] != string.Empty)//visible toegevoegd
                    {
                        _gevonden = true;
                        menuItemsInDitMenu[_teller, kol] = aMenu[rij, kol];

                    }
                }
                if (_gevonden)
                {
                    _teller++;

                    //array met 1 vergroten, chekken op dubbele waarde, toevoegen (of fout)
                    Array.Resize(ref mogenlijkeNummerKeuzes, mogenlijkeNummerKeuzes.Length + 1);
                    int toeTevoegenSelectvalue = Convert.ToInt32(aMenu[rij, (int)MenuItem.SelectionValue]);
                    if (mogenlijkeNummerKeuzes.Contains(toeTevoegenSelectvalue))
                    {
                        ToonFatalErrorBoodschap($"selectionvalue {toeTevoegenSelectvalue} in menu met " +
                            $"id {aMenuIdTeTonen} komt meermaals voor", true);
                    }
                    mogenlijkeNummerKeuzes[mogenlijkeNummerKeuzes.Length - 1] = toeTevoegenSelectvalue;


                    //array met 1 vergroten, chekken op dubbele waarde, toevoegen (of fout)
                    Array.Resize(ref mogenlijkeMenuItemIds, mogenlijkeMenuItemIds.Length + 1);
                    int toeTevoegenMenuItemID = Convert.ToInt32(aMenu[rij, (int)MenuItem.MenuItemId]);
                    if (mogenlijkeMenuItemIds.Contains(toeTevoegenMenuItemID))
                    {
                        ToonFatalErrorBoodschap($"MenuItemID {toeTevoegenMenuItemID} in menu met " +
                            $"id {aMenuIdTeTonen} komt meermaals voor", true);
                    }
                    mogenlijkeMenuItemIds[mogenlijkeNummerKeuzes.Length - 1] = toeTevoegenMenuItemID;


                    if (!_isIsSelectedGevonden)
                    {
                        if (aMenu[rij, (int)MenuItem.IsSelected] != string.Empty)
                        {
                            huidigeSelectieIndex = mogenlijkeNummerKeuzes.Length - 1;
                            _isIsSelectedGevonden = true;
                        }
                    }

                }
            }


            //----------------------------------------------------------------------------------------
            if (mogenlijkeNummerKeuzes.Length == 0)
            {
                ToonFatalErrorBoodschap($"je wil een menu tonen maar er wordt geen menu gevonden met " +
                            $"id {aMenuIdTeTonen}", true);
            }
            if (huidigeSelectieIndex ==-1)
            {
                ToonFatalErrorBoodschap($"je wil een menu tonen maar er is geen beginselectie ingegeven, " +
                            $"menu met id {aMenuIdTeTonen}", true);
            }

            //----------------------------------------------------------------------------------------


            TekenMenu();

            ConsoleKeyInfo cki;
            do
            {
                cki = Console.ReadKey(true);
                switch (cki.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (huidigeSelectieIndex > 0)
                        {
                            huidigeSelectieIndex--;
                            TekenMenu();
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if (huidigeSelectieIndex < mogenlijkeNummerKeuzes.Length-1)
                        {
                            huidigeSelectieIndex++;
                            TekenMenu();
                        }
                        break;
                    case ConsoleKey.Enter:
                        ExecuteMenuItem(aMenu, mogenlijkeMenuItemIds[huidigeSelectieIndex], aGebruiker);
                        TekenMenu();
                        break;
                    default:
                        int andereInvoer;
                        //Console.WriteLine(cki.Key.ToString());
                        if (Int32.TryParse(cki.KeyChar.ToString() , out andereInvoer)){
                            if(mogenlijkeNummerKeuzes.Contains(andereInvoer))
                            {

                                ExecuteMenuItem(
                                    aMenu,
                                    Convert.ToInt32(GetMenuStringWaardeVanKolom(
                                                        andereInvoer.ToString(),
                                                        MenuItem.SelectionValue,
                                                        MenuItem.MenuItemId)) ,
                                    aGebruiker);
                                TekenMenu();
                            }
                            //anders niets doen
                        }
                        //anders niets doen
                        break;
                }
            }
            while (cki.Key != ConsoleKey.Escape);


            //methode in methode, ik wist niet dat dat bestond, 
            //gezien bij github van Kenny, mercie Kenny ;-)
            //-----------------------------------------------------------------------
            void TekenMenu(){


                bool cursorVisibleTerugzetten =  Console.CursorVisible;
                Console.CursorVisible = false;
                ConsoleColor achtergrondkleurterugzetten = Console.BackgroundColor;
                ConsoleColor voorgrondkleurTerugzetten = Console.ForegroundColor;

                Console.SetCursorPosition(32, 10);
                if (aMenuIdTeTonen == 0)
                {
                    Console.WriteLine("[esc] om af te sluiten");
                }
                else
                {
                    Console.WriteLine("[esc] om terug te gaan");
                }

                for (int i = 0; i < menuItemsInDitMenu.GetLength(0); i++)
                {
                    //-------------
                    Console.SetCursorPosition(34+(i*2),12+(i*2));
                    //----------
                    string menuItemText = menuItemsInDitMenu[i, (int)MenuItem.Text];
                    string menuItemSelectionValue = menuItemsInDitMenu[i, (int)MenuItem.SelectionValue];
                    if (huidigeSelectieIndex == i)
                    {
                        Console.BackgroundColor = KLEUR_BACK_GESELECTEERD;
                        Console.ForegroundColor = KLEUR_FOR_GESELECTEERD;
                        Console.WriteLine($" [{menuItemSelectionValue}] {menuItemText.ToUpper()} ");
                    }
                    else
                    {
                        Console.BackgroundColor = achtergrondkleurterugzetten;
                        Console.ForegroundColor = KLEUR_FOR_NIET_GESELECTEERD;
                        Console.WriteLine($"[{menuItemSelectionValue}] {menuItemText}  ");
                    }
                }
                Console.CursorVisible = cursorVisibleTerugzetten;
                Console.BackgroundColor=achtergrondkleurterugzetten;
                Console.ForegroundColor=voorgrondkleurTerugzetten;
            }
            string GetMenuStringWaardeVanKolom(string aKolomWaardeTeVergelijken, MenuItem aKolomTeVergelijken, MenuItem aKolomWaardeTerugTeGeven)
            {
                for (int row = 0; row < menuItemsInDitMenu.GetLength(0); row++)
                {
                   if(menuItemsInDitMenu[row,(int)aKolomTeVergelijken] == aKolomWaardeTeVergelijken)
                    {
                        return menuItemsInDitMenu[row, (int)aKolomWaardeTerugTeGeven];
                   }
                }
                return string.Empty;
            }


            Debug.WriteLine($"Je verlaat de methode ToonMenu({aMenuIdTeTonen}), aantal ToonMenu's op de callstack: {--_debug_ToonMenu_teller}");
        }

        #endregion =======================================================================================================================



        static void ToonFatalErrorBoodschap(string aBoodschap, bool afsluiten)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: " + aBoodschap);
            Console.ReadKey();
            if (afsluiten)
                Environment.Exit(0);
        }




        static void TekenUitBestand(string aFileName, ConsoleColor aKleur, int aCursorLeft)
        {
            ConsoleColor kleurterugtezetten = Console.ForegroundColor;
            Console.ForegroundColor = aKleur;
            //bron: https://stackoverflow.com/questions/8037070/whats-the-fastest-way-to-read-a-text-file-line-by-line
            const Int32 BufferSize = 128;
            using (var fileStream = File.OpenRead(aFileName))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
            {
                String line;

                while ((line = streamReader.ReadLine()) != null)
                {
                    Console.CursorLeft = aCursorLeft;
                    Console.WriteLine(line);

                }
            }
            Console.ForegroundColor = kleurterugtezetten;
        }

        public static void TekenVlak(int x, int y, int breedte, int hoogte, ConsoleColor kleur)
        {
            ConsoleColor vorigeAchtergrondkleur = Console.BackgroundColor;
            ConsoleColor vorigeVoorgrondkleur = Console.ForegroundColor;
            Console.BackgroundColor = kleur;
            Console.ForegroundColor = kleur;
            for (int _x = x; _x < x + breedte; _x++)
            {
                for (int _y = y; _y < y + hoogte; _y++)
                {
                    Console.SetCursorPosition(_x, _y);
                    Console.Write(" ");
                }
            }
            Console.BackgroundColor = vorigeAchtergrondkleur;
            Console.ForegroundColor = vorigeVoorgrondkleur;

        }




        #region=========================================================================================================== TEST


        static void Test_Encryp_Decrypt()
        {
            string kleineLetters = "abcdefghijklmnopqrstuvwxyz";
            string groteLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string cijfers = "0123456789";
            string toegelatenPaswoord = "!@#$%^&*()_+=[{]};:<>|./?,-";
            string toegelatenGebruikersnaam = "_";

            string ttPaswoord = kleineLetters + groteLetters + toegelatenPaswoord + cijfers;
            string ttGebruikersnaam = kleineLetters + groteLetters + toegelatenGebruikersnaam + cijfers;

            Random rnd = new Random();

            Console.CursorLeft = 0; Console.Write("gegenereerd paswoord");
            Console.CursorLeft = 22; Console.Write("gegenereerd gebruiker");
            Console.CursorLeft = 44; Console.Write("paswoord encripted");
            Console.CursorLeft = 90; Console.WriteLine("paswoord decripted");
            Console.Write("----------------------------------------------------------");
            Console.WriteLine("--------------------------------------------------------");

            for (int i = 0; i < 100; i++)
            {
                //randomPaswoord aanmaken
                //--------------------------------------------------------------
                int lengteString = rnd.Next(8, 21);//van 8 tem 20
                string nieuwePasw = string.Empty;
                for (int j = 0; j < lengteString; j++)
                {
                    int randomKarakter = rnd.Next(ttPaswoord.Length);
                    nieuwePasw += ttPaswoord[randomKarakter];
                }

                //random key aanmaken (key is gebruikersnaam)
                //--------------------------------------------------------------

                lengteString = rnd.Next(5, 21);//van 5 tem 20 (+1tjes om te testen)
                string nieuweGebr = string.Empty;
                for (int j = 0; j < lengteString; j++)
                {
                    int randomKarakter = rnd.Next(ttGebruikersnaam.Length);
                    nieuweGebr += ttGebruikersnaam[randomKarakter];
                }

                string encripted = EncriptString(nieuwePasw, nieuweGebr);
                string decripted = DecriptString(encripted, nieuweGebr);

                //int r = rnd.Next(100); //testen dat het werkt :-)
                //if (r == 1) decripted += "p";

                if (nieuwePasw != decripted)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Beep(1200,100);
                }
                else Console.ForegroundColor = ConsoleColor.White;

                Console.CursorLeft = 0;  Console.Write(nieuwePasw);
                Console.CursorLeft = 22; Console.Write(nieuweGebr);
                Console.CursorLeft = 44; Console.Write(encripted);
                Console.CursorLeft = 90; Console.WriteLine(decripted);

                using (StreamWriter writer = File.AppendText("test.txt"))
                {
                    writer.WriteLine(nieuweGebr + " , " );
                    writer.WriteLine(encripted);
                }

            }

        }




        static void Test_valideerPaswoord()
        {
            string kleineLetters = "abcdefghijklmnopqrstuvwxyz";
            string groteLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string cijfers = "0123456789";
            string toegelatenPaswoord = "!@#$%^&*()_+=[{]};:<>|./?,-";

            string ttPaswoord = kleineLetters + groteLetters + toegelatenPaswoord + cijfers;

            Random rnd = new Random();
            for (int i = 0; i < 1000; i++)
            {
                //int lengteString = rnd.Next(5, 21);//van 5 tem 20
                int lengteString = rnd.Next(8, 21);//van 8 tem 20
                string nieuweString = string.Empty;
                for (int j = 0; j < lengteString; j++)
                {
                    int randomKarakter = rnd.Next(ttPaswoord.Length);
                    nieuweString += ttPaswoord[randomKarakter];
                }
                bool valOk = IsValidPaswoord(nieuweString);
                if (valOk == false) Console.ForegroundColor = ConsoleColor.Red;
                else Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(nieuweString);
            }
        }

        static void Test_valideerGebruikersnaam()
        {
            string kleineLetters = "abcdefghijklmnopqrstuvwxyz";
            string groteLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string cijfers = "0123456789";
            //string toegelatenGebruikersnaam = "_";

            string ttGebruikersnaam = kleineLetters + groteLetters /*+ toegelatenGebruikersnaam*/ + cijfers;

            Random rnd = new Random();
            for (int i = 0; i < 1000; i++)
            {
                int lengteString = rnd.Next(5-1, 21+1);//van 5 tem 20 (+1tjes om te testen)
                string nieuweString = string.Empty;
                for (int j = 0; j < lengteString; j++)
                {
                    int randomKarakter = rnd.Next(ttGebruikersnaam.Length);
                    nieuweString += ttGebruikersnaam[randomKarakter];
                }
                bool valOk = IsValidGebruikersnaam(nieuweString);
                if (valOk == false) Console.ForegroundColor = ConsoleColor.Red;
                else Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(nieuweString);
            }
        }

        #endregion=============================================================================================================


    }
}
