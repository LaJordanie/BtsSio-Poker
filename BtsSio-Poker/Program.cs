using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Globalization;

namespace BtsSioPoker
{
    class Program
    {
        // -----------------------
        // DECLARATION DES DONNEES
        // -----------------------
        // Importation des DL (librairies de code) permettant de gérer les couleurs en mode console
        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleTextAttribute(IntPtr hConsoleOutput, int wAttributes);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetStdHandle(uint nStdHandle);
        static uint STD_OUTPUT_HANDLE = 0xfffffff5;
        static IntPtr hConsole = GetStdHandle(STD_OUTPUT_HANDLE);
        // Pour utiliser la fonction C 'getchar()' : sasie d'un caractère
        [DllImport("msvcrt")]
        static extern int _getche();

        //-------------------
        // TYPES DE DONNEES
        //-------------------

        public static Random rnd = new Random();
        // Fin du jeu
        public static bool fin = false;
        // Codes COULEUR
        public enum couleur { VERT = 10, ROUGE = 12, JAUNE = 14, BLANC = 15, NOIRE = 0, ROUGESURBLANC = 252, NOIRESURBLANC = 240 };

        // Coordonnées pour l'affichage
        public struct coordonnees
        {
            public int x;
            public int y;
        }

        // Une echange_carte
        public struct carte
        {
            public char valeur;
            public int famille;
        };

        // Liste des combinaisons possibles
        public enum combinaison { RIEN, PAIRE, DOUBLE_PAIRE, BRELAN, QUINTE, FULL, COULEUR, CARRE, QUINTE_FLUSH };

        // Valeurs des cartes : As, Roi,...
        public static char[] valeurs = { 'A', 'R', 'D', 'V', 'X', '9', '8', '7', '6', '5', '4', '3', '2' };

        // Codes ASCII (3 : coeur, 4 : carreau, 5 : trèfle, 6 : pique)
        public static int[] familles = { 9829, 9830, 9827, 9824 };

        // Numéros des cartes à échanger
        public static int[] echange = { 0, 0, 0, 0 };

        // Jeu de 5 cartes
        public static carte[] MonJeu = new carte[5];

        //----------
        // FONCTIONS
        //----------

        public static carte tirage()
        {

            carte N_carte = new carte();
            N_carte.valeur = valeurs[rnd.Next(0, 12)];
            N_carte.famille = familles[rnd.Next(0, 3)];

            return N_carte;
        }
        public static bool carteUnique(carte uneCarte, carte[] unJeu, int numero)
        {
            bool returntype = true;
            for (int i = 0; i < unJeu.Length; i++)
            {
                if (uneCarte.valeur.Equals(unJeu[i].valeur) && (uneCarte.famille.Equals(unJeu[i].famille)))
                {
                    returntype = false;
                }
            }
            if (returntype) { return true; }
            else { return false; }
        }
        public static combinaison chercheCombinaison(ref carte[] unJeu)
        {
            int[] similaire = { 0, 0, 0, 0, 0 };
            int c_paire = 0;
            bool paire = false;
            bool doublepaire = false;
            bool brelan = false;
            bool carre = false;
            bool sim_1 = true;
            bool b_quint = false;
            bool couleur = true;
            char[,] quint = {
                                {'X','V','D','R','A'},
                                {'9','X','V','D','4'},
                                {'8','9','X','V','D'},
                                {'7','8','9','X','V'},
                            };

            // Similaire Counter
            for (int i = 0; i < MonJeu.Length; i++)
            {
                for (int j = 0; j < MonJeu.Length; j++)
                {
                    if (MonJeu[i].valeur == MonJeu[j].valeur)
                    {
                        similaire[i]++;
                    }
                }
            }
            // Similaire Check
            foreach (int num in similaire)
            {
                if (num != 1)
                {
                    sim_1 = false;
                }
                if (num == 2)
                {
                    paire = true;
                    c_paire++;
                }
                if (num == 3)
                {
                    brelan = true;
                }
                if (num == 4)
                {
                    carre = true;
                }
            }
            // Double Paire Check
            if (c_paire / 2 == 2)
            {
                doublepaire = true;
            }
            // Couleur Check
            foreach (carte c2 in MonJeu)
            {
                if (MonJeu[0].famille != c2.famille)
                {
                    couleur = false;
                }
            }
            //Quint Check
            for (int c_quint = 0; c_quint < quint.GetLength(0); c_quint++)
            {
                if (MonJeu[0].valeur == quint[c_quint, 0] && MonJeu[1].valeur == quint[c_quint, 1] && MonJeu[2].valeur == quint[c_quint, 2] && MonJeu[3].valeur == quint[c_quint, 3] && MonJeu[4].valeur == quint[c_quint, 4])
                {
                    b_quint = true;
                }
            }

            if (couleur && !sim_1)
            {
                return combinaison.COULEUR;
            }
            else if (paire && brelan)
            {
                return combinaison.FULL;
            }
            else if (couleur && sim_1)
            {
                return combinaison.QUINTE_FLUSH;
            }
            else if (b_quint && sim_1)
            {
                return combinaison.QUINTE;
            }
            else if (brelan)
            {
                return combinaison.BRELAN;
            }
            else if (carre)
            {
                return combinaison.CARRE;
            }
            else if (doublepaire)
            {
                return combinaison.DOUBLE_PAIRE;
            }
            else if (paire)
            {
                return combinaison.PAIRE;
            }
            else
            {
                return combinaison.RIEN;
            }
        }

