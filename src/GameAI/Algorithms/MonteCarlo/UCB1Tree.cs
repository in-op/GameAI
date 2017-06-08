using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using SystemExtensions.Copying;
using SystemExtensions.Random;
using GameAI.GameInterfaces;
using ParallelNET35.Concurrent;

namespace GameAI.Algorithms.MonteCarlo
{

    /// <summary>
    /// A method class for selecting moves in determinsitic, two-player, back-and-forth, zero-sum or zero-sum-tie games
    /// </summary>
    public static class UCB1Tree<TGame, TMove, TPlayer>
        where TGame : UCB1Tree<TGame, TMove, TPlayer>.IGame
    {
        /// <summary>
        /// The interface games must implement in order to use the Monte Carlo tree search algorithm.
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
        /// A custom data structure for storing a move and the gamestate's resultant hash code after performing that move.
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
            /// Returns a new Transition with the specified move to perform and a hash representing the resulting gamestate.
            /// </summary>
            /// <param name="move">The move to perform to execute the transition.</param>
            /// <param name="hash">The hash code of the resulting gamestate after performing the move.</param>
            public Transition(TMove move, long hash)
            {
                Move = move;
                Hash = hash;
            }

            /// <summary>
            /// Custom override for debugging and testing purposes.
            /// </summary>
            public override string ToString()
            {
                return "Move: " + Move.ToString() + ". Hash: " + Hash.ToString();
            }
        }

