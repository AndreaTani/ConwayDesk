using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace ConwayDesk
{
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

        public event PropertyChangedEventHandler? PropertyChanged;

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

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public GameViewModel()
        {
            // Inizializza qui i comandi e la griglia
            InitializeCommands();
            InitializeGrid();
        }

        private void InitializeCommands()
        {
            // Inizializza i comandi
            StartStopCommand = new RelayCommand(ExecuteStartStop);
            ResetCommand = new RelayCommand(ExecuteReset, param => !_isGameRunning);
            SeedCommand = new RelayCommand(ExecuteSeed, param => !_isGameRunning);

            // Inizializza il timer per il loop di gioco
            _gameLoopTimer = new DispatcherTimer();
            _gameLoopTimer.Interval = TimeSpan.FromMilliseconds(200);
            _gameLoopTimer.Tick += GameLoop_Tick;
        }

        // ---------------------------------------------------------------------
        //                           GAME LOGIC
        // ---------------------------------------------------------------------

        public void IncreaseScore()
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

        private async void GameLoop_Tick(object sender, EventArgs e)
        {
            _gameLoopTimer.Stop();

            bool[] nextState = await Task.Run(() => ComputeNextStateInParallel());

            for (int i = 0; i < Cells.Count; i++)
            {
                Cells[i].IsActive = nextState[i];
            }

            IncreaseScore();

            _gameLoopTimer.Start();
        }

        // Cambia lo stato del pulsante ed il suo funzionamento a conseguenza dello stato del gioco
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

        // Reset: Imposta tutte le celle a IsActive = false
        private void ExecuteReset(object parameter)
        {
            foreach (var cell in Cells)
            {
                cell.IsActive = false;
            }
            Generation = 0;
        }

        // Seed Casuale: Imposta una cella come attiva con una probabilità del 25%
        private void ExecuteSeed(object parameter)
        {
            var random = new Random();
            foreach (var cell in Cells)
            {
                cell.IsActive = random.Next(100) < 25;
            }
        }

        // Logica centrale per il calcolo dello stato successivo delle celle
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
