using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace ConwayDesk
{
    /// <summary>
    /// Represents the view model for this game, managing the state and behavior
    /// of the game grid and its cells.
    /// </summary>
    /// <remarks>This class implements the <see cref="INotifyPropertyChanged"/>
    /// interface to support data  binding in a UI context. It manages the game 
    /// logic, including starting, stopping, and resetting the game, as well as
    /// computing the next state of the game grid based on the rules of Conway's
    /// Game of Life. The view model also provides commands for user 
    /// interactions and maintains properties that reflect the current state of 
    /// game.</remarks>
    public class GameViewModel : INotifyPropertyChanged
    {
        // ---------------------------------------------------------------------
        //                             FIELDS
        // ---------------------------------------------------------------------

        private const int DefaultGridSize = 30;
        private ObservableCollection<Cell> _cells;
        private int _gridSize = DefaultGridSize;
        private int _generation = 0;
        private string _startStopButtonText = "Start";
        private bool _isGameRunning = false;
        private DispatcherTimer _gameLoopTimer;
        private double _cellSize;


        // ---------------------------------------------------------------------
        //                           PROPERTIES
        // ---------------------------------------------------------------------
        public bool IsGameRunning
        {
            get => _isGameRunning;
            set
            {
                _isGameRunning = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Cell> Cells
        {
            get => _cells;
            set
            {
                _cells = value;
                OnPropertyChanged();
            }
        }

        public double CellSize
        {
            get => _cellSize;
            set
            {
                if (_cellSize != value)
                {
                    _cellSize = value;
                    OnPropertyChanged();
                }
            }
        }

        public int GridSize
        {
            get => _gridSize;
            set
            {
                if (_gridSize != value)
                {
                    _gridSize = value;
                    OnPropertyChanged();
                    InitializeGrid();
                }
            }
        }

        public int Generation
        {
            get { return _generation; }
            set
            {
                if (_generation != value)
                {
                    _generation = value;
                    OnPropertyChanged();
                }
            }
        }

        public string StartStopButtonText
        {
            get => _startStopButtonText;
            set
            {
                if (_startStopButtonText != value)
                {
                    _startStopButtonText = value;
                    OnPropertyChanged();
                }
            }
        }

        public RelayCommand StartStopCommand { get; private set; }
        public RelayCommand ResetCommand { get; private set; }
        public RelayCommand SeedCommand { get; private set; }


        // ---------------------------------------------------------------------
        //                          WINDOW LOGIC
        // ---------------------------------------------------------------------

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public GameViewModel()
        {
            InitializeCommands();
            InitializeGrid();
        }

        private void InitializeCommands()
        {
            // Initialize comands
            StartStopCommand = new RelayCommand(ExecuteStartStop);
            ResetCommand = new RelayCommand(ExecuteReset, param => !_isGameRunning);
            SeedCommand = new RelayCommand(ExecuteSeed, param => !_isGameRunning);

            // Initialize game loop timer
            _gameLoopTimer = new DispatcherTimer();
            _gameLoopTimer.Interval = TimeSpan.FromMilliseconds(200);
            _gameLoopTimer.Tick += GameLoop_Tick;
        }

        // ---------------------------------------------------------------------
        //                           GAME LOGIC
        // ---------------------------------------------------------------------

        /// <summary>
        /// Increments the generation counter by one.
        /// </summary>
        /// <remarks>This method increases the value of the generation counter, 
        /// which is used int he UI to track the number of iterations or cycles 
        /// in the game/simulation.</remarks>
        public void IncreaseGenerationCounter()
        {
            Generation++;
        }

        private void StartGame()
        {
            IsGameRunning = true;
            _gameLoopTimer.Start();
        }

        private void StopGame()
        {
            IsGameRunning = false;
            _gameLoopTimer.Stop();
        }

        /// <summary>
        /// Handles the tick event of the game loop, updating the state of each 
        /// cell asynchronously.
        /// </summary>
        /// <remarks>This method stops the game loop timer, computes the next 
        /// state of the cells in parallel, updates the cell states, increments 
        /// the generation counter, and then restarts the timer.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private async void GameLoop_Tick(object sender, EventArgs e)
        {
            _gameLoopTimer.Stop();

            bool[] nextState = await Task.Run(() => ComputeNextStateInParallel());

            for (int i = 0; i < Cells.Count; i++)
            {
                Cells[i].IsActive = nextState[i];
            }

            IncreaseGenerationCounter();

            _gameLoopTimer.Start();
        }

        /// <summary>
        /// Toggles the game state between running and stopped.
        /// </summary>
        /// <remarks>When the game is running, this method stops the game and 
        /// updates the button text to "Start". Conversely, when the game is not
        /// running, it starts the game and updates the button text to "Stop".
        /// Additionally, it triggers a re-evaluation of the execution status 
        /// for the reset and seed commands.</remarks>
        /// <param name="parameter">An optional parameter that is not used in 
        /// this method.</param>
        private void ExecuteStartStop(object parameter)
        {
            if (_isGameRunning)
            {
                StartStopButtonText = "Start";
                StopGame();
            }
            else
            {
                StartStopButtonText = "Stop";
                StartGame();
            }

            ResetCommand.RaiseCanExecuteChanged();
            SeedCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Resets the state of all cells and the generation counter.
        /// </summary>
        /// <remarks>This method iterates through all cells, setting their 
        /// active state to false, and resets the generation count to zero.
        /// </remarks>
        /// <param name="parameter">An optional parameter that is not used in 
        /// this method.</param>
        private void ExecuteReset(object parameter)
        {
            foreach (var cell in Cells)
            {
                cell.IsActive = false;
            }
            Generation = 0;
        }

        /// <summary>
        /// Randomly activates cells within the collection based on a 
        /// probability threshold.
        /// </summary>
        /// <remarks>Each cell in the collection has a 25% chance of being 
        /// activated. The method iterates over all cells and sets their 
        /// <see cref="Cell.IsActive"/> property accordingly.</remarks>
        /// <param name="parameter">An object parameter that is currently unused
        /// in this method.</param>
        private void ExecuteSeed(object parameter)
        {
            var random = new Random();
            foreach (var cell in Cells)
            {
                cell.IsActive = random.Next(100) < 25;
            }
        }

        /// <summary>
        /// Computes the next state of each cell in parallel based on the 
        /// current state and the number of live
        /// neighbors.
        /// </summary>
        /// <remarks>This method uses parallel processing to efficiently 
        /// calculate the next generation of cell states in a grid. Each cell's
        /// next state is determined by the number of live neighbors it has,
        /// following the rules of Conway's Game of Life.</remarks>
        /// <returns>An array of boolean values representing the next state of 
        /// each cell. Each element is <see langword="true"/>
        /// if the cell is active in the next generation; otherwise, 
        /// <see langword="false"/>.</returns>
        private bool[] ComputeNextStateInParallel()
        {
            bool[] nextGenerationState = new bool[Cells.Count];

            Parallel.For(0, Cells.Count, i =>
            {
                Cell currentCell = Cells[i];
                int liveNeighbors = CountLiveNeighbors(currentCell.Row, currentCell.Col);

                if (currentCell.IsActive)
                {
                    if (liveNeighbors == 2 || liveNeighbors == 3)
                        nextGenerationState[i] = true;
                    else
                        nextGenerationState[i] = false;
                }
                else
                {
                    if (liveNeighbors == 3)
                        nextGenerationState[i] = true;
                    else
                        nextGenerationState[i] = false;
                }
            });

            return nextGenerationState;
        }

        /// <summary>
        /// Counts the number of active neighboring cells around a specified 
        /// cell in the grid.
        /// </summary>
        /// <remarks>This method considers all eight possible neighbors around 
        /// the specified cell, wrapping around the grid edges.</remarks>
        /// <param name="row">The row index of the cell for which to count live
        /// neighbors.</param>
        /// <param name="col">The column index of the cell for which to count 
        /// live neighbors.</param>
        /// <returns>The number of active neighboring cells surrounding the 
        /// specified cell.</returns>
        private int CountLiveNeighbors(int row, int col)
        {
            int liveNeighbors = 0;

            for (int r = row - 1; r <= row + 1; r++)
            {
                for (int c = col - 1; c <= col + 1; c++)
                {
                    if (r == row && c == col)
                        continue;

                    int neighborRow = (r + GridSize) % GridSize;
                    int neighborCol = (c + GridSize) % GridSize;

                    int index = neighborRow * GridSize + neighborCol;
                    if (index >= 0 && index < Cells.Count && Cells[index].IsActive)
                    {
                        liveNeighbors++;
                    }
                }
            }
            return liveNeighbors;
        }

        /// <summary>
        /// Initializes the grid by creating a new collection of cells.
        /// </summary>
        /// <remarks>This method populates the grid with inactive cells, 
        /// setting up the initial state for
        /// further operations. Each cell is assigned a row and column index 
        /// corresponding to its position in the
        /// grid.</remarks>
        private void InitializeGrid()
        {
            var newCells = new ObservableCollection<Cell>();
            for (int r = 0; r < GridSize; r++)
            {
                for (int c = 0; c < GridSize; c++)
                {
                    newCells.Add(new Cell { Row = r, Col = c, IsActive = false });
                }
            }
            Cells = newCells;
        }

    }
}
