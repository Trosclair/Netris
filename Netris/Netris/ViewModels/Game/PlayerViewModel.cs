﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using WPFUtilities;
using Netris.ViewModels.Game.Pieces;
using Netris.ViewModels.Parameters;
using Netris.ViewModels.Settings;
using Netris.ViewModels.Settings.Controls;
using Netris.Models;
using System.Diagnostics;
using System.Linq;

namespace Netris.ViewModels.Game
{
    public class PlayerViewModel : ObservableObject
    {
        #region Fields
        private readonly Player model;
        private readonly ParametersViewModel parameters;
        private readonly Stopwatch gameTime = new();
        private readonly Shadow shadow;
        private readonly Action pause;
        private readonly PieceFactory pieceFactory;
        private readonly List<DASStateViewModel> dasControls = new();
        private readonly KeyboardPlayerControlsViewModel playerControls;
        private PieceViewModel currentPiece;
        private long currentDropTime = 0;
        private bool hasHeld = false;
        private bool gameOver = false;
        private bool isTSpin = false;
        private PieceViewModel next, holdPiece, one, two, three, four;
        private LevelViewModel level = new(new());
        #endregion

        #region Properties
        #region Pieces
        public PieceViewModel Next { get => next; set { next = value; OnPropertyChanged(nameof(Next)); } }
        public PieceViewModel HoldPiece { get => holdPiece; set { holdPiece = value; OnPropertyChanged(nameof(HoldPiece)); } }
        public PieceViewModel One { get => one; set { one = value; OnPropertyChanged(nameof(One)); } }
        public PieceViewModel Two { get => two; set { two = value; OnPropertyChanged(nameof(Two)); } }
        public PieceViewModel Three { get => three; set { three = value; OnPropertyChanged(nameof(Three)); } }
        public PieceViewModel Four { get => four; set { four = value; OnPropertyChanged(nameof(Four)); } }
        #endregion

        #region Indexers and Board
        public ObservableCollection<BlockViewModel> Board { get; } = new();
        public BlockViewModel this[int i, int j] { get => Board[(i * 10) + j]; set => Board[(i * 10) + j] = value; }
        public BlockViewModel this[BlockViewModel block] { get => this[block.X, block.Y]; set => this[block.X, block.Y] = value; }
        #endregion

        #region Game State
        public bool GameOver { get => gameOver; set { gameOver = value; OnPropertyChanged(nameof(GameOver)); } }
        public bool IsTSpin { get => isTSpin; set { isTSpin = value; OnPropertyChanged(nameof(IsTSpin)); } }
        public LevelViewModel Level { get => level; set { level = value; OnPropertyChanged(nameof(Level)); } }
        #endregion

