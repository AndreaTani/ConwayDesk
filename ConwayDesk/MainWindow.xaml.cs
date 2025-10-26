using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ConwayDesk
{
    /// <summary>
    /// Represents the main window of the application, providing the 
    /// user interface for interacting with the game.
    /// </summary>
    /// <remarks>The <c>MainWindow</c> class is responsible for initializing 
    /// the user interface, handling user interactions, and managing the layout
    /// of the game area. It interacts with the <see cref="GameViewModel"/> to
    /// reflect the current state of the game and respond to changes.</remarks>
    public partial class MainWindow : Window
    {
        // ---------------------------------------------------------------------
        //                             FIELDS
        // ---------------------------------------------------------------------

        private const double FixedHeaderHeight = 80;
        private const double UITitleBarApproxHeight = 32;
        private const double HorizontalMarginCompensation = 20;
        private const double TotalVerticalMargins = 20;
        private bool _isDrawingMode = false;
        private Cell _lastClickedCell = null;
        private const double MouseDetectionArea = 10;


        // ---------------------------------------------------------------------
        //                           PROPERTIES
        // ---------------------------------------------------------------------

        private readonly GameViewModel _viewModel = new GameViewModel();


        // ---------------------------------------------------------------------
        //                          WINDOW LOGIC
        // ---------------------------------------------------------------------

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;

            _viewModel.PropertyChanged += ViewModel_PropertyChanged;

            UpdateHeaderAndGameAreaSize();
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GameViewModel.IsGameRunning))
            {
                GameArea.IsHitTestVisible = !_viewModel.IsGameRunning;
            }
        }

        public int GridSize => _viewModel.GridSize;

        /// <summary>
        /// Handles the PreviewMouseLeftButtonDown event for a cell in the game 
        /// grid.
        /// </summary>
        /// <remarks>This method toggles the active state of a cell when the 
        /// game is not running.  It captures the mouse to allow for continuous 
        /// drawing and marks the event as handled.</remarks>
        /// <param name="sender">The source of the event, expected to be a 
        /// FrameworkElement representing a cell.</param>
        /// <param name="e">The MouseButtonEventArgs containing event data.
        /// </param>
        private void Cell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.IsGameRunning)
            {
                return;
            }

            if (!(sender is FrameworkElement element) || !(element.DataContext is Cell cell))
            {
                return;
            }

            _isDrawingMode = !cell.IsActive;

            cell.IsActive = _isDrawingMode;
            _lastClickedCell = cell;

            GameArea.CaptureMouse();

            e.Handled = true;
        }

        /// <summary>
        /// Updates the sizes of the header and game area based on the current 
        /// window dimensions and grid size.
        /// </summary>
        /// <remarks>This method calculates the available space for the game 
        /// area and header by considering the window's actual size, margins, 
        /// and non-client area height. It then adjusts the cell size in
        /// the view model and updates the dimensions of the game area and 
        /// header accordingly.</remarks>
        private void UpdateHeaderAndGameAreaSize()
        {
            double nonClientAreaHeight = 0;
            if (WindowStyle == WindowStyle.SingleBorderWindow)
            {
                nonClientAreaHeight = UITitleBarApproxHeight;
            }

            double availableWidth = ActualWidth - HorizontalMarginCompensation;
            double availableHeight = ActualHeight - FixedHeaderHeight - nonClientAreaHeight - TotalVerticalMargins;
            double gameAreaSize = Math.Max(0, Math.Min(availableWidth, availableHeight));

            if (GridSize > 0)
            {
                _viewModel.CellSize = gameAreaSize / GridSize;
            }
            else
            {
                _viewModel.CellSize = 0;
            }
            Dispatcher.BeginInvoke(new Action(() =>
            {
                GameArea.Width = gameAreaSize;
                GameArea.Height = gameAreaSize;
                Header.Width = gameAreaSize;

            }), DispatcherPriority.Background);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged || e.HeightChanged)
            {
                UpdateHeaderAndGameAreaSize();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateHeaderAndGameAreaSize();
        }

        /// <summary>
        /// Handles the mouse movement events within the window, updating cell 
        /// states and managing fullscreen mode
        /// transitions.
        /// </summary>
        /// <remarks>This method updates the state of cells in the game area 
        /// when the left mouse button is pressed and the mouse is moved.
        /// It also toggles the window's fullscreen mode based on the mouse 
        /// position relative to the top of the window.</remarks>
        /// <param name="sender">The source of the event, typically the window.
        /// </param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing
        /// the event data, including mouse position and button states.</param>
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            Point position;

            if (e.LeftButton == MouseButtonState.Pressed && GameArea.IsMouseCaptured)
            {
                position = e.GetPosition(GameArea);

                int col = (int)(position.X / _viewModel.CellSize);
                int row = (int)(position.Y / _viewModel.CellSize);

                if (row >= 0 && row < GridSize && col >= 0 && col < GridSize)
                {
                    int index = row * GridSize + col;

                    if (index < _viewModel.Cells.Count)
                    {
                        Cell cell = _viewModel.Cells[index];

                        if (cell != _lastClickedCell)
                        {
                            cell.IsActive = _isDrawingMode;
                            _lastClickedCell = cell;
                        }
                    }
                }
            }

            if (WindowState != WindowState.Maximized)
            {
                if (WindowStyle != WindowStyle.SingleBorderWindow)
                {
                    SetFullscreenMode(false);
                }
                return;
            }

            position = e.GetPosition(this);
            bool mouseIsAtTop = position.Y < MouseDetectionArea;

            if (mouseIsAtTop && WindowStyle != WindowStyle.SingleBorderWindow)
            {
                SetFullscreenMode(false);
            }
            else if (!mouseIsAtTop && WindowStyle != WindowStyle.None)
            {
                SetFullscreenMode(true);
            }
        }

        private void SetFullscreenMode(bool isFullscreen)
        {
            if (isFullscreen)
            {
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
            }
            else
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                ResizeMode = ResizeMode.CanResizeWithGrip;
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                UpdateHeaderAndGameAreaSize();
            }), DispatcherPriority.Loaded);
        }

        /// <summary>
        /// Handles the MouseLeftButtonUp event for the window.
        /// </summary>
        /// <remarks>This method releases any mouse capture held by the GameArea
        /// and resets the last clicked cell. It also marks the event as handled
        /// to prevent further processing.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data containing information about the
        /// mouse button event.</param>
        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (GameArea.IsMouseCaptured)
            {
                GameArea.ReleaseMouseCapture();
            }

            if (Mouse.Captured != null)
            {
                Mouse.Capture(null);
            }

            _lastClickedCell = null;
            e.Handled = true;
        }
    }
}
