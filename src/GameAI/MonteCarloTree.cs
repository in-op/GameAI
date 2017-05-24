using System;
using System.Collections.Generic;
using CSharpExtras.Random;

namespace GameAI
{
    /// <summary>
    /// A method class for selecting moves in
    /// determinsitic, two-player, back-and-forth,
    /// zero-sum or zero-sum-tie games
    /// </summary>
    public static class MonteCarloTree
    {
        public interface Game
        {
            int GetCurrentPlayer();
            Game DeepCopy();
            /// <summary>
            /// Perform the specified transition. Implementations
            /// must update the hash value.
            /// </summary>
            /// <param name="t"></param>
            void DoMove(Transition t);
            /// <summary>
            /// Perform any random move. To optimize this method,
            /// omit the use and update of the hash value.
            /// </summary>
            void DoRandomMove();
            bool IsGameOver();
            int WhoWon();
            List<Transition> GetLegalTransitions();
            long GetHash();
        }

        public class Transition
        {
            /// <summary>
            /// The move to perform the transition
            /// </summary>
            public Move move;
            /// <summary>
            /// The hash of the resulting gamestate
            /// </summary>
            public long hash;

            public Transition(Move move, long hash)
            {
                this.move = move;
                this.hash = hash;
            }

            public override string ToString()
            {
                return "Move: " + move.ToString() + ". Hash: " + hash.ToString();
            }
        }

        public interface Move { }

        public static Transition Search(Game game, int simulations)
        {
            Dictionary<long, Node> tree = new Dictionary<long, Node>();
            tree.Add(game.GetHash(), new Node(game.GetCurrentPlayer()));

            List<Node> path = new List<Node>();

            Game copy;
            List<Transition> allTransitions;
            List<Transition> transitionsNoStats;

            Random rng = new Random();

            for (int i = 0; i < simulations; i++)
            {
                copy = game.DeepCopy();  
                path.Clear(); 
                path.Add(tree[game.GetHash()]); 
                
                while (true)
                {
                    if (copy.IsGameOver()) break;
                    
                    allTransitions = copy.GetLegalTransitions(); 
                    transitionsNoStats = new List<Transition>(); 
                    foreach (Transition t in allTransitions) 
                        if (!tree.ContainsKey(t.hash))
                            transitionsNoStats.Add(t);
                    
                    // SELECTION
                    if (transitionsNoStats.Count == 0) 
                    {
                        float bestScore = float.MinValue;
                        float parentPlays = path[path.Count - 1].plays;
                        float ucb1Score;
                        int indexOfBestTransition = 0;
                        //select move with highest UCB1
                        for (int j = 0; j < allTransitions.Count; j++)
                        {
                            ucb1Score = tree[allTransitions[j].hash].UCBScoreForParent(parentPlays);
                            if (ucb1Score > bestScore)
                            {
                                bestScore = ucb1Score;
                                indexOfBestTransition = j;
                            }
                        }
                        Transition bestTransition = allTransitions[indexOfBestTransition];
                        // do the move
                        copy.DoMove(bestTransition);
                        // add to path
                        path.Add(tree[bestTransition.hash]);
                    }

                    // EXPANSION
                    else
                    {
                        //select random move with no stats, do the move
                        copy.DoMove(transitionsNoStats.RandomItem(rng));

                        //add to the tree, and add to the path
                        Node n = new Node(copy.GetCurrentPlayer());
                        tree.Add(copy.GetHash(), n);
                        path.Add(n);
                        
                        break;
                    }
                }

                // ROLLOUT
                while (!copy.IsGameOver())
                    copy.DoRandomMove();

                // BACKPROP
                foreach (Node n in path)
                {
                    n.plays++;
                    if (copy.WhoWon() == n.player) n.wins++;
                }
            }

            // Simulations are over. Pick the best move, then return it
            allTransitions = game.GetLegalTransitions();
            int indexOfBestMoveFound = 0;
            float worstScoreFound = float.MaxValue;
            float score;

            Console.WriteLine("Root: plays-{0} wins-{1} plyr-{2}", tree[game.GetHash()].plays, tree[game.GetHash()].wins, tree[game.GetHash()].player);

            for (int i = 0; i < allTransitions.Count; i++)
            {
                Node n = tree[allTransitions[i].hash];
                Console.WriteLine("Move {0}: plays-{1} wins-{2} plyr-{3}", i, n.plays, n.wins, n.player);


                // **NOTE**
                // The best move chosen is the move with gives the
                // opponent the least number of victories
                score = tree[allTransitions[i].hash].ScoreForCurrentPlayer();
                if (score < worstScoreFound)
                {
                    worstScoreFound = score;
                    indexOfBestMoveFound = i;
                }
            }

            return allTransitions[indexOfBestMoveFound];
        }







        private static float UCB1(float childWins, float childPlays, float parentPlays)
        {
            return (childWins / childPlays) + (float)Math.Sqrt(2f * Math.Log(parentPlays) / childPlays);
        }



        private class Node
        {
            public float plays;
            public float wins;
            public int player;

            public float ScoreForCurrentPlayer()
            {
                return wins / plays;
            }

            public float UCBScoreForParent(float parentPlays)
            {   // plays - wins indicates how many winners for the opposing player (the player of the parent node)
                return UCB1(plays - wins, plays, parentPlays); 
            }

            private Node() { }

            public Node(int player)
            {
                this.player = player;
                plays = 0f;
                wins = 0f;
            }
        }

    }
}