        #region Statistics
        public int PlayerNumber { get => model.PlayerNumber; set { model.PlayerNumber = value; OnPropertyChanged(nameof(PlayerNumber)); } }
        public long Score { get => model.Score; set { model.Score = value; OnPropertyChanged(nameof(Score)); } }
        public int PiecesGenerated { get => model.PiecesGenerated; set { model.PiecesGenerated = value; OnPropertyChanged(nameof(PiecesGenerated)); } }
        public int LinesCleared { get => model.LinesCleared; set { model.LinesCleared = value; OnPropertyChanged(nameof(LinesCleared)); } }
        public int SingleCount { get => model.SingleCount; set { model.SingleCount = value; OnPropertyChanged(nameof(SingleCount)); } }
        public int DoubleCount { get => model.DoubleCount; set { model.DoubleCount = value; OnPropertyChanged(nameof(DoubleCount)); } }
        public int TripleCount { get => model.TripleCount; set { model.TripleCount = value; OnPropertyChanged(nameof(TripleCount)); } }
        public int TetrisCount { get => model.TetrisCount; set { model.TetrisCount = value; OnPropertyChanged(nameof(TetrisCount)); } }
        public int TSpinSingleCount { get => model.TSpinSingleCount; set { model.TSpinSingleCount = value; OnPropertyChanged(nameof(TSpinSingleCount)); } }
        public int TSpinDoubleCount { get => model.TSpinDoubleCount; set { model.TSpinDoubleCount = value; OnPropertyChanged(nameof(TSpinDoubleCount)); } }
        public int TSpinTripleCount { get => model.TSpinTripleCount; set { model.TSpinTripleCount = value; OnPropertyChanged(nameof(TSpinTripleCount)); } }
        public int PerfectClearsCount { get => model.PerfectClearsCount; set { model.PerfectClearsCount = value; OnPropertyChanged(nameof(PerfectClearsCount)); } }
        public int ICount { get => model.ICount; set { model.ICount = value; OnPropertyChanged(nameof(ICount)); } }
        public int UCount { get => model.UCount; set { model.UCount = value; OnPropertyChanged(nameof(UCount)); } }
        public int TCount { get => model.TCount; set { model.TCount = value; OnPropertyChanged(nameof(TCount)); } }
        public int SCount { get => model.SCount; set { model.SCount = value; OnPropertyChanged(nameof(SCount)); } }
        public int ZCount { get => model.ZCount; set { model.ZCount = value; OnPropertyChanged(nameof(ZCount)); } }
        public int LCount { get => model.LCount; set { model.LCount = value; OnPropertyChanged(nameof(LCount)); } }
        public int JCount { get => model.JCount; set { model.JCount = value; OnPropertyChanged(nameof(JCount)); } }
        #endregion
        #endregion

        public PlayerViewModel(SettingsViewModel settings, ParametersViewModel parameters, PieceFactory pieceFactory, Action pause, int playerNumber)
        {
            for (int i = 0; i < 200; i++)
                Board.Add(new BlockViewModel(i / 10, i % 10, Colors.Transparent, Brushes.Transparent));

            model = new();
            this.pieceFactory = pieceFactory;

            next = pieceFactory.GetNextPiece(this);
            one = pieceFactory.GetNextPiece(this);
            two = pieceFactory.GetNextPiece(this);
            three = pieceFactory.GetNextPiece(this);
            four = pieceFactory.GetNextPiece(this);
            holdPiece = new Empty(this);

            PlayerNumber = playerNumber;
            this.pause = pause;
            this.parameters = parameters;

            shadow = new(this);
            currentPiece = Pop();
            AddPieceToBoard(currentPiece);

            playerControls = settings.ControlSettings.KeyboardViewModels[playerNumber];
            dasControls.Add(new(parameters.DAS, MoveDown, () => Keyboard.IsKeyDown(playerControls.MoveDown)));
            dasControls.Add(new(parameters.DAS, MoveRight, () => Keyboard.IsKeyDown(playerControls.MoveRight)));
            dasControls.Add(new(parameters.DAS, MoveLeft, () => Keyboard.IsKeyDown(playerControls.MoveLeft)));
            dasControls.Add(new(parameters.DAS, RotateClockwise, () => Keyboard.IsKeyDown(playerControls.RotateClockwise)));
            dasControls.Add(new(parameters.DAS, RotateCounterClockwise, () => Keyboard.IsKeyDown(playerControls.RotateCounterClockwise)));
            dasControls.Add(new(parameters.DAS, HardDrop, () => Keyboard.IsKeyDown(playerControls.HardDrop)));

            UpdateLevel();

            gameTime.Start();
        }

        private void UpdateLevel()
        {
            foreach (LevelViewModel lvl in parameters.Levels)
            {
                if (!lvl.Equals(Level) && CanStartThisLevel(lvl))
                {
                    Level = lvl;
                    return;
                }
            }
        }

        private bool CanStartThisLevel(LevelViewModel lvl)
        {
            return
                Score >= lvl.ScoreRequirement &&
                LinesCleared >= lvl.LinesClearedRequirement &&
                PiecesGenerated >= lvl.PiecesGeneratedRequirement &&
                SingleCount >= lvl.SingleCountRequirement &&
                DoubleCount >= lvl.DoubleCountRequirement &&
                TripleCount >= lvl.TripleCountRequirement &&
                TetrisCount >= lvl.TetrisCountRequirement &&
                TSpinSingleCount >= lvl.TSpinSingleCountRequirement &&
                TSpinDoubleCount >= lvl.TSpinDoubleCountRequirement &&
                TSpinTripleCount >= lvl.TSpinTripleCountRequirement &&
                PerfectClearsCount >= lvl.PerfectClearsCountRequirement;
        }

