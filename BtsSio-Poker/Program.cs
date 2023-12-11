//debut
﻿using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace Poker
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

        // Une carte
        public struct carte
        {
            public char valeur;
            public int famille;
        };

        // Liste des combinaisons possibles
        public enum combinaison { RIEN, PAIRE, DOUBLE_PAIRE, BRELAN, QUINTE, FULL, COULEUR, CARRE, QUINTE_FLUSH };

        // Valeurs des cartes : As, Roi,...
        public static char[] valeurs = { 'A', 'R', 'D', 'V', 'X', '9', '8', '7', '6', '5', '4', '3', '2' };

        // Codes ASCII (9829 : coeur, 9830 : carreau, 9827 : trèfle, 9824 : pique)
        public static int[] familles = {9829, 9830, 9827, 9824};

        // Numéros des cartes à échanger
        public static int[] echange = { 0, 0, 0, 0 };

        // Jeu de 5 cartes
        public static carte[] MonJeu = new carte[5];

        //----------
        // FONCTIONS
        //----------

        // Génère aléatoirement une carte : {valeur;famille}
        // Retourne une expression de type "structure carte"
        public static carte tirage()
        {
        	
        	carte N_carte = new carte();
        	N_carte.valeur =  valeurs[rnd.Next(0,12)];
        	N_carte.famille =  familles[rnd.Next(0,4)];
        	
        	return N_carte;
        }

        // Indique si une carte est déjà présente dans le jeu
        // Paramètres : une carte, le jeu 5 cartes, le numéro de la carte dans le jeu
        // Retourne un entier (booléen)
        public static bool carteUnique(carte uneCarte, carte[] unJeu, int numero)
        {
        	bool returntype = true;
        	for(int i = 0;i<unJeu.Length;i++)
        	{
        		if(uneCarte.valeur.Equals(unJeu[i].valeur)&&(uneCarte.famille.Equals(unJeu[i].famille)))
        		{
        			returntype = false;
        		}
        	}
        	if(returntype){return true;}
        	else{return false;}
        }

        // Calcule et retourne la COMBINAISON (paire, double-paire... , quinte-flush)
        // pour un jeu complet de 5 cartes.
        // La valeur retournée est un élement de l'énumération 'combinaison' (=constante)
        public static combinaison chercheCombinaison(ref carte[] unJeu)
        {
        	int[] similaire = {0,0,0,0,0};
        	int paire = 0;
        	int c_quint = 0;
        	int couleur = 0;
        	bool DoublePaire = false;
        	bool Brelan = false;
        	bool Carre = false;
        	bool SetupQuint = false;

        	char [,] quintes = {{'X','V','D','R','A'},
								{'9','X','V','D','R'},
								{'8','9','X','V','D'},
								{'7','8','9','X','V'}
								};
        	
        	//Paire check
        	for(int i=0;i<unJeu.Length;i++)
        	{
        		for(int j=0;j<unJeu.Length;j++)
	        	{
        			if(unJeu[i].Equals(unJeu[j]))
        			{
        				similaire[i]++;
        				paire++;
        				//Double paire check
        				if(paire%2==0)
        				{
        					DoublePaire = true;
        				}
        			}
	        	}
        	}
        	
        	//Brelan, Carre and Quint setup check
        	foreach(var value in similaire)
        	{
        		if(value==3)
        		{
        			Brelan = true;
        		}
        		else if(value==4){
        			Carre = true;
        		}
        		if(value>1){
        			SetupQuint=false;
        		}
        	}
        	
        	//Quinte Check
        	if(SetupQuint)
        	{
        		for(int i=0;i<quintes.GetLength(0);i++)
        		{
        			c_quint=0;
        			for(int j=0;j<quintes.GetLength(1);j++)
	        		{
        				if(quintes[i,j].Equals(unJeu[j]))
        				{
        					c_quint++;
        				}
	        		}
        		}
        	}
        	//Couleur Check
        	for(int col=0;col<unJeu.Length;col++)
        	{
        		if(unJeu[0].famille.Equals(unJeu[col].famille))
        		{
        			couleur++;
        		}
        	}
        	Console.Write("\n\n|");
        	foreach(var num in similaire)
        	{
        		Console.Write("{0}|", num);
        	}
        	if(couleur==5 && c_quint!=5)
        	{
        		return combinaison.COULEUR;
        	}
        	else if(Brelan && paire==2)
        	{
    			return combinaison.FULL;
        	}
        	else if(c_quint==5&&couleur==5)
        	{
        		return combinaison.QUINTE_FLUSH;
        	}
        	else if(SetupQuint&&c_quint==5)
        	{
        		return combinaison.QUINTE;
        	}
        	else if(Brelan)
        	{
        		return combinaison.BRELAN;
        	}
        	else if(Carre)
        	{
        		return combinaison.CARRE;
        	}
        	else if(DoublePaire)
        	{
        		return combinaison.DOUBLE_PAIRE;
        	}
        	else if(Array.IndexOf(similaire, 2)!=-1)
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
        	
        	for(int m=0;m<menu.Length;m++)
        	{
        		Console.SetCursorPosition((Console.WindowWidth/2)-menu[0].Length/2,((Console.WindowHeight/2)-menu.Length/2)+m);
        		Console.WriteLine(menu[m]);
        	}
        }

        // Jouer au Poker
		// Ici que vous appellez toutes les fonction permettant de joueur au poker
        private static void jouerAuPoker()
        {	
        	Console.Clear();
    		tirageDuJeu(MonJeu);
    		Console.ReadKey(true);
        	Main();
        }

        // Tirage d'un jeu de 5 cartes
        // Paramètre : le tableau de 5 cartes à remplir
        private static void tirageDuJeu(carte[] unJeu)
        {
        	int c=0;
        	while(c<5)
        	{
        		carte temp = tirage();
        		if(carteUnique(temp, MonJeu, c))
				{
					unJeu[c].famille = temp.famille;
        			unJeu[c].valeur = temp.valeur;
        			c++;
        		}
        	}
        	affichageCarte(unJeu[0]);
        }

        // Affiche à l'écran une carte {valeur;famille} 
        private static void affichageCarte(carte uneCarte)
        {
            //----------------------------
            // TIRAGE D'UN JEU DE 5 CARTES
            //----------------------------
            int left = 0;
            int c = 1;
            // Tirage aléatoire de 5 cartes
            Console.SetCursorPosition(0,0);
            for (int i = 0; i < 5; i++)
            {
                // Tirage de la carte n°i (le jeu doit être sans doublons !)

                // Affichage de la carte
                if (MonJeu[i].famille == 9829 || MonJeu[i].famille == 9830)
                    SetConsoleTextAttribute(hConsole, 252);
                else
                    SetConsoleTextAttribute(hConsole, 240);
                Console.SetCursorPosition(left, 5);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '*', '-', '-', '-', '-', '-', '-', '-', '-', '-', '*');
                Console.SetCursorPosition(left, 6);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, '|');
                Console.SetCursorPosition(left, 7);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '|');
                Console.SetCursorPosition(left, 8);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)MonJeu[i].famille, ' ', ' ', ' ', ' ', ' ', ' ', ' ', (char)MonJeu[i].famille, '|');
                Console.SetCursorPosition(left, 9);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', ' ', MonJeu[i].valeur, MonJeu[i].valeur, MonJeu[i].valeur, ' ', ' ', ' ', '|');
                Console.SetCursorPosition(left, 10);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)MonJeu[i].famille, ' ', ' ', MonJeu[i].valeur, MonJeu[i].valeur, MonJeu[i].valeur, ' ', ' ', (char)MonJeu[i].famille, '|');
                Console.SetCursorPosition(left, 11);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', ' ', MonJeu[i].valeur, MonJeu[i].valeur, MonJeu[i].valeur, ' ', ' ', ' ', '|');
                Console.SetCursorPosition(left, 12);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)MonJeu[i].famille, ' ', ' ', ' ', ' ', ' ', ' ', ' ', (char)MonJeu[i].famille, '|');
                Console.SetCursorPosition(left, 13);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '|');
                Console.SetCursorPosition(left, 14);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, '|');
                Console.SetCursorPosition(left, 15);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '*', '-', '-', '-', '-', '-', '-', '-', '-', '-', '*');
                Console.SetCursorPosition(left, 16);
                SetConsoleTextAttribute(hConsole, 10);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", ' ', ' ', ' ', ' ', ' ', c, ' ', ' ', ' ', ' ', ' ');
                left = left + 15;
                c++;
            }
            afficheResultat(MonJeu);
        }

        // Enregistre le score dans le txt
        private static void enregistrerJeu(carte[] unJeu)
        {
          
        }

        // Affiche le Scores
        private static void voirScores()
        {
           
        }

        // Affiche résultat
        private static void afficheResultat(carte[] unJeu)
        {
            SetConsoleTextAttribute(hConsole, 012);
            Console.Write("RESULTAT - Vous avez : ");
            try
            {
                // Test de la combinaison
                switch (chercheCombinaison(ref MonJeu))
                {
                    case combinaison.RIEN:
                        Console.WriteLine("rien du tout... desole!"); break;
                    case combinaison.PAIRE:
                        Console.WriteLine("une simple paire..."); break;
                    case combinaison.DOUBLE_PAIRE:
                        Console.WriteLine("une double paire; on peut esperer..."); break;
                    case combinaison.BRELAN:
                        Console.WriteLine("un brelan; pas mal..."); break;
                    case combinaison.QUINTE:
                        Console.WriteLine("une quinte; bien!"); break;
                    case combinaison.FULL:
                        Console.WriteLine("un full; ouahh!"); break;
                    case combinaison.COULEUR:
                        Console.WriteLine("une couleur; bravo!"); break;
                    case combinaison.CARRE:
                        Console.WriteLine("un carre; champion!"); break;
                    case combinaison.QUINTE_FLUSH:
                        Console.WriteLine("une quinte-flush; royal!"); break;
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
	                    voirScores();
	
	                if (reponse == '3')
	                    break;
            	}
            }
            Console.Clear();
        }
    }
}
