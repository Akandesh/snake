using System.Collections.Generic;
using static snake.HamiltonianCycle;


namespace snake
{
    public class ReturnData
    {
        public List<Coordinate> ResetThesePositions; // "old" apple and snake positions
        public List<DirectionEvent> ShotCutMoveDirections;
        public List<Coordinate> SnakePositions;
        public Coordinate ApplePosition;
        public bool DrawApple;
        public bool IslogicalEndReached;
    }


    public class ShortCut
    {
        // NOTICE:
        // The here used terms "above", "left", "right" and "under/below" are "seen visually".
        // Example:
        //   Snakehead.Y should be equal to  5 
        // while apple.Y should be equal to 10
        // a) seen visually:
        //    The snakehaed is ABOVE the apple.
        // b) seen mathimatically:
        //    For sure snakehead.Y is less than apple.Y. So it is "under/ lower". 
        //
        // Keep this in mind when reading the following comments.

        public void CalcShortCutForCase1And3( ReturnData returnDatas, HamiltonianCycleData HamiltonianCycleData ) {
            var snakesHeadPosition = returnDatas.SnakePositions[ 0 ];
            var applePosition = returnDatas.ApplePosition;

            if ( snakesHeadPosition.x < 1 || snakesHeadPosition.y < 1 ) {
                // No need to calculate a shortcut because snakes head is already in
                // .X == 0  -> ... the column 0
                // .Y == 0  -> ... the first row that leads the snake to column 0
                return;
            }

            if ( snakesHeadPosition.y == applePosition.y
                && (
                        ( snakesHeadPosition.x < applePosition.x && HamiltonianCycleData.Data.MoveDirections[ snakesHeadPosition.x, snakesHeadPosition.y ] == DirectionEvent.RIGHT )
                        ||
                        ( snakesHeadPosition.x > applePosition.x && HamiltonianCycleData.Data.MoveDirections[ snakesHeadPosition.x, snakesHeadPosition.y ] == DirectionEvent.LEFT )
                    )
                ) {
                // No need to calculate a shortcut because snakes head is 
                // already in the row that contains the apple
                // and
                // (    snakes head is left  from the apple and the snake movedirection is right towards the apple
                //      or
                //      snakes head is right from the apple and the snake movedirection is left towards the apple
                // )
                return;
            }

            // Check whether all snake parts are in the Hamiltonia Cycle. Leave if not.
            if ( !AreAllSnakePartsInTheHamiltonianCycle( returnDatas, HamiltonianCycleData ) ) {
                return;
            }

            if ( snakesHeadPosition.y > applePosition.y ) {
                // The snake is below the apple.
                // The shortcut path to be calculated here should lead the snake one row below the row that contains the apple.
                for ( int y = snakesHeadPosition.y - 1; applePosition.y < y; y-- ) {
                    returnDatas.ShotCutMoveDirections.Add( DirectionEvent.UP );
                }
                // The short cut path ends here exact below the row with the apple. Check whether the apple is up next to the snakehead.
                if ( snakesHeadPosition.x == applePosition.x ) {
                    // The apple is up next to the snakehead.
                    returnDatas.ShotCutMoveDirections.Add( DirectionEvent.UP );
                } else if ( snakesHeadPosition.x < applePosition.x && !IsValueEven( applePosition.y ) ) {
                    // Snakes head is left from the apple
                    // AND
                    // the apple is in a row with an odd number like 1, 3, 5, 7, ... 
                    // The direction in "odd rows" in right, so we can additionally lead the snake into the row with the apple
                    // because the then following non-shortcut path (aka "normal" Hamiltonian Cylce) is going to lead the snake into the apple.
                    returnDatas.ShotCutMoveDirections.Add( DirectionEvent.UP );
                } else if ( snakesHeadPosition.x > applePosition.x && IsValueEven( applePosition.y ) ) {
                    // Snakes head is right from the apple ... -> See previous comment.
                    returnDatas.ShotCutMoveDirections.Add( DirectionEvent.UP );
                }
            } else {
                // The snake is above the apple.
                // The snake should be lead to column 0.
                if ( !IsValueEven( snakesHeadPosition.y ) ) {
                    // Do not use:
                    //      if (HamiltonianCycleData.Data.MoveDirections[snakesHeadPosition.X, snakesHeadPosition.Y] == DirectionEvent.RIGHT)
                    // because the snake head can be in an odd row while its move direction is ".Up"!
                    // Do also not use:
                    //      if (HamiltonianCycleData.Data.MoveDirections[2, snakesHeadPosition.Y] == DirectionEvent.RIGHT)
                    // becasue the minimum square size is 2-by-2.
                    // But this would work:
                    //      if (HamiltonianCycleData.Data.MoveDirections[1, snakesHeadPosition.Y] == DirectionEvent.RIGHT)
                    returnDatas.ShotCutMoveDirections.Add( DirectionEvent.UP );
                }

                for ( int x = snakesHeadPosition.x; x > 0; x-- ) {
                    returnDatas.ShotCutMoveDirections.Add( DirectionEvent.LEFT );
                }
            }
        }