        private PieceViewModel Pop()
        {
            PieceViewModel result = Next;
            Next = One;
            One = Two;
            Two = Three;
            Three = Four;
            Four = pieceFactory.GetNextPiece(this);
            hasHeld = false;
            return result;
        }

        private PieceViewModel SwapHoldPiece(PieceViewModel currentPiece)
        {
            if (!hasHeld)
            {
                PieceViewModel result;

                currentPiece.ResetPiecePosition();

                if (HoldPiece is Empty)
                {
                    HoldPiece = currentPiece;
                    result = Pop();
                }
                else
                {
                    result = HoldPiece;
                    HoldPiece = currentPiece;
                }
                hasHeld = true;

                currentDropTime = gameTime.ElapsedMilliseconds;
                return result;
            }
            else
            {
                return currentPiece;
            }
        }

        private void MoveLeft()
        {
            currentPiece.MoveLeft();
        }

        private void MoveRight()
        {
            currentPiece.MoveRight();
        }

        private void MoveDown()
        {
            if (!currentPiece.MoveDown())
            {
                CheckBoardForLineClears();
                currentPiece = Pop();
                AddPieceToBoard(currentPiece);
            }
            currentDropTime = gameTime.ElapsedMilliseconds;
        }

        private void Hold()
        {
            RemovePieceFromBoard(currentPiece);
            currentPiece = SwapHoldPiece(currentPiece);
            AddPieceToBoard(currentPiece);
        }

        private void RotateCounterClockwise()
        {
            currentPiece.RotateCounterClockwise();
        }

        private void RotateClockwise()
        {
            currentPiece.RotateClockwise();
        }

        private void HardDrop()
        {
            currentPiece.HardDrop();
            CheckBoardForLineClears();
            currentPiece = Pop();
            AddPieceToBoard(currentPiece);
            currentDropTime = gameTime.ElapsedMilliseconds;
        }

        public void Pause()
        {
            gameTime.Stop();
        }

        public void UnPause()
        {
            gameTime.Start();
        }

        public void CheckForInput()
        {
            if (Keyboard.IsKeyDown(playerControls.Pause))
            {
                pause();
            }
            else if (Keyboard.IsKeyDown(playerControls.Hold))
            {
                Hold();
            }
            else
            {
                foreach (DASStateViewModel dasControl in dasControls)
                {
                    if (dasControl.Predicate())
                    {
                        if (!dasControl.IsDown)
                        {
                            dasControl.Move();
                            dasControl.IsDown = true;
                        }
                        dasControl.Update();
                    }
                    else
                    {
                        if (dasControl.IsDown)
                            dasControl.IsDown = false;
                    }
                }
            }
        }

        public void UpdateBoard()
        {
            if (gameTime.ElapsedMilliseconds > currentDropTime + (Level.FramesBeforeAutoDrop * 16.67))
            {
                MoveDown();
                currentDropTime = gameTime.ElapsedMilliseconds;
            }

            UpdateLevel();
        }

        public bool MakeMoveIfValid(PieceViewModel piece, Action makeMove, Action revertMove)
        {
            bool isMoveValid;

            RemovePieceFromBoard(piece);

            makeMove();

            isMoveValid = !PieceViewModelExtensions.IsOutOfBounds(piece);

            if (isMoveValid)
            {
                foreach (BlockViewModel block in piece.Blocks)
                {
                    if (!this[block.X, block.Y].IsEmpty)
                    {
                        isMoveValid = false;
                        revertMove();
                        break;
                    }
                }
            }
            else
            {
                revertMove();
            }

            AddPieceToBoard(piece);

            return isMoveValid;
        }