        // Echange des cartes
        // Paramètres : le tableau de 5 cartes et le tableau des numéros des cartes à échanger
        private static void echangeCarte(carte[] unJeu, int[] e)
        {
            for (int i = 0; i < e.Length; i++)
            {
                if (e[i] == 1)
                {
                    carte temp = tirage();
                    if (carteUnique(temp, MonJeu, i))
                    {
                        unJeu[i].famille = temp.famille;
                        unJeu[i].valeur = temp.valeur;
                    }
                }
            }
        }

        // Pour afficher le Menu pricipale
        private static void afficheMenu()
        {
            string[] menu = {"+---------+",
                             "|         |",
                             "|  POKER  |",
                             "|         |",
                             "| 1 Jouer |",
                             "| 2 Score |",
                             "| 3 Fin   |",
                             "|         |",
                             "+---------+"};

            for (int m = 0; m < menu.Length; m++)
            {
                Console.SetCursorPosition((Console.WindowWidth / 2) - menu[0].Length / 2, ((Console.WindowHeight / 2) - menu.Length / 2) + m);
                Console.WriteLine(menu[m]);
            }
        }

        // Jouer au Poker
        // Ici que vous appellez toutes les fonction permettant de joueur au poker
        private static void jouerAuPoker()
        {
            Console.Clear();
            tirageDuJeu(MonJeu);
            affichageCarte(MonJeu);
            afficheResultat(MonJeu);
            char reponse_echange;
            char reponse_carte;
            int[] echange_carte = { 0, 0, 0, 0, 0 };
            int y_offset = 15;
            while (true)
            {
                SetConsoleTextAttribute(hConsole, 15);
                Console.SetCursorPosition(0, y_offset);
                Console.Write("\n\nCombien de cartes souhaitez vous changer (0-4)?: ");

                reponse_echange = (char)_getche();
                if (int.Parse(reponse_echange.ToString()) > 0 && int.Parse(reponse_echange.ToString()) <= 4)
                {
                    for (int i = 0; i < int.Parse(reponse_echange.ToString()); i++)
                    {
                        Console.SetCursorPosition(8, y_offset + 2 + i);
                        Console.Write("\nCarte (1-5): ");
                        Console.SetCursorPosition(15, y_offset + 3 + i);
                        reponse_carte = (char)_getche();
                        while (true)
                        {
                            if (int.Parse(reponse_carte.ToString()) >= 1 && int.Parse(reponse_carte.ToString()) <= 5 && echange_carte[int.Parse(reponse_carte.ToString()) - 1] != 1)
                            {
                                echange_carte[int.Parse(reponse_carte.ToString()) - 1] = 1;
                                break;
                            }
                        }
                    }
                    Console.SetCursorPosition(0, 30);
                    echangeCarte(MonJeu, echange_carte);
                    affichageCarte(MonJeu);
                    afficheResultat(MonJeu);
                    break;

                }
                else if (int.Parse(reponse_echange.ToString()) == 0)
                {
                    break;
                }
                else
                {
                    Console.WriteLine("\nEntrée invalide!");
                }
            }
            char reponse_save;
            string nom;
            while (true)
            {
                Console.SetCursorPosition(0, 3 + y_offset + int.Parse(reponse_echange.ToString()));
                Console.Write("\n\nVoulez vous enregistrer le jeu ? (O/N): ");
                reponse_save = (char)_getche();
                if (reponse_save == 'O' || reponse_save == 'o')
                {
                    Console.Clear();
                    Console.Write("Quelle est votre nom (ou pseudo): ");
                    nom = Console.ReadLine();
                    enregistrerJeu(MonJeu, nom);
                    break;
                }
                else if (reponse_save == 'n' || reponse_save == 'N')
                {
                    break;
                }
            }
            Main();
        }

