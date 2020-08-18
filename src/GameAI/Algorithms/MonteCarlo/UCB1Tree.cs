using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GameAI.GameInterfaces;

namespace GameAI.Algorithms.MonteCarlo
{
    /// <summary>
    /// A static class to run UCB1 Tree Monte Carlo simulations on a game,
    /// with both parallel and single-threaded algorithms.
    /// </summary>
    public static class UCB1Tree<TGame, TMove, TPlayer>
        where TGame : UCB1Tree<TGame, TMove, TPlayer>.IGame
    {
        /// <summary>
        /// Interface implemented by games to use the Monte Carlo tree search algorithm.
        /// </summary>
        public interface IGame :
            ICopyable<TGame>,
            IInt64Hash,
            IGameOver,
            ICurrentPlayer<TPlayer>,
            IWinner<TPlayer>,
            ITransition<Transition>,
            ILegalTransitions<Transition>,
            IRollout
        { }

        /// <summary>
        /// A data class for storing a move and the gamestate's resultant hash code after performing that move.
        /// </summary>
        public class Transition
        {
            /// <summary>
            /// The move to perform the transition.
            /// </summary>
            public TMove Move;

            /// <summary>
            /// The hash of the resulting gamestate.
            /// </summary>
            public long Hash;

            /// <summary>
            /// Return a new Transition with the specified move to perform and a hash representing the resulting gamestate.
            /// </summary>
            /// <param name="move">The move to perform to execute the transition.</param>
            /// <param name="hash">The hash code of the resulting gamestate after performing the move.</param>
            public Transition(TMove move, long hash)
            {
                Move = move;
                Hash = hash;
            }

            public override string ToString() => $"Move: {Move} - Hash: {Hash}";
        }

        /// <summary>
        /// Return the best Transition found after performing in parallel the specified number of simulations on the game.
        /// </summary>
        /// <param name="game">The initial gamestate from which to find the best move for the current player.</param>
        /// <param name="milliseconds">The length of time to run the search.</param>
        public static Transition ParallelSearch(TGame game, long milliseconds)
        {
            ConcurrentDictionary<long, Node> tree = new ConcurrentDictionary<long, Node>();
            tree.TryAdd(game.Hash, new Node(game.CurrentPlayer));
            Stopwatch sw = new Stopwatch();
            sw.Start();

            Parallel.For(0L, Int64.MaxValue,

                () => new ThreadLocalVars(RandomFactory.Create(), new List<Node>(50)),

                (i, loop, localVars) =>
                {
                    if (sw.ElapsedMilliseconds > milliseconds)
                        loop.Stop();

                    TGame copy = game.DeepCopy();
                    localVars.path.Clear();
                    localVars.path.Add(tree[game.Hash]);

                    while (!copy.IsGameOver())
                    {
                        List<Transition> transitions = copy.GetLegalTransitions();
                        List<Transition> transitionsNoStats = new List<Transition>();
                        foreach (Transition transition in transitions)
                            if (!tree.ContainsKey(transition.Hash))
                                transitionsNoStats.Add(transition);

                        // SELECTION
                        if (transitionsNoStats.Count == 0)
                        {
                            double bestScore = double.MinValue;
                            int parentPlays = localVars.path[localVars.path.Count - 1].plays;
                            double ucb1Score;
                            int indexOfBestTransition = 0;
                            for (int j = 0; j < transitions.Count; j++)
                            {
                                ucb1Score = tree[transitions[j].Hash].ParentUCBScore(parentPlays);
                                if (ucb1Score > bestScore)
                                {
                                    bestScore = ucb1Score;
                                    indexOfBestTransition = j;
                                }
                            }
                            Transition bestTransition = transitions[indexOfBestTransition];
                            copy.Transition(bestTransition);
                            localVars.path.Add(tree[bestTransition.Hash]);
                        }

                        // EXPANSION
                        else
                        {
                            copy.Transition(transitionsNoStats.RandomItem(localVars.random));
                            Node n = new Node(copy.CurrentPlayer);

                            if (tree.TryAdd(copy.Hash, n))
                                localVars.path.Add(n);
                            else
                                localVars.path.Add(tree[copy.Hash]);
                            break;
                        }
                    }

                    // ROLLOUT
                    copy.Rollout();

                    // BACKPROP
                    foreach (Node node in localVars.path)
                    {
                        Interlocked.Add(ref node.plays, 1);
                        if (copy.IsWinner(node.player))
                            Interlocked.Add(ref node.wins, 1);
                    }

                    return localVars;
                },

                x => { }
            );

            // Simulations are over. Pick the best move, then return it
            List<Transition> allTransitions = game.GetLegalTransitions();
            int indexOfBestMoveFound = 0;
            double worstScoreFound = double.MaxValue;
            double score;

            //Console.WriteLine("Root: plays-{0} wins-{1} plyr-{2}", tree[game.GetHash()].plays, tree[game.GetHash()].wins, tree[game.GetHash()].player);

            for (int i = 0; i < allTransitions.Count; i++)
            {
                // Node n = tree[allTransitions[i].Hash];
                //Console.WriteLine("Move {0}: plays-{1} wins-{2} plyr-{3}", i, n.plays, n.wins, n.player);

                // **NOTE**
                // The best move chosen is the move with gives the
                // opponent the least number of victories
                score = tree[allTransitions[i].Hash].CurrentPlayerScore();
                if (score < worstScoreFound)
                {
                    worstScoreFound = score;
                    indexOfBestMoveFound = i;
                }
            }

            return allTransitions[indexOfBestMoveFound];
        }

        /// <summary>
        /// Return the best Transition found after performing in parallel the specified number of simulations on the game.
        /// </summary>
        /// <param name="game">The initial gamestate from which to find the best move for the current player.</param>
        /// <param name="simulations">The number of simulations to run on the game.</param>
        public static Transition ParallelSearch(TGame game, int simulations)
        {
            ConcurrentDictionary<long, Node> tree = new ConcurrentDictionary<long, Node>();
            tree.TryAdd(game.Hash, new Node(game.CurrentPlayer));

            Parallel.For(0, simulations,

                () => new ThreadLocalVars(RandomFactory.Create(), new List<Node>(50)),
                
                (i, loop, localVars) =>
                {
                    TGame copy = game.DeepCopy();
                    localVars.path.Clear();
                    localVars.path.Add(tree[game.Hash]);

                    while (!copy.IsGameOver())
                    {
                        List<Transition> transitions = copy.GetLegalTransitions();
                        List<Transition> transitionsNoStats = new List<Transition>();
                        foreach (Transition transition in transitions)
                            if (!tree.ContainsKey(transition.Hash))
                                transitionsNoStats.Add(transition);

                        // SELECTION
                        if (transitionsNoStats.Count == 0)
                        {
                            double bestScore = double.MinValue;
                            int parentPlays = localVars.path[localVars.path.Count - 1].plays;
                            double ucb1Score;
                            int indexOfBestTransition = 0;
                            for (int j = 0; j < transitions.Count; j++)
                            {
                                ucb1Score = tree[transitions[j].Hash].ParentUCBScore(parentPlays);
                                if (ucb1Score > bestScore)
                                {
                                    bestScore = ucb1Score;
                                    indexOfBestTransition = j;
                                }
                            }
                            Transition bestTransition = transitions[indexOfBestTransition];
                            copy.Transition(bestTransition);
                            localVars.path.Add(tree[bestTransition.Hash]);
                        }

                        // EXPANSION
                        else
                        {
                            copy.Transition(transitionsNoStats.RandomItem(localVars.random));

                            Node node = new Node(copy.CurrentPlayer);
                            if (tree.TryAdd(copy.Hash, node))
                                localVars.path.Add(node);
                            else
                                localVars.path.Add(tree[copy.Hash]);

                            break;
                        }
                    }

                    // ROLLOUT
                    copy.Rollout();

                    // BACKPROP
                    foreach (Node node in localVars.path)
                    {
                        Interlocked.Add(ref node.plays, 1);
                        if (copy.IsWinner(node.player))
                            Interlocked.Add(ref node.wins, 1);
                    }

                    return localVars;
                },

                x => { }
            );

            // Simulations are over. Pick the best move, then return it
            List<Transition> allTransitions = game.GetLegalTransitions();
            int indexOfBestMoveFound = 0;
            double worstScoreFound = double.MaxValue;
            double score;

            //Console.WriteLine("Root: plays-{0} wins-{1} plyr-{2}", tree[game.GetHash()].plays, tree[game.GetHash()].wins, tree[game.GetHash()].player);

            for (int i = 0; i < allTransitions.Count; i++)
            {
                //Node n = tree[allTransitions[i].Hash];
                //Console.WriteLine("Move {0}: plays-{1} wins-{2} plyr-{3}", i, n.plays, n.wins, n.player);

                // **NOTE**
                // The best move chosen is the move with gives the
                // opponent the least number of victories
                score = tree[allTransitions[i].Hash].CurrentPlayerScore();
                if (score < worstScoreFound)
                {
                    worstScoreFound = score;
                    indexOfBestMoveFound = i;
                }
            }

            return allTransitions[indexOfBestMoveFound];
        }

        /// <summary>
        /// Return the best Transition found after running the UCB1 Tree algorithm for the specified length of time.
        /// </summary>
        /// <param name="game">The initial gamestate from which to find the best move for the current player.</param>
        /// <param name="milliseconds">The length of time the algorithm will run.</param>
        public static Transition Search(TGame game, long milliseconds)
        {
            Dictionary<long, Node> tree = new Dictionary<long, Node>
            {
                { game.Hash, new Node(game.CurrentPlayer) }
            };
            List<Node> path = new List<Node>();
            TGame copy;
            List<Transition> allTransitions;
            List<Transition> transitionsNoStats;
            Random rng = new Random();
            Stopwatch sw = new Stopwatch();

            sw.Start();
            while (sw.ElapsedMilliseconds < milliseconds)
            {
                copy = game.DeepCopy();
                path.Clear();
                path.Add(tree[game.Hash]);

                while (!copy.IsGameOver())
                {
                    allTransitions = copy.GetLegalTransitions();
                    transitionsNoStats = new List<Transition>();
                    foreach (Transition transition in allTransitions)
                        if (!tree.ContainsKey(transition.Hash))
                            transitionsNoStats.Add(transition);

                    // SELECTION
                    if (transitionsNoStats.Count == 0)
                    {
                        double bestScore = float.MinValue;
                        int parentPlays = path[path.Count - 1].plays;
                        double ucb1Score;
                        int indexOfBestTransition = 0;
                        for (int j = 0; j < allTransitions.Count; j++)
                        {
                            ucb1Score = tree[allTransitions[j].Hash].ParentUCBScore(parentPlays);
                            if (ucb1Score > bestScore)
                            {
                                bestScore = ucb1Score;
                                indexOfBestTransition = j;
                            }
                        }
                        Transition bestTransition = allTransitions[indexOfBestTransition];
                        copy.Transition(bestTransition);
                        path.Add(tree[bestTransition.Hash]);
                    }

                    // EXPANSION
                    else
                    {
                        copy.Transition(transitionsNoStats.RandomItem(rng));

                        Node node = new Node(copy.CurrentPlayer);
                        tree.Add(copy.Hash, node);
                        path.Add(node);

                        break;
                    }
                }

                // ROLLOUT
                copy.Rollout();

                // BACKPROP
                foreach (Node node in path)
                {
                    node.plays++;
                    if (copy.IsWinner(node.player))
                        node.wins++;
                }
            }

            // Simulations are over. Pick the best move, then return it
            allTransitions = game.GetLegalTransitions();
            int indexOfBestMoveFound = 0;
            double worstScoreFound = double.MaxValue;
            double score;

            //Console.WriteLine("Root: plays-{0} wins-{1} plyr-{2}", tree[game.GetHash()].plays, tree[game.GetHash()].wins, tree[game.GetHash()].player);

            for (int i = 0; i < allTransitions.Count; i++)
            {
                //Node n = tree[allTransitions[i].Hash];
                //Console.WriteLine("Move {0}: plays-{1} wins-{2} plyr-{3}", i, n.plays, n.wins, n.player);

                // **NOTE**
                // The best move chosen is the move with gives the
                // opponent the least number of victories
                score = tree[allTransitions[i].Hash].CurrentPlayerScore();
                if (score < worstScoreFound)
                {
                    worstScoreFound = score;
                    indexOfBestMoveFound = i;
                }
            }

            return allTransitions[indexOfBestMoveFound];
        }

        /// <summary>
        /// Return the best Transition found after performing the specified number of simulations on the game.
        /// </summary>
        /// <param name="game">The initial gamestate from which to find the best move for the current player.</param>
        /// <param name="simulations">The number of simulations to run on the game.</param>
        public static Transition Search(TGame game, int simulations)
        {
            Dictionary<long, Node> tree = new Dictionary<long, Node>
            {
                { game.Hash, new Node(game.CurrentPlayer) }
            };
            List<Node> path = new List<Node>();
            IGame copy;
            List<Transition> allTransitions;
            List<Transition> transitionsNoStats;
            Random rng = new Random();

            for (int i = 0; i < simulations; i++)
            {
                copy = game.DeepCopy();
                path.Clear();
                path.Add(tree[game.Hash]);

                while (!copy.IsGameOver())
                {
                    allTransitions = copy.GetLegalTransitions();
                    transitionsNoStats = new List<Transition>();
                    foreach (Transition transition in allTransitions)
                        if (!tree.ContainsKey(transition.Hash))
                            transitionsNoStats.Add(transition);

                    // SELECTION
                    if (transitionsNoStats.Count == 0)
                    {
                        double bestScore = float.MinValue;
                        int parentPlays = path[path.Count - 1].plays;
                        double ucb1Score;
                        int indexOfBestTransition = 0;
                        for (int j = 0; j < allTransitions.Count; j++)
                        {
                            ucb1Score = tree[allTransitions[j].Hash].ParentUCBScore(parentPlays);
                            if (ucb1Score > bestScore)
                            {
                                bestScore = ucb1Score;
                                indexOfBestTransition = j;
                            }
                        }
                        Transition bestTransition = allTransitions[indexOfBestTransition];
                        copy.Transition(bestTransition);
                        path.Add(tree[bestTransition.Hash]);
                    }

                    // EXPANSION
                    else
                    {
                        copy.Transition(transitionsNoStats.RandomItem(rng));

                        Node node = new Node(copy.CurrentPlayer);
                        tree.Add(copy.Hash, node);
                        path.Add(node);

                        break;
                    }
                }

                // ROLLOUT
                copy.Rollout();

                // BACKPROP
                foreach (Node node in path)
                {
                    node.plays++;
                    if (copy.IsWinner(node.player))
                        node.wins++;
                }
            }

            // Simulations are over. Pick the best move, then return it
            allTransitions = game.GetLegalTransitions();
            int indexOfBestMoveFound = 0;
            double worstScoreFound = double.MaxValue;
            double score;

            //Console.WriteLine("Root: plays-{0} wins-{1} plyr-{2}", tree[game.GetHash()].plays, tree[game.GetHash()].wins, tree[game.GetHash()].player);

            for (int i = 0; i < allTransitions.Count; i++)
            {
                //Node n = tree[allTransitions[i].Hash];
                //Console.WriteLine("Move {0}: plays-{1} wins-{2} plyr-{3}", i, n.plays, n.wins, n.player);

                // **NOTE**
                // The best move chosen is the move with gives the
                // opponent the least number of victories
                score = tree[allTransitions[i].Hash].CurrentPlayerScore();
                if (score < worstScoreFound)
                {
                    worstScoreFound = score;
                    indexOfBestMoveFound = i;
                }
            }

            return allTransitions[indexOfBestMoveFound];
        }

        private static double UCB1(double childWins, double childPlays, double parentPlays)
            => (childWins / childPlays) + Math.Sqrt(2f * Math.Log(parentPlays) / childPlays);

        private class Node
        {
            internal int plays;
            internal int wins;
            internal TPlayer player;

            internal double CurrentPlayerScore() => (double)wins / plays;

            internal double ParentUCBScore(int parentPlays)
            {
                int parentWins = plays - wins;
                return UCB1(parentWins, plays, parentPlays);
            }

            private Node() { }

            internal Node(TPlayer player)
            {
                this.player = player;
                plays = 0;
                wins = 0;
            }
        }

        private class ThreadLocalVars
        {
            internal readonly Random random;
            internal readonly List<Node> path;

            internal ThreadLocalVars(Random random, List<Node> path)
            {
                this.random = random;
                this.path = path;
            }
        }

    }
}
