using System;
using System.Collections.Generic;
using System.Drawing;

namespace snake
{
    public class HamiltonianCycle
    {
        public static bool IsValueEven( int value ) {
            // "true" for 0, 2, 4 ,6, ...
            return value % 2 == 0;
        }

        public enum Case
        {
            Case_1_And_3 = 0,
            Case_2
        }

        public class HamiltonianCycleData : ICloneable
        {
            public Data Data;
            public Case Case;

            public object Clone( ) {
                return this.MemberwiseClone( );
            }
        }

        public class Data
        {
            // Example: width = 6, height = 6 -> even / even -> is case 1: 
            public DirectionEvent[,] MoveDirections; //        MoveDirections[2, 3] = .RIGHT
            public int[,] PointToSequenceNumber;    // PointToSequenceNumber[2, 3] = 19            (20th Element because zero base counted)    
            public Point[] SequenceNumberToPoint;   // SequenceNumberToPoint[19]   = Point(2, 3)
        }

        public HamiltonianCycleData GetHamiltonianCycleData( int playFieldWidth, int playFieldHeight ) {
            if ( playFieldWidth < 2 ) {
                throw new Exception( "The 'PlayFieldWidth' value must be equal or greater than 2!" );
            }
            if ( playFieldHeight < 2 ) {
                throw new Exception( "The 'PlayFieldHeight' value must be equal or greater than 2!" );
            }

            // |------|------|------|
            // | Case | x    | y    |
            // |------|------|------|
            // | 1    | even | even |
            // | 2    | even | odd  |
            // |------|------|------|
            // | 3    | odd  | even |
            // | 4    | odd  | odd  | -> No Hamiltonian cycle possible 
            // |------|------|------|

            if ( IsValueEven( playFieldHeight ) ) {
                return new HamiltonianCycleData { Data = Case_1_and_3( playFieldWidth, playFieldHeight ), Case = Case.Case_1_And_3 };
            } else if ( IsValueEven( playFieldWidth ) ) {
                return new HamiltonianCycleData { Data = Case_2( playFieldWidth, playFieldHeight ), Case = Case.Case_2 };
            }

            // Case 4
            throw new Exception( "Both 'PlayFieldWidth' and 'PlayFieldHeight' can not be odd at the same time!" );
        }

        private Data Case_1_and_3( int width, int height ) {
            var ret = new Data( ) {
                MoveDirections = new DirectionEvent[ width, height ],
                PointToSequenceNumber = new int[ width, height ],
                SequenceNumberToPoint = new Point[ width * height ]
            };

            // 1) Column 0
            for ( int y = 0; y < height - 1; y++ ) {
                ret.MoveDirections[ 0, y ] = DirectionEvent.DOWN;
            }
            ret.MoveDirections[ 0, height - 1 ] = DirectionEvent.RIGHT;

            // 2) Column 1
            for ( int y = 1; y < height - 1; y++ ) {
                if ( width == 2 ) {
                    ret.MoveDirections[ 1, y ] = DirectionEvent.UP;
                } else {
                    if ( IsValueEven( y ) ) {
                        ret.MoveDirections[ 1, y ] = DirectionEvent.UP;
                    } else {
                        ret.MoveDirections[ 1, y ] = DirectionEvent.RIGHT;
                    }
                }
            }

            // 3) Last row
            for ( int x = 1; x < width - 1; x++ ) {
                ret.MoveDirections[ x, height - 1 ] = DirectionEvent.RIGHT;
            }
            ret.MoveDirections[ width - 1, height - 1 ] = DirectionEvent.UP;

            // 4) First row (special because of ret [1,0] which is ".LEFT" and not ".UP".
            for ( int x = 1; x < width; x++ ) {
                ret.MoveDirections[ x, 0 ] = DirectionEvent.LEFT;
            }

            if ( width > 2 ) {
                // 5) Last column 
                for ( int y = 0; y < height - 1; y++ ) {
                    if ( IsValueEven( y ) ) {
                        // y = 2
                        ret.MoveDirections[ width - 1, y ] = DirectionEvent.LEFT;
                    } else {
                        // y = 1
                        ret.MoveDirections[ width - 1, y ] = DirectionEvent.UP;
                    }
                }

                // 6) Pattern
                for ( int y = 1; y < height - 1; y++ )    // width = 6, height = 4 ->  y:= 1, 2     (4-1 = 3 -> ... < 3 -> max y value is 2)       
                {
                    for ( int x = 2; x < width - 1; x++ ) //                           x:= 2, 3, 4  (6-1 = 5 -> ... < 5 -> max x value is 4)
                    {
                        if ( IsValueEven( y ) ) {
                            // y = 2
                            ret.MoveDirections[ x, y ] = DirectionEvent.LEFT;
                        } else {
                            // y = 1
                            ret.MoveDirections[ x, y ] = DirectionEvent.RIGHT;
                        }
                    }
                }
            }
            GenerateSequence( width, height, ret );
            return ret;
        }