        /// <summary>
        /// Returns the best Transition discovered after performing the specified number of simulations, in parallel, on the game.
        /// </summary>
        /// <param name="game">The current state of the game from which to find the best move for the current player.</param>
        /// <param name="milliseconds">The length of time the search to run.</param>
        public static Transition ParallelSearch(TGame game, long milliseconds)
        {
            ConcurrentDictionary<long, Node> tree = new ConcurrentDictionary<long, Node>();
            tree.TryAdd(game.Hash, new Node(game.CurrentPlayer));
            Stopwatch sw = new Stopwatch();
            sw.Start();
            ParallelNET35.Parallel.For(0L, Int64.MaxValue,

                () => { return new ThreadLocalVars(ThreadLocalRandom.NewRandom(), new List<Node>(50)); },

                (i, loop, localVars) =>
                {
                    if (sw.ElapsedMilliseconds > milliseconds) loop.Stop();

                    TGame copy = game.DeepCopy();
                    localVars.path.Clear();
                    localVars.path.Add(tree[game.Hash]);

                    while (!copy.IsGameOver())
                    {
                        List<Transition> transitions = copy.GetLegalTransitions();
                        List<Transition> transitionsNoStats = new List<Transition>();
                        foreach (Transition t in transitions)
                            if (!tree.ContainsKey(t.Hash))
                                transitionsNoStats.Add(t);

                        // SELECTION
                        if (transitionsNoStats.Count == 0)
                        {
                            double bestScore = double.MinValue;
                            int parentPlays = localVars.path[localVars.path.Count - 1].plays;
                            double ucb1Score;
                            int indexOfBestTransition = 0;
                            for (int j = 0; j < transitions.Count; j++)
                            {
                                ucb1Score = tree[transitions[j].Hash].UCBScoreForParent(parentPlays);
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

                            if (tree.TryAdd(copy.Hash, n)) localVars.path.Add(n);
                            else localVars.path.Add(tree[copy.Hash]);

                            break;
                        }
                    }

                    // ROLLOUT
                    copy.Rollout();

                    // BACKPROP
                    foreach (Node n in localVars.path)
                    {
                        Interlocked.Add(ref n.plays, 1);
                        if (copy.IsWinner(n.player)) Interlocked.Add(ref n.wins, 1);
                    }

                    return localVars;
                },


                (x) => { }
                );



            // Simulations are over. Pick the best move, then return it
            List<Transition> allTransitions = game.GetLegalTransitions();
            int indexOfBestMoveFound = 0;
            double worstScoreFound = double.MaxValue;
            double score;

            //Console.WriteLine("Root: plays-{0} wins-{1} plyr-{2}", tree[game.GetHash()].plays, tree[game.GetHash()].wins, tree[game.GetHash()].player);

            for (int i = 0; i < allTransitions.Count; i++)
            {
                Node n = tree[allTransitions[i].Hash];
                //Console.WriteLine("Move {0}: plays-{1} wins-{2} plyr-{3}", i, n.plays, n.wins, n.player);


                // **NOTE**
                // The best move chosen is the move with gives the
                // opponent the least number of victories
                score = tree[allTransitions[i].Hash].ScoreForCurrentPlayer();
                if (score < worstScoreFound)
                {
                    worstScoreFound = score;
                    indexOfBestMoveFound = i;
                }
            }

            return allTransitions[indexOfBestMoveFound];
        }


        /// <summary>
        /// Returns the best Transition discovered after performing the specified number of simulations, in parallel, on the game.
        /// </summary>
        /// <param name="game">The current state of the game from which to find the best move for the current player.</param>
        /// <param name="simulations">The number of simulations of the game to perform.</param>
        public static Transition ParallelSearch(TGame game, int simulations)
        {
            ConcurrentDictionary<long, Node> tree = new ConcurrentDictionary<long, Node>();
            tree.TryAdd(game.Hash, new Node(game.CurrentPlayer));
            

            ParallelNET35.Parallel.For(0, simulations,

                () => { return new ThreadLocalVars(ThreadLocalRandom.NewRandom(), new List<Node>(50)); },
                
                (i, loop, localVars) =>
                {
                    TGame copy = game.DeepCopy();
                    localVars.path.Clear();
                    localVars.path.Add(tree[game.Hash]);

                    while (!copy.IsGameOver())
                    {
                        List<Transition>  transitions = copy.GetLegalTransitions();
                        List<Transition> transitionsNoStats = new List<Transition>();
                        foreach (Transition t in transitions)
                            if (!tree.ContainsKey(t.Hash))
                                transitionsNoStats.Add(t);

                        // SELECTION
                        if (transitionsNoStats.Count == 0)
                        {
                            double bestScore = double.MinValue;
                            int parentPlays = localVars.path[localVars.path.Count - 1].plays;
                            double ucb1Score;
                            int indexOfBestTransition = 0;
                            for (int j = 0; j < transitions.Count; j++)
                            {
                                ucb1Score = tree[transitions[j].Hash].UCBScoreForParent(parentPlays);
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
                            if (tree.TryAdd(copy.Hash, n)) localVars.path.Add(n);
                            else localVars.path.Add(tree[copy.Hash]);

                            break;
                        }
                    }

                    // ROLLOUT
                    copy.Rollout();

                    // BACKPROP
                    foreach (Node n in localVars.path)
                    {
                        Interlocked.Add(ref n.plays, 1);
                        if (copy.IsWinner(n.player)) Interlocked.Add(ref n.wins, 1);
                    }

                    return localVars;
                },


                (x) => { }
                );
            

            // Simulations are over. Pick the best move, then return it
            List<Transition> allTransitions = game.GetLegalTransitions();
            int indexOfBestMoveFound = 0;
            double worstScoreFound = double.MaxValue;
            double score;

            //Console.WriteLine("Root: plays-{0} wins-{1} plyr-{2}", tree[game.GetHash()].plays, tree[game.GetHash()].wins, tree[game.GetHash()].player);

            for (int i = 0; i < allTransitions.Count; i++)
            {
                Node n = tree[allTransitions[i].Hash];
                //Console.WriteLine("Move {0}: plays-{1} wins-{2} plyr-{3}", i, n.plays, n.wins, n.player);


                // **NOTE**
                // The best move chosen is the move with gives the
                // opponent the least number of victories
                score = tree[allTransitions[i].Hash].ScoreForCurrentPlayer();
                if (score < worstScoreFound)
                {
                    worstScoreFound = score;
                    indexOfBestMoveFound = i;
                }
            }

            return allTransitions[indexOfBestMoveFound];
        }


















        /// <summary>
        /// Returns the best Transition discovered after performing the specified number of simulations on the game.
        /// </summary>
        /// <param name="game">The current state of the game from which to find the best move for the current player.</param>
        /// <param name="milliseconds">The length of time search will run.</param>
        public static Transition Search(TGame game, long milliseconds)
        {
            Dictionary<long, Node> tree = new Dictionary<long, Node>();
            tree.Add(game.Hash, new Node(game.CurrentPlayer));

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
                    foreach (Transition t in allTransitions)
                        if (!tree.ContainsKey(t.Hash))
                            transitionsNoStats.Add(t);

                    // SELECTION
                    if (transitionsNoStats.Count == 0)
                    {
                        double bestScore = float.MinValue;
                        int parentPlays = path[path.Count - 1].plays;
                        double ucb1Score;
                        int indexOfBestTransition = 0;
                        for (int j = 0; j < allTransitions.Count; j++)
                        {
                            ucb1Score = tree[allTransitions[j].Hash].UCBScoreForParent(parentPlays);
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

                        Node n = new Node(copy.CurrentPlayer);
                        tree.Add(copy.Hash, n);
                        path.Add(n);

                        break;
                    }
                }

                // ROLLOUT
                copy.Rollout();

                // BACKPROP
                foreach (Node n in path)
                {
                    n.plays++;
                    if (copy.IsWinner(n.player)) n.wins++;
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
                Node n = tree[allTransitions[i].Hash];
                //Console.WriteLine("Move {0}: plays-{1} wins-{2} plyr-{3}", i, n.plays, n.wins, n.player);


                // **NOTE**
                // The best move chosen is the move with gives the
                // opponent the least number of victories
                score = tree[allTransitions[i].Hash].ScoreForCurrentPlayer();
                if (score < worstScoreFound)
                {
                    worstScoreFound = score;
                    indexOfBestMoveFound = i;
                }
            }

            return allTransitions[indexOfBestMoveFound];
        }









        /// <summary>
        /// Returns the best Transition discovered after performing the specified number of simulations on the game.
        /// </summary>
        /// <param name="game">The current state of the game from which to find the best move for the current player.</param>
        /// <param name="simulations">The number of simulations of the game to perform.</param>
        public static Transition Search(TGame game, int simulations)
        {
            Dictionary<long, Node> tree = new Dictionary<long, Node>();
            tree.Add(game.Hash, new Node(game.CurrentPlayer));

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
                    foreach (Transition t in allTransitions)
                        if (!tree.ContainsKey(t.Hash))
                            transitionsNoStats.Add(t);

                    // SELECTION
                    if (transitionsNoStats.Count == 0)
                    {
                        double bestScore = float.MinValue;
                        int parentPlays = path[path.Count - 1].plays;
                        double ucb1Score;
                        int indexOfBestTransition = 0;
                        for (int j = 0; j < allTransitions.Count; j++)
                        {
                            ucb1Score = tree[allTransitions[j].Hash].UCBScoreForParent(parentPlays);
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

                        Node n = new Node(copy.CurrentPlayer);
                        tree.Add(copy.Hash, n);
                        path.Add(n);

                        break;
                    }
                }

                // ROLLOUT
                copy.Rollout();

                // BACKPROP
                foreach (Node n in path)
                {
                    n.plays++;
                    if (copy.IsWinner(n.player)) n.wins++;
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
                Node n = tree[allTransitions[i].Hash];
                //Console.WriteLine("Move {0}: plays-{1} wins-{2} plyr-{3}", i, n.plays, n.wins, n.player);


                // **NOTE**
                // The best move chosen is the move with gives the
                // opponent the least number of victories
                score = tree[allTransitions[i].Hash].ScoreForCurrentPlayer();
                if (score < worstScoreFound)
                {
                    worstScoreFound = score;
                    indexOfBestMoveFound = i;
                }
            }

            return allTransitions[indexOfBestMoveFound];
        }







        private static double UCB1(double childWins, double childPlays, double parentPlays)
        {
            return (childWins / childPlays) + Math.Sqrt(2f * Math.Log(parentPlays) / childPlays);
        }



        private class Node
        {
            public int plays;
            public int wins;
            public TPlayer player;

            public double ScoreForCurrentPlayer()
            {
                double w = wins;
                double p = plays;
                return w / p;
            }

            public double UCBScoreForParent(int parentPlays)
            {   // plays - wins indicates how many winners for the opposing player (the player of the parent node)
                return UCB1(plays - wins, plays, parentPlays);
            }

            private Node() { }

            public Node(TPlayer player)
            {
                this.player = player;
                plays = 0;
                wins = 0;
            }
        }



        private class ThreadLocalVars
        {
            internal Random random;
            internal List<Node> path;

            internal ThreadLocalVars(Random random, List<Node> path)
            {
                this.random = random;
                this.path = path;
            }
        }

    }
}
