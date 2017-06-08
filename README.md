# 0 - Summary
C# implementations of artificial intelligence to play games.
Includes pure MiniMax, pure Monte-Carlo simulation, and
Monte-Carlo-Tree-Search with Upper Confidence Bounds. All
algorithms support both single-threaded and multi-threaded
versions.






# 1 - Game Interfaces
To run an algorithm on a game implementation, have the game
implement the `IGame` interface associated with the respective
algorithm, e.g. `MiniMax.TreeSearch<TGame, TMove, TPlayer>.IGame`.
Each algorithm's `IGame` interface extends some number of
individual, single-method interfaces such as `IGameOver`,
`IDoMove`, etc. Because many individual interfaces
are shared by the algorithms, this makes it easy for games
to implement multiple algorithms' `IGame` interfaces
without having to implement them each explicitly.

### 1.1 - ICurrentPlayer\<TPlayer\>
```c#
TPlayer CurrentPlayer { get; }
```
The readonly property returns the player whose
turn it is within the current gamestate. It
is parameterized over the client-defined player
type.

### 1.2 - IDoMove\<TMove\>
```c#
void DoMove(TMove move)
```
Updates the game's internal representation
to reflect the execution of the input move.
It is parameterized over the client-defined
move type.

### 1.3 - IGameOver
```c#
bool IsGameOver()
```
Returns true if the current gamestate is
a terminal (game-over) gamestate. Returns
false if there are still possible moves
to perform.

### 1.4 - IInt64Hash
```c#
long Hash { get; }
```
Gets a 64-bit hash value representing
the current, distinct gamestate. Each
distinct gamestate should generate a
distinct hash value. Consider using
Zobrist hashing.

### 1.5 - ILegalMoves\<TMove\>
```c#
List<TMove> GetLegalMoves()
```
Returns a list of legal moves that
can be performed in the current gamestate.
It is parameterized over the client-defined
move type.

### 1.6 - ILegalTransitions\<TTransition\>
```c#
List<TTransition> GetLegalTransitions()
```
Returns a list of legal transitions that
can be performed in the current gamestate.
It is parameterized over the internal
transition type respective to the algorithm.

### 1.7 - IRollout
```c#
void Rollout()
```
Play the game until it ends. This is used
by Monte-Carlo algorithms to run their 
simulations. Light playouts (random) or
heavy playouts (weighted towards moves
that would normally occur in-game) can
be implemented by the client.

### 1.8 - IScore
```c#
int Score(TPlayer player)
```
Returns the score for the input player
in the current gamestate. Used by MiniMax
algorithms to determine the relative
utility of performing possible moves. It
is parameterized over the client-defined
player type.

### 1.9 - ITransition\<TTransition\>
```c#
void Transition(TTransition t)
```
Execute the given move on the internal game
state and update its hash with the given
value. It is parameterized over the
client-defined transition type.

### 1.10 - IUndoMove
```c#
void UndoMove()
```
Transforms the game back to its previous
state before the last-used move was executed.
Used by recursive algorithms to maintain
correct state in the client game when
climbing up a gametree to previous states.

### 1.11 - IWinner\<TPlayer\>
```c#
bool IsWinner(TPlayer player)
```
Returns whether the input player is
victorious in the current gamestate.
It is parameterized over the client-defined
player type.







# 2 - Algorithms

### 2.1 MiniMax
MiniMax is a recursive search algorithm which solves games
of simple complexity, such as Tic-Tac-Toe.

### 2.2 Pure Monte-Carlo
Pure Monte-Carlo simulation creates deep copies
of the current gamestate and plays random moves
upon it until the game ends. It calculates which
possible move - from the set of all legal moves in
the initial state - has the highest win-rate.

### 2.3 Monte-Carlo Tree-Search with Upper Confidence Bounds (MCTS UCB1)
An upgrade the pure Monte-Carlo, MCTS UCB1
generates a game tree rooted at the
initial gamestate. Each simulation adds one
node to the tree. As the tree expands upon
further exploration, when each new copy runs
its simulation it can garner useful information
from the tree and choose to execute moves
that exhibit a perfect trade-off between
always performing moves it already knows
are good, versus always performing novel moves
in order to explore the state space.
