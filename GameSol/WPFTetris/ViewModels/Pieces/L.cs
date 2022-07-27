﻿namespace WPFTetris.ViewModels.Pieces
{
    public class L : Piece
    {
        public L() : base(PieceType.L, 'L')
        {
            One = new BlockViewModel(0, 5);
            Two = new BlockViewModel(1, 5);
            Three = new BlockViewModel(2, 5);
            Four = new BlockViewModel(2, 6);
        }

        ///*****
        ///**1**
        ///**2**
        ///**34*
        ///*****

        public override void RotateRight(BoardViewModel board)
        {
            if (Three.Y + 1 == Four.Y)
            {
                if (Two.Y > 0)
                {
                    if (board[Two.X, Two.Y - 1] == 0 && board[Two.X + 1, Two.Y - 1] == 0 && board[Two.X, Two.Y + 1] == 0)
                    {
                        if (Three.Y != 0)
                        {
                            One.X++;
                            One.Y++;
                            Three.X--;
                            Three.Y--;
                            Four.Y -= 2;
                        }
                    }
                }
            }
            else if (Three.X + 1 == Four.X)
            {
                if (board[Two.X - 1, Two.Y] == 0 && board[Two.X - 1, Two.Y - 1] == 0 && board[Two.X + 1, Two.Y] == 0)
                {
                    One.X++;
                    One.Y--;
                    Three.X--;
                    Three.Y++;
                    Four.X -= 2;
                }
            }
            else if (Four.Y + 1 == Three.Y)
            {
                if (Two.Y != 9)
                {
                    if (board[Two.X, Two.Y + 1] == 0 && board[Two.X - 1, Two.Y + 1] == 0 && board[Two.X, Two.Y - 1] == 0)
                    {
                        if (Three.Y != 9)
                        {
                            One.X--;
                            One.Y--;
                            Three.X++;
                            Three.Y++;
                            Four.Y += 2;
                        }
                    }
                }
            }
            else if (Three.X - 1 == Four.X)
            {
                if (board[Two.X + 1, Two.Y] == 0 && board[Two.X + 1, Two.Y + 1] == 0 && board[Two.X - 1, Two.Y] == 0)
                {
                    if (Three.X != 19)
                    {
                        One.X--;
                        One.Y++;
                        Three.X++;
                        Three.Y--;
                        Four.X += 2;
                    }
                }
            }
        }

        public override void RotateLeft(BoardViewModel board)
        {
            if (Three.Y + 1 == Four.Y)
            {
                if (Two.Y > 0)
                {
                    if (board[Two.X, Two.Y - 1] == 0 && board[Two.X + 1, Two.Y + 1] == 0 && board[Two.X, Two.Y + 1] == 0)
                    {
                        if (Three.Y != 0)
                        {
                            One.X++;
                            One.Y--;
                            Three.X--;
                            Three.Y++;
                            Four.X -= 2;
                        }
                    }
                }
            }
            else if (Three.X - 1 == Four.X)
            {
                if (board[Two.X - 1, Two.Y] == 0 && board[Two.X + 1, Two.Y] == 0 && board[Two.X - 1, Two.Y - 1] == 0)
                {
                    if (Three.X != 19)
                    {
                        One.X++;
                        One.Y++;
                        Three.X--;
                        Three.Y--;
                        Four.Y -= 2;
                    }
                }
            }
            else if (Three.Y - 1 == Four.Y)
            {
                if (Two.Y != 9)
                {
                    if (board[Two.X, Two.Y - 1] == 0 && board[Two.X + 1, Two.Y - 1] == 0 && board[Two.X, Two.Y + 1] == 0)
                    {
                        if (Three.Y != 9)
                        {
                            One.X--;
                            One.Y++;
                            Three.X++;
                            Three.Y--;
                            Four.X += 2;
                        }
                    }
                }
            }
            else
            {
                if (board[Two.X - 1, Two.Y] == 0 && board[Two.X + 1, Two.Y] == 0 && board[Two.X + 1, Two.Y + 1] == 0)
                {
                    One.X--;
                    One.Y--;
                    Three.X++;
                    Three.Y++;
                    Four.Y += 2;
                }
            }
        }
    }
}
