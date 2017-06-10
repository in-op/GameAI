# Summary
C# implementations of artificial intelligence to play games.
Current algorithms:
* [MiniMax](https://github.com/CampbellAlexander/GameAI/blob/master/README.md#minimax)
* [Pure Monte-Carlo simulation](https://github.com/CampbellAlexander/GameAI/blob/master/README.md#pure-monte-carlo)
* [Monte-Carlo-Tree-Search with Upper Confidence Bounds](https://github.com/CampbellAlexander/GameAI/blob/master/README.md#monte-carlo-tree-search-with-upper-confidence-bounds-mcts-ucb1)

Algorithms compute the best possible move for 2-player, back-and-forth,
zero-sum games of perfect information. This includes:
* Chess
* TicTacToe
* Connect-Four
* Checkers
* Go
* etc.

All algorithms support both single-threaded and multi-threaded
versions.

C# 4.0, .NET 3.5 compatible for integrating in Unity projects.



# Algorithms
Algorithms execute various search techniques upon the 
gamestate tree to select a move. In general, lists
of legal moves are created by the game implementation,
and the algorithm plays moves based on its search technique.
Depending on the search algorithm, it may query the game
for whether the game is over, what the hashcode is for
the current state, ask the game to make a deep copy of
itself, etc. After performing its computation and adhering
to any constraints (such as stopping after a certain time
limit or after so many number of simulations), the algorithm
will return the best move that it found.

The algorithms are each generically parameterized across three types:
* TGame
* TMove
* TPlayer
These types are defined by the client game implementation.
TGame is the type of the game itself. TMove is
whatever type is use to represent moves
within that game. TPlayer is whatever type is used
to identify specific players within that game.

Each game type defined by the client must implement
the IGame interface associated with each algorithm.
The IGame interface determines which properties and
methods are necessary for the algorithm to correctly
perform its calculation. Many properties and methods
are shared across algorithms, so each IGame interface
is actually a blank interface that merely inherits
from multiple,
[individual interfaces](https://github.com/CampbellAlexander/GameAI/blob/master/README.md#interfaces) 
which each define one piece of functionality that a
game can implement.

Using generics in this way allows for client flexibility
in determining their own types.  It also allows for
ease of use when creating a game that implements multiple algorithms.
(You may want to do this for testing, or for different
difficulty levels). Instead of having to explicitly
implement each IGame interface, only implement each 
individual interface once and it can be used across
any algorithm that requires it.

### MiniMax
[MiniMax](https://en.wikipedia.org/wiki/Minimax)
is a recursive search algorithm which solves games
of simple complexity, such as Tic-Tac-Toe.

### Pure Monte-Carlo
Pure [Monte-Carlo](https://en.wikipedia.org/wiki/Monte_Carlo_method)
simulation creates deep copies
of the current gamestate and plays random moves
upon it until the game ends. It calculates which
possible move - from the set of all legal moves in
the initial state - has the highest win-rate.

### Monte-Carlo Tree-Search with Upper Confidence Bounds (MCTS UCB1)
A domain-general upgrade to pure Monte-Carlo,
[MCTS UCB1](https://en.wikipedia.org/wiki/Monte_Carlo_tree_search)
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





# Interfaces
To run an algorithm on a game implementation, have the game
implement the `IGame` interface associated with the respective
algorithm, e.g. `MiniMax.TreeSearch<TGame, TMove, TPlayer>.IGame`.
Each algorithm's `IGame` interface extends some number of
individual, single-method interfaces such as `IGameOver`,
`IDoMove`, etc. Because many individual interfaces
are shared by the algorithms, this makes it easy for games
to implement multiple algorithms' `IGame` interfaces
without having to implement them each explicitly.

### ICurrentPlayer\<TPlayer\>
```c#
TPlayer CurrentPlayer { get; }
```
The readonly property returns the player whose
turn it is within the current gamestate. It
is parameterized over the client-defined player
type.

### IDoMove\<TMove\>
```c#
void DoMove(TMove move)
```
Updates the game's internal representation
to reflect the execution of the input move.
It is parameterized over the client-defined
move type.

### IGameOver
```c#
bool IsGameOver()
```
Returns true if the current gamestate is
a terminal (game-over) gamestate. Returns
false if there are still possible moves
to perform.

### IInt64Hash
```c#
long Hash { get; }
```
Gets a 64-bit hash value representing
the current, distinct gamestate. Each
distinct gamestate should generate a
distinct hash value. Consider using
Zobrist hashing.

### ILegalMoves\<TMove\>
```c#
List<TMove> GetLegalMoves()
```
Returns a list of legal moves that
can be performed in the current gamestate.
It is parameterized over the client-defined
move type.

### ILegalTransitions\<TTransition\>
```c#
List<TTransition> GetLegalTransitions()
```
Returns a list of legal transitions that
can be performed in the current gamestate.
It is parameterized over the internal
transition type respective to the algorithm.

### IRollout
```c#
void Rollout()
```
Play the game until it ends. This is used
by Monte-Carlo algorithms to run their 
simulations. Light playouts (random) or
heavy playouts (weighted towards moves
that would normally occur in-game) can
be implemented by the client.

### IScore
```c#
int Score(TPlayer player)
```
Returns the score for the input player
in the current gamestate. Used by MiniMax
algorithms to determine the relative
utility of performing possible moves. It
is parameterized over the client-defined
player type.

### ITransition\<TTransition\>
```c#
void Transition(TTransition t)
```
Execute the given move on the internal game
state and update its hash with the given
value. It is parameterized over the
client-defined transition type.

### IUndoMove
```c#
void UndoMove()
```
Transforms the game back to its previous
state before the last-used move was executed.
Used by recursive algorithms to maintain
correct state in the client game when
climbing up a gametree to previous states.

### IWinner\<TPlayer\>
```c#
bool IsWinner(TPlayer player)
```
Returns whether the input player is
victorious in the current gamestate.
It is parameterized over the client-defined
player type.