        private Data Case_2( int width, int height ) {
            var ret = new Data( ) {
                MoveDirections = new DirectionEvent[ width, height ],
                PointToSequenceNumber = new int[ width, height ],
                SequenceNumberToPoint = new Point[ width * height ]
            };

            // 1) Column 0
            for ( int y = 0; y < height - 1; y++ ) {
                ret.MoveDirections[ 0, y ] = DirectionEvent.DOWN;
            }
            ret.MoveDirections[ 0, height - 1 ] = DirectionEvent.RIGHT;

            // 2) Last row
            for ( int x = 1; x < width - 1; x++ ) {
                if ( IsValueEven( x ) ) {
                    ret.MoveDirections[ x, height - 1 ] = DirectionEvent.RIGHT;
                } else {
                    ret.MoveDirections[ x, height - 1 ] = DirectionEvent.UP;
                }
            }

            // 3) Last column 
            ret.MoveDirections[ width - 1, 0 ] = DirectionEvent.LEFT;
            for ( int y = 1; y < height; y++ ) {
                ret.MoveDirections[ width - 1, y ] = DirectionEvent.UP;
            }

            // 4) first two rows (y =0  and y = 1)
            for ( int x = 1; x < width - 1; x++ ) {
                ret.MoveDirections[ x, 0 ] = DirectionEvent.LEFT;
                if ( IsValueEven( x ) ) {
                    ret.MoveDirections[ x, 1 ] = DirectionEvent.DOWN;
                } else {
                    ret.MoveDirections[ x, 1 ] = DirectionEvent.RIGHT;
                }
            }

            // 5) Two "inner" rows
            //      height   (height - 2) / 2
            //        2         0
            //        4         1
            //        6         2
            //        8         3
            //              ...     ...
            var nTimes = ( height - 2 ) / 2;
            for ( int n = 0; n < nTimes; n++ ) {
                for ( int x = 1; x < width - 1; x += 2 ) {
                    for ( int y = 2; y < height - 1; y += 2 ) {
                        ret.MoveDirections[ x, y ] = DirectionEvent.UP;
                        ret.MoveDirections[ x + 1, y ] = DirectionEvent.DOWN;

                        ret.MoveDirections[ x, y + 1 ] = DirectionEvent.UP;
                        ret.MoveDirections[ x + 1, y + 1 ] = DirectionEvent.DOWN;
                    }
                }
            }
            GenerateSequence( width, height, ret );

            return ret;
        }

        public void GenerateSequence( int width, int height, Data hamiltonianCycle, int pxOffset = 0 ) {
            var index = -1;
            var count = width * height;
            var currentPosition = new Point( 0 + pxOffset, 0 + pxOffset );
            hamiltonianCycle.PointToSequenceNumber[ 0 + pxOffset, 0 + pxOffset ] = ++index;
            hamiltonianCycle.SequenceNumberToPoint[ 0 ] = currentPosition;
            do {
                switch ( hamiltonianCycle.MoveDirections[ currentPosition.X, currentPosition.Y ] ) {
                    case DirectionEvent.UP:
                        currentPosition.Y--;

                        break;
                    case DirectionEvent.LEFT:
                        currentPosition.X--;
                        break;
                    case DirectionEvent.DOWN:
                        currentPosition.Y++;
                        break;
                    case DirectionEvent.RIGHT:
                        currentPosition.X++;
                        break;
                }
                index++;
                hamiltonianCycle.PointToSequenceNumber[ currentPosition.X, currentPosition.Y ] = index;
                hamiltonianCycle.SequenceNumberToPoint[ index ] = new Point( currentPosition.X, currentPosition.Y );
            } while ( index < count - 1 );

            ShowHamiltonianCycle( width, height, hamiltonianCycle );
        }

        private void ShowHamiltonianCycle( int width, int height, Data hamiltonianCycle ) {
            return;
            Console.WriteLine( $"Testdata for ({width}, {height}):" );

            Console.WriteLine( "Part 1:" );
            for ( int hamHeight = 0; hamHeight < height; hamHeight++ ) {
                var oneRow = $"row {hamHeight}: ";
                for ( int hamWidth = 0; hamWidth < width; hamWidth++ ) {
                    oneRow += $" | ({hamWidth}, {hamHeight}) = {hamiltonianCycle.PointToSequenceNumber[ hamWidth, hamHeight ],-4}";
                }
                Console.WriteLine( oneRow + " |" );
            }

            Console.WriteLine( "" );
            Console.WriteLine( "" );

            Console.WriteLine( "Part 2:" );
            for ( int hamHeight = 0; hamHeight < height; hamHeight++ ) {
                var oneRow = $"row {hamHeight}: ";
                for ( int hamWidth = 0; hamWidth < width; hamWidth++ ) {
                    oneRow += $" | ({hamWidth}, {hamHeight}) = {hamiltonianCycle.MoveDirections[ hamWidth, hamHeight ],-5} ";
                }
                Console.WriteLine( oneRow + " |" );
            }

            Console.WriteLine( "------------------------------------------------------------------------------" );
        }

        public void Test( ) {
            var TestData = new List<Size>
            {
                //// Data for case 1 (= x is even and y is even) 
                ////      and case 3 (= x is odd  and y is even)
                //new Size(2, 2), new Size(2, 4), new Size(2, 6),
                //new Size(3, 2), new Size(3, 4), new Size(3, 6),
                //new Size(4, 2), new Size(4, 4), new Size(4, 6),

                // Data for case 2 ( = x is even and y is odd)
                new Size(2, 3), new Size(2, 5), new Size(2, 7),
                new Size(4, 3), new Size(4, 5), new Size(4, 7),
                new Size(6, 3), new Size(6, 5), new Size(6, 7),
            };

            foreach ( var item in TestData ) {
                var hamiltonianCycle = GetHamiltonianCycleData( item.Width, item.Height );
            }
        }
    }
}