        public void CalcShortCutForCase2( ReturnData returnDatas, HamiltonianCycleData HamiltonianCycleData ) {
            var snakesHeadPosition = returnDatas.SnakePositions[ 0 ];
            var applePosition = returnDatas.ApplePosition;

            if ( snakesHeadPosition.x < 1 || snakesHeadPosition.y < 1 ) {
                // No need to calculate a shortcut because snakes head is already in
                // .X == 0  -> ... the column 0
                // .Y == 0  -> ... the first row that leads the snake to column 0
                return;
            }

            if ( snakesHeadPosition.x == applePosition.x
                && (
                        ( snakesHeadPosition.y < applePosition.y && HamiltonianCycleData.Data.MoveDirections[ snakesHeadPosition.x, snakesHeadPosition.y ] == DirectionEvent.DOWN )
                        ||
                        ( snakesHeadPosition.y > applePosition.y && HamiltonianCycleData.Data.MoveDirections[ snakesHeadPosition.x, snakesHeadPosition.y ] == DirectionEvent.UP )
                    )
                ) {
                // No need to calculate a shortcut because snakes head is 
                // already in the column that contains the apple
                // and
                // (    snakes head is above the apple and the snake movedirection is down towards the apple
                //      or
                //      snakes head is under the apple and the snake movedirection is up   towards the apple
                // )
                return;
            }

            if ( !AreAllSnakePartsInTheHamiltonianCycle( returnDatas, HamiltonianCycleData ) ) {
                return;
            }

            if ( snakesHeadPosition.x < applePosition.x ) {
                // The snake is to the left of the apple.
                // The shortcut path to be calculated here should lead the snake one column before the column that contains the apple.
                for ( int x = snakesHeadPosition.x; x < applePosition.x - 1; x++ ) {
                    returnDatas.ShotCutMoveDirections.Add( DirectionEvent.RIGHT );
                }
                // The short cut path ends here exact one column before the row with the apple. Check whether the apple is right next to the snake head.
                if ( snakesHeadPosition.y == applePosition.y ) {
                    // The apple is right next to the snake head.
                    returnDatas.ShotCutMoveDirections.Add( DirectionEvent.RIGHT );
                } else if ( snakesHeadPosition.y < applePosition.y && IsValueEven( applePosition.x ) ) {
                    // Snakes head is above the apple
                    // AND
                    // the apple is in a column with an even number like 2, 4, 6, 8, ... 
                    // The direction in "even columns" in down, so we can additionally lead the snake into this apple containing column
                    // because the then following non-shortcut path (aka "normal" Hamiltonian Cylce) is going to lead the snake into the apple.
                    returnDatas.ShotCutMoveDirections.Add( DirectionEvent.RIGHT );
                } else if ( snakesHeadPosition.y > applePosition.y && !IsValueEven( applePosition.x ) ) {
                    // Snakes head is below the apple ... -> See previous comment.
                    returnDatas.ShotCutMoveDirections.Add( DirectionEvent.RIGHT );
                } else if ( applePosition.y == 0 ) {
                    returnDatas.ShotCutMoveDirections.Add( DirectionEvent.RIGHT );
                    // snake head is no in the same column as the apple.
                    // Lead the snake up into the apple.
                    for ( int y = snakesHeadPosition.y; y > 0; y-- ) {
                        returnDatas.ShotCutMoveDirections.Add( DirectionEvent.UP );
                    }
                }
            } else {
                // The snake is to the right of the apple.
                // The snake should be lead to row 0.
                if ( IsValueEven( snakesHeadPosition.x ) ) {
                    // Snake head is a "even column". 
                    // The move direction of "even columns" is down, so go one step to the right, to a "odd column.
                    // The move direction of "odd  columns" is up.
                    returnDatas.ShotCutMoveDirections.Add( DirectionEvent.RIGHT );
                }

                for ( int y = snakesHeadPosition.y; y > 0; y-- ) {
                    returnDatas.ShotCutMoveDirections.Add( DirectionEvent.UP );
                }
            }
        }

        private bool AreAllSnakePartsInTheHamiltonianCycle( ReturnData returnDatas, HamiltonianCycleData HamiltonianCycleData ) {
            // Check whether all snake parts are in the Hamiltonia Cycle.
            for ( int i = 0; i < returnDatas.SnakePositions.Count - 1; i++ ) {
                var pointA = returnDatas.SnakePositions[ i ];
                var pointB = returnDatas.SnakePositions[ i + 1 ];

                if ( HamiltonianCycleData.Data.PointToSequenceNumber[ pointA.x, pointA.y ] == 0 ) {
                    if ( HamiltonianCycleData.Data.PointToSequenceNumber[ pointB.x, pointB.y ] != HamiltonianCycleData.Data.PointToSequenceNumber.Length - 1 ) {
                        return false;
                    }

                } else if ( HamiltonianCycleData.Data.PointToSequenceNumber[ pointA.x, pointA.y ] - 1 != HamiltonianCycleData.Data.PointToSequenceNumber[ pointB.x, pointB.y ] ) {
                    return false;
                }
            }
            // All snake parts are in the Hamiltonian Cycle.
            return true;
        }
    }
}