        // Tirage d'un jeu de 5 cartes
        // Paramètre : le tableau de 5 cartes à remplir
        private static void tirageDuJeu(carte[] unJeu)
        {
            int c = 0;
            while (c < 5)
            {
                carte temp = tirage();
                if (carteUnique(temp, MonJeu, c))
                {
                    unJeu[c].famille = temp.famille;
                    unJeu[c].valeur = temp.valeur;
                    c++;
                }
            }
        }
        // Affiche à l'écran une echange_carte {valeur;famille} 
        private static void affichageCarte(carte[] unJeu)
        {
            //----------------------------
            // TIRAGE D'UN JEU DE 5 CARTES
            //----------------------------
            int left = 10;
            int H_offset = 2; //Height offset
            int c = 1;
            // Tirage aléatoire de 5 cartes
            for (int i = 0; i < unJeu.Length; i++)
            {
                if (unJeu[i].famille == 9829 || unJeu[i].famille == 9830)
                    SetConsoleTextAttribute(hConsole, 252);
                else
                    SetConsoleTextAttribute(hConsole, 240);
                Console.SetCursorPosition(left, H_offset);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '*', '-', '-', '-', '-', '-', '-', '-', '-', '-', '*');
                Console.SetCursorPosition(left, H_offset + 1);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)unJeu[i].famille, ' ', (char)unJeu[i].famille, ' ', (char)unJeu[i].famille, ' ', (char)unJeu[i].famille, ' ', (char)unJeu[i].famille, '|');
                Console.SetCursorPosition(left, H_offset + 2);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '|');
                Console.SetCursorPosition(left, H_offset + 3);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)unJeu[i].famille, ' ', ' ', ' ', ' ', ' ', ' ', ' ', (char)unJeu[i].famille, '|');
                Console.SetCursorPosition(left, H_offset + 4);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', ' ', unJeu[i].valeur, unJeu[i].valeur, unJeu[i].valeur, ' ', ' ', ' ', '|');
                Console.SetCursorPosition(left, H_offset + 5);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)unJeu[i].famille, ' ', ' ', unJeu[i].valeur, unJeu[i].valeur, unJeu[i].valeur, ' ', ' ', (char)unJeu[i].famille, '|');
                Console.SetCursorPosition(left, H_offset + 6);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', ' ', unJeu[i].valeur, unJeu[i].valeur, unJeu[i].valeur, ' ', ' ', ' ', '|');
                Console.SetCursorPosition(left, H_offset + 7);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)unJeu[i].famille, ' ', ' ', ' ', ' ', ' ', ' ', ' ', (char)unJeu[i].famille, '|');
                Console.SetCursorPosition(left, H_offset + 8);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '|');
                Console.SetCursorPosition(left, H_offset + 9);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)unJeu[i].famille, ' ', (char)unJeu[i].famille, ' ', (char)unJeu[i].famille, ' ', (char)unJeu[i].famille, ' ', (char)unJeu[i].famille, '|');
                Console.SetCursorPosition(left, H_offset + 10);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '*', '-', '-', '-', '-', '-', '-', '-', '-', '-', '*');
                Console.SetCursorPosition(left, H_offset + 11);
                SetConsoleTextAttribute(hConsole, 10);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", ' ', ' ', ' ', ' ', ' ', c, ' ', ' ', ' ', ' ', ' ');
                left = left + 15;
                c++;
            }
        }

        // chiffre en cesar
        private static string Encrypt(string input, int shift)
        {
            StringBuilder encryptedText = new StringBuilder();

            foreach (char character in input)
            {
                if (char.IsLetter(character))
                {
                    char start = char.IsUpper(character) ? 'A' : 'a';
                    encryptedText.Append((char)((character + shift - start) % 26 + start));
                }
                else
                {
                    encryptedText.Append(character);
                }
            }

            return encryptedText.ToString();
        }

        // dechiffres les données en cesar
        private static string Decrypt(string input, int shift)
        {
            return Encrypt(input, 26 - shift);
        }

        // Enregistre le score dans le txt
        private static void enregistrerJeu(carte[] unJeu, string nom)
        {
            string jeu = "";
            string ligne;
            char delimiters = ' ';
            BinaryWriter writer;

            try
            {
                // créé ou ouvre le fichier scores.txt
                writer = new BinaryWriter(new FileStream("scores.txt", FileMode.Append, FileAccess.Write));

                // ecrit les données chiffrer dans le fichier
                for (int i = 0; i < unJeu.Length; i++)
                {
                    jeu += "[" + Encrypt(unJeu[i].valeur.ToString(), 3) + "," + Encrypt(unJeu[i].famille.ToString(), 3) + "]";
                }

                ligne = Encrypt(nom + delimiters + "{" + jeu + "}", 3);
                writer.Write(ligne);
                Console.WriteLine("Encrypted: " + ligne);

                // ferme le fichier
                writer.Close();
            }
            catch (IOException)
            {
                Console.WriteLine("Erreurs d'écriture dans le fichier scores.txt");
            }
            Console.ReadKey(true);
        }

        // Affiche le Scores
        private static void voirScores()
        {
            BinaryReader reader;
            string encryptedData;

            try
            {
                // ouvre scores.txt 
                reader = new BinaryReader(new FileStream("scores.txt", FileMode.Open, FileAccess.Read));

                // lit les données chiffrer
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    encryptedData = reader.ReadString();
                    string decryptedData = Decrypt(encryptedData, 3);
                    Console.WriteLine(decryptedData);
                }

                // ferme le fichier
                reader.Close();
            }
            catch (IOException)
            {
                Console.WriteLine("Erreurs d'ouverture du fichier scores.txt");
            }
        }

        // Affiche résultat
        private static void afficheResultat(carte[] unJeu)
        {
            SetConsoleTextAttribute(hConsole, 012);
            try
            {
                Console.SetCursorPosition(0, 15);
                switch (chercheCombinaison(ref unJeu))
                {
                    case combinaison.RIEN:
                        Console.WriteLine("RESULTAT - Vous avez : rien du tout... desole!"); break;
                    case combinaison.PAIRE:
                        Console.WriteLine("RESULTAT - Vous avez : une simple paire..."); break;
                    case combinaison.DOUBLE_PAIRE:
                        Console.WriteLine("RESULTAT - Vous avez : une double paire; on peut esperer..."); break;
                    case combinaison.BRELAN:
                        Console.WriteLine("RESULTAT - Vous avez : un brelan; pas mal..."); break;
                    case combinaison.QUINTE:
                        Console.WriteLine("RESULTAT - Vous avez : une quinte; bien!"); break;
                    case combinaison.FULL:
                        Console.WriteLine("RESULTAT - Vous avez : un full; ouahh!"); break;
                    case combinaison.COULEUR:
                        Console.WriteLine("RESULTAT - Vous avez : une couleur; bravo!"); break;
                    case combinaison.CARRE:
                        Console.WriteLine("RESULTAT - Vous avez : un carre; champion!"); break;
                    case combinaison.QUINTE_FLUSH:
                        Console.WriteLine("RESULTAT - Vous avez : une quinte-flush; royal!"); break;
                };
            }
            catch { }
        }
        //--------------------
        // Fonction PRINCIPALE
        //--------------------
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Clear();
            //---------------
            // BOUCLE DU JEU
            //---------------
            char reponse;
            while (true)
            {
                afficheMenu();
                reponse = (char)_getche();
                if (reponse != '1' && reponse != '2' && reponse != '3')
                {
                    Console.Clear();
                    afficheMenu();
                }
                else
                {
                    SetConsoleTextAttribute(hConsole, 015);
                    // Jouer au Poker
                    if (reponse == '1')
                    {
                        jouerAuPoker();
                        break;
                    }
                    if (reponse == '2')
                        Console.Clear();
                        voirScores();

                    if (reponse == '3')
                        break;
                }
            }
        }
    }
}