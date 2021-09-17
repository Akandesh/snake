using System;
using System.Diagnostics;
using System.Threading;

namespace snake
{
    enum MenuOptions
    {
        PLAY,
        HIGHSCORES,
        QUIT
    }

    class Program
    {
        const int gameWidth = 80;
        const int gameHeight = 20;
        void EntryPoint( ) {
            setupGame( );
            Console.SetCursorPosition( 0, gameHeight + 1 );
            Console.ForegroundColor = ConsoleColor.Gray;
            return;
        }


        void setupGame( ) {
            Console.Title = "Snake";
            drawBorder( );
            mainMenu( );
        }

        void mainMenu( ) {
            const string greetingText = "Welcome to Snake! What's your name?";
            Coordinate greetingLocation = new Coordinate {
                x = gameWidth / 2 - greetingText.Length / 2,
                y = gameHeight / 3
            };

            var selectedMenu = MenuOptions.PLAY;
            DrawMenuOptions( greetingLocation, ref selectedMenu );

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.SetCursorPosition( greetingLocation.x, greetingLocation.y );
            Console.Write( greetingText );

            Console.ForegroundColor = ConsoleColor.White;
            const string nameString = "Name: ";
            Console.SetCursorPosition( gameWidth / 2 - nameString.Length, greetingLocation.y + 2 );
            Console.Write( nameString );
            
            var name = Console.ReadLine( );
            

            bool waitingForSelection = true;
            while ( waitingForSelection ) {
                var key = Console.ReadKey( );
                switch ( key.Key ) {
                    case ConsoleKey.DownArrow: {
                        selectedMenu++;
                        DrawMenuOptions( greetingLocation, ref selectedMenu );
                        break;
                    }
                    case ConsoleKey.UpArrow: {
                        selectedMenu--;
                        DrawMenuOptions( greetingLocation, ref selectedMenu );
                        break;
                    }
                    case ConsoleKey.Enter:
                        waitingForSelection = false;
                        break;
                    default:
                        break;
                }
            }

            ClearGameBoard( );
            switch ( selectedMenu ) {
                case MenuOptions.QUIT:
                    Environment.Exit(0);
                    break;
                case MenuOptions.PLAY:
                    GameLoop();
                    break;
            }
        }

        void GameLoop()
        {
            var snake = new Snake( gameWidth, gameHeight ); ;
            while ( true ) {
                snake.Tick( );
                Thread.Sleep( 1000 / 15 );
            }
        }

        void ClearGameBoard( ) {

        }

        private MenuOptions _lastMenuSelection = MenuOptions.QUIT;
        void DrawMenuOptions( Coordinate greetingLocation, ref MenuOptions selectedMenu ) {
            // Keeps the selected menu in range
            while ( !Enum.IsDefined( typeof( MenuOptions ), selectedMenu ) ) {
                if ( (int)selectedMenu < 0 )
                    selectedMenu++;
                else if ( (int)selectedMenu > (int)MenuOptions.QUIT ) // QUIT is the back of MenuOptions
                    selectedMenu--;
            }

            // Prevents menu being redrawn when it doesn't need to be
            if ( selectedMenu == _lastMenuSelection )
                return;

            Console.ForegroundColor = ConsoleColor.White;

            
            string playString = $"[{( selectedMenu == MenuOptions.PLAY ? '*' : ' ' )}] PLAY";
            int offsetFromCenter = playString.Length / 2;
            Console.SetCursorPosition( gameWidth / 2 - offsetFromCenter, greetingLocation.y + 4 );
            Console.Write( playString );

            string highScoreString = $"[{( selectedMenu == MenuOptions.HIGHSCORES ? '*' : ' ' )}] HIGHSCORES";
            Console.SetCursorPosition( gameWidth / 2 - offsetFromCenter, greetingLocation.y + 5 );
            Console.Write( highScoreString );

            string quitString = $"[{( selectedMenu == MenuOptions.QUIT ? '*' : ' ' )}] QUIT";
            Console.SetCursorPosition( gameWidth / 2 - offsetFromCenter, greetingLocation.y + 6 );
            Console.Write( quitString );
            _lastMenuSelection = selectedMenu;
        }

        void drawBorder( ) {
            Console.Clear( );
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            for ( int y = 0; y <= gameHeight; y++ ) {
                bool onTop = y == 0;
                bool onBottom = y == gameHeight;
                for ( int x = 0; x <= gameWidth; x++ ) {
                    bool onEdge = ( onTop && x == 0 ) ||
                                  ( onTop && x == gameWidth ) ||
                                  ( onBottom && x == 0 ) ||
                                  ( onBottom && x == gameWidth );
                    if ( onEdge ) {
                        Console.Write( '+' );
                        continue;
                    }

                    if ( x == 0 || x == gameWidth || onTop || onBottom ) {
                        Console.Write( '#' );
                        continue;
                    }

                    Console.Write( ' ' );
                }
                Console.WriteLine( );
            }
        }

        static void Main( ) {
            new Program( ).EntryPoint( ); // silly static escape
        }
    }
}