        private void AddPieceToBoard(PieceViewModel piece)
        {
            UpdateShadow(piece);

            foreach (BlockViewModel block in piece.Blocks)
            {
                this[block.X, block.Y].Color = piece.One.Color;
                this[block.X, block.Y].Brush = piece.One.Brush;
            }
        }

        private void RemovePieceFromBoard(PieceViewModel piece)
        {
            foreach (BlockViewModel block in piece.Blocks)
            {
                this[block.X, block.Y].Color = Colors.Transparent;
                this[block.X, block.Y].Brush = Brushes.Transparent;
            }
        }

        private void CheckBoardForLineClears()
        {
            int linesCleared = 0;

            for (int i = 19; i >= 0; i--)
            {
                bool isClear = true;

                for (int j = 0; j < 10; j++)
                {
                    isClear &= !this[i, j].IsEmpty;
                }

                if (isClear)
                {
                    linesCleared++;
                    ClearLine(i);
                    i++;
                }
            }

            if (linesCleared > 0)
            {
                if (Board.All(x => x.IsEmpty))
                {
                    PerfectClearsCount++;
                    Score += Level.PointsForPerfectClear;
                }

                if (IsTSpin)
                {
                    if (linesCleared == 1)
                    {
                        TSpinSingleCount++;
                        Score += Level.PointsForTSpinSingle;
                    }
                    else if (linesCleared == 2)
                    {
                        TSpinDoubleCount++;
                        Score += Level.PointsForTSpinDouble;
                    }
                    else
                    {
                        TSpinTripleCount++;
                        Score += Level.PointsForTSpinTriple;
                    }
                    IsTSpin = false;
                }
                else
                {
                    if (linesCleared == 1)
                    {
                        SingleCount++;
                        Score += Level.PointsForSingle;
                    }
                    else if (linesCleared == 2)
                    {
                        DoubleCount++;
                        Score += Level.PointsForDouble;
                    }
                    else if (linesCleared == 3)
                    {
                        TripleCount++;
                        Score += Level.PointsForTriple;
                    }
                    else
                    {
                        TetrisCount++;
                        Score += Level.PointsForTetris;
                    }
                }
            }
        }

        private void UpdateShadow(PieceViewModel piece)
        {
            foreach (BlockViewModel block in shadow.Blocks)
            {
                if (this[block].Brush == Brushes.White)
                {
                    this[block].Brush = Brushes.Transparent;
                    this[block].Color = Colors.Transparent;
                }
            }

            shadow.One = new(piece.One.X, piece.One.Y, Colors.White, Brushes.White);
            shadow.Two = new(piece.Two.X, piece.Two.Y, Colors.White, Brushes.White);
            shadow.Three = new(piece.Three.X, piece.Three.Y, Colors.White, Brushes.White);
            shadow.Four = new(piece.Four.X, piece.Four.Y, Colors.White, Brushes.White);

            do
            {
                shadow.One.X++;
                shadow.Two.X++;
                shadow.Three.X++;
                shadow.Four.X++;
            }
            while (!shadow.IsOutOfBounds() && this[shadow.One].IsEmpty && this[shadow.Two].IsEmpty && this[shadow.Three].IsEmpty && this[shadow.Four].IsEmpty);

            shadow.One.X--;
            shadow.Two.X--;
            shadow.Three.X--;
            shadow.Four.X--;

            foreach (BlockViewModel block in shadow.Blocks)
            {
                this[block].Color = shadow.One.Color;
                this[block].Brush = shadow.One.Brush;
            }
        }

        private void ClearLine(int rowToClear)
        {
            for (int i = rowToClear; i >= 1; i--)
            {
                for (int j = 0; j < 10; j++)
                {
                    this[i, j].Color = this[i - 1, j].Color;
                    this[i, j].Brush = this[i - 1, j].Brush;
                }
            }

            for (int i = 0; i < 10; i++)
            {
                this[0, i].Color = Colors.Transparent;
                this[0, i].Brush = Brushes.Transparent;
            }
        }
    }
}
